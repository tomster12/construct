
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;


public class ClassInspectorObject : ClassInspector
{
    [SerializeField] private IConstructPart IPart;


    protected virtual void Start()
    {
        AddVariable("controlledBy", "null");
        AddVariable("isConstructed", "false");
        AddVariable("isControlled", "false");
    }

    protected void Update()
    {
        SetVariable("controlledBy", IPart.GetIController() == null ? "NA" : IPart.GetIController().GetIControllerType().ToString());
        SetVariable("isConstructed", IPart.IsConstructed().ToString());
        SetVariable("isControlled", IPart.IsControlled().ToString());
    }
}
