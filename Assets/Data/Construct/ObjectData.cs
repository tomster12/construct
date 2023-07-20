
using UnityEngine;


[CreateAssetMenu(fileName = "Object Data", menuName = "Data/ObjectData")]
public class ObjectData : InspectableData
{
    public int health;
    public int energy;
    public int energyRegen;
    public int slotCount;
}
