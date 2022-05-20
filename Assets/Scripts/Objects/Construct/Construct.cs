
using UnityEngine;


public class Construct : MonoBehaviour, IMovable
{
    // Declare constants, references, variables
    public readonly string[] abilityButtons = new string[] { "_0", "1", "2", "3", "4" };

    [Header("References")]
    [SerializeField] private ConstructCore core;
    
    public SkillBindings skills { get; private set; } = new SkillBindings();
    public bool isForging { get; private set; }


    private void Start()
    {
        // Initialize variables
        InitConstruct();
    }

    public void InitConstruct()
    {
        // Clear skills and intialize core
        skills.Clear();
        SetCore(core);
        SetControlled(true);
    }


    public void Update()
    {
        // Update binded skills
        skills.UpdateSkills();
    }


    public void MoveInDirection(Vector3 dir) => core.MoveInDirection(dir);
    
    public void AimAtPosition(Vector3 pos) => core.AimAtPosition(pos);


    public bool GetControlled() => core.GetControlled();
    
    public bool GetContainsWO(WorldObject checkWO) => core.GetContainsWO(checkWO);

    public bool GetContainsCO(ConstructObject checkCO) => core.GetContainsCO(checkCO);

    public ConstructObject GetCentreCO() => core.GetCentreCO();

    public bool GetCanUseSkill() => !isForging;

    public bool GetCanForge() => core.GetState() == CoreState.Detached || core.GetState() == CoreState.Attached;


    private void SetCore(ConstructCore core_) { core = core_; core.SetConstruct(this); }
 
    public void SetControlled(bool isControlled_) => core.SetControlled(isControlled_);

    public void SetForging(bool isForging_)
    {
        if (!GetCanForge()) return;

        // Set forging and pass through parts
        isForging = isForging_;
        core.SetForging(isForging_);
    }
}
