
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CoreState { Detached, Attaching, Attached, Detaching };


public class ConstructCore : ConstructObject
{
    // Declare references, config, variables
    private ICCMovement movement;

    private AttachSkill attachSkill;
    private DetachSkill detachSkill;
    private CoreState state = CoreState.Detached;
    private ConstructObject attachedCO;


    protected override void Awake()
    {
        base.Awake();

        // Initialize references and variables
        SetCCMovement(GetComponent<ICCMovement>());
        attachSkill = new AttachSkill(this);
        detachSkill = new DetachSkill(this);
    }


    public override void MoveInDirection(Vector3 dir)
    {
        // Pass to attached construct object otherwise self
        if (state == CoreState.Detached) base.MoveInDirection(dir);
        else if (state == CoreState.Attached) attachedCO.MoveInDirection(dir);
    }

    public override void AimAtPosition(Vector3 pos)
    {
        // Pass to attached construct object otherwise self
        if (state == CoreState.Detached) base.AimAtPosition(pos);
        else if (state == CoreState.Attached) attachedCO.AimAtPosition(pos);
    }


    public override bool GetContainsWO(WorldObject checkWO) => state == CoreState.Attached ? attachedCO.GetContainsWO(checkWO) : baseWO == checkWO;

    public override bool GetContainsCO(ConstructObject checkCO) => state == CoreState.Attached ? attachedCO.GetContainsCO(checkCO) : checkCO == this;

    public override ConstructObject GetCentreCO() => state == CoreState.Attached ? attachedCO.GetCentreCO() : this;

    public override bool GetCanForge() => base.GetCanForge()
        && (state == CoreState.Detached || state == CoreState.Attached)
        && (state == CoreState.Attached ? attachedCO.GetCanForge() : true);

    public CoreState GetState() => state;

    public ConstructObject GetAttachedCO() => attachedCO;

    public bool GetCanAttach(ConstructObject checkCO) => checkCO.construct == null && !checkCO.GetControlled();


    public override void SetControlled(bool isControlled_)
    {
        base.SetControlled(isControlled_);

        // Bind skills to construct
        if (isControlled_)
        {
            if (state == CoreState.Detached && !attachSkill.isBinded) construct.skills.RequestBinding(attachSkill, "f", true);
            else if (state == CoreState.Attached && !detachSkill.isBinded) construct.skills.RequestBinding(detachSkill, "f", true);
        }
    }

    public override void SetForging(bool isForging_)
    {
        // Update state and rb values
        base.SetForging(isForging_);
        if (state == CoreState.Attached) attachedCO.SetForging(isForging_);
        if (!isForging_) SetState(state);
    }

    private void SetCCMovement(ICCMovement movement_) { SetCOMovement(movement_); movement = movement_; movement_.SetCC(this); }

    public void SetState(CoreState state_)
    {
        // Update state and rb values
        state = state_;
        if (state == CoreState.Attached)
        {
            SetLoose(false);
            SetFloating(true);
        }
    }

    public void SetAttachedCO(ConstructObject attachedCO_) => attachedCO = attachedCO_; 


    private class AttachSkill : Skill
    {
        // Declare variables
        private ConstructCore core;
        private ConstructObject targetedCO;
        

        public AttachSkill(ConstructCore core_) : base(0.0f) { core = core_; }


        public override void Bind(SkillBindings bindings)
        {
            base.Bind(bindings);
            
            // Update player instance UI variables
            PlayerController.instance.showNearby = true;
            PlayerController.instance.canHighlight = true;
            PlayerController.instance.allowedHoverableStates[IHoverableState.LOOSE] = true;
        }

        public override void Unbind()
        {
            // Update player instance UI variables
            PlayerController.instance.showNearby = false;
            PlayerController.instance.canHighlight = false;
            PlayerController.instance.allowedHoverableStates[IHoverableState.LOOSE] = false;
        }


        public override void Update()
        {
            // If can attach and not currently attaching
            if (core.state == CoreState.Detached && core.construct.GetCanUseSkill() && !isActive)
            {
                ConstructObject aimedCO = PlayerController.instance.aimedCO;
                if (aimedCO != null && core.GetCanAttach(aimedCO)) targetedCO = aimedCO;
            }
        }


        public override void Use() => core.StartCoroutine(UseIE());
        
        private IEnumerator UseIE()
        {
            if (!GetUsable()) yield break;

            // As long as is still detached
            if (core.state == CoreState.Detached)
            {
                // Attach core then replace skill
                SetActive(true);
                yield return core.movement.AttachCoreIE(targetedCO, PlayerController.instance.aimedPos);
                core.construct.skills.RequestBinding(core.detachSkill, "f", true);
                SetActive(false);
            }
        }


        protected override bool GetUsable() => base.GetUsable() && targetedCO != null && core.construct.GetCanUseSkill();

        public CoreState GetState() => core.state;

        public ConstructObject GetAttachedCO() => core.attachedCO;
    }

    private class DetachSkill : Skill
    {
        // Declare variables
        private ConstructCore core;


        public DetachSkill(ConstructCore core_) : base(0.0f) { core = core_; }


        public override void Use() => core.StartCoroutine(UseIE());
        private IEnumerator UseIE()
        {
            if (!GetUsable()) yield break;

            // Detach core from current CO
            if (core.state == CoreState.Attached)
            {
                SetActive(true);
                yield return core.movement.DetachCoreIE();
                core.construct.skills.RequestBinding(core.attachSkill, "f", true);
                SetActive(false);
            }
        }


        public CoreState GetState() => core.state;

        public ConstructObject GetAttachedCO() => core.attachedCO;
    }
}
