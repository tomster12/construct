
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[Serializable]
public class StatList : ISerializationCallbackReceiver
{
    [SerializeField] private bool randomizeStats;
    [HideInInspector] [SerializeField] private Dictionary<String, int> keys = new Dictionary<String, int>();
    [SerializeField] private List<Stat> stats = new List<Stat>();

    public float this[string name] { get { return GetFinal(name); } set { SetBase(name, value); } }
    public float[] this[string name, bool _] { get { return GetBaseRange(name); } set { SetBaseRange(name, value); } }


    public float GetBase(string name)
    {
        // Get base value, default to 0.0f
        if (!keys.ContainsKey(name)) return 0.0f;
        else return stats[keys[name]].baseValue;
    }

    public void SetBase(string name, float value)
    {
        // Create a new stat, otherwise set current
        if (!keys.ContainsKey(name))
        {
            keys[name] = stats.Count;
            stats.Add(new Stat(name, value));
        }
        else stats[keys[name]].baseValue = value;
    }

    public float[] GetBaseRange(string name)
    {
        // Get base range, default to { 0.0f, 0.0f }
        if (!keys.ContainsKey(name)) return new float[] { 0.0f, 0.0f };
        else return stats[keys[name]].baseRange;
    }

    public void SetBaseRange(string name, float[] range)
    {
        // Create a new random stat, otherwise set current
        if (!keys.ContainsKey(name))
        {
            keys[name] = stats.Count;
            stats.Add(new Stat(name, range));
        }
        else stats[keys[name]].baseRange = range;
    }

    public float GetFinal(string name)
    {
        // Return if exists, default to 0.0f
        if (!keys.ContainsKey(name)) return 0.0f;
        else return stats[keys[name]].finalValue;
    }

    public int AddAffector(string name, float value, bool mult)
    {
        // Tell stat to add affector, default to -1
        if (!keys.ContainsKey(name)) return -1;
        else return stats[keys[name]].AddAffector(value, mult);
    }

    public bool RemoveAffector(string name, int id)
    {
        // Tell stat to add affector, default to false
        if (!keys.ContainsKey(name)) return false;
        else return stats[keys[name]].RemoveAffector(id);
    }

    public void Randomize() { stats.ForEach(s => s.Randomize()); }


    #region Serialization

    public void OnBeforeSerialize()
    {
        // Calculate final value for all stats by calling getter
        float _;
        foreach (Stat stat in stats) _ = stat.finalValue;
    }

    public void OnAfterDeserialize()
    {

        // Add new keys if stats longer
        if (stats.Count > keys.Count)
        {
            for (int i = keys.Count; i < stats.Count; i++)
            {
                stats[i].name = stats[i - 1].name + "(1)";
            }
        }

        // Recalculate keys mapping
        keys.Clear();
        for (int i = 0; i < stats.Count; i++)
        {
            keys[stats[i].name] = i;
            if (stats[i].isRandom && randomizeStats) stats[i].Randomize();
        }
        randomizeStats = false;
    }


    [CustomPropertyDrawer(typeof(Stat))]
    public class StatDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Include variable height
            float height = EditorGUIUtility.singleLineHeight;

            // Add height of affector foldout
            if (property.FindPropertyRelative("affectors").isExpanded)
            {
                int rowCount = Mathf.Max(property.FindPropertyRelative("affectors").arraySize, 1);
                float affectorHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                height += affectorHeight * (rowCount + 2);
            }

            // Return height
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin property / label at position, cache indent
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Initialize variables
            float lh = EditorGUIUtility.singleLineHeight;
            bool affExp = property.FindPropertyRelative("affectors").isExpanded;
            bool isRandom = property.FindPropertyRelative("isRandom").boolValue;

            // Normal stat
            if (!isRandom)
            {
                EditorGUI.PropertyField( // [ 15 -> 135 ]
                    new Rect(position.x + 15, position.y, 120, lh),
                    property.FindPropertyRelative("affectors"), includeChildren: true
                );
                EditorGUI.PropertyField( // [ 140 -> -95 ]
                    new Rect(position.x + 140, position.y, (position.width - 140 - 95), lh),
                    property.FindPropertyRelative("name"), GUIContent.none
                );
                EditorGUI.PropertyField( // [ -90 -> -60 ]
                    new Rect(position.x + position.width - 90, position.y, 30, lh),
                    property.FindPropertyRelative("baseValue"), GUIContent.none
                );
                GUI.enabled = false;
                EditorGUI.PropertyField( // [ -55 -> -30 ]
                    new Rect(position.x + position.width - 55, position.y, 30, lh),
                    property.FindPropertyRelative("_finalValue"), GUIContent.none
                );
                GUI.enabled = true;
                EditorGUI.PropertyField(// [ -20 -> 0 ]
                    new Rect(position.x + position.width - 20, position.y, 20, lh),
                    property.FindPropertyRelative("isRandom"), GUIContent.none
                );
            }

