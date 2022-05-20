
using System.Collections;
using UnityEngine;


public interface ICCMovement : ICOMovement
{
    void AttachCore(ConstructObject targetCO, Vector3 targetPos);
    IEnumerator AttachCoreIE(ConstructObject targetCO, Vector3 targetPos);

    void DetachCore();
    IEnumerator DetachCoreIE();

    void SetCC(ConstructCore baseCO_);
}
