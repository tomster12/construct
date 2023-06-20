
using System.Collections;


public enum CoreState { Detached, Attaching, Attached, Detaching };


public class ConstructCore : ConstructObject
{
    protected ConstructCoreMovement inherentCoreMovement => (ConstructCoreMovement) inherentMovement;
    public ShapeCoreAttachment shapeCoreAttachment => inherentCoreMovement.shapeCoreAttachment;
    public CoreState state { get; private set; } = CoreState.Detached;
    public ConstructObject attachedCO { get; private set; }
    protected override int movementPriority => 0;

    public bool canTransition => isConstructed && !construct.isBlocking && !inherentCoreMovement.isBlocking;
    public bool canAttach(ConstructObject checkCO) => canTransition && isDetached && inherentCoreMovement.isActive && (checkCO != null && !checkCO.isConstructed);
    public bool canDetach => canTransition && isAttached;
    public bool isAttached => state == CoreState.Attached;
    public bool isDetached => state == CoreState.Detached;
    public bool isTransitioning => state == CoreState.Attaching || state == CoreState.Detaching;
    public bool isBlocking => isTransitioning;


    public void Attach(ConstructObject targetCO) => StartCoroutine(IE_Attach(targetCO));

    private IEnumerator IE_Attach(ConstructObject targetCO)
    {
        if (!canAttach(targetCO)) yield break;

        // Tell movement to attach and update state
        state = CoreState.Attaching;
        yield return inherentCoreMovement.IE_Attach(targetCO);
        state = CoreState.Attached;

        // Add object to construct and set movement
        attachedCO = targetCO;
        attachedCO.transform.parent = construct.transform;
        attachedCO.OnJoinConstruct(construct);
    }


    public void Detach() => StartCoroutine(IE_Detach());

    private IEnumerator IE_Detach()
    {
        if (!canDetach) yield break;

        // Remove object from construct and parent to world
        attachedCO.transform.parent = construct.transform.parent;
        attachedCO.OnExitConstruct();
        attachedCO = null;

        // Tell movement to detach and update state
        state = CoreState.Detaching;
        yield return inherentCoreMovement.IE_Detach();
        state = CoreState.Detached;
    }


    #region Getters

    public override bool GetContainsWO(WorldObject checkWO) => state == CoreState.Attached ? attachedCO.GetContainsWO(checkWO) : baseWO == checkWO;

    public override bool GetContainsCO(ConstructObject checkCO) => state == CoreState.Attached ? attachedCO.GetContainsCO(checkCO) : checkCO == this;

    public override ConstructObject GetCentreCO() => state == CoreState.Attached ? attachedCO.GetCentreCO() : this;

    #endregion
}
