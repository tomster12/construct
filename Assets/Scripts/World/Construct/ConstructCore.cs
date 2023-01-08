
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum CoreState { Detached, Attaching, Attached, Detaching };


public class ConstructCore : ConstructObject
{
    // Declare references, config, variables
    [SerializeField] protected ConstructCoreMovement inherentCoreMovement => (ConstructCoreMovement)inherentMovement;
    public CoreState state { get; private set; } = CoreState.Detached;
    public ConstructObject attachedCO { get; private set; }
    public bool isTransitioning => state == CoreState.Attaching || state == CoreState.Detaching;
    public bool isAttached => state == CoreState.Attached;
    public bool isDetached => state == CoreState.Detached;
    public bool isBlocking => isTransitioning;
    public bool canTransition => isConstructed && !isBlocking && !inherentCoreMovement.isBlocking;
    public bool canAttach(ConstructObject checkCO) => canTransition && isDetached && inherentCoreMovement.isActive && (checkCO != null && !checkCO.isConstructed);
    public bool canDetach => canTransition && isAttached;

    protected override void Awake()
    {
        // Assign core to movement
        if (inherentCoreMovement != null) inherentCoreMovement.SetCore(this);
    }


    public void Attach(ConstructObject targetCO) => StartCoroutine(IE_Attach(targetCO));

    private IEnumerator IE_Attach(ConstructObject targetCO)
    {
        if (!canAttach(targetCO)) yield break;

        // Tell movement to attach and update state
        state = CoreState.Attaching;
        yield return inherentCoreMovement.IE_Attach(targetCO);
        state = CoreState.Attached;

        // Attach core to object and update physics
        inherentCoreMovement.SetActive(false);
        SetLoose(false);
        SetFloating(true);
        SetColliding(false);
        attachedCO = targetCO;
        transform.parent = attachedCO.transform;

        // Add object to construct and set movement
        attachedCO.SetConstruct(construct);
        attachedCO.transform.parent = construct.transform;
        construct.OverwriteMovement(attachedCO.inherentMovement);
    }


    public void Detach() => StartCoroutine(IE_Detach());

    private IEnumerator IE_Detach()
    {
        if (!canDetach) yield break;

        // Tell movement to detach and update state
        state = CoreState.Detaching;
        yield return inherentCoreMovement.IE_Detach();
        state = CoreState.Detached;

        // Remove object from construct and parent to world
        construct.movement?.SetActive(false);
        attachedCO.SetConstruct(null);
        attachedCO.transform.parent = construct.transform.parent;

        // Add core back to construct and overwrite movement
        transform.parent = construct.transform;
        construct.OverwriteMovement(inherentCoreMovement);
        attachedCO = null;

    }


    public override void SetConstruct(Construct construct_)
    {
        base.SetConstruct(construct_);

        // As a core overwrite movement
        construct.OverwriteMovement(inherentMovement);
    }


    #region Helper

    public override bool GetContainsWO(WorldObject checkWO) => state == CoreState.Attached ? attachedCO.GetContainsWO(checkWO) : baseWO == checkWO;

    public override bool GetContainsCO(ConstructObject checkCO) => state == CoreState.Attached ? attachedCO.GetContainsCO(checkCO) : checkCO == this;

    public override ConstructObject GetCentreCO() => state == CoreState.Attached ? attachedCO.GetCentreCO() : this;

    #endregion
}
