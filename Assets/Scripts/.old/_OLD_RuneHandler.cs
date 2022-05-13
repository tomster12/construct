
using UnityEngine;


public class RuneHandler : MonoBehaviour
{
    // Declare references, variables
    [SerializeField] private Transform[] slotTransforms;
    public WorldObject selfWJ { get; private set; }

    private Rune[] slottedRunes;


    public void Awake()
    {
        // Initialize references
        selfWJ = GetComponent<WorldObject>();
    }


    public void Start()
    {
        // Initialize variables
        slottedRunes = new Rune[slotTransforms.Length];
    }


    public bool slotRune(int index, Rune rune)
    {
        // Index out of range
        if (index < 0 || index >= slotTransforms.Length) return false;

        // Slot already taken
        if (slottedRunes[index] != null) return false;

        // Slot rune
        rune.slot(this, slotTransforms[index]);
        slottedRunes[index] = rune;
        return true;
    }
}
