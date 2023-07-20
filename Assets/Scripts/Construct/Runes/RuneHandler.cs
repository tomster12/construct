
using UnityEngine;


public class RuneHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] slots;
    
    private Construct construct;


    public void SlotRune(Rune rune, int slot=-1)
    {
        Debug.Log("Slotting rune " + rune + " into " + this);
        if (slot == -1) slot = 0;
        rune.SetSlotted(this);
        rune.transform.parent = slots[slot];
        rune.transform.localPosition = Vector3.zero;
        rune.transform.localRotation = Quaternion.identity;
    }


    public virtual void OnJoinConstruct(Construct construct_)
    {
        construct = construct_;
    }

    public virtual void OnExitConstruct()
    {
        construct = null;
    }
}
