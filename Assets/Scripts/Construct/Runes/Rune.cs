
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class Rune : MonoBehaviour, IHighlightable, IInspectable
{
    [Header("References")]
    [SerializeField] protected Element element;
    [SerializeField] protected WorldObject _baseWO;
    [SerializeField] protected RuneData _runeData;

    public WorldObject baseWO => _baseWO;
    public RuneData runeData => _runeData;

    private RuneHandler slottedRuneHandler;
    private bool isSlotted => slottedRuneHandler != null;


    protected virtual void Start()
    {
        // Initialize label
        GenerateInspectableLabel();
    }


    public Vector3 GetCentrePosition() => transform.position;

    public void SetSlotted(RuneHandler runeHandler)
    {
        slottedRuneHandler = runeHandler;
        baseWO.isColliding = !isSlotted;
        baseWO.isLoose = !isSlotted;
        baseWO.isFloating = isSlotted;

    }


    #region IHoverable

    public Vector3 IHGetPosition() => GetCentrePosition();

    public bool IHGetHighlighted() => baseWO.isHighlighted;

    public ObjectType IHGetObjectType() => isSlotted ? ObjectType.CONSTRUCTED_CO : ObjectType.LOOSE_CO;

    public void IHSetNearby(bool isNearby) => inspectableLabel.SetNearby(isNearby);

    public void IHSetHighlighted(bool isHighlighted) { inspectableLabel.SetHighlighted(isHighlighted); baseWO.isHighlighted = isHighlighted; }

    #endregion


    #region IInspectable

    public InspectableLabel inspectableLabel { get; private set; }

    protected virtual void GenerateInspectableLabel()
    {
        // Instantiate data label
        if (runeData.inspectableLabelPrefab == null) return;
        GameObject inspectableLabelGO = Instantiate(runeData.inspectableLabelPrefab);
        inspectableLabel = inspectableLabelGO.GetComponent<InspectableLabel>();
        inspectableLabel.SetObject(this, baseWO.maxExtent);
    }

    public Sprite IIGetIconSprite() => runeData.inspectableIcon;

    public string IIGetName() => runeData.name; // "Rune";

    public string IIGetDescription() => runeData.description; // "A standard rune for use with a construct.";

    public Element IIGetElement() => runeData.element;

    public virtual List<string> IIGetAttributes() => new List<string>()
    {
        "Damage: 10",
        "Crit. Chance: 0%",
        "Energy Cost: 15"
    };

    public virtual List<string> IIGetModifiers() => new List<string>();

    public Vector3 IIGetPosition() => GetCentrePosition();

    public float IIGetMass() => 0.0f;

    #endregion
}
