
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WorldObject : MonoBehaviour {

  // #region - Setup

  // Declare static, references, variables
  private static float[] MOVE_RESIST = new float[] { 60.0f, 125f, 0.3f, 1.0f };

  [SerializeField] public ObjectData objectData;

  public MeshFilter mf { get; private set; }
  public Collider cl { get; private set; }
  public Outline ol { get; private set; }
  public Rigidbody rb { get; private set; }

  public float density { get; private set; } = 150f;
  public float volume { get; private set; }
  public float mass { get; private set; }
  public float moveResist { get; private set; }


  public void Awake() {
    // Intialize references
    mf = GetComponent<MeshFilter>();
    cl = GetComponent<Collider>();
    ol = GetComponent<Outline>();
    rb = GetComponent<Rigidbody>();
  }


  public void Start() {
    // Initialize config
    calculatePhysical();
  }


  private void calculatePhysical() {
    // Calculate physical properties
    volume = volumeOfMesh(mf.sharedMesh);
    mass = volume * density;

    // Map mass from [0 - 1] to [2 - 3] and clamp as moveResist
    moveResist = (mass - WorldObject.MOVE_RESIST[0]) / (WorldObject.MOVE_RESIST[1] - WorldObject.MOVE_RESIST[0]);
    moveResist = 1 - Mathf.Clamp(moveResist, 0.0f, 1.0f);
    moveResist = moveResist * (WorldObject.MOVE_RESIST[3] - WorldObject.MOVE_RESIST[2]) + WorldObject.MOVE_RESIST[2];
  }

  // #endregion


  // #region - Main

  public bool isHighlighted() { return ol.enabled; }
  public void highlight() => ol.enabled = true;
  public void unhighlight() => ol.enabled = false;


  public void setLayer(int layer) {
    // Set self and children to specific layer
    Transform[] all = GetComponentsInChildren<Transform>();
    foreach (Transform t in all) t.gameObject.layer = layer;
  }


  public float getMaxExtent() {
    // Returns max extent
    return Mathf.Max(cl.bounds.extents.x, cl.bounds.extents.y, cl.bounds.extents.z);
  }

  // #endregion


  // #region - Static

  public static float signedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
    // Get volume of a triangle
    float v321 = p3.x * p2.y * p1.z;
    float v231 = p2.x * p3.y * p1.z;
    float v312 = p3.x * p1.y * p2.z;
    float v132 = p1.x * p3.y * p2.z;
    float v213 = p2.x * p1.y * p3.z;
    float v123 = p1.x * p2.y * p3.z;
    return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
  }


  public static float volumeOfMesh(Mesh mesh) {
    // Return volume of a mesh
    float volume = 0;
    Vector3[] vertices = mesh.vertices;
    int[] triangles = mesh.triangles;

    for (int i = 0; i < mesh.triangles.Length; i += 3) {
      Vector3 p1 = vertices[triangles[i + 0]];
      Vector3 p2 = vertices[triangles[i + 1]];
      Vector3 p3 = vertices[triangles[i + 2]];
      volume += signedVolumeOfTriangle(p1, p2, p3);
    }

    return Mathf.Abs(volume);
  }

  // #endregion
}
