
using UnityEngine;


public class Rune : MonoBehaviour
{
    // Declare references, variables
    public WorldObject selfWJ { get; private set; }

    public bool isSlotted { get; protected set; }
    public RuneHandler slottedHandler { get; protected set; }


    public void Awake()
    {
        // Initialize references
        selfWJ = GetComponent<WorldObject>();
    }


    public virtual void slot(RuneHandler handler_, Transform slot_)
    {
        // Set to be slotted
        isSlotted = true;
        slottedHandler = handler_;

        // Update position / rotation / collision
        transform.parent = slot_;
        selfWJ.cl.enabled = false;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        selfWJ.rb.isKinematic = true;
    }


    public virtual void unslot()
    {
        // Set to be not slotted
        isSlotted = false;
        slottedHandler = null;

        // Update collision
        // TODO: Consolidate collider / rigidbody state
        selfWJ.rb.isKinematic = false;
    }
}
