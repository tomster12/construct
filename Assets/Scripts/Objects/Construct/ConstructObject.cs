
using UnityEngine;


public class ConstructObject : MonoBehaviour, IMovable
{
    // Declare references, variables
    [Header("References")]
    [SerializeField] protected WorldObject _baseWO;
    [SerializeField] protected RuneHandler _runeHandler;
    public WorldObject baseWO => _baseWO;
    public RuneHandler runeHandler => _runeHandler;
    private ICOMovement movement;

    public Construct construct { get; private set; }


    protected virtual void Awake()
    {
        // Initialize references
        SetCOMovement(GetComponent<ICOMovement>());
    }


    public virtual void MoveInDirection(Vector3 dir)
    {
        // Only move if on construct and has movement
        if (construct != null && movement != null) movement.MoveInDirection(dir);
    }

    public virtual void AimAtPosition(Vector3 pos)
    {
        // Only aim if on construct and has movement
        if (construct != null && movement != null) movement.AimAtPosition(pos);
    }

    
    public bool GetControlled() => movement != null && movement.GetControlled();

    public virtual bool GetContainsWO(WorldObject checkWO) => baseWO == checkWO;

    public virtual bool GetContainsCO(ConstructObject checkCO) => checkCO == this;

    public virtual ConstructObject GetCentreCO() => this;


    public virtual void SetConstruct(Construct construct_)
    {
        // Update colliders / rigidbody and set construct
        if (construct_ == null) SetControlled(false);
        construct = construct_;
    }

    public virtual void SetControlled(bool isControlled_)
    {
        // Only allow controllable if has movement and construct
        if (movement == null || (isControlled_ && construct == null)) return;
        movement.SetControlled(isControlled_);
    }

    public virtual void SetForging(bool isForging_)
    {
        // Update movement and rb values
        SetLoose(false);
        SetFloating(true);
        movement.SetForging(isForging_);
        if (!isForging_) SetControlled(movement.GetControlled());
    }

    protected void SetCOMovement(ICOMovement movement_) { movement = movement_; movement_.SetCO(this); }

    public virtual void SetLoose(bool isLoose) => baseWO.rb.isKinematic = !isLoose;
 
    public virtual void SetFloating(bool isFloating) => baseWO.rb.useGravity = !isFloating;
}
