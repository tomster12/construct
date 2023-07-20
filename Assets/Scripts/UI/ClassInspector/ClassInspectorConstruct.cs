
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
        SetVariable("canMove", construct.canMove.ToString());
        SetVariable("canUseSkill", construct.canUseSkill.ToString());
        SetVariable("isBlocking", construct.isBlocking.ToString());
    }
}
