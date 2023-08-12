
using UnityEngine;


[CreateAssetMenu(fileName = "Inspected Data", menuName = "Data/InspectedData")]
public class InspectedData : ScriptableObject
{
    public GameObject inspectableLabelPrefab;

    public Sprite icon;
    public new string name;
    public string description;
    public Element element;
}
