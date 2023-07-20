
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public static PlayerController instance { get; private set; }
    public static float MAX_CAM_REACH { get; private set; } = 100.0f;

    [Header("References")]
    [SerializeField] private UIController uiController;
    [SerializeField] private Construct _construct;
    [SerializeField] private Transform camPivot;
    [SerializeField] private Transform camOrbit;
    [SerializeField] private Camera _cam;

    [Header("Config")]
    [SerializeField]
    private StatList camStats = new StatList()
    {
        ["rotateSpeed"] = 0.5f,
        ["followSpeed"] = 15.0f,
        ["offsetSpeed"] = 8.0f,
        ["zoomSpeed"] = 6.0f,
        ["clipDistance"] = 0.2f,
        ["nearbyRange"] = 10.0f
    };

    public Construct construct => _construct;
    public Camera cam => _cam;
    public HoverComponentCache currentHover { get; private set; } = new HoverComponentCache();

    private IngameState ingameState;
    private ForgingState forgingState;
    private State state;
    private HashSet<IHighlightable> nearbyIH = new HashSet<IHighlightable>();
    private bool toHighlightNearby = false;
    private bool toHighlightHovered = false;
    private Dictionary<ObjectType, bool> highlightable = new Dictionary<ObjectType, bool>()
    {
        [ObjectType.INTERACTABLE] = true,
        [ObjectType.CONSTRUCTED_CO] = false,
        [ObjectType.LOOSE_CO] = false
    };


    private void Awake()
    {
        // Handle singleton
        if (instance != null) return;
        instance = this;

        // Initialize states
        ingameState = new IngameState(this);
        forgingState = new ForgingState(this);
    }

    private void Start()
    {
        SetStateIngame();
    }


    private void Update()
    {
        // Run updates
        UpdateHovered();
        UpdateHighlighted();
        state.Update();
    }

    private void UpdateHovered()
    {
        // Raycast out from the camera and detect hitting new transform
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        currentHover.Rehover(ray);
    }

    private void UpdateHighlighted()
    {
        // Unhighlight previously hovered
        if (currentHover.hoveredIH != currentHover.prevHoveredIH && currentHover.prevHoveredIH != null) currentHover.prevHoveredIH.IHSetHighlighted(false);

        // Unhighlight / highlight hovered highlightable
        if (currentHover.hoveredIH != null)
        {
            float dist = Vector3.Distance(currentHover.hoveredIH.IHGetPosition(), construct.GetCentrePosition());
            bool isHighlightable = toHighlightHovered && highlightable[currentHover.hoveredIH.IHGetObjectType()] && dist < camStats["nearbyRange"];
            if (!isHighlightable) currentHover.hoveredIH.IHSetHighlighted(false);
            else currentHover.hoveredIH.IHSetHighlighted(true);
        }

        // Unhighlight / highlight nearby highlightable
        if (toHighlightNearby)
        {
            // Loop over all world objects of type IHighlightable
            IEnumerable<IHighlightable> allIH = FindObjectsOfType<MonoBehaviour>().OfType<IHighlightable>();
            foreach (IHighlightable currentIH in allIH)
            {
                // Make visible if is in range
                float dist = Vector3.Distance(currentIH.IHGetPosition(), construct.GetCentrePosition());
                if (dist < camStats["nearbyRange"] && highlightable[currentIH.IHGetObjectType()])
                {
                    if (!nearbyIH.Contains(currentIH)) nearbyIH.Add(currentIH);
                    currentIH.IHSetNearby(true);
                }
                else
                {
                    if (nearbyIH.Contains(currentIH)) nearbyIH.Remove(currentIH);
                    currentIH.IHSetNearby(false);
                }
            }
        }

        // Hide all nearby shown hoverables
        else
        {
            foreach (IHighlightable currentIH in nearbyIH) currentIH.IHSetNearby(false);
            nearbyIH.Clear();
        }
    }

    private void FixedUpdate()
    {
        // Run fixed updates
        state.FixedUpdate();
    }

    private void LateUpdate()
    {
        // Run late updates
        state.LateUpdate();
    }


    public Vector3 GetBillboardTarget() => cam.transform.position;

    public Vector3 GetBillboardDirection() => cam.transform.forward;

    private void SetState(State state_)
    {
        // Update state to new state
        if (state == state_) return;
        if (state != null) state.Unset();
        state = state_;
        state.Set();
    }

    private void SetStateIngame() => SetState(ingameState);

    private void SetStateForging() => SetState(forgingState);


    public abstract class State
    {
        protected PlayerController pcn;


        public State(PlayerController pcn_) { pcn = pcn_; }


        public virtual void Set() { }
        public virtual void Unset() { }


        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate() { }
    }

    private class IngameState : State
    {
        private static float[] ZOOM_RANGE = new float[] { -35f, -3.5f };

        private Vector3 inputMoveDir;
        private ConstructObject centreCO;
        private float[] zoomRange;
        private Vector3 camOffset;
        private float lastPivotUpdateTime;


        public IngameState(PlayerController pcn_) : base(pcn_) { }


        public override void Set()
        {
            // Initialize variables
            pcn.uiController.SetIngame(true);
            pcn.construct.SetState(ConstructState.ACTIVE);
            Cursor.lockState = CursorLockMode.Locked;
            SetCentre(pcn.construct.GetCentreCO());

            // Update UI
            pcn.toHighlightNearby = true;
            pcn.toHighlightHovered = true;
            pcn.highlightable[ObjectType.LOOSE_CO] = true;
            pcn.highlightable[ObjectType.CONSTRUCTED_CO] = false;
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
            if (Input.GetKeyDown("tab") && pcn.construct.GetStateAccessible(ConstructState.FORGING)) pcn.SetStateForging();
        }

        private void UpdateConstruct()
        {
            // Input movement with "Horizontal" and "Vertical"
            inputMoveDir = Vector3.zero;
            if (Input.GetAxisRaw("Horizontal") != 0.0f || Input.GetAxisRaw("Vertical") != 0.0f)
            {
                Transform camPivot = pcn.camPivot;
                Vector3 flatForward = Vector3.ProjectOnPlane(camPivot.forward, Vector3.up).normalized;
                inputMoveDir += camPivot.right * Input.GetAxisRaw("Horizontal");
                inputMoveDir += flatForward * Input.GetAxisRaw("Vertical");
            }

            // Send ability skill controls
            pcn.construct.skills.UpdateInput();

            // Try attach / detach
            if (Input.GetKeyDown("f"))
            {
                if (pcn.construct.core.canAttach(pcn.currentHover.hoveredCO)) pcn.construct.core.Attach(pcn.currentHover.hoveredCO);
                else if (pcn.construct.core.canDetach) pcn.construct.core.Detach();
            }
        }

        private void UpdateCamera()
        {
            // Update current CO and offset
            SetCentre(pcn.construct.GetCentreCO());

            // Update pivot position and rotation
            pcn.camPivot.Rotate(0, Input.GetAxis("Mouse X") * pcn.camStats["rotateSpeed"], 0, Space.World);
            pcn.camPivot.Rotate(-Input.GetAxis("Mouse Y") * pcn.camStats["rotateSpeed"], 0, 0, Space.Self);

            // Zoom in / out offset z based on scroll wheel
            float zoom = Input.GetAxis("Mouse ScrollWheel");
            if (zoom != 0.0f) camOffset.z = Mathf.Clamp(camOffset.z + zoom * pcn.camStats["zoomSpeed"], zoomRange[0], zoomRange[1]);

            // Raycast out towards target offset position to prevent clipping
            Vector3 rayDir = pcn.camPivot.rotation * camOffset;
            LayerMask layer = LayerMask.GetMask("Environment");
            if (Physics.Raycast(pcn.camPivot.position, rayDir, out RaycastHit hit, camOffset.magnitude, layer))
            {
                Vector3 clampedDir = (hit.point - pcn.camPivot.position);
                Vector3 clampedPos = pcn.camPivot.position + clampedDir + clampedDir.normalized * -pcn.camStats["clipDistance"];
                pcn.camOrbit.position = clampedPos;
            }

            // No clipping so lerp offset towards target offset
            else pcn.camOrbit.localPosition = Vector3.Lerp(pcn.camOrbit.localPosition, camOffset, pcn.camStats["offsetSpeed"] * Time.deltaTime);
        }

        public override void FixedUpdate()
        {
            // Run fixed updates
            FixedUpdateConstruct();
            FixedUpdateCamera();
        }

        private void FixedUpdateConstruct()
        {
            if (!pcn.construct.GetContainsWO(pcn.currentHover.hoveredWO)) pcn.construct.AimAtPosition(pcn.currentHover.hoveredPos);
            if (inputMoveDir != Vector3.zero) pcn.construct.MoveInDirection(inputMoveDir);
        }

        private void FixedUpdateCamera()
        {
            // Calculate delta time and update position
            float deltaTime = Time.time - lastPivotUpdateTime;
            lastPivotUpdateTime = Time.time;
            pcn.camPivot.position = Vector3.Lerp(pcn.camPivot.position, centreCO.transform.position, pcn.camStats["followSpeed"] * deltaTime);
        }


        private void SetCentre(ConstructObject centreCO_)
        {
            // Setting to new centre CO
            if (centreCO == centreCO_) return;
            centreCO = centreCO_;

            // Update zoom range and target offset
            float maxExtent = centreCO.baseWO.maxExtent;
            zoomRange = new float[] { maxExtent * ZOOM_RANGE[0], maxExtent * ZOOM_RANGE[1] };
            camOffset = new Vector3(
                Mathf.Max(0.25f, maxExtent + 0.2f),
                Mathf.Max(0.48f, maxExtent + 0.25f),
                (zoomRange[0] + zoomRange[1]) / 2.0f
            );
        }
    }

    private class ForgingState : State
    {
        private static float[] ZOOM_RANGE = new float[] { -35f, -3.5f };

        private ConstructObject centreCO;
        private float[] zoomRange;
        private Vector3 camOffset;
        private bool isDragging;

        private Quaternion prevRot;
        private float prevLocalZ;


        public ForgingState(PlayerController pcn_) : base(pcn_) { }


        public override void Set()
        {
            // Update construct and offset
            pcn.uiController.SetForging(true);
            pcn.construct.SetState(ConstructState.FORGING);
            SetCentre(pcn.construct.GetCentreCO());

            // Set camera variables TODO: Look into
            prevRot = pcn.camPivot.rotation;
            pcn.camPivot.rotation = Quaternion.AngleAxis(180, Vector3.up) * centreCO.GetForwardRot();
            prevLocalZ = pcn.camOrbit.localPosition.z;

            // Update UI
            pcn.toHighlightNearby = false;
            pcn.toHighlightHovered = true;
            pcn.highlightable[ObjectType.LOOSE_CO] = true;
            pcn.highlightable[ObjectType.CONSTRUCTED_CO] = true;
        }

        public override void Unset()
        {
            // Set camera variables
            pcn.camPivot.rotation = prevRot;
            pcn.camOrbit.localPosition = new Vector3(pcn.camOrbit.localPosition.x, pcn.camOrbit.localPosition.y, prevLocalZ);
        }


        public override void Update()
        {
            // Run updates
            UpdateCamera();

            // Handle changing state
            if (Input.GetKeyDown("tab")) pcn.SetStateIngame();
        }

        private void UpdateCamera()
        {
            // Handle dragging
            if (Input.GetMouseButtonDown(0)) isDragging = true;
            else if (!Input.GetMouseButton(0)) isDragging = false;
            if (isDragging)
            {
                pcn.camPivot.Rotate(0, Input.GetAxis("Mouse X") * pcn.camStats["rotateSpeed"] * 2.0f, 0, Space.World);
                pcn.camPivot.Rotate(-Input.GetAxis("Mouse Y") * pcn.camStats["rotateSpeed"] * 2.0f, 0, 0, Space.Self);
            }

            // Zoom in / out based on scroll wheel
            float zoom = Input.GetAxis("Mouse ScrollWheel");
            if (zoom != 0.0f) camOffset.z = Mathf.Clamp(camOffset.z + zoom * pcn.camStats["zoomSpeed"], zoomRange[0], zoomRange[1]);

            // Move camera towards centre object
            pcn.camPivot.position = Vector3.Lerp(pcn.camPivot.position, centreCO.transform.position, pcn.camStats["followSpeed"] * Time.deltaTime);
            pcn.camOrbit.localPosition = Vector3.Lerp(pcn.camOrbit.localPosition, camOffset, pcn.camStats["offsetSpeed"] * Time.deltaTime);
        }


        private void SetCentre(ConstructObject centreCO_)
        {
            // Setting to new centre CO
            if (centreCO == centreCO_) return;
            centreCO = centreCO_;

            // Update zoom range and target offset
            float maxExtent = centreCO.baseWO.maxExtent;
            zoomRange = new float[] { maxExtent * ZOOM_RANGE[0], maxExtent * ZOOM_RANGE[1] };
            camOffset = new Vector3(
                0.0f, 0.0f, (zoomRange[0] + zoomRange[1]) / 2.0f
            );
        }
    }


    public class HoverComponentCache
    {
        public Transform prevHoveredT { get; private set; }
        public Vector3 prevHoveredPos { get; private set; }
        public IHighlightable prevHoveredIH { get; private set; }
        public WorldObject prevHoveredWO { get; private set; }
        public ConstructObject prevHoveredCO { get; private set; }
        public Transform hoveredT { get; private set; }
        public Vector3 hoveredPos { get; private set; }
        public IHighlightable hoveredIH { get; private set; }
        public WorldObject hoveredWO { get; private set; }
        public ConstructObject hoveredCO { get; private set; }


        public void Rehover(Ray ray)
        {
            // Raycast the ray
            Physics.Raycast(ray, out RaycastHit hit, PlayerController.MAX_CAM_REACH);

            // Update with variables
            prevHoveredT = hoveredT;
            prevHoveredPos = hoveredPos;
            hoveredT = hit.transform;
            hoveredPos = hit.point;

            // pos at max reach
            if (hoveredT == null) hoveredPos = ray.GetPoint(PlayerController.MAX_CAM_REACH);

            // Update cache on new object
            if (hoveredT != prevHoveredT)
            {
                prevHoveredIH = hoveredIH;
                prevHoveredWO = hoveredWO;
                prevHoveredCO = hoveredCO;
                if (hoveredT != null)
                {
                    hoveredIH = hoveredT.GetComponent<IHighlightable>();
                    hoveredWO = hoveredT.GetComponent<WorldObject>();
                    hoveredCO = hoveredT.GetComponent<ConstructObject>();
                }
                else
                {
                    hoveredIH = null;
                    hoveredWO = null;
                    hoveredCO = null;
                }
            }
        }
    }
}
