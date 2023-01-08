
using System.Collections.Generic;
using UnityEngine;


public class ConstructObject : MonoBehaviour, IHighlightable, IInspectable
{
    // Declare references, variables
    [Header("Prefabs")]
    [SerializeField] protected ConstructMovement _inherentMovement;
    [SerializeField] private GameObject inspectableLabelPrefab;
    public ConstructMovement inherentMovement => _inherentMovement;

    [Header("References")]
    [SerializeField] protected Sprite inspectableIcon;
    [SerializeField] protected WorldObject _baseWO;
    [SerializeField] protected RuneHandler _runeHandler;
    [SerializeField] protected ObjectData _objectData;
    public WorldObject baseWO => _baseWO;
    public RuneHandler runeHandler => _runeHandler;
    public ObjectData objectData => _objectData;
    public bool isConstructed => construct != null;

    public Construct construct { get; private set; }
    public InspectableLabel inspectableLabel { get; private set; }


    protected virtual void Awake()
    {
        // Assign object to movement
        if (inherentMovement != null) inherentMovement.SetObject(this);
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


    public virtual void SetConstruct(Construct construct_) => construct = construct_;

    public virtual void SetLoose(bool isLoose) => baseWO.rb.isKinematic = !isLoose;

    public virtual void SetFloating(bool isFloating) => baseWO.rb.useGravity = !isFloating;

    public virtual void SetColliding(bool toCollide) => baseWO.cl.enabled = toCollide;


    #region Helper

    public virtual bool GetContainsWO(WorldObject checkWO) => baseWO == checkWO;

    public virtual bool GetContainsCO(ConstructObject checkCO) => checkCO == this;

    public virtual ConstructObject GetCentreCO() => this;

    public virtual Vector3 GetCentrePosition() => GetCentreCO().transform.position;

    public virtual Quaternion GetForwardRot() => Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up);

    #endregion


    #region IHoverable

    public Vector3 GetIHPosition() => GetCentrePosition();

    public bool GetIHHovered() => baseWO.isHighlighted;

    public IHighlightableState GetIHState() => construct != null ? IHighlightableState.CONSTRUCTED : IHighlightableState.LOOSE;


    public void SetIHNearby(bool isNearby) { if (inspectableLabel != null) inspectableLabel.SetNearby(isNearby); }

    public void SetIHHighlighted(bool isHighlighted) { if (inspectableLabel != null) inspectableLabel.SetHighlighted(isHighlighted); baseWO.isHighlighted = isHighlighted; }

    #endregion


    #region IInspectable

    public Sprite GetIIIconSprite() => inspectableIcon;

    public string GetIIName() => objectData.name;

    public string GetIIDescription() => objectData.description;

    public Element GetIIElement() => objectData.element;

    public virtual List<string> GetIIAttributes() => new List<string>()
    {
        "Health: " + objectData.health,
        "Energy: " + objectData.energy + " (" + objectData.energyRegen + "/s)",
        "Slots: " + objectData.slotCount
    };

    public virtual List<string> GetIIModifiers() => new List<string>() { "Rapid (+10% Speed)", "Energetic (+15% e. regen)" };

    public Vector3 GetIIPosition() => GetCentrePosition();

    public float GetIIMass() => baseWO.rb.mass;

    #endregion
}
