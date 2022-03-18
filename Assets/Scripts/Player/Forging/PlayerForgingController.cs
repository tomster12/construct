
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerForgingController : MonoBehaviour {

  // Declare references
  private PlayerForgingCamera pcam;
  private PlayerConstructController constructController;

  public Construct mainConstruct { get; private set; }
  public WorldObject mainWJ { get; private set; }
  private Vector3 centreTargetPos;
  private Quaternion centreTargetRot;


  private void Awake() {
    // Initialize references
    pcam = GetComponent<PlayerForgingCamera>();
    constructController = GetComponent<PlayerConstructController>();
  }


  private void startForging() {
    // Get centre object and freeze
    mainConstruct = constructController.controlledConstruct;
    mainWJ = mainConstruct.mainWJ;
    mainConstruct.setKinematic(true);

    // Find closest position below centre
    int firstHit = -1;
    RaycastHit[] hits = Physics.RaycastAll(mainWJ.transform.position, Vector3.down, 100.0f);
    for (int i = 0; i < hits.Length; i++) {
      if (hits[i].transform != mainWJ.transform && (firstHit == -1 || hits[i].distance < hits[firstHit].distance)) firstHit = i;
    }

    // Set centre target position if found ground
    if (firstHit != -1) {
      centreTargetPos = hits[firstHit].point + Vector3.up * mainWJ.getMaxExtent() * 5.0f;
    }

    // Set target rotation
    centreTargetRot = Quaternion.Euler(0.0f, mainWJ.transform.rotation.eulerAngles.y + 180, 0.0f);
  }


  private void stopForging() {
    // Unfreeze centre object
    mainConstruct.setKinematic(false);
  }


  private void Update() {
    // Lerp mainConstruct to position and rotation
    mainWJ.transform.position = Vector3.Lerp(mainWJ.transform.position, centreTargetPos, 4.0f * Time.deltaTime);
    mainWJ.transform.rotation = Quaternion.Lerp(mainWJ.transform.rotation, centreTargetRot, 4.0f * Time.deltaTime);
  }


  public void setActive(bool active) {
    // Update enabled
    this.enabled = active;

    // Start / stop forging
    if (this.enabled)
      startForging();
    else stopForging();

    // Update camera
    pcam.setActive(active);
  }
}
