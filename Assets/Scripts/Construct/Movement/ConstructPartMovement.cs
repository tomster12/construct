
using System.Collections;
using UnityEngine;


public abstract class ConstructPartMovement : MonoBehaviour, IObjectController
{
    public bool IsSubscribed() => subscribedIConstruct != null;
    public virtual bool IsBlocking() => false;
    
    public bool isAssigned { get; private set; } = false;
    public bool isActive { get; private set; } = false;
    public bool isPaused { get; private set; } = false;
    public virtual bool canActivate { get; private set; } = true;
    protected IConstruct subscribedIConstruct;


    public abstract void MoveInDirection(Vector3 dir);

    public abstract void AimAtPosition(Vector3 pos);


    public virtual void OnJoinConstruct(IConstruct construct)
    {
        if (isAssigned) throw new System.Exception("Cannot OnJoinConstruct() if isAssigned");
        subscribedIConstruct = construct;
    }

    public virtual void OnExitConstruct()
    {
        if (!isAssigned) throw new System.Exception("Cannot OnExitConstruct() if !isAssigned");
        subscribedIConstruct = null;
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


    public ObjectControllerType GetIControllerType() => ObjectControllerType.MOVEMENT;

    public virtual bool SetActive(bool isActive_)
    {
        if ((isActive_ && (!isAssigned || !canActivate)) || IsBlocking()) throw new System.Exception("Cannot SetActive() if  isBlocking, nor to active !isAssigned or !canActivate");
        if (isActive == isActive_) return false;
        isActive = isActive_;
        if (!isActive) isPaused = false;
        return true;
    }

    public virtual bool SetPaused(bool isPaused_)
    {
        if (!isAssigned || !isActive || IsBlocking()) throw new System.Exception("Cannot SetPaused() if !isAssigned or !isActive or isBlocking");
        if (isPaused == isPaused_) return false;
        isPaused = isPaused_;
        return true;
    }

    public virtual bool SetCanActivate(bool canActivate_)
    {
        canActivate = canActivate_;
        if (subscribedIConstruct != null && canActivate) subscribedIConstruct.OnMovementUpdate();
        return true;
    }
}
