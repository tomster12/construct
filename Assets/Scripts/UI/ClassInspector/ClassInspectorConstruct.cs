
using UnityEngine;


public class ClassInspectorConstruct : ClassInspector
{
    [SerializeField] private Construct construct;


    private void Start()
    {
        AddVariable("canMove", "false");
        AddVariable("canUseSkill", "false");
        AddVariable("isBlocking", "false");
    }

    private void Update()
    {
        SetVariable("canMove", construct.CanMove().ToString());
        SetVariable("canUseSkill", construct.CanUseSkill().ToString());
        SetVariable("isBlocking", construct.IsBlocking().ToString());
    }
}
