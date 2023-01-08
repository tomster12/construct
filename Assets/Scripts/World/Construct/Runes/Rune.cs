
using System.Collections.Generic;
using UnityEngine;


public class Rune : MonoBehaviour, IHighlightable, IInspectable
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


    #region - IHoverable

    public Vector3 GetIHPosition() => GetCentrePosition();

    public bool GetIHHovered() => baseWO.isHighlighted;

    public IHighlightableState GetIHState() => IHighlightableState.LOOSE;


    public Sprite GetIIIconSprite() => inspectableIcon;

    public string GetIIName() => "Rune";

    public string GetIIDescription() => "A standard rune for use with a construct.";

    public Element GetIIElement() => element;

    public virtual List<string> GetIIAttributes() => new List<string>()
    {
        "Damage: 10",
        "Crit. Chance: 0%",
        "Energy Cost: 15"
    };

    public virtual List<string> GetIIModifiers() => new List<string>();

    public Vector3 GetIIPosition() => GetCentrePosition();

    public float GetIIMass() => 0.0f;


    public void SetIHNearby(bool isNearby) => inspectableLabel.SetNearby(isNearby);

    public void SetIHHighlighted(bool isHighlighted) { inspectableLabel.SetHighlighted(isHighlighted); baseWO.isHighlighted = isHighlighted; }

    #endregion
}
