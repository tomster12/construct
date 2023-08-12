
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public static PlayerController instance { get; private set; }
    public static float MAX_CAM_REACH { get; private set; } = 100.0f;

    [Header("References")]
    [SerializeReference] private Construct _construct;
    [SerializeField] private UIController uiController;
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

    private IConstruct IConstruct => _construct;
    private IngameState ingameState;
    private ForgingState forgingState;
    private State state;
    private HashSet<IHighlightable>  nearbyHighlightables = new HashSet<IHighlightable>();
    private bool toHighlightNearby = false;
    private bool toHighlightHovered = false;
    private Dictionary<ObjectType, bool> highlightableObjects = new Dictionary<ObjectType, bool>()
    {
        [ObjectType.INTERACTABLE] = true,
        [ObjectType.PartNSTRUCTED] = false,
        [ObjectType.LOOSE] = false
    };

    public Camera cam => _cam;
    public HoverComponentCache currentHover { get; private set; } = new HoverComponentCache();
    public bool isHoveringInteractable => IConstruct.GetICore().CanAttach(currentHover.hoveredIPart);


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


    public Vector3 GetBillboardTarget() => cam.transform.position;

    public Vector3 GetBillboardDirection() => cam.transform.forward;

    public InteractionState GetAttachmentInteractionState()
    {
        if (IConstruct.GetICore() == null) return InteractionState.CLOSED;
        return IConstruct.GetICore().IsTransitioning() ? InteractionState.BLOCKED
            : IConstruct.GetICore().CanAttach(currentHover.hoveredIPart) ? InteractionState.OPEN
            : InteractionState.CLOSED;
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
        // Update hover
        currentHover.Rehover(cam);
    }

    private void UpdateHighlighted()
    {
        // Unhighlight previously hovered if changed
        if (currentHover.hoveredIH != currentHover.prevHoveredIH && currentHover.prevHoveredIH != null) currentHover.prevHoveredIH.SetIsHighlighted(false);

        // Unhighlight / highlight hovered highlightableObjects
        if (currentHover.hoveredIH != null)
        {
            float dist = Vector3.Distance(currentHover.hoveredIH.GetPosition(), IConstruct.GetPosition());
            bool ishighlightableObjects = toHighlightHovered && highlightableObjects[currentHover.hoveredIH.GetObjectType()] && dist < camStats["nearbyRange"];
            if (!ishighlightableObjects) currentHover.hoveredIH.SetIsHighlighted(false);
            else currentHover.hoveredIH.SetIsHighlighted(true);
        }

        // Unhighlight / highlight nearby highlightableObjects
        if (toHighlightNearby)
        {
            // Loop over all world objects of type IHighlightable
            IEnumerable<IHighlightable> allIH = FindObjectsOfType<MonoBehaviour>().OfType<IHighlightable>();
            foreach (IHighlightable currentIH in allIH)
            {
                // Make visible if is in range
                float dist = Vector3.Distance(currentIH.GetPosition(), IConstruct.GetPosition());
                if (dist < camStats["nearbyRange"] && highlightableObjects[currentIH.GetObjectType()])
                {
                    if (! nearbyHighlightables.Contains(currentIH))  nearbyHighlightables.Add(currentIH);
                    currentIH.SetIsNearby(true);
                }
                else
                {
                    if ( nearbyHighlightables.Contains(currentIH))  nearbyHighlightables.Remove(currentIH);
                    currentIH.SetIsNearby(false);
                }
            }
        }

        // Hide all nearby shown hoverables
        else
        {
            foreach (IHighlightable currentIH in  nearbyHighlightables) currentIH.SetIsNearby(false);
             nearbyHighlightables.Clear();
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
        protected PlayerController controller;


        public State(PlayerController controller_) { controller = controller_; }


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
        private IConstructPart centreIPart;
        private float[] zoomRange;
        private Vector3 camOffset;
        private float lastPivotUpdateTime;


        public IngameState(PlayerController controller_) : base(controller_) { }


        public override void Set()
        {
            // Initialize variables
            controller.uiController.SetIngame(true);
            controller.IConstruct.SetState(ConstructState.ACTIVE);
            Cursor.lockState = CursorLockMode.Locked;
            SetCentre(controller.IConstruct.GetCentreIPart());

            // Update UI
            controller.toHighlightNearby = true;
            controller.toHighlightHovered = true;
            controller.highlightableObjects[ObjectType.LOOSE] = true;
            controller.highlightableObjects[ObjectType.PartNSTRUCTED] = false;
        }

        public override void Unset()
        {
            // Update variables
            controller.uiController.SetIngame(false);
            Cursor.lockState = CursorLockMode.None;
        }

        public override void Update()
        {
            // Run updates
            UpdateConstruct();
            UpdateCamera();

            // Handle changing state
            if (Input.GetKeyDown("tab") && controller.IConstruct.GetStateAccessible(ConstructState.FORGING)) controller.SetStateForging();
        }

        public override void FixedUpdate()
        {
            // Run fixed updates
            FixedUpdateConstruct();
            FixedUpdateCamera();
        }


        private void UpdateConstruct()
        {
            // Input movement with "Horizontal" and "Vertical"
            inputMoveDir = Vector3.zero;
            if (Input.GetAxisRaw("Horizontal") != 0.0f || Input.GetAxisRaw("Vertical") != 0.0f)
            {
                Transform camPivot = controller.camPivot;
                Vector3 flatForward = Vector3.ProjectOnPlane(camPivot.forward, Vector3.up).normalized;
                inputMoveDir += camPivot.right * Input.GetAxisRaw("Horizontal");
                inputMoveDir += flatForward * Input.GetAxisRaw("Vertical");
            }

            // Try attach / detach
            if (Input.GetKeyDown("f"))
            {
                if (controller.IConstruct.GetICore().CanAttach(controller.currentHover.hoveredIPart)) controller.IConstruct.GetICore().Attach(controller.currentHover.hoveredIPart);
                else if (controller.IConstruct.GetICore().CanDetach()) controller.IConstruct.GetICore().Detach();
            }
        }

        private void UpdateCamera()
        {
            // Update current Part and offset
            SetCentre(controller.IConstruct.GetCentreIPart());

            // Update pivot position and rotation
            controller.camPivot.Rotate(0, Input.GetAxis("Mouse X") * controller.camStats["rotateSpeed"], 0, Space.World);
            controller.camPivot.Rotate(-Input.GetAxis("Mouse Y") * controller.camStats["rotateSpeed"], 0, 0, Space.Self);

            // Zoom in / out offset z based on scroll wheel
            float zoom = Input.GetAxis("Mouse ScrollWheel");
            if (zoom != 0.0f) camOffset.z = Mathf.Clamp(camOffset.z + zoom * controller.camStats["zoomSpeed"], zoomRange[0], zoomRange[1]);

            // Raycast out towards target offset position to prevent clipping
            Vector3 rayDir = controller.camPivot.rotation * camOffset;
            LayerMask layer = LayerMask.GetMask("Environment");
            if (Physics.Raycast(controller.camPivot.position, rayDir, out RaycastHit hit, camOffset.magnitude, layer))
            {
                Vector3 clampedDir = (hit.point - controller.camPivot.position);
                Vector3 clampedPos = controller.camPivot.position + clampedDir + clampedDir.normalized * -controller.camStats["clipDistance"];
                controller.camOrbit.position = clampedPos;
            }

            // No clipping so lerp offset towards target offset
            else controller.camOrbit.localPosition = Vector3.Lerp(controller.camOrbit.localPosition, camOffset, controller.camStats["offsetSpeed"] * Time.deltaTime);
        }

        private void FixedUpdateConstruct()
        {
            if (!controller.IConstruct.ContainsObject(controller.currentHover.hoveredObject)) controller.IConstruct.AimAtPosition(controller.currentHover.hoveredPos);
            if (inputMoveDir != Vector3.zero) controller.IConstruct.MoveInDirection(inputMoveDir);
        }

        private void FixedUpdateCamera()
        {
            // Calculate delta time and update position
            float deltaTime = Time.time - lastPivotUpdateTime;
            lastPivotUpdateTime = Time.time;
            controller.camPivot.position = Vector3.Lerp(controller.camPivot.position, centreIPart.GetPosition(), controller.camStats["followSpeed"] * deltaTime);
        }

        private void SetCentre(IConstructPart centreIPart_)
        {
            // Setting to new centre Part
            if (centreIPart == centreIPart_) return;
            centreIPart = centreIPart_;

            // Update zoom range and target offset
            float maxExtent = centreIPart.GetObject().maxExtent;
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

        private IConstructPart centreIPart;
        private float[] zoomRange;
        private Vector3 camOffset;
        private bool isDragging;

        private Quaternion prevRot;
        private float prevLocalZ;


        public ForgingState(PlayerController controller_) : base(controller_) { }


        public override void Set()
        {
            // Update IConstruct and offset
            controller.uiController.SetForging(true);
            controller.IConstruct.SetState(ConstructState.FORGING);
            SetCentre(controller.IConstruct.GetCentreIPart());

            // Set camera variables TODO: Look into
            prevRot = controller.camPivot.rotation;
            controller.camPivot.rotation = Quaternion.AngleAxis(180, Vector3.up) * centreIPart.GetForwardRot();
            prevLocalZ = controller.camOrbit.localPosition.z;

            // Update UI
            controller.toHighlightNearby = false;
            controller.toHighlightHovered = true;
            controller.highlightableObjects[ObjectType.LOOSE] = true;
            controller.highlightableObjects[ObjectType.PartNSTRUCTED] = true;
        }

        public override void Unset()
        {
            // Set camera variables
            controller.camPivot.rotation = prevRot;
            controller.camOrbit.localPosition = new Vector3(controller.camOrbit.localPosition.x, controller.camOrbit.localPosition.y, prevLocalZ);
        }

        public override void Update()
        {
            // Run updates
            UpdateCamera();

            // Handle changing state
            if (Input.GetKeyDown("tab")) controller.SetStateIngame();
        }


        private void UpdateCamera()
        {
            // Handle dragging
            if (Input.GetMouseButtonDown(0)) isDragging = true;
            else if (!Input.GetMouseButton(0)) isDragging = false;
            if (isDragging)
            {
                controller.camPivot.Rotate(0, Input.GetAxis("Mouse X") * controller.camStats["rotateSpeed"] * 2.0f, 0, Space.World);
                controller.camPivot.Rotate(-Input.GetAxis("Mouse Y") * controller.camStats["rotateSpeed"] * 2.0f, 0, 0, Space.Self);
            }

            // Zoom in / out based on scroll wheel
            float zoom = Input.GetAxis("Mouse ScrollWheel");
            if (zoom != 0.0f) camOffset.z = Mathf.Clamp(camOffset.z + zoom * controller.camStats["zoomSpeed"], zoomRange[0], zoomRange[1]);

            // Move camera towards centre object
            controller.camPivot.position = Vector3.Lerp(controller.camPivot.position, centreIPart.GetPosition(), controller.camStats["followSpeed"] * Time.deltaTime);
            controller.camOrbit.localPosition = Vector3.Lerp(controller.camOrbit.localPosition, camOffset, controller.camStats["offsetSpeed"] * Time.deltaTime);
        }

        private void SetCentre(IConstructPart centreIPart_)
        {
            // Setting to new centre Part
            if (centreIPart == centreIPart_) return;
            centreIPart = centreIPart_;

            // Update zoom range and target offset
            float maxExtent = centreIPart.GetObject().maxExtent;
            zoomRange = new float[] { maxExtent * ZOOM_RANGE[0], maxExtent * ZOOM_RANGE[1] };
            camOffset = new Vector3(
                0.0f, 0.0f, (zoomRange[0] + zoomRange[1]) / 2.0f
            );
        }
    }


    public class HoverComponentCache
    {
        public Vector3 prevHoveredPos { get; private set; }
        public Transform prevHoveredT { get; private set; }
        public Object prevHoveredObject { get; private set; }
        public ConstructPart prevHoveredPart { get; private set; }
        public IHighlightable prevHoveredIH { get; private set; }
        public IInspectable prevHoveredII { get; private set; }

        public Vector3 hoveredPos { get; private set; }
        public Transform hoveredT { get; private set; }
        public Object hoveredObject { get; private set; }
        public ConstructPart hoveredIPart { get; private set; }
        public IHighlightable hoveredIH { get; private set; }
        public IInspectable hoveredII { get; private set; }


        public void Rehover(Camera cam)
        {
            // Raycast out from camera
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit, PlayerController.MAX_CAM_REACH);

            // Always update pos
            prevHoveredT = hoveredT;
            prevHoveredPos = hoveredPos;
            hoveredT = hit.transform;
            hoveredPos = hit.point;
            if (hoveredT == null) hoveredPos = ray.GetPoint(PlayerController.MAX_CAM_REACH);

            // Update component cache
            if (hoveredT != prevHoveredT)
            {
                prevHoveredObject = hoveredObject;
                prevHoveredPart = hoveredIPart;
                prevHoveredIH = hoveredIH;
                prevHoveredII = hoveredII;

                if (hoveredT != null)
                {
                    hoveredObject = hoveredT.GetComponent<Object>();
                    hoveredIPart = hoveredT.GetComponent<ConstructPart>();
                    hoveredIH = hoveredT.GetComponent<IHighlightable>();
                    hoveredII = hoveredT.GetComponent<IInspectable>();
                }
                else
                {
                    hoveredObject = null;
                    hoveredIPart = null;
                    hoveredIH = null;
                    hoveredII = null;
                }
            }
        }
    }
}
