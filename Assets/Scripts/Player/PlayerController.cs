
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    // Declare static, references, variables
    public static PlayerController instance { get; private set; }

    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private Construct playerConstruct;

    private Vector3 inputMoveDir;


    private void Awake()
    {
        // Handle singleton
        if (instance != null) return;
        instance = this;
    }


    private void Update()
    {
        UpdateInput();
    }

    private void UpdateInput() {
        // [inputMoveDir]: "Horizontal" and "Vertical"
        inputMoveDir = Vector3.zero;
        if ((Input.GetAxisRaw("Horizontal") != 0.0f)
        || (Input.GetAxisRaw("Vertical") != 0.0f))
        {
            Transform camPivot = playerCamera.GetPivot();
            Vector3 flatForward = Vector3.ProjectOnPlane(camPivot.forward, Vector3.up).normalized;
            inputMoveDir += camPivot.right * Input.GetAxisRaw("Horizontal");
            inputMoveDir += flatForward * Input.GetAxisRaw("Vertical");
        }

        // [Skills]: Send skill controls
        if (Input.GetMouseButtonDown(0)) playerConstruct.skills.Use("l");
        if (Input.GetKeyDown("1")) playerConstruct.skills.Use("1");
        if (Input.GetKeyDown("2")) playerConstruct.skills.Use("2");
        if (Input.GetKeyDown("3")) playerConstruct.skills.Use("3");
        if (Input.GetKeyDown("4")) playerConstruct.skills.Use("4");
        if (Input.GetKeyDown("f")) playerConstruct.skills.Use("f");
    }


    private void FixedUpdate()
    {
        FixedUpdateInput();
    }

    private void FixedUpdateInput()
    {
        // Aim and move construct
        if (!playerConstruct.GetContainsWO(playerCamera.aimedWO))
            playerConstruct.AimAtPosition(playerCamera.aimedPos);
        if (inputMoveDir != Vector3.zero) playerConstruct.MoveInDirection(inputMoveDir);
    }
}
