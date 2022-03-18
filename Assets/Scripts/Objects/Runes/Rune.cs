
using System.Collections;
using UnityEngine;


public class Rune : MonoBehaviour {

  // #region - Setup

  // Declare references, variables
  public WorldObject wj { get; private set; }

  private bool slotted;
  private RuneHandler slottedAG;


  public void Awake() {
    // Initialize references
    wj = GetComponent<WorldObject>();
  }

  // #endregion


  // #region - Main

  public virtual void slot(RuneHandler ag_, Transform slot_) {
    // Set to be slotted
    slotted = true;
    slottedAG = ag_;

    // Update position / rotation / collision
    transform.parent = slot_;
    wj.cl.enabled = false;
    transform.localPosition = Vector3.zero;
    transform.localRotation = Quaternion.identity;
    wj.rb.isKinematic = true;
  }


  public virtual void unslot() {
    // Set to be not slotted
    slotted = false;
    slottedAG = null;

    // Update collision
    wj.rb.isKinematic = false;
  }

  // #endregion
}
