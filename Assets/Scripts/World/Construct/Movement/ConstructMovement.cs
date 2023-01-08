
using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

 
public interface IMovable
{
    void MoveInDirection(Vector3 dir);
    void AimAtPosition(Vector3 pos);
}


public abstract class ConstructMovement : MonoBehaviour
{
    protected ConstructObject controlledCO;
    public bool isConstructed => controlledCO != null;
    public bool isActive { get; private set; } = false;
    public bool isPaused { get; private set; } = false;
    public bool isTransitioning = false;
    public virtual bool isBlocking => isTransitioning;


    public abstract void MoveInDirection(Vector3 dir);

    public abstract void AimAtPosition(Vector3 pos);


    public virtual void SetObject(ConstructObject controlledCO_)
    {
        if (isConstructed || isActive || isTransitioning) throw new System.Exception("Cannot SetObject() if isConstructed or isActive or isTransitioning");
        controlledCO = controlledCO_;
    }

    public virtual void SetActive(bool isActive_)
    {
        if (!isConstructed || isTransitioning) throw new System.Exception("Cannot SetActive() if !isConstructed or isTransitioning");
        isActive = isActive_;
    }

    public virtual void SetPaused(bool isPaused_)
    {
        if (!isConstructed || !isActive || isTransitioning) throw new System.Exception("Cannot SetPaused() if !isConstructed or !isActive or isTransitioning");
        isPaused = isPaused_;
    }

    protected virtual void SetTransitioning(bool isTransitioning_)
    {
        if (!isConstructed || isPaused) throw new System.Exception("Cannot SetTransitioning() if !isConstucted || isPaused");
        isTransitioning = isTransitioning_;
    }
}


public abstract class ConstructCoreMovement : ConstructMovement
{
    protected ConstructCore controlledCC;


    public abstract IEnumerator IE_Attach(ConstructObject targetCO);

    public abstract IEnumerator IE_Detach();


    public virtual void SetCore(ConstructCore controlledCC_)
    {
        if (isConstructed || isActive || isTransitioning) throw new System.Exception("Cannot SetCore() if isConstructed or isActive or isBusy");
        controlledCC = controlledCC_; SetObject(controlledCC);
    }
}
