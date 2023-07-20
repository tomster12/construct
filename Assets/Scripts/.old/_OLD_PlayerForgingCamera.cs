
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerForgingCamera : MonoBehaviour
{

    // Declare references
    private PlayerForgingController pcl;
    [SerializeField] private GameObject uiParent;
    [SerializeField] private Transform camPivot;
    private Transform camWrapper;
    private Camera camMain;

    [SerializeField]
    private StatList stats = new StatList()
    {
        ["rotateSpeed"] = 4.0f,
        ["offsetSpeed"] = 4.0f
    };
    private Transform followTfm;
    private Vector3 followOffset;
    private Quaternion targetRotation;


    private void Awake()
    {
        // Initialize variables
        pcl = GetComponent<PlayerForgingController>();
        camWrapper = camPivot.GetChild(0).gameObject.transform;
        camMain = camWrapper.GetComponentInChildren<Camera>();
    }


    private void Update()
    {
        // Lerp position towards target
        camWrapper.localPosition = Vector3.Lerp(camWrapper.localPosition, followOffset, stats["offsetSpeed"] * Time.deltaTime);
        camPivot.position = Vector3.Lerp(camPivot.position, followTfm.position, stats["offsetSpeed"] * Time.deltaTime);
        camPivot.rotation = Quaternion.Lerp(camPivot.rotation, targetRotation, stats["rotateSpeed"] * Time.deltaTime);
    }


    public void setActive(bool active)
    {
        // Update enabled
        this.enabled = active;
        uiParent.SetActive(active);

        if (active)
        {
            // Update camera positioning
            followTfm = pcl.mainConstruct.mainOrbWJ.transform;
            followOffset = Vector3.back * pcl.mainConstruct.mainOrbWJ.maxExtent * 10.0f;
            targetRotation = Quaternion.Euler(0.0f, followTfm.rotation.eulerAngles.y, 0.0f);

            // Unlock camera
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
