
using UnityEngine;


public class ConstructShape : MonoBehaviour, IObjectController
{
    public bool isActive { get; private set; }
    public bool isPaused { get; private set; }
    public virtual bool canActivate => true;


    public virtual bool SetActive(bool isActive_)
    {
        if (isActive == isActive_) return false;
        if (!canActivate && isActive_) return false;
        isActive = isActive_;
        return true;
    }

    public virtual bool SetPaused(bool isPaused_)
    {
        isPaused = isPaused_;
        return true;
    }


    public ObjectControllerType GetIControllerType() => ObjectControllerType.SHAPE;
}
