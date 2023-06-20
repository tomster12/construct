
using System.Collections;
using UnityEngine;


public abstract class ConstructObjectMovement : MonoBehaviour, IObjectController
{
    protected Construct subscribedConstruct;

    public bool isAssigned { get; private set; } = false;
    public bool isActive { get; private set; } = false;
    public bool isPaused { get; private set; } = false;
    public bool isSubscribed => subscribedConstruct != null;
    public virtual bool canActivate { get; private set; } = true;
    public virtual bool isBlocking => false;


    public abstract void MoveInDirection(Vector3 dir);

    public abstract void AimAtPosition(Vector3 pos);


    public ObjectControllerType GetControllerType() => ObjectControllerType.MOVEMENT;

    
    public virtual bool SetActive(bool isActive_)
    {
        if ((isActive_ && (!isAssigned || !canActivate)) || isBlocking) throw new System.Exception("Cannot SetActive() if  isBlocking, nor to active !isAssigned or !canActivate");
        if (isActive == isActive_) return false;
        isActive = isActive_;
        if (!isActive) isPaused = false;
        return true;
    }

    public virtual bool SetPaused(bool isPaused_)
    {
        if (!isAssigned || !isActive || isBlocking) throw new System.Exception("Cannot SetPaused() if !isAssigned or !isActive or isBlocking");
        if (isPaused == isPaused_) return false;
        isPaused = isPaused_;
        return true;
    }

    public virtual bool SetCanActivate(bool canActivate_)
    {
        canActivate = canActivate_;
        if (subscribedConstruct != null && canActivate) subscribedConstruct.PickBestMovement();
        return true;
    }


    public virtual void OnJoinConstruct(Construct subscribedConstruct_)
    {
        if (isAssigned) throw new System.Exception("Cannot OnJoinConstruct() if isAssigned");
        subscribedConstruct = subscribedConstruct_;
    }

    public virtual void OnExitConstruct()
    {
        if (!isAssigned) throw new System.Exception("Cannot OnExitConstruct() if !isAssigned");
        subscribedConstruct = null;
    }


    public virtual void OnAssign()
    {
        if (isAssigned) throw new System.Exception("Cannot OnAssign() if isAssigned");
        isAssigned = true;
    }

    public virtual void OnUnassign()
    {
        if (!isAssigned) throw new System.Exception("Cannot OnUnassign() if !isAssigned");
        isAssigned = false;
        SetActive(false);
    }
}


public abstract class ConstructCoreMovement : ConstructObjectMovement
{
    [Header("Core References")]
    [SerializeField] protected ConstructCore controlledCC;

    public ShapeCoreAttachment shapeCoreAttachment { get; protected set; }
    public bool isTransitioning { get; private set; } = false;
    public override bool isBlocking => isTransitioning;


    protected virtual void SetTransitioning(bool isTransitioning_)
    {
        if (isPaused) throw new System.Exception("Cannot SetTransitioning() if isPaused");
        isTransitioning = isTransitioning_;
        if (isTransitioning) controlledCC.SetControlledBy(this);
        else if (isActive) controlledCC.SetControlledBy(this);
        else controlledCC.SetControlledBy(null);
    }


    public IEnumerator IE_Attach(ConstructObject targetCO)
    {
        if (isBlocking) yield break;

        // Update state and run main attach bits
        SetTransitioning(true);
        yield return StartCoroutine(_IE_RunAttach(targetCO));
        SetTransitioning(false);
        SetCanActivate(false);
        SetActive(false);
        shapeCoreAttachment.SetActive(true);
    }

    public IEnumerator IE_Detach()
    {
        if (isBlocking) yield break;

        // Update state and run main detach bits
        shapeCoreAttachment.SetActive(false);
        SetTransitioning(true);
        yield return StartCoroutine(_IE_RunDetach());
        SetTransitioning(false);
        SetCanActivate(true);
        shapeCoreAttachment.Clear();
    }


    protected abstract IEnumerator _IE_RunAttach(ConstructObject targetCO);

    protected abstract IEnumerator _IE_RunDetach();
}
