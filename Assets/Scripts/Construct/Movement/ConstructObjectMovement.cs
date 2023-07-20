
using System.Collections;
using UnityEngine;


public abstract class ConstructObjectMovement : MonoBehaviour, IObjectController
{
    public bool isAssigned { get; private set; } = false;
    public bool isActive { get; private set; } = false;
    public bool isPaused { get; private set; } = false;
    public bool isSubscribed => subscribedConstruct != null;
    public virtual bool canActivate { get; private set; } = true;
    public virtual bool isBlocking => false;

    protected Construct subscribedConstruct;


    public abstract void MoveInDirection(Vector3 dir);

    public abstract void AimAtPosition(Vector3 pos);


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


    #region IObjectController
        
    public ObjectControllerType GetControllerType() => ObjectControllerType.MOVEMENT;

    #endregion
}
