
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;


[Serializable]
public class StatList : ISerializationCallbackReceiver {

  // #region - Main

  // Declare variables
  [SerializeField] private bool randomizeStats;
  [HideInInspector]
  [SerializeField] private Dictionary<String, int> keys;
  [SerializeField] private List<Stat> stats;


  public StatList() {
    // Initialize variables
    keys = new Dictionary<String, int>();
    stats = new List<Stat>();
  }


  public float this[string s] {
    // Use custom getters and setters
    get { return getFinal(s); }
    set { setBase(s, value); }
  }


  public float[] this[string s, bool _] {
    // Use custom getters and setters
    get { return getBaseRange(s); }
    set { setBaseRange(s, value); }
  }


  public void randomize() {
    // Randomize all stats
    foreach(Stat s in stats) s.randomize();
  }


  public float getBase(string name) {
    // Get base value, default to 0.0f
    if (!keys.ContainsKey(name)) return 0.0f;
    else return stats[keys[name]].baseValue;
  }


  public void setBase(string name, float value) {
    // Create a new stat, otherwise set current
    if (!keys.ContainsKey(name)) {
      keys[name] = stats.Count;
      stats.Add(new Stat(name, value));
    } else stats[keys[name]].baseValue = value;
  }


  public float[] getBaseRange(string name) {
    // Get base range, default to { 0.0f, 0.0f }
    if (!keys.ContainsKey(name)) return new float[] { 0.0f, 0.0f };
    else return stats[keys[name]].baseRange;
  }


  public void setBaseRange(string name, float[] range) {
    // Create a new random stat, otherwise set current
    if (!keys.ContainsKey(name)) {
      keys[name] = stats.Count;
      stats.Add(new Stat(name, range));
    } else stats[keys[name]].baseRange = range;
  }


  public float getFinal(string name) {
    // Return if exists, default to 0.0f
    if (!keys.ContainsKey(name)) return 0.0f;
    else return stats[keys[name]].finalValue;
  }


  public int addAffector(string name, float value, bool mult) {
    // Tell stat to add affector, default to -1
    if (!keys.ContainsKey(name)) return -1;
    else return stats[keys[name]].addAffector(value, mult);
  }


  public bool removeAffector(string name, int id) {
    // Tell stat to add affector, default to false
    if (!keys.ContainsKey(name)) return false;
    else return stats[keys[name]].removeAffector(id);
  }

  // #endregion


  // #region - Serialization

  public void OnBeforeSerialize() {
    // Calculate final value for all stats by calling getter
    float _;
    foreach (Stat stat in stats) _ = stat.finalValue;
  }


  public void OnAfterDeserialize() {
    // Add new keys if stats longer
    if (stats.Count > keys.Count) {
      for (int i = keys.Count; i < stats.Count; i++) {
        stats[i].name = stats[i - 1].name + "(1)";
        keys[stats[i].name] = i;
      }
    }

    // Loop over current keys
    Dictionary<string, int> newKeys = new Dictionary<string, int>();
    foreach(KeyValuePair<string, int> entry in keys) {
      int index = entry.Value;

      // As long as stat still exists
      if (index < stats.Count) {

        // Rewrite corrected stat key
        string name = stats[index].name;
        newKeys[name] = index;

        // Randomize if needed
        if (stats[index].isRandom && randomizeStats) stats[index].randomize();
      }
    }

    // Update variables
    randomizeStats = false;
    keys = newKeys;
  }


  [CustomPropertyDrawer(typeof(Stat))]
  public class StatDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      // Include variable height
      float height = EditorGUIUtility.singleLineHeight;

      // Add height of affector foldout
      float size = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
      if (property.FindPropertyRelative("affectors").isExpanded)
        height += size * (property.FindPropertyRelative("affectors").arraySize + 1);

