
using UnityEngine;


[CreateAssetMenu(fileName = "Inspectable Data", menuName = "Data/InspectableData")]
public class InspectableData : ScriptableObject
{
    public GameObject inspectableLabelPrefab;
    public Sprite inspectableIcon;

    public new string name;
    public string description;
    public Element element;
}
