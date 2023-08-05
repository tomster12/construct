
using System;
using System.Collections.Generic;
using UnityEngine;


public class Construct : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ConstructCore initCore;

    public Action onLayoutChanged;
    public Action<ConstructState> onStateChanged;
    public ConstructCore core { get; private set; }
    public HashSet<ConstructObject> trackedObjects { get; private set; } = new HashSet<ConstructObject>();
    public SkillBindings skills { get; private set; } = new SkillBindings(new List<string>() { "_0", "1", "2", "3", "4", "f" });
    public ConstructState state { get; private set; } = ConstructState.INACTIVE;
    public bool canMove => state == ConstructState.ACTIVE && !core.isBlocking && currentMovement != null;
    public bool canUseSkill => state == ConstructState.ACTIVE && !isBlocking;
    public bool isBlocking => core.isBlocking || currentMovement == null || currentMovement.isBlocking || skills.isBlocking;

    private List<MovementOption> movementOptions = new List<MovementOption>();
    private ConstructObjectMovement currentMovement = null;


    private void Start()
    {
        // Initialize variables
        skills.Clear();
        InitConstruct(initCore);
    }

    public void InitConstruct(ConstructCore core)
    {
        if (core == null) throw new Exception("Need a core to InitConstruct");
        SetCore(core);
        SetState(ConstructState.INACTIVE);
    }


    public void Update()
    {
        // Update binded skills
        skills.UpdateSkills();
    }

    public void MoveInDirection(Vector3 dir)
    {
        if (canMove) currentMovement?.MoveInDirection(dir);
    }

    public void AimAtPosition(Vector3 pos)
    {
        if (canMove) currentMovement?.AimAtPosition(pos);
    }

    public void SubscribeMovement(ConstructObjectMovement movement_, int priority_)
    {
        if (movement_ == null) return;

        // Add a new option to the ordered list
        movementOptions.Add(new MovementOption(movement_, priority_));
        movement_.OnJoinConstruct(this);
        PickBestMovement();
    }

    public void UnsubscribeMovement(ConstructObjectMovement movement_)
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

    public void PickBestMovement()
    {
        // Find new best movement
        movementOptions.Sort();
        ConstructObjectMovement newMovement = null;
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

    public void AddObject(ConstructObject trackedCO)
    {
        trackedObjects.Add(trackedCO);
        trackedCO.OnJoinConstruct(this);
        if (onLayoutChanged != null) onLayoutChanged();
    }

    public void RemoveObject(ConstructObject trackedCO)
    {
        trackedObjects.Remove(trackedCO);
        trackedCO.OnExitConstruct();
        if (onLayoutChanged != null) onLayoutChanged();
    }


    public bool GetStateAccessible(ConstructState state_)
    {
        switch (state_)
        {
            case ConstructState.INACTIVE:
                return (state == ConstructState.ACTIVE && !isBlocking);

            case ConstructState.ACTIVE:
                return (state == ConstructState.INACTIVE && currentMovement != null) || (state == ConstructState.FORGING);

            case ConstructState.FORGING:
                return (state == ConstructState.ACTIVE && !isBlocking);
            
            default: return false;
        }
    }

    public bool GetContainsWO(WorldObject checkWO) => core.GetContainsWO(checkWO);

    public bool GetContainsCO(ConstructObject checkCO) => core.GetContainsCO(checkCO);

    public ConstructObject GetCentreCO() => core.GetCentreCO();

    public Vector3 GetCentrePosition() => core.GetCentrePosition();

    private void SetCore(ConstructCore core_)
    {
        if (core != null) throw new Exception("core already set on Construct.");
        core = core_;
        AddObject(core);
    }

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
    

    [Serializable]
    public class MovementOption : IComparable<MovementOption>
    {
        public ConstructObjectMovement movement { get; private set; }
        public int priority { get; private set; }

        public MovementOption(ConstructObjectMovement movement_, int priority_)
        {
            movement = movement_;
            priority = priority_;
        }

        public int CompareTo(MovementOption other) => -priority.CompareTo(other.priority);
    }
}
