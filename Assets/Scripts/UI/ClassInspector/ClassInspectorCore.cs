
using UnityEngine;


public class ClassInspectorCore : ClassInspector
{
    [SerializeField] private ConstructCore core;


    private void Start()
    {
        AddVariable("controlledBy", "null");
        AddVariable("isConstructed", "false");
        AddVariable("isControlled", "false");
    }

    private void Update()
    {
        SetVariable("isTransitioning", core.IsTransitioning().ToString());
        SetVariable("isAttached", core.IsAttached().ToString());
        SetVariable("isDetached", core.IsDetached().ToString());
        SetVariable("canTransition", core.CanTransition().ToString());
        SetVariable("canDetach", core.CanDetach().ToString());
        SetVariable("isBlocking", core.IsBlocking().ToString());
        SetVariable("controlledBy", core.GetIController() == null ? "NA" : core.GetIController().GetIControllerType().ToString());
        SetVariable("isConstructed", core.IsConstructed().ToString());
        SetVariable("isControlled", core.IsControlled().ToString());
    }
}
