
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ClassInspector : MonoBehaviour
{
    [SerializeField] private GameObject variablePrefab;
    [SerializeField] private TextMeshProUGUI titleText;
    private Dictionary<string, ClassInspectorVariable> variables = new Dictionary<string, ClassInspectorVariable>();



    public void AddVariable(string name, string value)
    {
        // Check variable not in already
        if (variables.ContainsKey(name)) return;

        // Create variable
        GameObject go = Instantiate(variablePrefab);
        ClassInspectorVariable goS = go.GetComponent<ClassInspectorVariable>();
        go.transform.parent = transform;
        goS.nameText.text = name;
        variables[name] = goS;
    }

    public void SetVariable(string name, string value)
    {
        // Check variable not in already
        if (!variables.ContainsKey(name)) return;

        // Set value text
        variables[name].valueText.text = value;
    }

    [ContextMenu("Clear Variables")]
    public void ClearVariables()
    {
        // Delete all current variables
        ClassInspectorVariable[] variablesList = GetComponentsInChildren<ClassInspectorVariable>();
        for (int i = 0; i < variablesList.Length; i++) DestroyImmediate(variablesList[i].gameObject);
    }
}
