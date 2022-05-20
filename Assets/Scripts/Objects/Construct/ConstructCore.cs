
using System.Collections;
using UnityEngine;


public enum CoreState { Detached, Attaching, Attached, Detaching };


public class ConstructCore : ConstructObject
{
    // Declare references, variables
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

    public CoreState GetState() => state;

    public ConstructObject GetAttachedCO() => attachedCO;


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
        private Vector3 hoveredPos;
        private ConstructObject hoveredCO;
        private WorldObject hoveredWO;


        public AttachSkill(ConstructCore core_) : base(0.0f) { core = core_; }


        public override void Unbind()
        {
            // Unhover current construct object
            if (hoveredCO != null) Unhover();
        }


        public override void Update()
        {
            // Check if hovering new potential WO
            if (core.state == CoreState.Detached && core.construct.GetCanUseSkill())
            {
                hoveredPos = PlayerController.instance.aimedPos;
                WorldObject aimedWO = PlayerController.instance.aimedWO;
                if (hoveredWO != aimedWO)
                {
                    // Unhighlight old, and highlight new if CO
                    if (hoveredWO != null) Unhover();
                    ConstructObject aimedCO = aimedWO == null ? null : aimedWO.GetComponent<ConstructObject>();
                    if (aimedCO != null && !core.GetContainsCO(aimedCO)) Hover(aimedCO);
                }
            }

            // Unhover current construct object
            else if (hoveredCO != null) Unhover();
        }


        private void Hover(ConstructObject aimedCO)
        {
            // Highlight new CO
            aimedCO.baseWO.isHighlighted = true;
            hoveredCO = aimedCO;
            hoveredWO = aimedCO.baseWO;
        }

        private void Unhover()
        {
            // Unhighlight old CO
            if (hoveredWO != null) hoveredWO.isHighlighted = false;
            hoveredPos = Vector3.zero;
            hoveredCO = null;
            hoveredWO = null;
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
                yield return core.movement.AttachCoreIE(hoveredCO, hoveredPos);
                core.construct.skills.RequestBinding(core.detachSkill, "f", true);
                SetActive(false);
            }
        }


        protected override bool GetUsable() => base.GetUsable() && hoveredCO != null && core.construct.GetCanUseSkill();

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
