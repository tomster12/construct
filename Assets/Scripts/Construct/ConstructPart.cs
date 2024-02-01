
using System.Collections.Generic;
using UnityEngine;


public class ConstructPart : MonoBehaviour, IConstructPart
{
    [Header("References")]
    [SerializeField] protected Object baseObject;
    [SerializeField] protected ConstructPartData partData;
    [SerializeField] protected ConstructPartMovement inherentMovement;
    [SerializeField] protected RuneHandler runeHandler;

    protected IConstruct IConstruct;
    private IObjectController IController;
    private HashSet<ConstructShape> parentShapes = new HashSet<ConstructShape>();
    private InspectableLabel inspectableLabel;
    private GetRuneSkill getRuneSkill;
    protected virtual int movementPriority => 1;

    public bool IsConstructed() => IConstruct != null;
    public bool IsControlled() => IController != null;
    public virtual bool IsBlocking() => false;
    public virtual bool IsAttachable() => true;


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

    protected virtual void GenerateInspectableLabel()
    {
        // Instantiate data label
        if (partData.inspectableLabelPrefab == null) return;
        GameObject inspectableLabelGO = Instantiate(partData.inspectableLabelPrefab);
        inspectableLabel = inspectableLabelGO.GetComponent<InspectableLabel>();
        inspectableLabel.SetObject(this, baseObject.maxExtent);
    }


    public virtual bool ContainsObject(Object checkObject) => baseObject == checkObject;

    public virtual bool ContainsIPart(IConstructPart checkIPart) => checkIPart == (IConstructPart)this;

    public virtual InspectedData Inspect() => partData;

    public virtual List<string> GetAttributes() => new List<string>()
    {
        "Health: " + partData.health,
        "Energy: " + partData.energy + " (" + partData.energyRegen + "/s)",
        "Slots: " + partData.slotCount
    };

    public virtual List<string> GetModifiers() => new List<string>()
    {
        "Rapid (+10% Speed)",
        "Energetic (+15% e. regen)"
    };

    public ObjectType GetObjectType() => IsConstructed() ? ObjectType.PartNSTRUCTED : ObjectType.LOOSE;
 
    public virtual IConstructPart GetCentreIPart() => (IConstructPart)this;

    public IConstruct GetIConstruct() => IConstruct;
 
    public Vector3 GetPosition() => baseObject.transform.position;

    public Object GetObject() => baseObject;

    public IObjectController GetIController() => IController;

    public bool GetIsNearby() => inspectableLabel.isNearby;

    public bool GetIsHighlighted() => baseObject.isHighlighted;

    public Transform GetTransform() => baseObject.transform;

    public virtual Quaternion GetForwardRot() => Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up);

    public void SetIsNearby(bool isNearby) { inspectableLabel?.SetIsNearby(isNearby); }

    public void SetIsHighlighted(bool isHighlighted) { inspectableLabel?.SetIsHighlighted(isHighlighted); baseObject.isHighlighted = isHighlighted; }

    public bool SetControlledBy(IObjectController controlledBy_)
    {
        if (IController == controlledBy_) return true;
        if (IController != null && controlledBy_ != null) throw new System.Exception("Cannot SetControlledBy() when already controlled.");
        IController = controlledBy_;
        return false;
    }

    public void OnJoinShape(ConstructShape shape) => parentShapes.Add(shape);

    public void OnExitShape(ConstructShape shape) => parentShapes.Remove(shape);

    public virtual void OnJoinConstruct(Construct IConstruct_)
    {
        // Notify everything about joining IConstruct
        IConstruct = IConstruct_;
        runeHandler?.OnJoinConstruct(IConstruct_);
        if (getRuneSkill != null)
            IConstruct.SubscribeSkill(getRuneSkill, "_0");
        IConstruct.SubscribeMovement(inherentMovement, movementPriority);
        IConstruct.SubscribeOnStateChanged(OnConstructStateChanged);
    }

    public virtual void OnExitConstruct()
    {
        // Notify everything about exiting IConstruct
        IConstruct.SubscribeOnStateChanged(OnConstructStateChanged);
        IConstruct.UnsubscribeMovement(inherentMovement);
        if (getRuneSkill != null) IConstruct.SubscribeSkill(getRuneSkill);
        runeHandler?.OnExitConstruct();
        IConstruct = null;
    }

    public virtual void OnConstructStateChanged(ConstructState state) { }    
}
