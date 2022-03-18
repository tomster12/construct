
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RuneHandler : MonoBehaviour {

  // #region - Setup

  // Declare references, variables
  public WorldObject wj { get; private set; }

  public Transform[] slotTransforms;
  private Rune[] slottedRunes;


  public void Awake() {
    // Initialize references
    wj = GetComponent<WorldObject>();
  }


  public void Start() {
    // Initialize variables
    slottedRunes = new Rune[slotTransforms.Length];
  }

  // #endregion


  // #region - Main

  public bool slotRune(int index, Rune Rune) {
    // Index out of range
    if (index < 0 || index >= slotTransforms.Length) return false;

    // Slot already taken
    if (slottedRunes[index] != null) return false;

    // Slot rune
    slottedRunes[index] = Rune;
    Rune.slot(this, slotTransforms[index]);
    return true;
  }

  // #endregion
}
