
using UnityEngine;


public class ClassInspectorCoreShape : ClassInspector
{
    [SerializeField] private ConstructCore core;


    private void Start()
    {
        AddVariable("isActive", "false");
    }

    private void Update()
    {
        SetVariable("isActive", core.shapeCoreAttachment == null ? "false" : core.shapeCoreAttachment.isActive.ToString());
    }
}
