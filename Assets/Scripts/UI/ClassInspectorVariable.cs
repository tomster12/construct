
using UnityEngine;
using TMPro;


public class ClassInspectorVariable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _valueText;
    public TextMeshProUGUI nameText => _nameText;
    public TextMeshProUGUI valueText => _valueText;
}
