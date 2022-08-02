
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "Element", menuName = "Data/Element")]
public class Element : ScriptableObject
{
    public new string name;
    public Sprite sprite;
    public Color color;
}
