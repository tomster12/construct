
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class Rune : MonoBehaviour, IHighlightable, IInspectable
{
    [Header("References")]
    [SerializeField] protected Element element;
    [SerializeField] protected WorldObject _baseWO;
    public WorldObject baseWO => _baseWO;

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
        baseWO.SetColliding(!isSlotted);
        baseWO.SetLoose(!isSlotted);
        baseWO.SetFloating(isSlotted);

    }


    #region IHoverable

    public Vector3 IH_GetPosition() => GetCentrePosition();

    public bool IH_GetHighlighted() => baseWO.isHighlighted;

    public IHighlightableState IH_GetState() => isSlotted ? IHighlightableState.CONSTRUCTED : IHighlightableState.INACTIVE;

    public void IH_SetNearby(bool isNearby) => inspectableLabel.SetNearby(isNearby);

    public void IH_SetHighlighted(bool isHighlighted) { inspectableLabel.SetHighlighted(isHighlighted); baseWO.isHighlighted = isHighlighted; }

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

    public string II_GetName() => "Rune";

    public string II_GetDescription() => "A standard rune for use with a construct.";

    public Element II_GetElement() => element;

    public virtual List<string> II_GetAttributes() => new List<string>()
    {
        "Damage: 10",
        "Crit. Chance: 0%",
        "Energy Cost: 15"
    };

    public virtual List<string> II_GetModifiers() => new List<string>();

    public Vector3 II_GetPosition() => GetCentrePosition();

    public float II_GetMass() => 0.0f;

    #endregion
}
