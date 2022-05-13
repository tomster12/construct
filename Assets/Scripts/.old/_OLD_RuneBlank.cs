
using UnityEngine;


public class RuneBlank : Rune
{
    // Declare variables
    private StatList slottedStats;
    private int speedId = -1;


    public override void slot(RuneHandler handler_, Transform slot_)
    {
        base.slot(handler_, slot_);

        // Add speedup effect
        MovementI movementI = slottedHandler.GetComponent<MovementI>();
        if (movementI != null)
        {
            slottedStats = movementI.getStats();
            speedId = slottedStats.AddAffector("MovementStrength", 2.0f, true);
        }
    }


    public override void unslot()
    {
        base.unslot();

        // Remove speedup effect
        slottedStats.RemoveAffector("MovementStrength", speedId);
        slottedStats = null;
        speedId = -1;
    }
}
