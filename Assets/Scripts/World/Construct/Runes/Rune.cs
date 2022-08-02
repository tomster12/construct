
using System.Collections.Generic;
using UnityEngine;


public class Rune : MonoBehaviour, IHoverable, IInspectable
{
    // Declare references, variables
    [Header("Prefabs")]
    [SerializeField] private GameObject inspectableLabelPrefab;

    [Header("References")]
    [SerializeField] protected Sprite inspectableIcon;
    [SerializeField] protected Element element;
    [SerializeField] protected WorldObject _baseWO;
    public WorldObject baseWO => _baseWO;

    public InspectableLabel inspectableLabel { get; private set; }


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


    public Vector3 GetCentrePosition() => transform.position;


    public Vector3 GetHoverablePosition() => GetCentrePosition();

    public bool GetHoverableHighlighted() => baseWO.isHighlighted;

    public IHoverableState GetHoverableState() => IHoverableState.LOOSE;


    public Sprite GetInspectableIconSprite() => inspectableIcon;

    public string GetInspectableName() => "Rune";

    public string GetInspectableDescription() => "A standard rune for use with a construct.";

    public Element GetInspectableElement() => element;

    public virtual List<string> GetInspectableAttributes() => new List<string>()
    {
        "Damage: 10",
        "Crit. Chance: 0%",
        "Energy Cost: 15"
    };

    public virtual List<string> GetInspectableModifiers() => new List<string>();

    public Vector3 GetInspectablePosition() => GetCentrePosition();

    public float GetInspectableMass() => 0.0f;


    public void SetHoverableNearby(bool isNearby) => inspectableLabel.SetNearby(isNearby);

    public void SetHoverableHighlighted(bool isHighlighted) { inspectableLabel.SetHighlighted(isHighlighted); baseWO.isHighlighted = isHighlighted; }
}
