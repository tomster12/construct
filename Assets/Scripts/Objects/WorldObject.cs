
using UnityEngine;


public class WorldObject : MonoBehaviour
{
    // Declare static, references, variables
    private static float[] MOVE_RESIST = new float[] { 60.0f, 125f, 0.3f, 1.0f };

    [Header("References")]
    [SerializeField] private ObjectData _objectData;
    [SerializeField] private MeshFilter _mf;
    [SerializeField] private Collider _cl;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Outline _ol;
    public ObjectData objectData => _objectData;
    public MeshFilter mf => _mf;
    public Collider cl => _cl;
    public Rigidbody rb => _rb;
    public Outline ol => _ol;

    public float density { get; private set; } = 150f;
    public float volume { get; private set; }
    public float mass { get; private set; }
    public float moveResist { get; private set; }
    public bool isHighlighted { get => ol.enabled; set => ol.enabled = value; }


    public void Start()
    {
        // Intialize variables
        CalculatePhysical();
    }

    private void CalculatePhysical()
    {
        // Calculate physical properties
        volume = VolumeOfMesh(mf.sharedMesh);
        mass = volume * density;

        // Map mass from [0 - 1] to [2 - 3] and clamp as moveResist
        moveResist = Mathf.Clamp((mass - WorldObject.MOVE_RESIST[0]) / (WorldObject.MOVE_RESIST[1] - WorldObject.MOVE_RESIST[0]), 0.0f, 1.0f);
        moveResist = (1.0f - moveResist) * (WorldObject.MOVE_RESIST[3] - WorldObject.MOVE_RESIST[2]) + WorldObject.MOVE_RESIST[2];
    }

    
    public float GetMaxExtent() => Mathf.Max(cl.bounds.extents.x, cl.bounds.extents.y, cl.bounds.extents.z);


    public void SetLayer(int layer) => SetLayer(transform, layer);


    #region Static

    public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Get volume of a triangle
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }


    public static float VolumeOfMesh(Mesh mesh)
    {
        // Return volume of a mesh
        float volume = 0;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = mesh.vertices[mesh.triangles[i + 0]];
            Vector3 p2 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 p3 = mesh.vertices[mesh.triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }


    public static void SetLayer(Transform t, int layer)
    {
        // Recursive transform layer update
        t.gameObject.layer = layer;
        foreach (Transform c in t) SetLayer(c, layer);
    }

    #endregion
}
