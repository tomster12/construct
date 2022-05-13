
using UnityEngine;


public class Construct : MonoBehaviour, IMovable
{
    // Declare constants, references, variables
    public readonly string[] abilityButtons = new string[] { "1", "2", "3", "4" };

    [SerializeField] private ConstructCore core;
    
    public SkillBindings skills { get; private set; } = new SkillBindings();


    private void Start()
    {
        // Initialize main parts
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


    public bool GetContainsWO(WorldObject checkWO) => core.GetContainsWO(checkWO);

    public bool GetContainsCO(ConstructObject checkCO) => core.GetContainsCO(checkCO);

    public ConstructObject GetCentreCO() => core.GetCentreCO();

    public bool GetControlled() => core.GetControlled();


    public void SetControlled(bool isControlled_) => core.SetControlled(isControlled_);

    private void SetCore(ConstructCore core_) { core = core_; core.SetConstruct(this); }
}
