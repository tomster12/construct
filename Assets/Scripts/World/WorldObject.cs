
using UnityEngine;


public class WorldObject : MonoBehaviour
{
    // Declare static, references, variables
    private static float[] MASS_RESIST_MAP = new float[] { 1.0f, 5.0f, 1.0f, 2.0f };

    [Header("References")]
    [SerializeField] private MeshFilter _mf;
    [SerializeField] private Collider _cl;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Outline _ol;
    public MeshFilter mf => _mf;
    public Collider cl => _cl;
    public Rigidbody rb => _rb;
    public Outline ol => _ol;

    public float volume { get; private set; }
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
        moveResist = 1.0f / Util.Map(rb.mass, MASS_RESIST_MAP[0], MASS_RESIST_MAP[1], MASS_RESIST_MAP[2], MASS_RESIST_MAP[3]);
    }

    
    public float GetMaxExtent() => Mathf.Max(cl.bounds.extents.x, cl.bounds.extents.y, cl.bounds.extents.z);


    public void SetLayer(int layer) => Util.SetLayer(transform, layer);


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

    #endregion
}
