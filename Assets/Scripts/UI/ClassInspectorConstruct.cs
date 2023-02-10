
using UnityEngine;


public class ClassInspectorConstruct : ClassInspector
{
    [SerializeField] private Construct construct;


    private void Start()
    {
        AddVariable("isBlocking", "false");
        AddVariable("canUseSkill", "false");
    }


    private void Update()
    {
        SetVariable("isBlocking", construct.isBlocking.ToString());
        SetVariable("canUseSkill", construct.canUseSkill.ToString());
    }
}
