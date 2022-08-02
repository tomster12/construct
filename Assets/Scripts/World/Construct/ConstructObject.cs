
using System.Collections.Generic;
using UnityEngine;


public class ConstructObject : MonoBehaviour, IMovable, IHoverable, IInspectable
{
    // Declare references, variables
    [Header("Prefabs")]
    [SerializeField] private GameObject inspectableLabelPrefab;

    [Header("References")]
    [SerializeField] protected Sprite inspectableIcon;
    [SerializeField] protected WorldObject _baseWO;
    [SerializeField] protected RuneHandler _runeHandler;
    [SerializeField] protected ObjectData _objectData;
    public WorldObject baseWO => _baseWO;
    public RuneHandler runeHandler => _runeHandler;
    public ObjectData objectData => _objectData;
    private ICOMovement movement;

    public Construct construct { get; private set; }
    public InspectableLabel inspectableLabel { get; private set; }


    protected virtual void Awake()
    {
        // Initialize references
        SetCOMovement(GetComponent<ICOMovement>());
    }


    protected virtual void Start()
    {
        // Initialize label
        GenerateDataLabel();
    }


    protected virtual void GenerateDataLabel()
    {
        // Instantiate data label
        if (inspectableLabelPrefab == null) return;
        GameObject inspectableLabelGO = Instantiate(inspectableLabelPrefab);
        inspectableLabel = inspectableLabelGO.GetComponent<InspectableLabel>();
        inspectableLabel.SetObject(this, baseWO.GetMaxExtent());
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

    public virtual bool GetCanForge() => movement.GetCanForge();

    public virtual ConstructObject GetCentreCO() => this;

    public virtual Vector3 GetCentrePosition() => GetCentreCO().transform.position;

    public virtual Quaternion GetForwardRot() => Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up);


    public Vector3 GetHoverablePosition() => GetCentrePosition();

    public bool GetHoverableHighlighted() => baseWO.isHighlighted;

    public IHoverableState GetHoverableState() => (construct != null || GetControlled()) ? IHoverableState.CONSTRUCTED : IHoverableState.LOOSE;


    public Sprite GetInspectableIconSprite() => inspectableIcon;

    public string GetInspectableName() => objectData.name;
    
    public string GetInspectableDescription() => objectData.description;

    public Element GetInspectableElement() => objectData.element;

    public virtual List<string> GetInspectableAttributes() => new List<string>()
    {
        "Health: " + objectData.health,
        "Energy: " + objectData.energy + " (" + objectData.energyRegen + "/s)",
        "Slots: " + objectData.slotCount
    };

    public virtual List<string> GetInspectableModifiers() => new List<string>() { "Rapid (+10% Speed)", "Energetic (+15% e. regen)" };

    public Vector3 GetInspectablePosition() => GetCentrePosition();

    public float GetInspectableMass() => baseWO.rb.mass;


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


    public void SetHoverableNearby(bool isNearby) { if (inspectableLabel != null) inspectableLabel.SetNearby(isNearby); }

    public void SetHoverableHighlighted(bool isHighlighted) { if (inspectableLabel != null) inspectableLabel.SetHighlighted(isHighlighted); baseWO.isHighlighted = isHighlighted; }
}
