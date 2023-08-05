
using System.Collections;
using System.Collections.Generic;


public class ConstructCore : ConstructObject
{
    public CoreData coreData => (CoreData)_objectData;
    public ConstructCoreMovement inherentCoreMovement => (ConstructCoreMovement)inherentMovement;
    public ShapeCoreAttachment shapeCoreAttachment => inherentCoreMovement.shapeCoreAttachment;
    public CoreState state { get; private set; } = CoreState.DETACHED;
    public ConstructObject attachedCO { get; private set; }

    protected override int movementPriority => 0;
    public bool canTransition => isConstructed && !construct.isBlocking && !inherentCoreMovement.isBlocking;
    public bool canDetach => canTransition && isAttached;
    public bool isAttached => state == CoreState.ATTACHED;
    public bool isDetached => state == CoreState.DETACHED;
    public bool isTransitioning => state == CoreState.ATTACHING || state == CoreState.DETACHING;
    public bool isBlocking => isTransitioning;
    public override bool isAttachable => false;


    public void Attach(ConstructObject targetCO) => StartCoroutine(IE_Attach(targetCO));

    public void Detach() => StartCoroutine(IE_Detach());

    private IEnumerator IE_Attach(ConstructObject targetCO)
    {
        if (!GetCanAttach(targetCO)) yield break;

        // Tell movement to attach and update state
        state = CoreState.ATTACHING;
        yield return inherentCoreMovement.IE_Attach(targetCO);
        state = CoreState.ATTACHED;

        // Add object to construct and set movement
        attachedCO = targetCO;
        attachedCO.transform.parent = construct.transform;
        construct.AddObject(attachedCO);
    }

    private IEnumerator IE_Detach()
    {
        if (!canDetach) yield break;

        // Remove object from construct and parent to world
        attachedCO.transform.parent = construct.transform.parent;
        construct.RemoveObject(attachedCO);
        attachedCO = null;

        // Tell movement to detach and update state
        state = CoreState.DETACHING;
        yield return inherentCoreMovement.IE_Detach();
        state = CoreState.DETACHED;
    }


    public override bool GetContainsWO(WorldObject checkWO) => state == CoreState.ATTACHED ? attachedCO.GetContainsWO(checkWO) : baseWO == checkWO;

    public override bool GetContainsCO(ConstructObject checkCO) => state == CoreState.ATTACHED ? attachedCO.GetContainsCO(checkCO) : checkCO == this;

    public override ConstructObject GetCentreCO() => state == CoreState.ATTACHED ? attachedCO.GetCentreCO() : this;

    public bool GetCanAttach(ConstructObject checkCO)
    {
        return checkCO != null && !checkCO.isConstructed
            && canTransition && isDetached
            && inherentCoreMovement.GetCanAttach(checkCO);
    }


    #region IInspectable override

    public override List<string> IIGetAttributes() => new List<string>()
    {
        "Health: " + coreData.health,
        "Energy: " + coreData.energy + " (" + coreData.energyRegen + "/s)",
        "Max Construct Size: " + coreData.maxConstructSize
    };
    
    public override List<string> IIGetModifiers() => new List<string>();

    public override float IIGetMass() => 0.0f;

    #endregion
}
