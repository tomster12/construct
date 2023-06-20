
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;


public enum IHighlightableState { INTERACTABLE, INACTIVE, CONSTRUCTED };

public interface IHighlightable
{
    Vector3 IH_GetPosition();
    bool IH_GetHighlighted();
    IHighlightableState IH_GetState();
    void IH_SetNearby(bool isNearby);
    void IH_SetHighlighted(bool isHighlighted);
}

public interface IInspectable
{
    Sprite II_GetIconSprite();
    string II_GetName();
    string II_GetDescription();
    Element II_GetElement();
    List<string> II_GetAttributes();
    List<string> II_GetModifiers();
    Vector3 II_GetPosition();
    float II_GetMass();
}


public class HoveredData
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

    public void Hover(Ray ray)
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
            } else
            {
                hoveredIH = null;
                hoveredWO = null;
                hoveredCO = null;
            }
        }
    }
}


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
    public Construct construct => _construct;
    public Camera cam => _cam;

    [Header("Config")]
    [SerializeField] private StatList camStats = new StatList()
    {
        ["rotateSpeed"] = 0.5f,
        ["followSpeed"] = 15.0f,
        ["offsetSpeed"] = 8.0f,
        ["zoomSpeed"] = 6.0f,
        ["clipDistance"] = 0.2f,
        ["nearbyRange"] = 10.0f
    };

    private IngameState ingameState;
    private ForgingState forgingState;
    private State state;
    private HashSet<IHighlightable> nearbyIH = new HashSet<IHighlightable>();
    public HoveredData hovered { get; private set; } = new HoveredData();
    private bool showNearby = false;
    private bool showHighlighted = false;
    private Dictionary<IHighlightableState, bool> highlightable = new Dictionary<IHighlightableState, bool>()
    {
        [IHighlightableState.INTERACTABLE] = true,
        [IHighlightableState.CONSTRUCTED] = false,
        [IHighlightableState.INACTIVE] = false
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
        state.OnUpdate();
    }

    private void UpdateHovered()
    {
        // Raycast out from the camera and detect hitting new transform
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        hovered.Hover(ray);
    }

    private void UpdateHighlighted()
    {
        // Unhighlight previously hovered
        if (hovered.hoveredIH != hovered.prevHoveredIH && hovered.prevHoveredIH != null) hovered.prevHoveredIH.IH_SetHighlighted(false);

        // Unhighlight / highlight current hovered
        if (hovered.hoveredIH != null)
        {
            float dist = Vector3.Distance(hovered.hoveredIH.IH_GetPosition(), construct.GetCentrePosition());
            bool isHighlightable = showHighlighted && highlightable[hovered.hoveredIH.IH_GetState()] && dist < camStats["nearbyRange"];
            if (!isHighlightable) hovered.hoveredIH.IH_SetHighlighted(false);
            else hovered.hoveredIH.IH_SetHighlighted(true);
        }

        // Unhighlight / highlight nearby highlightable
        if (showNearby)
        {
            // Loop over all world objects of type IHighlightable
            IEnumerable<IHighlightable> allIH = FindObjectsOfType<MonoBehaviour>().OfType<IHighlightable>();
            foreach (IHighlightable currentIH in allIH)
            {
                // Make visible if is in range
                float dist = Vector3.Distance(currentIH.IH_GetPosition(), construct.GetCentrePosition());
                if (dist < camStats["nearbyRange"] && highlightable[currentIH.IH_GetState()])
                {
                    if (!nearbyIH.Contains(currentIH)) nearbyIH.Add(currentIH);
                    currentIH.IH_SetNearby(true);
                }
                else
                {
                    if (nearbyIH.Contains(currentIH)) nearbyIH.Remove(currentIH);
                    currentIH.IH_SetNearby(false);
                }
            }
        }

        // Hide all nearby shown hoverables
        else
        {
            foreach (IHighlightable currentIH in nearbyIH) currentIH.IH_SetNearby(false);
            nearbyIH.Clear();
        }
    }

    private void FixedUpdate()
    {
        // Run fixed updates
        state.OnFixedUpdate();
    }

    private void LateUpdate()
    {
        // Run late updates
        state.OnLateUpdate();
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

        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate() { }
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
            pcn.showNearby = true;
            pcn.showHighlighted = true;
            pcn.highlightable[IHighlightableState.INACTIVE] = true;
        }

        public override void Unset()
        {
            // Update variables
            pcn.uiController.SetIngame(false);
            Cursor.lockState = CursorLockMode.None;
        }


        public override void OnUpdate()
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
            pcn.construct.skills.TryUseAll();

            // Try attach / detach
            if (Input.GetKeyDown("f"))
            {
                if (pcn.construct.core.canAttach(pcn.hovered.hoveredCO)) pcn.construct.core.Attach(pcn.hovered.hoveredCO);
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


        public override void OnFixedUpdate()
        {
            // Run fixed updates
            FixedUpdateConstruct();
            FixedUpdateCamera();
        }

        private void FixedUpdateConstruct()
        {
            if (!pcn.construct.GetContainsWO(pcn.hovered.hoveredWO)) pcn.construct.AimAtPosition(pcn.hovered.hoveredPos);
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
            float maxExtent = centreCO.baseWO.GetMaxExtent();
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
        // Declare static, variables
        private static float[] ZOOM_RANGE = new float[] { -35f, -3.5f };

        ConstructObject centreCO;
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
            pcn.showNearby = false;
            pcn.showHighlighted = true;
            pcn.highlightable[IHighlightableState.INACTIVE] = true;
            pcn.highlightable[IHighlightableState.CONSTRUCTED] = true;
        }

        public override void Unset()
        {
            // Set camera variables
            pcn.camPivot.rotation = prevRot;
            pcn.camOrbit.localPosition = new Vector3(pcn.camOrbit.localPosition.x, pcn.camOrbit.localPosition.y, prevLocalZ);
        }


        public override void OnUpdate()
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
            float maxExtent = centreCO.baseWO.GetMaxExtent();
            zoomRange = new float[] { maxExtent * ZOOM_RANGE[0], maxExtent * ZOOM_RANGE[1] };
            camOffset = new Vector3(
                0.0f, 0.0f, (zoomRange[0] + zoomRange[1]) / 2.0f
            );
        }
    }
}
