
using UnityEngine;


public class PlayerConstructCamera : MonoBehaviour
{

    // #region - Setup

    // Declare static, references, variables
    private float MAX_REACH = 100.0f;
    private float[] ZOOM_RANGE = new float[] { -35f, -3.5f };

    [SerializeField] private GameObject uiParent;
    [SerializeField] private Transform camPivot;
    private Transform camTarget;
    private Camera camMain;

    [SerializeField]
    private StatList stats = new StatList()
    {
        ["rotateSpeed"] = 0.5f,
        ["offsetSpeed"] = 4.0f
    };
    private Construct followConstruct;
    private Object followPrevWJ;
    private float[] zoomRange;
    private Vector3 camOffset;
    public Vector3 aimedPos { get; private set; }
    public Object prevAimedWJ { get; private set; }
    public Object aimedWJ { get; private set; }


    private void Awake()
    {
        // Initialize references
        camTarget = camPivot.GetChild(0).gameObject.transform;
        camMain = camTarget.GetComponentInChildren<Camera>();
    }


    private void Start()
    {
        // Lock mouse
        Cursor.lockState = CursorLockMode.Locked;
    }

    // #endregion


    // #region - Main

    private void Update()
    {
        // Rotate based on mouse movement
        camPivot.Rotate(0, Input.GetAxis("Mouse X") * stats["rotateSpeed"], 0, Space.World);
        camPivot.Rotate(-Input.GetAxis("Mouse Y") * stats["rotateSpeed"], 0, 0, Space.Self);

        // Zoom in / out based on scroll wheel
        if (zoomRange != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                Vector3 local = camTarget.transform.localPosition;
                local.z *= (1 - scroll);
                camTarget.localPosition = local;
            }
        }

        if (followConstruct != null)
        {
            // Check if followConstruct has changed focus
            if (followConstruct.mainOrbWJ != followPrevWJ)
            {
                updateCamOffset();
                followPrevWJ = followConstruct.mainOrbWJ;
            }

            // Set pivot to target, and lerp target to offset
            camPivot.position = followConstruct.mainOrbWJ.transform.position;
            camTarget.localPosition = new Vector3(
              Mathf.Lerp(camTarget.localPosition.x, camOffset.x, stats["offsetSpeed"] * Time.deltaTime),
              Mathf.Lerp(camTarget.localPosition.y, camOffset.y, stats["offsetSpeed"] * Time.deltaTime),
              Mathf.Clamp(camTarget.localPosition.z, zoomRange[0], zoomRange[1])
            );
        }

        // Raycast mouse to find hovered
        RaycastHit hit;
        Ray ray = camMain.ScreenPointToRay(Input.mousePosition);
        bool hasHit = Physics.Raycast(ray, out hit, MAX_REACH);
        prevAimedWJ = aimedWJ;

        // Hovering something
        if (hasHit)
        {
            aimedPos = hit.point;
            aimedWJ = hit.transform.GetComponent<Object>();

            // Not hovering anything
        }
        else
        {
            aimedPos = ray.GetPoint(MAX_REACH);
            aimedWJ = null;
        }
    }


    public Transform getCamCentre()
    {
        // Returns the camera centre transform
        return camPivot;
    }


    public void setFollow(Construct newFollowConstruct)
    {
        // Set follow transform to object
        followConstruct = newFollowConstruct;
        followPrevWJ = followConstruct.mainOrbWJ;
        updateCamOffset();
    }


    private void updateCamOffset()
    {
        // Set the cameras offset based on a max extent
        float maxExtent = followConstruct.mainOrbWJ.maxExtent;
        zoomRange = new float[] { maxExtent * ZOOM_RANGE[0], maxExtent * ZOOM_RANGE[1] };
        float xOff = maxExtent * 0.8f;
        float yOff = maxExtent * 1.1f;
        float zOff = Mathf.Clamp(camTarget.transform.localPosition.z, zoomRange[0], zoomRange[1]);
        camOffset = new Vector3(xOff, yOff, zOff);
    }


    public void setActive(bool active)
    {
        // Update enabled
        this.enabled = active;
        uiParent.SetActive(active);

        if (active)
        {
            // Reupdate camera offset
            if (followConstruct != null) updateCamOffset();

            // Lock camera
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // #endregion
}