      // Return height
      return height;
    }


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      // Begin property / label at position, cache indent
      EditorGUI.BeginProperty(position, label, property);
      var indent = EditorGUI.indentLevel;
      EditorGUI.indentLevel = 0;

      // Initialize variables
      float lh = EditorGUIUtility.singleLineHeight;
      bool affExp = property.FindPropertyRelative("affectors").isExpanded;
      bool isRandom = property.FindPropertyRelative("isRandom").boolValue;

      // Normal stat
      if (!isRandom) {
        EditorGUI.PropertyField(new Rect(position.x + 30, position.y, affExp ? (position.width - 30) : 55, lh),
          property.FindPropertyRelative("affectors"), includeChildren: true);                       // [ 30 ->  85]

        EditorGUI.PropertyField(new Rect(position.x + 90, position.y, (position.width - 90 - 95), lh),
          property.FindPropertyRelative("name"), GUIContent.none);                                  // [ 90 -> -95]

        EditorGUI.PropertyField(new Rect(position.x + position.width - 90, position.y, 30, lh),
          property.FindPropertyRelative("baseValue"), GUIContent.none);                             // [-90 -> -60]

        GUI.enabled = false;
        EditorGUI.PropertyField(new Rect(position.x + position.width - 55, position.y, 30, lh),
          property.FindPropertyRelative("_finalValue"), GUIContent.none);                           // [-55 -> -25]
        GUI.enabled = true;

        EditorGUI.PropertyField(new Rect(position.x + position.width - 20, position.y, 20, lh),
          property.FindPropertyRelative("isRandom"), GUIContent.none);                              // [-20 -> -00]

      // Random stat
      } else {
        EditorGUI.PropertyField(new Rect(position.x + 30, position.y, affExp ? (position.width - 30) : 55, lh),
          property.FindPropertyRelative("affectors"), includeChildren: true);                       // [ 30  ->   85]

        EditorGUI.PropertyField(new Rect(position.x + 90, position.y, (position.width - 90 - 165), lh),
          property.FindPropertyRelative("name"), GUIContent.none);                                  // [ 90  -> -165]

        EditorGUI.PropertyField(new Rect(position.x + position.width - 160, position.y, 30, lh),
          property.FindPropertyRelative("baseRange").GetArrayElementAtIndex(0), GUIContent.none);   // [-160 -> -130]

        EditorGUI.PropertyField(new Rect(position.x + position.width - 125, position.y, 30, lh),
          property.FindPropertyRelative("baseRange").GetArrayElementAtIndex(1), GUIContent.none);   // [-125 ->  -95]

        EditorGUI.PropertyField(new Rect(position.x + position.width - 90, position.y, 30, lh),
          property.FindPropertyRelative("baseValue"), GUIContent.none);                             // [ -90 ->  -60]

        GUI.enabled = false;
        EditorGUI.PropertyField(new Rect(position.x + position.width - 55, position.y, 30, lh),
          property.FindPropertyRelative("_finalValue"), GUIContent.none);                           // [ -55 ->  -25]
        GUI.enabled = true;

        EditorGUI.PropertyField(new Rect(position.x + position.width - 20, position.y, 20, lh),
          property.FindPropertyRelative("isRandom"), GUIContent.none);                              // [ -20 ->   -0]
      }

      // Reload indent, end property
      EditorGUI.indentLevel = indent;
      EditorGUI.EndProperty();
    }
  }


  [CustomPropertyDrawer(typeof(Affector))]
  public class AffectorDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      // Begin property / label at position, cache indent, get variable
      EditorGUI.BeginProperty(position, label, property);
      var indent = EditorGUI.indentLevel;
      EditorGUI.indentLevel = 0;
      float lh = EditorGUIUtility.singleLineHeight;

      // Draw properties
      GUI.enabled = false;
      EditorGUI.PropertyField(new Rect(position.x, position.y, 30, lh),
        property.FindPropertyRelative("id"), GUIContent.none);                                      // [  0 ->  30]
      GUI.enabled = true;

      EditorGUI.PropertyField(new Rect(position.x + 35, position.y, (position.width - 35 - 25), lh),
        property.FindPropertyRelative("amount"), GUIContent.none);                                  // [ 35 -> -25]

      EditorGUI.PropertyField(new Rect(position.x + position.width - 20, position.y, 20, lh),
        property.FindPropertyRelative("mult"), GUIContent.none);                                    // [-20 ->  -0]

      // Reload indent, end property
      EditorGUI.indentLevel = indent;
      EditorGUI.EndProperty();
    }
  }

  // #endregion


  [Serializable]
  private class Stat {

    // #region - Main

    // Declare static, variables
    private static System.Random R = new System.Random();
    private static int NEXT_AFFECTOR_ID = 1;

    [SerializeField] public string name;
    [SerializeField] public bool isRandom;
    [SerializeField] public float[] baseRange;
    [SerializeField] public float baseValue;
    [SerializeField] public float _finalValue;
    [SerializeField] public float finalValue { get { return calculateFinal(); } }
    [SerializeField] public List<Affector> affectors;


    public Stat(string name_, float baseValue_) {
      // Initialize variables
      name = name_;
      isRandom = false;
      baseRange = new float[2] { baseValue_, baseValue_ };
      baseValue = baseValue_;
      affectors = new List<Affector>();
      calculateFinal();
    }


    public Stat(string name_, float[] baseRange_) {
      // Initialize variables
      name = name_;
      isRandom = true;
      baseRange = baseRange_;
      randomize();
      affectors = new List<Affector>();
      calculateFinal();
    }


    public void randomize() {
      // Pick a random value in the range
      if (isRandom) {
        float rv = baseRange[0] + (float)R.NextDouble() * (baseRange[1] - baseRange[0]);
        baseValue = rv;
      }
    }


    private float calculateFinal() {
      // Recalculate the final value - addition then multiplication
      _finalValue = baseValue;
      foreach (Affector aff in affectors) {
        if (!aff.mult) _finalValue = aff.affect(_finalValue);
      }
      foreach (Affector aff in affectors) {
        if (aff.mult) _finalValue = aff.affect(_finalValue);
      }
      return _finalValue;
    }


    public int addAffector(float value, bool mult) {
      // Put an affector in with a given type and value
      Affector aff = new Affector(NEXT_AFFECTOR_ID++, value, mult);
      affectors.Add(aff);
      return aff.id;
    }


    public bool removeAffector(int id) {
      // Remove an affector from a given name with given id
      for (int i = 0; i < affectors.Count; i++) {
        if (affectors[i].id == id) {
          affectors.Remove(affectors[i]);
          return true;
        }
      }

      // Name or id does not exist
      return false;
    }

    // #endregion
  }


  [Serializable]
  private class Affector {

    // #region - Main

    // Declare variables
    [SerializeField] public int id;
    [SerializeField] public float amount;
    [SerializeField] public bool mult;


    public Affector(int id_) {
      // Initialize variables
      id = id_;
      amount = 0.0f;
      mult = false;
    }


    public Affector(int id_, float amount_, bool mult_) {
      // Initialize variables
      id = id_;
      amount = amount_;
      mult = mult_;
    }


    public float affect(float value) {
      // Multiply / add amount to value
      if (mult) return value * amount;
      else return value + amount;
    }

    // #endregion
  }
}
