
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
        SetVariable("isTransitioning", core.isTransitioning.ToString());
        SetVariable("isAttached", core.isAttached.ToString());
        SetVariable("isDetached", core.isDetached.ToString());
        SetVariable("canTransition", core.canTransition.ToString());
        SetVariable("canDetach", core.canDetach.ToString());
        SetVariable("isBlocking", core.isBlocking.ToString());
        SetVariable("controlledBy", core.currentController == null ? "NA" : core.currentController.GetControllerType().ToString());
        SetVariable("isConstructed", core.isConstructed.ToString());
        SetVariable("isControlled", core.isControlled.ToString());
    }
}
