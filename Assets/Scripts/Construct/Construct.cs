
using System;
using System.Collections.Generic;
using UnityEngine;


public class Construct : MonoBehaviour, IConstruct
{
    [Header("References")]
    [SerializeField] private ConstructCore initConstructCore;

    private IConstructCore IConstructCore;
    private HashSet<IConstructPart> containedIParts = new HashSet<IConstructPart>();
    private SkillBindings skills = new SkillBindings(new List<string>() { "_0", "1", "2", "3", "4", "f" });
    private ConstructState state = ConstructState.INACTIVE;
    private List<MovementOption> movementOptions = new List<MovementOption>();
    private ConstructPartMovement currentMovement = null;
    private Action onLayoutChanged;
    private Action<ConstructState> onStateChanged;

    public bool CanMove() => state == ConstructState.ACTIVE && !IConstructCore.IsBlocking() && currentMovement != null;
    public bool CanUseSkill() => state == ConstructState.ACTIVE && !IsBlocking();
    public bool IsBlocking() => IConstructCore.IsBlocking() || currentMovement == null || currentMovement.IsBlocking() || skills.IsBlocking;


    private void Start()
    {
        // Initialize variables
        skills.Clear();
        InitConstruct(initConstructCore);
    }

    private void Update()
    {
        // Update binded skills
        skills.UpdateSkills();
    }


    public void MoveInDirection(Vector3 dir)
    {
        if (CanMove()) currentMovement?.MoveInDirection(dir);
    }

    public void AimAtPosition(Vector3 pos)
    {
        if (CanMove()) currentMovement?.AimAtPosition(pos);
    }

    public void AddIPart(IConstructPart IPart)
    {
        containedIParts.Add(IPart);
        IPart.OnJoinConstruct(this);
        onLayoutChanged?.Invoke();
    }

    public void RemoveIPart(IConstructPart IPart)
    {
        containedIParts.Remove(IPart);
        IPart.OnExitConstruct();
        onLayoutChanged?.Invoke();
    }

    public void SubscribeMovement(ConstructPartMovement movement_, int priority_)
    {
        if (movement_ == null) return;

        // Add a new option to the ordered list
        movementOptions.Add(new MovementOption(movement_, priority_));
        movement_.OnJoinConstruct(this);
        PickBestMovement();
    }

    public void UnsubscribeMovement(ConstructPartMovement movement_)
    {
        if (movement_ == null) return;

        // Remove a movement from the options
        for (int i = 0; i < movementOptions.Count; i++)
        {
            if (movementOptions[i].movement == movement_)
            {
                movementOptions[i].movement.OnExitConstruct();
                movementOptions.RemoveAt(i);
                break;
            }
        }
        PickBestMovement();
    }

    public void SubscribeSkill(Skill skill, string binding) => skills.RequestBinding(skill, binding);

    public void UnsubscribeSkill(Skill skill) => skills.Unbind(skill);

    public bool ContainsObject(Object checkObject) => IConstructCore.ContainsObject(checkObject);

    public bool ContainsIPart(IConstructPart IPart) => IConstructCore.ContainsIPart(IPart);

    public IConstructPart GetCentreIPart() => IConstructCore.GetCentreIPart();

    public HashSet<IConstructPart> GetContainedIParts() => containedIParts;

    public IConstructCore GetICore() => IConstructCore;

    public Vector3 GetPosition() => GetCentreIPart().GetPosition();

    public Transform GetTransform() => transform;

    public bool GetStateAccessible(ConstructState newState)
    {
        return newState switch
        {
            ConstructState.INACTIVE => (state == ConstructState.ACTIVE && !IsBlocking()),
            ConstructState.ACTIVE => (state == ConstructState.INACTIVE && currentMovement != null) || (state == ConstructState.FORGING),
            ConstructState.FORGING => (state == ConstructState.ACTIVE && !IsBlocking()),
            _ => false,
        };
    }

    public ConstructState GetState() => state;

    public bool SetState(ConstructState state_)
    {
        if (!GetStateAccessible(state_)) return false;

        // Update construct states
        state = state_;
        switch (state_)
        {
            case ConstructState.INACTIVE:
                currentMovement?.SetActive(false);
                onStateChanged(state);
                return true;

            case ConstructState.ACTIVE:
                currentMovement?.SetActive(true);
                currentMovement?.SetPaused(false);
                onStateChanged(state);
                return true;

            case ConstructState.FORGING:
                currentMovement?.SetPaused(true);
                onStateChanged(state);
                return true;
        }
        return false;
    }

    public void SubscribeOnLayoutChanged(System.Action action) => onLayoutChanged += action;
    
    public void UnsubscribeOnLayoutChanged(System.Action action) => onLayoutChanged -= action;

    public void SubscribeOnStateChanged(System.Action<ConstructState> action) => onStateChanged += action;
    
    public void UnsubscribeOnStateChanged(System.Action<ConstructState> action) => onStateChanged -= action;
    
    public void OnMovementUpdate() => PickBestMovement();


    private void InitConstruct(IConstructCore newICore)
    {
        if (newICore == null) throw new Exception("Need a core to InitConstruct");
        SetCore(newICore);
    }

    private void PickBestMovement()
    {
        // Find new best movement
        movementOptions.Sort();
        ConstructPartMovement newMovement = null;
        for (int i = 0; i < movementOptions.Count; i++)
        {
            if (movementOptions[i].movement.canActivate)
            {
                newMovement = movementOptions[i].movement;
                break;
            }
        }

        // Transfer to new movement
        if (newMovement != currentMovement)
        {
            if (currentMovement != null)
            {
                currentMovement.OnUnassign();
            }
            if (newMovement != null)
            {
                newMovement.OnAssign();
                if (state == ConstructState.ACTIVE) newMovement.SetActive(true);
            }
            currentMovement = newMovement;
        }
    }

    private void SetCore(IConstructCore newICore)
    {
        if (IConstructCore != null) throw new Exception("Core already set on Construct.");
        IConstructCore = newICore;
        AddIPart(IConstructCore);
        IConstructCore.GetTransform().parent = GetTransform();
    }


    [Serializable]
    public class MovementOption : IComparable<MovementOption>
    {
        public ConstructPartMovement movement { get; private set; }
        public int priority { get; private set; }

        public MovementOption(ConstructPartMovement movement_, int priority_)
        {
            movement = movement_;
            priority = priority_;
        }

        public int CompareTo(MovementOption other) => -priority.CompareTo(other.priority);
    }
}
