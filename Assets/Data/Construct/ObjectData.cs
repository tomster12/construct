
using UnityEngine;


[CreateAssetMenu(fileName = "Object Data", menuName = "Data/ObjectData")]
public class ObjectData : ScriptableObject
{
    public new string name;
    public string description;
    public Element element;

    public int health;
    public int energy;
    public int energyRegen;
    public int slotCount;
}
