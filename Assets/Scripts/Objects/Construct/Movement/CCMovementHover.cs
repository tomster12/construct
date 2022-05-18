
using System.Collections;
using UnityEngine;


public class CCMovementHover : COMovementHover, ICCMovement
{
    // Declare references, variables
    [Header("References")]
    [SerializeField] private CameraEffects camFX;
    [SerializeField] private AudioClip coreAttachSFX;
    [SerializeField] private AudioClip coreChargeSFX;
    private AudioSource sfxAudio;
    private ConstructCore baseCC;


    protected override void Awake()
    {
        base.Awake();

        // Initialize references
        SetConstructCore(GetComponent<ConstructCore>());
        sfxAudio = gameObject.AddComponent<AudioSource>();
    }


    public void AttachCore(ICCMovementController stateController, ConstructObject targetCO, Vector3 targetPos) { StartCoroutine(AttachCoreIE(stateController, targetCO, targetPos)); }

    public IEnumerator AttachCoreIE(ICCMovementController stateController, ConstructObject targetCO, Vector3 targetPos)
    {
        if (stateController.GetState() == CoreState.Detached)
        {
            // Turn off physics / colliders, update state
            stateController.SetState(CoreState.Attaching);
            baseCC.baseWO.rb.useGravity = false;
            baseCC.baseWO.rb.isKinematic = true;
            baseCC.baseWO.cl.enabled = false;
            if (coreChargeSFX != null) sfxAudio.PlayOneShot(coreChargeSFX);
            overrideControl = true;

            // Move backwards, start spinning and point at targetCO
            Coroutine moveBackwardsCR = StartCoroutine(_AttachCoreIEMoveBackwards(targetCO, targetPos));
            Coroutine lookAtCR = StartCoroutine(_AttachCoreIELookAt(targetCO, targetPos));
            yield return moveBackwardsCR;
            yield return lookAtCR;

            // Jab forwards into targetCO
            Coroutine jabIntoCR = StartCoroutine(_AttachCoreIEJabInto(targetCO, targetPos));
            yield return jabIntoCR;

            // Play VFX (chromatic aberration / camera shake) and play SFX
            StartCoroutine(camFX.Vfx_Shake(0.15f, 0.05f));
            StartCoroutine(camFX.Vfx_Chromatic(0.4f, 0.65f));
            if (coreAttachSFX != null) sfxAudio.PlayOneShot(coreAttachSFX);

            // Update parent object, pass over control, update state
            stateController.SetState(CoreState.Attached);
            targetCO.SetConstruct(baseCC.construct);
            targetCO.SetControlled(true);
            baseCC.SetControlled(false);
            baseCC.baseWO.transform.parent = targetCO.transform;
            baseCC.baseWO.rb.useGravity = false;
            baseCC.baseWO.rb.isKinematic = true;
            stateController.SetAttachedCO(targetCO);
            overrideControl = false;
        }
    }

    private IEnumerator _AttachCoreIEMoveBackwards(ConstructObject targetCO, Vector3 targetPos)
    {
        // Initialize variables
        Vector3 rawOffset = Quaternion.Inverse(targetCO.transform.rotation) * (targetPos - targetCO.transform.position);
        float startDist = (targetPos - baseCC.transform.position).magnitude;
        Vector3 dir, start, end;

        // Move towards a point which is start + 1.0 distance away
        for (float t = 0, movePct; t < 0.65f;)
        {
            Vector3 newTargetPos = targetCO.transform.position + targetCO.transform.rotation * rawOffset;
            dir = newTargetPos - baseCC.transform.position;
            start = newTargetPos + -dir.normalized * startDist;
            end = start + -dir.normalized * 1.0f;

            movePct = Easing.EaseOutSine(Mathf.Min(t, 0.65f) / 0.65f);
            baseCC.baseWO.transform.position = Vector3.Lerp(start, end, movePct);

            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator _AttachCoreIELookAt(ConstructObject targetCO, Vector3 targetPos)
    {
        // Initialize variables
        Vector3 rawOffset = Quaternion.Inverse(targetCO.transform.rotation) * (targetPos - targetCO.transform.position);
        Vector3 dir, startUp = baseCC.baseWO.transform.up;

        // Lerp rotate local y towards targetCO, lerp rotate around local y
        for (float t = 0, aimPct, spinPct; t < 0.85f;)
        {
            Vector3 newTargetPos = targetCO.transform.position + targetCO.transform.rotation * rawOffset;
            dir = newTargetPos - baseCC.transform.position;

            aimPct = Easing.EaseOutSine(Mathf.Min(t / 0.65f, 1.0f));
            spinPct = Easing.EaseInSine(Mathf.Min(t / 0.85f, 1.0f));
            baseCC.baseWO.transform.up = Vector3.Lerp(startUp, dir, aimPct);
            baseCC.baseWO.transform.rotation *= Quaternion.AngleAxis(360 * spinPct, Vector3.up);

            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator _AttachCoreIEJabInto(ConstructObject targetCO, Vector3 targetPos)
    {
        // Initialize variables
        Vector3 rawOffset = Quaternion.Inverse(targetCO.transform.rotation) * (targetPos - targetCO.transform.position);
        Vector3 dir;
        float speed;

        // Raycast then move towards targetCO
        while (true)
        {
            Vector3 newTargetPos = targetCO.transform.position + targetCO.transform.rotation * rawOffset;
            dir = newTargetPos - baseCC.transform.position;

            speed = 12.0f * Time.deltaTime;
            bool reached = dir.magnitude < speed;
            baseCC.baseWO.transform.position += dir.normalized * Mathf.Min(dir.magnitude, speed);

            if (reached) break;
            yield return null;
        }
    }


    public void DetachCore(ICCMovementController stateController) { StartCoroutine(DetachCoreIE(stateController)); }

    public IEnumerator DetachCoreIE(ICCMovementController stateController)
    {
        Vector3 popDir = (baseCC.baseWO.transform.position - stateController.GetAttachedCO().transform.position).normalized;

        // Detach but without control
        stateController.SetState(CoreState.Detaching);
        baseCC.baseWO.rb.isKinematic = false;
        baseCC.baseWO.rb.useGravity = true;
        baseCC.baseWO.cl.enabled = true;

        // Apply popping force and torque and wait 0.5s
        float prevDrag = baseCC.baseWO.rb.angularDrag;
        baseCC.baseWO.rb.angularDrag = 0.0f;
        baseCC.baseWO.rb.AddForce(popDir * 2.5f, ForceMode.VelocityChange);
        baseCC.baseWO.rb.AddTorque(transform.right * 15.0f, ForceMode.VelocityChange); // FIX
        yield return new WaitForSeconds(0.5f);

        // Reactive moveset and angular drag
        stateController.SetState(CoreState.Detached);
        stateController.GetAttachedCO().SetControlled(false);
        stateController.GetAttachedCO().SetConstruct(null);
        baseCC.SetControlled(true);
        baseCC.baseWO.rb.angularDrag = prevDrag;
        stateController.SetAttachedCO(null);
    }


    private void SetConstructCore(ConstructCore baseCC_) { SetConstructObject(baseCC_); baseCC = baseCC_; }
}
