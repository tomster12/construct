
using UnityEngine;


public class GetRuneSkill : Skill
{
    RuneHandler runeHandler;
    PlayerController playerController;


    public void Init(RuneHandler runeHandler_, PlayerController playerController_)
    {
        runeHandler = runeHandler_;
        playerController = playerController_;
    }
    

    public override void Use()
    {
        Debug.Log("Trying to get rune");
        Transform hoveredTF = playerController.currentHover.hoveredT;
        if (hoveredTF != null)
        {
            Rune hoveredRune = hoveredTF.GetComponent<Rune>();
            if (hoveredRune != null)
            {
                runeHandler.SlotRune(hoveredRune);
            }
        }
    }
}
