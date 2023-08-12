
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class Rune : MonoBehaviour, IHighlightable, IInspectable
{
    [Header("References")]
    [SerializeField] protected Element element;
    [SerializeField] protected Object _baseObject;
    [SerializeField] protected RuneData _runeData;

    public Object baseObject => _baseObject;
    public RuneData runeData => _runeData;
    public InspectableLabel inspectableLabel { get; private set; }

    private RuneHandler slottedRuneHandler;
    private bool isSlotted => slottedRuneHandler != null;


    protected virtual void Start()
    {
        // Initialize label
        GenerateInspectableLabel();
    }

    protected virtual void GenerateInspectableLabel()
    {
        // Instantiate data label
        if (runeData.inspectableLabelPrefab == null) return;
        GameObject inspectableLabelGO = Instantiate(runeData.inspectableLabelPrefab);
        inspectableLabel = inspectableLabelGO.GetComponent<InspectableLabel>();
        inspectableLabel.SetObject(this, baseObject.maxExtent);
    }


    public virtual InspectedData Inspect() => runeData;

    public Vector3 GetPosition() => transform.position;

    public bool GetIsHighlighted() => baseObject.isHighlighted;

    public bool GetIsNearby() => inspectableLabel.isNearby;

    public ObjectType GetObjectType() => isSlotted ? ObjectType.PartNSTRUCTED : ObjectType.LOOSE;
    
    public Object GetObject() => baseObject;

    public virtual List<string> GetAttributes() => new List<string>()
    {
        "Damage: 10",
        "Crit. Chance: 0%",
        "Energy Cost: 15"
    };

    public virtual List<string> GetModifiers() => new List<string>();


    public void SetSlotted(RuneHandler runeHandler)
    {
        slottedRuneHandler = runeHandler;
        baseObject.isColliding = !isSlotted;
        baseObject.isLoose = !isSlotted;
        baseObject.isFloating = isSlotted;
    }

    public void SetIsNearby(bool isNearby) => inspectableLabel.SetIsNearby(isNearby);

    public void SetIsHighlighted(bool isHighlighted) { inspectableLabel.SetIsHighlighted(isHighlighted); baseObject.isHighlighted = isHighlighted; }
}
