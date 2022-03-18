
using System.Collections;
using UnityEngine;


public class RuneBlank : Rune {

  // Declare variables
  private StatList slottedStats;
  public int speedID = -1;


  public override void slot(RuneHandler ag_, Transform slot_) {
    base.slot(ag_, slot_);

    // Add effect to MovementI
    MovementI MovementI = ag_.GetComponent<MovementI>();
    if (MovementI != null) {
      slottedStats = MovementI.getStats();
      speedID = slottedStats.addAffector("MovementStrength", 2.0f, true);
    }
  }


  public override void unslot() {
    base.unslot();

    // Remove effect from MovementI
    slottedStats.removeAffector("MovementStrength", speedID);
    slottedStats = null;
    speedID = -1;
  }
}