            // Random stat
            else
            {
                EditorGUI.PropertyField( // [ 30  -> 85 ]
                    new Rect(position.x + 15, position.y, 120, lh),
                    property.FindPropertyRelative("affectors"), includeChildren: true
                );
                EditorGUI.PropertyField( // [ 90  -> -165 ]
                    new Rect(position.x + 140, position.y, (position.width - 140 - 165), lh),
                    property.FindPropertyRelative("name"), GUIContent.none
                );
                EditorGUI.PropertyField( // [ -160 -> -130 ]
                    new Rect(position.x + position.width - 160, position.y, 30, lh),
                    property.FindPropertyRelative("baseRange").GetArrayElementAtIndex(0), GUIContent.none
                );
                EditorGUI.PropertyField( // [ -125 ->  -95 ]
                    new Rect(position.x + position.width - 125, position.y, 30, lh),
                    property.FindPropertyRelative("baseRange").GetArrayElementAtIndex(1), GUIContent.none
                );
                EditorGUI.PropertyField( // [ -90 ->  -60 ]
                    new Rect(position.x + position.width - 90, position.y, 30, lh),
                    property.FindPropertyRelative("baseValue"), GUIContent.none
                );
                GUI.enabled = false;
                EditorGUI.PropertyField( // [ -55 -> -25 ]
                    new Rect(position.x + position.width - 55, position.y, 30, lh),
                    property.FindPropertyRelative("_finalValue"), GUIContent.none
                );
                GUI.enabled = true;
                EditorGUI.PropertyField( // [ -20 -> -0 ]
                    new Rect(position.x + position.width - 20, position.y, 20, lh),
                    property.FindPropertyRelative("isRandom"), GUIContent.none
                );
            }

            // Reload indent, end property
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(Affector))]
    public class AffectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin property / label at position, cache indent, get variable
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float lh = EditorGUIUtility.singleLineHeight;

            // Draw properties
            GUI.enabled = false;
            EditorGUI.PropertyField( // [  0 ->  30]
                new Rect(position.x, position.y, 30, lh),
                property.FindPropertyRelative("id"), GUIContent.none);
            GUI.enabled = true;
            EditorGUI.PropertyField( // [ 35 -> -25]
                new Rect(position.x + 35, position.y, (position.width - 35 - 25), lh),
                property.FindPropertyRelative("amount"), GUIContent.none
            );
            EditorGUI.PropertyField( // [-20 ->  -0]
                new Rect(position.x + position.width - 20, position.y, 20, lh),
                property.FindPropertyRelative("mult"), GUIContent.none
            );

            // Reload indent, end property
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    #endregion


    [Serializable]
    private class Stat
    {
        private static System.Random R = new System.Random();

        [SerializeField] public string name = "";
        [SerializeField] public bool isRandom = false;
        [SerializeField] public float[] baseRange = new float[2] { 0.0f, 0.0f };
        [SerializeField] public float baseValue = 0.0f;
        [SerializeField] public float _finalValue = 0.0f;
        [SerializeField] public float finalValue { get { return CalculateFinal(); } }
        [SerializeField] public List<Affector> affectors = new List<Affector>();


        public Stat(string name_, float baseValue_)
        {
            // Initialize variables
            name = name_;
            isRandom = false;
            baseValue = baseValue_;
            CalculateFinal();
        }

        public Stat(string name_, float[] baseRange_)
        {
            // Initialize variables
            name = name_;
            isRandom = true;
            baseRange = baseRange_;
            Randomize();
            CalculateFinal();
        }


        public void Randomize()
        {
            // Pick a random value in the range
            if (isRandom) baseValue = baseRange[0] + (float)R.NextDouble() * (baseRange[1] - baseRange[0]);
        }

        private float CalculateFinal()
        {
            // Recalculate the final value - addition then multiplication
            _finalValue = baseValue;
            affectors.ForEach(a => { if (!a.mult) _finalValue = a.Affect(_finalValue); });
            affectors.ForEach(a => { if (a.mult) _finalValue = a.Affect(_finalValue); });
            return _finalValue;
        }

        public int AddAffector(float value, bool mult)
        {
            // Put an affector in with a given type and value
            Affector aff = new Affector(value, mult);
            affectors.Add(aff);
            return aff.id;
        }

        public bool RemoveAffector(int id)
        {
            // Remove the given affector if exists
            for (int i = 0; i < affectors.Count; i++)
            {
                if (affectors[i].id == id) return affectors.Remove(affectors[i]);
            }
            return false;
        }
    }


    [Serializable]
    private class Affector
    {
        private static int NEXT_AFFECTOR_ID = 1;

        [SerializeField] public int id = -1;
        [SerializeField] public float amount = 0.0f;
        [SerializeField] public bool mult = false;


        public Affector() { id = NEXT_AFFECTOR_ID++; }
        public Affector(float amount_, bool mult_) { id = NEXT_AFFECTOR_ID++; amount = amount_; mult = mult_; }


        public float Affect(float value) => mult ? (value * amount) : (value + amount);
    }
}
