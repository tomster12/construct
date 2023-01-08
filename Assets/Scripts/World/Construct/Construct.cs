
using System.Collections.Generic;
using UnityEngine;


public enum ConstructState { LOOSE, ACTIVE, FORGING };


public class Construct : MonoBehaviour, IMovable
{
    [Header("References")]
    [SerializeField] private ConstructCore _core;
    public ConstructCore core => _core;

    public ConstructMovement movement { get; private set; }
    public SkillBindings skills { get; private set; } = new SkillBindings(new List<string>() { "_0", "1", "2", "3", "4", "f" });
    public ConstructState? state { get; private set; }
    public bool isBlocking => (movement != null && movement.isBlocking) || core.isBlocking;
    public bool canUseSkill => state == ConstructState.ACTIVE && !isBlocking;


    private void Start()
    {
        // Initialize variables
        skills.Clear();
        InitConstruct(core);
    }

    public void InitConstruct(ConstructCore core_)
    {
        SetCore(core_);
        SetState(ConstructState.LOOSE);
    }


    public void Update()
    {
        // Update binded skills
        skills.UpdateSkills();
    }


    public void MoveInDirection(Vector3 dir) => movement?.MoveInDirection(dir);
    
    public void AimAtPosition(Vector3 pos) => movement?.AimAtPosition(pos);


    public bool GetStateAccessible(ConstructState state_)
    {
        // Gate keeper for changing state
        if (isBlocking) return false;
        bool accessible = false;
        if (state_ == ConstructState.LOOSE) accessible = state == null || state == ConstructState.ACTIVE;
        if (state_ == ConstructState.ACTIVE) accessible = (state == ConstructState.LOOSE && movement != null) || state == ConstructState.FORGING;
        if (state_ == ConstructState.FORGING) accessible = state == ConstructState.ACTIVE && (core.state == CoreState.Detached || core.state == CoreState.Attached);
        return accessible;
    }


    private void SetCore(ConstructCore core_) { _core = core_; core.SetConstruct(this); }

    public bool SetState(ConstructState state_)
    {
        if (!GetStateAccessible(state_)) return false;

        // Update construct states
        if (state_ == ConstructState.LOOSE) movement?.SetActive(false);
        else if (state_ == ConstructState.ACTIVE) { movement?.SetActive(true); movement?.SetPaused(false); }
        else if (state_ == ConstructState.FORGING) movement?.SetPaused(true);

        // Update state variable
        state = state_;
        return true;
    }

    public void OverwriteMovement(ConstructMovement movement_)
    {
        // Overwrite movement and enable if needed
        movement = movement_;
        if (movement != null) movement.SetActive(state == ConstructState.ACTIVE);
    }


    #region Helper

    public bool GetContainsWO(WorldObject checkWO) => core.GetContainsWO(checkWO);

    public bool GetContainsCO(ConstructObject checkCO) => core.GetContainsCO(checkCO);

    public ConstructObject GetCentreCO() => core.GetCentreCO();

    public Vector3 GetCentrePosition() => core.GetCentrePosition();

    #endregion
}
