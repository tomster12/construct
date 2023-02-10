
using UnityEngine;


public class ClassInspectorMovement : ClassInspector
{
    [SerializeField] private ConstructMovement movement;


    private void Start()
    {
        AddVariable("isConstructed", "false");
        AddVariable("isActive", "false");
        AddVariable("isPaused", "false");
        AddVariable("isTransitioning", "false");
        AddVariable("isBlocking", "false");
}


    private void Update()
    {
        SetVariable("isConstructed", movement.isConstructed.ToString());
        SetVariable("isActive", movement.isActive.ToString());
        SetVariable("isPaused", movement.isPaused.ToString());
        SetVariable("isTransitioning", movement.isTransitioning.ToString());
        SetVariable("isBlocking", movement.isBlocking.ToString());
    }
}
