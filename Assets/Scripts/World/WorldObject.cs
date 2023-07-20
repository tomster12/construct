
using UnityEngine;


public class WorldObject : MonoBehaviour
{
    // Declare static, references, variables
    private static float[] MASS_RESIST_MAP = new float[] { 1.0f, 10.0f, 1.0f, 2.0f };

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
    public bool isLoose { get => !rb.isKinematic; set => rb.isKinematic = !value; }
    public bool isFloating { get => !rb.useGravity; set => rb.useGravity = !value; }
    public bool isColliding { get => cl.enabled; set => cl.enabled = value; }
    public float maxExtent => Mathf.Max(cl.bounds.extents.x, cl.bounds.extents.y, cl.bounds.extents.z);


    public void Start()
    {
        // Intialize variables
        CalculatePhysical();
    }

    private void CalculatePhysical()
    {
        // Calculate physical properties
        volume = Util.VolumeOfMesh(mf.sharedMesh);
        moveResist = 1.0f / Util.ConstrainMap(rb.mass, MASS_RESIST_MAP[0], MASS_RESIST_MAP[1], MASS_RESIST_MAP[2], MASS_RESIST_MAP[3]);
    }
    

    public void SetLayer(int layer) => Util.SetLayer(transform, layer);
}
