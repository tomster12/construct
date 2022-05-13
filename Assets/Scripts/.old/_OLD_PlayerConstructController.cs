
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerConstructController : MonoBehaviour
{

    // #region - Setup

    // Declare references, variables
    [HeaderAttribute("Construct Config")]
    [SerializeField] private WorldObject initialCoreWJ;
    [SerializeField] private Transform objectContainer;
    [SerializeField] DataViewer dataViewer;
    public PlayerConstructCamera pcam { get; private set; }

    [HeaderAttribute("Main Config")]
    [SerializeField]
    private StatList stats = new StatList()
    {
        ["MovementStrength"] = 1.0f
    };
    public Construct controlledConstruct { get; private set; }
    private Vector3 inputMoveDir;


    private void Awake()
    {
        // Initialize references
        pcam = GetComponent<PlayerConstructCamera>();
    }


    private void Start()
    {
        // Initialize construct
        initConstruct(initialCoreWJ);
    }


    private void initConstruct(WorldObject coreWJ)
    {
        // Initialize a construct on the given core WJ
        GameObject constructGO = new GameObject();
        Construct construct = constructGO.AddComponent<Construct>();
        constructGO.transform.parent = objectContainer;
        constructGO.name = "Player Construct Container";
        construct.initConstruct(coreWJ);

        // Set as controlled and followed
        controlledConstruct = construct;
        pcam.setFollow(controlledConstruct);

        // Set as active
        setActive(true);
    }

    // #endregion


    // #region - Main

    private void Update()
    {
        // Call updates
        handleInput();
    }


    private void FixedUpdate()
    {
        // Call fixed updates
        handleInputFixed();
    }


    private void handleInput()
    {
        // Ensure controlling a WJ
        if (controlledConstruct == null) return;

        // [inputMoveDir]: "Horizontal" and "Vertical"
        inputMoveDir = Vector3.zero;
        if ((Input.GetAxisRaw("Horizontal") != 0.0f)
        || (Input.GetAxisRaw("Vertical") != 0.0f))
        {
            Transform camTransform = pcam.getCamCentre();
            Vector3 flatForward = Vector3.ProjectOnPlane(camTransform.forward, Vector3.up).normalized;
            inputMoveDir += camTransform.right * Input.GetAxisRaw("Horizontal");
            inputMoveDir += flatForward * Input.GetAxisRaw("Vertical");
        }

        // [Detach control]: RMB
        if (Input.GetMouseButtonDown(1)) controlledConstruct.detachCore();

        // [Highlight]: Hover
        if (pcam.aimedWJ != null)
        {
            if (controlledConstruct.canInteract(pcam.aimedWJ))
            {
                if (!pcam.aimedWJ.isHighlighted) pcam.aimedWJ.isHighlighted = true;
                dataViewer.setActive(true);
                dataViewer.setWorldObject(pcam.aimedWJ);
            }

            // [Unhighlight]: Unhover
        }
        else if (pcam.prevAimedWJ != null)
        {
            if (pcam.prevAimedWJ.isHighlighted) pcam.prevAimedWJ.isHighlighted = false;
            dataViewer.setActive(false);
        }

        // [Interact]: LMB
        if (Input.GetMouseButtonDown(0))
        {
            controlledConstruct.interact(pcam.aimedWJ, pcam.aimedPos);
            if (pcam.aimedWJ != null) pcam.aimedWJ.isHighlighted = false;
            dataViewer.setActive(false);
        }
    }


    private void handleInputFixed()
    {
        // Ensure controlling a WJ
        if (controlledConstruct == null) return;

        // Aim at hovered position
        if (pcam.aimedPos != null && !controlledConstruct.getContainsWJ(pcam.aimedWJ))
        {
            controlledConstruct.aimAtPosition(pcam.aimedPos, stats["MovementStrength"]);
        }

        // Move in Movement direction
        if (inputMoveDir != Vector3.zero)
        {
            controlledConstruct.moveInDirection(inputMoveDir, stats["MovementStrength"]);
        }
    }


    public void setActive(bool active)
    {
        // Update enabled
        this.enabled = active;

        // Update whether active
        if (controlledConstruct != null)
        {
            controlledConstruct.setActive(active);
        }

        if (!active)
        {
            // Unhover any hovered objects
            if (pcam.aimedWJ != null)
            {
                pcam.aimedWJ.isHighlighted = false;
                dataViewer.setActive(false);
            }
        }

        // Update camera
        pcam.setActive(active);
    }

    // #endregion
}
