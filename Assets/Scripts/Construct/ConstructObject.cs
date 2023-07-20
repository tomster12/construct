
using System.Collections.Generic;
using UnityEngine;


public class ConstructObject : MonoBehaviour, IHighlightable, IInspectable
{
    [Header("References")]
    [SerializeField] protected ConstructObjectMovement _inherentMovement;
    [SerializeField] protected WorldObject _baseWO;
    [SerializeField] protected RuneHandler _runeHandler;
    [SerializeField] protected ObjectData _objectData;

    public ConstructObjectMovement inherentMovement => _inherentMovement;
    public WorldObject baseWO => _baseWO;
    public RuneHandler runeHandler => _runeHandler;
    public ObjectData objectData => _objectData;    
    public Construct construct { get; private set; }
    public IObjectController controlledBy { get; private set; }
    public bool isConstructed => construct != null;
    public bool isControlled => controlledBy != null;

    protected virtual int movementPriority => 1;
    private GetRuneSkill getRuneSkill;
    private HashSet<ConstructShape> shapesIncludedIn = new HashSet<ConstructShape>();


    protected virtual void Start()
    {
        // Initialize label
        GenerateInspectableLabel();

        // Create get rune skill
        if (runeHandler != null)
        {
            getRuneSkill = gameObject.AddComponent<GetRuneSkill>();
            getRuneSkill.Init(runeHandler, PlayerController.instance);
        }
    }


    public virtual bool GetContainsWO(WorldObject checkWO) => baseWO == checkWO;

    public virtual bool GetContainsCO(ConstructObject checkCO) => checkCO == this;

    public virtual ConstructObject GetCentreCO() => this;

    public virtual Vector3 GetCentrePosition() => GetCentreCO().transform.position;

    public virtual Quaternion GetForwardRot() => Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up);


    public void SetControlledBy(IObjectController controlledBy_)
    {
        if (controlledBy == controlledBy_) return;
        if (controlledBy != null && controlledBy_ != null) throw new System.Exception("Cannot SetControlledBy() when already controlled.");
        controlledBy = controlledBy_;
    }
    

    public virtual void OnJoinConstruct(Construct construct_)
    {
        // Notify everything about joining construct
        construct = construct_;
        construct.OnObjectJoined(this);
        runeHandler?.OnJoinConstruct(construct_);
        if (getRuneSkill != null) construct.skills.RequestBinding(getRuneSkill, "_0");
        construct.SubscribeMovement(inherentMovement, movementPriority);
    }

    public virtual void OnExitConstruct()
    {
        // Notify everything about exiting construct
        construct.UnsubscribeMovement(inherentMovement);
        if (getRuneSkill != null) construct.skills.Unbind(getRuneSkill);
        runeHandler?.OnExitConstruct();
        construct.OnObjectExit(this);
        construct = null;
    }

    public void OnJoinShape(ConstructShape shape) => shapesIncludedIn.Add(shape);

    public void OnLeaveShape(ConstructShape shape) => shapesIncludedIn.Remove(shape);


    #region IHoverable

    public Vector3 IHGetPosition() => GetCentrePosition();

    public bool IHGetHighlighted() => baseWO.isHighlighted;

    public ObjectType IHGetObjectType() => isConstructed ? ObjectType.CONSTRUCTED_CO : ObjectType.LOOSE_CO;

    public void IHSetNearby(bool isNearby) { if (inspectableLabel != null) inspectableLabel.SetNearby(isNearby); }

    public void IHSetHighlighted(bool isHighlighted) { if (inspectableLabel != null) inspectableLabel.SetHighlighted(isHighlighted); baseWO.isHighlighted = isHighlighted; }

    #endregion


    #region IInspectable

    private InspectableLabel inspectableLabel;


    protected virtual void GenerateInspectableLabel()
    {
        // Instantiate data label
        if (objectData.inspectableLabelPrefab == null) return;
        GameObject inspectableLabelGO = Instantiate(objectData.inspectableLabelPrefab);
        inspectableLabel = inspectableLabelGO.GetComponent<InspectableLabel>();
        inspectableLabel.SetObject(this, baseWO.maxExtent);
    }

    public virtual Sprite IIGetIconSprite() => objectData.inspectableIcon;

    public virtual string IIGetName() => objectData.name;

    public virtual string IIGetDescription() => objectData.description;

    public virtual Element IIGetElement() => objectData.element;

    public virtual List<string> IIGetAttributes() => new List<string>()
    {
        "Health: " + objectData.health,
        "Energy: " + objectData.energy + " (" + objectData.energyRegen + "/s)",
        "Slots: " + objectData.slotCount
    };

    public virtual List<string> IIGetModifiers() => new List<string>()
    {
        "Rapid (+10% Speed)",
        "Energetic (+15% e. regen)"
    };

    public virtual Vector3 IIGetPosition() => GetCentrePosition();

    public virtual float IIGetMass() => baseWO.rb.mass;

    #endregion
}
