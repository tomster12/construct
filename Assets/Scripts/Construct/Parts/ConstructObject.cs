
using System.Collections.Generic;
using UnityEngine;


public enum ObjectControllerType { SHAPE, MOVEMENT }

public interface IObjectController
{
    public ObjectControllerType GetControllerType();
}


public class GetRuneSkill : Skill
{
    RuneHandler runeHandler;
    PlayerController playerController;


    public void Init(RuneHandler runeHandler_, PlayerController playerController_)
    {
        runeHandler = runeHandler_;
        playerController = playerController_;
    }
    

    public override void Use()
    {
        Debug.Log("Trying to get rune");
        Transform hoveredTF = playerController.hovered.hoveredT;
        if (hoveredTF != null)
        {
            Rune hoveredRune = hoveredTF.GetComponent<Rune>();
            if (hoveredRune != null)
            {
                runeHandler.SlotRune(hoveredRune);
            }
        }
    }
}


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
    
    private GetRuneSkill getRuneSkill;
    protected virtual int movementPriority => 1;
    private HashSet<ConstructShape> shapesIncludedIn = new HashSet<ConstructShape>();
    public Construct construct { get; private set; }
    public IObjectController controlledBy { get; private set; }
    public bool isConstructed => construct != null;
    public bool isControlled => controlledBy != null;


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


    public virtual void OnJoinConstruct(Construct construct_)
    {
        // Notify everything about joining construct
        construct = construct_;
        construct.TrackObject(this);
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
        construct.UntrackObject(this);
        construct = null;
    }


    public void SetControlledBy(IObjectController controlledBy_)
    {
        if (controlledBy == controlledBy_) return;
        if (controlledBy != null && controlledBy_ != null) throw new System.Exception("Cannot SetControlledBy() when already controlled.");
        controlledBy = controlledBy_;
    }
    
    public void OnJoinShape(ConstructShape shape) => shapesIncludedIn.Add(shape);

    public void OnLeaveShape(ConstructShape shape) => shapesIncludedIn.Remove(shape);


    #region Getters

    public virtual bool GetContainsWO(WorldObject checkWO) => baseWO == checkWO;

    public virtual bool GetContainsCO(ConstructObject checkCO) => checkCO == this;

    public virtual ConstructObject GetCentreCO() => this;

    public virtual Vector3 GetCentrePosition() => GetCentreCO().transform.position;

    public virtual Quaternion GetForwardRot() => Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up);

    #endregion


    #region IHoverable

    public Vector3 IH_GetPosition() => GetCentrePosition();

    public bool IH_GetHighlighted() => baseWO.isHighlighted;

    public IHighlightableState IH_GetState() => isConstructed ? IHighlightableState.CONSTRUCTED : IHighlightableState.INACTIVE;

    public void IH_SetNearby(bool isNearby) { if (inspectableLabel != null) inspectableLabel.SetNearby(isNearby); }

    public void IH_SetHighlighted(bool isHighlighted) { if (inspectableLabel != null) inspectableLabel.SetHighlighted(isHighlighted); baseWO.isHighlighted = isHighlighted; }

    #endregion


    #region IInspectable

    [Header("Inspectable References")]
    [SerializeField] private GameObject inspectableLabelPrefab;
    [SerializeField] protected Sprite inspectableIcon;

    public InspectableLabel inspectableLabel { get; private set; }

    protected virtual void GenerateInspectableLabel()
    {
        // Instantiate data label
        if (inspectableLabelPrefab == null) return;
        GameObject inspectableLabelGO = Instantiate(inspectableLabelPrefab);
        inspectableLabel = inspectableLabelGO.GetComponent<InspectableLabel>();
        inspectableLabel.SetObject(this, baseWO.GetMaxExtent());
    }

    public Sprite II_GetIconSprite() => inspectableIcon;

    public string II_GetName() => objectData.name;

    public string II_GetDescription() => objectData.description;

    public Element II_GetElement() => objectData.element;

    public virtual List<string> II_GetAttributes() => new List<string>()
    {
        "Health: " + objectData.health,
        "Energy: " + objectData.energy + " (" + objectData.energyRegen + "/s)",
        "Slots: " + objectData.slotCount
    };

    public virtual List<string> II_GetModifiers() => new List<string>()
    {
        "Rapid (+10% Speed)",
        "Energetic (+15% e. regen)"
    };

    public Vector3 II_GetPosition() => GetCentrePosition();

    public float II_GetMass() => baseWO.rb.mass;

    #endregion
}
