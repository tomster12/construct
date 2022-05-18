
using System.Collections;
using UnityEngine;


public interface ICCMovement : ICOMovement
{
    void AttachCore(ICCMovementController stateController, ConstructObject targetCO, Vector3 targetPos);
    IEnumerator AttachCoreIE(ICCMovementController stateController, ConstructObject targetCO, Vector3 targetPos);

    void DetachCore(ICCMovementController stateController);
    IEnumerator DetachCoreIE(ICCMovementController stateController);
}
