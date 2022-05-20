
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    // Declare static, references, variables
    public static PlayerController instance { get; private set; }
    private static float MAX_CAM_REACH = 100.0f;

    [Header("References")]
    [SerializeField] private UIController uiController;
    [SerializeField] private Construct playerConstruct;
    [SerializeField] private Transform camPivot;
    [SerializeField] private Transform camOrbit;
    [SerializeField] private Camera _cam;
    public Camera cam => _cam;

    [Header("Config")]
    [SerializeField]
    private StatList camStats = new StatList()
    {
        ["rotateSpeed"] = 0.5f,
        ["offsetSpeed"] = 4.0f
    };

    private IngameState ingameState;
    private ForgingState forgingState;
    private State state;
    public WorldObject prevAimedWO { get; private set; }
    public WorldObject aimedWO { get; private set; }
    public Vector3 prevAimedPos { get; private set; }
    public Vector3 aimedPos { get; private set; }


    private void Awake()
    {
        // Handle singleton
        if (instance != null) return;
        instance = this;

        // Initialize state
        ingameState = new IngameState(this);
        forgingState = new ForgingState(this);
    }


    private void Start()
    {
        // Initialize variables
        SetState(ingameState);
    }


    private void Update()
    {
        // Run updates
        UpdateAimed();
        state.Update();
    }

    private void UpdateAimed()
    {
        // Raycast out from the camera
        prevAimedPos = aimedPos;
        prevAimedWO = aimedWO;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Hit some object
        if (Physics.Raycast(ray, out RaycastHit hit, MAX_CAM_REACH))
        {
            aimedPos = hit.point;
            aimedWO = hit.transform.GetComponent<WorldObject>();
        }

        // Did not hit anything
        else
        {
            aimedPos = ray.GetPoint(MAX_CAM_REACH);
            aimedWO = null;
        }
    }


    private void FixedUpdate()
    {
        // Run fixed updates
        state.FixedUpdate();
    }


    public Transform GetPivot() => camPivot;


    private void SetState(State state_)
    {
        // Update state to new state
        if (state == state_) return;
        if (state != null) state.Unset();
        state = state_;
        state.Set();
    }


    public abstract class State
    {
        protected PlayerController pcn;

        public State(PlayerController pcn_) { pcn = pcn_; }

        public virtual void Set() { }
        public virtual void Unset() { }

        public virtual void Update() { }
        public virtual void FixedUpdate() { }
    }

    private class IngameState : State
    {
        // Declare static, variables
        private static float[] ZOOM_RANGE = new float[] { -35f, -3.5f };

        private Vector3 inputMoveDir;
        private ConstructObject currentCO;
        private float[] zoomRange;
        private Vector3 camOffset;


        public IngameState(PlayerController pcn_) : base(pcn_) { }


        public override void Set()
        {
            // Initialize variables
            pcn.uiController.SetIngame(true);
            Cursor.lockState = CursorLockMode.Locked;
            currentCO = pcn.playerConstruct.GetCentreCO();
            ResetOffset();
        }

        public override void Unset()
        {
            // Update variables
            pcn.uiController.SetIngame(false);
            Cursor.lockState = CursorLockMode.None;
        }


        public override void Update()
        {
            // Run updates
            UpdateConstruct();
            UpdateCamera();

            // Handle changing state
            if (Input.GetKeyDown("tab") && pcn.playerConstruct.GetCanForge()) pcn.SetState(pcn.forgingState);
        }

        private void UpdateConstruct()
        {
            // Input movement with "Horizontal" and "Vertical"
            inputMoveDir = Vector3.zero;
            if (Input.GetAxisRaw("Horizontal") != 0.0f || Input.GetAxisRaw("Vertical") != 0.0f)
            {
                Transform camPivot = pcn.GetPivot();
                Vector3 flatForward = Vector3.ProjectOnPlane(camPivot.forward, Vector3.up).normalized;
                inputMoveDir += camPivot.right * Input.GetAxisRaw("Horizontal");
                inputMoveDir += flatForward * Input.GetAxisRaw("Vertical");
            }

            // Send ability skill controls (hardcode: f)
            if (Input.GetKeyDown("f")) pcn.playerConstruct.skills.Use("f");
            foreach (string key in pcn.playerConstruct.abilityButtons)
            {
                if (key.StartsWith("_"))
                {
                    if (Input.GetMouseButtonDown(key[1] - '0')) pcn.playerConstruct.skills.Use(key);
                }
                else if (Input.GetKeyDown(key)) pcn.playerConstruct.skills.Use(key);
            }
        }

        private void UpdateCamera()
        {
            // Update current CO and offset
            ConstructObject newCO = pcn.playerConstruct.GetCentreCO();
            if (currentCO != newCO)
            {
                currentCO = newCO;
                ResetOffset();
            }

            // Rotate based on mouse movement
            pcn.camPivot.Rotate(0, Input.GetAxis("Mouse X") * pcn.camStats["rotateSpeed"], 0, Space.World);
            pcn.camPivot.Rotate(-Input.GetAxis("Mouse Y") * pcn.camStats["rotateSpeed"], 0, 0, Space.Self);

            // Zoom in / out based on scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                Vector3 local = pcn.camOrbit.transform.localPosition;
                local.z *= (1 - scroll);
                pcn.camOrbit.localPosition = local;
            }

            // Update pivot and orbit location
            pcn.camPivot.position = currentCO.transform.position;
            pcn.camOrbit.localPosition = new Vector3(
                Mathf.Lerp(pcn.camOrbit.localPosition.x, camOffset.x, pcn.camStats["offsetSpeed"] * Time.deltaTime),
                Mathf.Lerp(pcn.camOrbit.localPosition.y, camOffset.y, pcn.camStats["offsetSpeed"] * Time.deltaTime),
                Mathf.Clamp(pcn.camOrbit.localPosition.z, zoomRange[0], zoomRange[1])
            );
        }


        public override void FixedUpdate()
        {
            // Run fixed updates
            FixedUpdateConstruct();
        }

        private void FixedUpdateConstruct() {
            // Aim and move construct
            if (!pcn.playerConstruct.GetContainsWO(pcn.aimedWO)) pcn.playerConstruct.AimAtPosition(pcn.aimedPos);
            if (inputMoveDir != Vector3.zero) pcn.playerConstruct.MoveInDirection(inputMoveDir);
        }


        private void ResetOffset()
        {
            // Update max extent and max extent based on construct
            float maxExtent = currentCO.baseWO.GetMaxExtent();
            zoomRange = new float[] { maxExtent * ZOOM_RANGE[0], maxExtent * ZOOM_RANGE[1] };
            float xOff = maxExtent * 0.8f;
            float yOff = maxExtent * 1.1f;
            float zOff = Mathf.Clamp(pcn.camOrbit.transform.localPosition.z, zoomRange[0], zoomRange[1]);
            camOffset = new Vector3(xOff, yOff, zOff);
        }
    }

    private class ForgingState : State
    {
        // Declare static, variables
        private static float[] ZOOM_RANGE = new float[] { -35f, -3.5f };

        ConstructObject currentCO;
        private float[] zoomRange;


        public ForgingState(PlayerController pcn_) : base(pcn_) { }


        public override void Set()
        {
            // Update construct and offset
            pcn.uiController.SetForging(true);
            pcn.playerConstruct.SetForging(true);
            currentCO = pcn.playerConstruct.GetCentreCO();
            ResetOffset();
        }

        public override void Unset()
        {
            // Update construct
            pcn.uiController.SetForging(false);
            pcn.playerConstruct.SetForging(false);
        }


        public override void Update()
        {
            // Run updates
            UpdateCamera();

            // Handle changing state
            if (Input.GetKeyDown("tab")) pcn.SetState(pcn.ingameState);
        }

        private void UpdateCamera()
        {
            // Zoom in / out based on scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                Vector3 local = pcn.camOrbit.transform.localPosition;
                local.z *= (1 - scroll);
                pcn.camOrbit.localPosition = local;
            }

            // Move camera towards centre object
            pcn.camPivot.position = currentCO.transform.position;
            pcn.camOrbit.localPosition = new Vector3(
                Mathf.Lerp(pcn.camOrbit.localPosition.x, 0.0f, pcn.camStats["offsetSpeed"] * Time.deltaTime),
                Mathf.Lerp(pcn.camOrbit.localPosition.y, 0.0f, pcn.camStats["offsetSpeed"] * Time.deltaTime),
                Mathf.Clamp(pcn.camOrbit.localPosition.z, zoomRange[0], zoomRange[1])
            );
        }


        private void ResetOffset()
        {
            // Update max extent and max extent based on construct
            float maxExtent = currentCO.baseWO.GetMaxExtent();
            zoomRange = new float[] { maxExtent * ZOOM_RANGE[0], maxExtent * ZOOM_RANGE[1] };
            pcn.camOrbit.localPosition = new Vector3(
                pcn.camOrbit.localPosition.x,
                pcn.camOrbit.localPosition.y,
                Mathf.Clamp(pcn.camOrbit.localPosition.z, zoomRange[0], zoomRange[1])
            );
        }
    }
}
