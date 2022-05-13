
using UnityEngine;


public class PlayerCamera : MonoBehaviour
{
    // Declare static, references, config, variables
    public static PlayerCamera instance { get; private set; }

    [SerializeField] private Construct playerConstruct;
    [SerializeField] private Transform camPivot;
    [SerializeField] private Transform camOrbit;
    [SerializeField] private Camera _cam;
    public Camera cam => _cam;

    [SerializeField] private StatList stats = new StatList()
    {
        ["rotateSpeed"] = 0.5f,
        ["offsetSpeed"] = 4.0f
    };

    private State cameraState;
    public WorldObject prevAimedWO { get; private set; }
    public WorldObject aimedWO { get; private set; }
    public Vector3 prevAimedPos { get; private set; }
    public Vector3 aimedPos { get; private set; }


    private void Awake()
    {
        // Handle singleton
        if (instance != null) return;
        instance = this;

        // Initialize variables and lock mouse
        cameraState = new IngameState(this);
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void Update()
    {
        // Update current state
        cameraState.Update();
    }


    public Transform GetPivot() => camPivot;


    public abstract class State
    {
        protected PlayerCamera pcam;

        public State(PlayerCamera pcam_) { pcam = pcam_; }
        public abstract void Update();
    }


    public class IngameState : State
    {
        // Declare static variables
        private static float MAX_REACH = 100.0f;
        private static float[] ZOOM_RANGE = new float[] { -35f, -3.5f };

        private ConstructObject currentCO;
        private float[] zoomRange;
        private Vector3 camOffset;


        public IngameState(PlayerCamera pcam_) : base(pcam_)
        {
            // Set offset
            currentCO = pcam.playerConstruct.GetCentreCO();
            ResetOffset();
        }


        public override void Update()
        {
            // Update current CO and offset
            ConstructObject newCO = pcam.playerConstruct.GetCentreCO();
            if (currentCO != newCO)
            {
                currentCO = newCO;
                ResetOffset();
            }

            // Rotate based on mouse movement
            pcam.camPivot.Rotate(0, Input.GetAxis("Mouse X") * pcam.stats["rotateSpeed"], 0, Space.World);
            pcam.camPivot.Rotate(-Input.GetAxis("Mouse Y") * pcam.stats["rotateSpeed"], 0, 0, Space.Self);

            // Zoom in / out based on scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                Vector3 local = pcam.camOrbit.transform.localPosition;
                local.z *= (1 - scroll);
                pcam.camOrbit.localPosition = local;
            }

            // Update pivot and orbit location
            pcam.camPivot.position = currentCO.transform.position;
            pcam.camOrbit.localPosition = new Vector3(
                Mathf.Lerp(pcam.camOrbit.localPosition.x, camOffset.x, pcam.stats["offsetSpeed"] * Time.deltaTime),
                Mathf.Lerp(pcam.camOrbit.localPosition.y, camOffset.y, pcam.stats["offsetSpeed"] * Time.deltaTime),
                Mathf.Clamp(pcam.camOrbit.localPosition.z, zoomRange[0], zoomRange[1])
            );

            // Raycast out from the camera
            pcam.prevAimedPos = pcam.aimedPos;
            pcam.prevAimedWO = pcam.aimedWO;
            Ray ray = pcam.cam.ScreenPointToRay(Input.mousePosition);
            
            // Hit some object
            if (Physics.Raycast(ray, out RaycastHit hit, MAX_REACH))
            {
                pcam.aimedPos = hit.point;
                pcam.aimedWO = hit.transform.GetComponent<WorldObject>();

            // Did not hit anything
            } else
            {
                pcam.aimedPos = ray.GetPoint(MAX_REACH);
                pcam.aimedWO = null;
            }
        }


        private void ResetOffset()
        {
            // Update max extent and max extent based on construct
            float maxExtent = currentCO.baseWO.GetMaxExtent();
            zoomRange = new float[] { maxExtent * ZOOM_RANGE[0], maxExtent * ZOOM_RANGE[1] };
            float xOff = maxExtent * 0.8f;
            float yOff = maxExtent * 1.1f;
            float zOff = Mathf.Clamp(pcam.camOrbit.transform.localPosition.z, zoomRange[0], zoomRange[1]);
            camOffset = new Vector3(xOff, yOff, zOff);
        }
    }
}
