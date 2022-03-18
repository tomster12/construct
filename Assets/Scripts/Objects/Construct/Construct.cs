
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Construct : MonoBehaviour {

  // #region - Setup

  // Declare references, variables
  private Transform objectContainer;
  public List<WorldObject> orbWJs { get; private set; }
  public WorldObject coreWJ { get; private set; }
  public CoreMovementI coreMovement { get; private set; }
  public WorldObject mainWJ { get; private set; }
  public MovementI mainMovement { get; private set; }


  public void initConstruct(WorldObject coreWJ_) {
    // Setup core variables
    objectContainer = transform.parent;
    orbWJs = new List<WorldObject>();
    coreWJ = coreWJ_;
    coreWJ.transform.parent = transform;
    coreMovement = coreWJ.GetComponent<CoreMovementI>();

    // Control core by default
    mainWJ = coreWJ;
    mainMovement = coreMovement;
  }


  public void terminateConstruct() {
    // Stop completely being a construct
    mainMovement.setActive(false);
    foreach (WorldObject wj in orbWJs)
      wj.transform.parent = objectContainer;
    Destroy(gameObject);
  }

  // #endregion


  // #region - Main

  public void aimAtPosition(Vector3 pos, float force) {
    // Pass aim onto correct MovementI
    if (mainMovement != null)
      mainMovement.aimAtPosition(pos, force);
  }


  public void moveInDirection(Vector3 dir, float force) {
    // Pass movement onto correct MovementI
    if (mainMovement != null)
      mainMovement.moveInDirection(dir, force);
  }


  public void interact(WorldObject targetWJ, Vector3 aimedPos) {
    // Try attach core (*1)
    if (targetWJ != null
      && getCoreState() == CoreState.Detached
      && !getContainsWJ(targetWJ)) attachCore(targetWJ, aimedPos);

    // Attack in direction
    else if (getCoreState() == CoreState.Attached) mainMovement.attack(targetWJ, aimedPos);
  }


  public bool canInteractWJ(WorldObject targetWJ) {
    // Can attach core (*1)
    bool canAttach = (targetWJ != null
      && getCoreState() == CoreState.Detached
      && !getContainsWJ(targetWJ));

    return canAttach;
  }


  public void setActive(bool active_) {
    // Update whether active
    mainMovement.setActive(active_);
  }

  // #endregion


  // #region - Orbs

  public void setKinematic(bool kinematic_) {
    if (getCoreState() == CoreState.Attached) {
      // Set isKinematic on all world objects
      foreach (WorldObject orbWJ in orbWJs)
        orbWJ.rb.isKinematic = kinematic_;

    } else if (getCoreState() == CoreState.Detached) {
      coreWJ.rb.isKinematic = kinematic_;
    }
  }


  private void setControlled(WorldObject wj) {
    // Update current controlled world object
    if (mainMovement != null) mainMovement.setActive(false);
    mainWJ = wj;
    mainMovement = mainWJ.GetComponent<MovementI>();
    if (mainMovement != null) mainMovement.setActive(true);
  }


  public bool getContainsWJ(WorldObject wj) {
    // Check whether WJ is within the construct
    return coreWJ == wj || orbWJs.Contains(wj);
  }

  // #endregion


  // #region - Core

  public void attachCore(WorldObject targetWJ, Vector3 aimedPos) { StartCoroutine(attachCoreIE(targetWJ)); }

  public IEnumerator attachCoreIE(WorldObject targetWJ) {
    if (getCoreState() == CoreState.Detached) {

      // Wait for core to attach - Also setActive(false) on core
      yield return coreMovement.attachCoreIE(targetWJ);

      // Handle attaching to world object
      setControlled(targetWJ);
      orbWJs.Add(targetWJ);
      mainWJ.transform.parent = transform;
    }
  }


  public void detachCore() { StartCoroutine(detachCoreIE()); }

  public IEnumerator detachCoreIE() {
    if (getCoreState() == CoreState.Attached) {

      // Handle detaching from current world
      setControlled(coreWJ);
      foreach (WorldObject orbWJ in orbWJs)
        orbWJ.transform.parent = objectContainer;
      orbWJs.Clear();

      // Update core parent
      coreWJ.transform.parent = transform;

      // Tell core to detach - Also setActive(true) on core
      yield return coreMovement.detachCoreIE();
    }
  }


  public CoreState getCoreState() {
    // Return core state
    return coreMovement.getCoreState();
  }

  // #endregion
}
