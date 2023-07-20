
using UnityEngine;


public class ClassInspectorMovement : ClassInspector
{
    [SerializeField] private ConstructObjectMovement movement;


    private void Start()
    {
        AddVariable("isSubscribed", "false");
        AddVariable("isAssigned", "false");
        AddVariable("isActive", "false");
        AddVariable("isPaused", "false");
        AddVariable("canActivate", "false");
        AddVariable("isBlocking", "false");
    }

    private void Update()
    {
        SetVariable("isSubscribed", movement.isSubscribed.ToString());
        SetVariable("isAssigned", movement.isAssigned.ToString());
        SetVariable("isActive", movement.isActive.ToString());
        SetVariable("isPaused", movement.isPaused.ToString());
        SetVariable("canActivate", movement.canActivate.ToString());
        SetVariable("isBlocking", movement.isBlocking.ToString());
    }
}
