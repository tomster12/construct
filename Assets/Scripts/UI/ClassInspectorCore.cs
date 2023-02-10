
using UnityEngine;


public class ClassInspectorCore : ClassInspector
{
    [SerializeField] private ConstructCore core;


    private void Start()
    {
        AddVariable("isTransitioning", "false");
        AddVariable("isAttached", "false");
        AddVariable("isDetached", "false");
        AddVariable("isBlocking", "false");
        AddVariable("canTransition", "false");
        AddVariable("canDetach", "false");
    }


    private void Update()
    {
        SetVariable("isTransitioning", core.isTransitioning.ToString());
        SetVariable("isAttached", core.isAttached.ToString());
        SetVariable("isDetached", core.isDetached.ToString());
        SetVariable("isBlocking", core.isBlocking.ToString());
        SetVariable("canTransition", core.canTransition.ToString());
        SetVariable("canDetach", core.canDetach.ToString());
    }
}
