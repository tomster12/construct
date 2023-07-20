
using System.Collections;
using UnityEngine;


public class MovementHover : ConstructCoreMovement
{
    private void Awake()
    {
        ObjectAwake();
        CoreAwake();
    }


    #region ConstructObjectMovement

    [Header("References")]
    [SerializeField] protected ConstructObject controlledCO;
    [SerializeField] private AudioClip hoverSFX;
    private AudioSource hoverAudio;
    private AudioSource sfxAudio;

    [Header("Config")]
    [SerializeField] protected StatList stats = new StatList()
    {
        ["MovementWalkForce"] = 5.0f,
        ["MovementSprintForce"] = 8.0f,
        ["MovementSprintTimer"] = 1.0f,
        ["WalkTilt"] = 0.12f,
        ["SprintTilt"] = 0.25f,
        ["AimForce"] = 100.0f,
        ["TiltForce"] = 100.0f,
        ["MovementDrag"] = 2.5f,
        ["HoverHeight"] = 2.0f,
        ["HoverSinRange"] = 0.1f,
        ["HoverSinFrequency"] = 0.25f,
        ["HoverForce"] = 3.0f
    };

    public bool isGrounded { get; private set; } = false;
    public bool isMoving { get; private set; } = false;
    public bool isSprinting => sprintTimer >= stats["MovementSprintTimer"];
    
    private Vector3 groundPosition;
    private Vector3 movementDir;
    private float sprintTimer = 0.0f;


    private void ObjectAwake()
    {
        // Initialize references
        hoverAudio = gameObject.AddComponent<AudioSource>();
        sfxAudio = gameObject.AddComponent<AudioSource>();

        // Setup hover SRC
        if (hoverSFX != null)
        {
            hoverAudio.clip = hoverSFX;
            hoverAudio.volume = 0.0f;
            hoverAudio.loop = true;
            hoverAudio.Play();
        }
    }


    private void FixedUpdate()
    {
        if (!isAssigned || !isActive || isTransitioning) return;
        FixedUpdateActive();
        FixedUpdatePaused();
    }

    private void FixedUpdateActive()
    {
        if (isPaused) return;

        // Oscillate above closest reasonable surface
        float targetY, hoverPct;
        LayerMask layer = LayerMask.GetMask("Environment");
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, GetMaxHoverHeight() * 1.5f, layer))
        {
            isGrounded = true;
            groundPosition = hit.point;
            targetY = groundPosition.y + GetCurrentHoverHeight();
            hoverPct = stats["HoverForce"] * controlledCO.baseWO.moveResist * Time.fixedDeltaTime;
        }

        // No valid hit so float downwards
        else
        {
            isGrounded = false;
            groundPosition = Vector3.zero;
            targetY = transform.position.y - 1.0f;
            hoverPct = controlledCO.baseWO.moveResist * Time.fixedDeltaTime;
        }

        // Lerp height and apply drag
        float lerpedY = Mathf.Lerp(transform.position.y, targetY, hoverPct);
        controlledCO.baseWO.rb.position = new Vector3(transform.position.x, lerpedY, transform.position.z);
        controlledCO.baseWO.rb.AddForce(-controlledCO.baseWO.rb.velocity * stats["MovementDrag"] * Time.fixedDeltaTime, ForceMode.VelocityChange);

        // Update hover SFX volume
        float targetHoverVolume = Mathf.Min(controlledCO.baseWO.rb.velocity.magnitude / 3f, 1.0f) * 0.25f + 0.15f;
        hoverAudio.volume += (targetHoverVolume - hoverAudio.volume) * 0.08f;

        // Lean with a sprint
        if (isMoving)
        {
            float tiltAmount = isSprinting ? stats["SprintTilt"] : stats["WalkTilt"];
            Vector3 axis = -Vector3.Cross(movementDir, Vector3.up).normalized;
            float torqueAmount = stats["TiltForce"] * tiltAmount * controlledCO.baseWO.moveResist;
            controlledCO.baseWO.rb.AddTorque(axis * torqueAmount, ForceMode.Acceleration);
        }

        // Handle sprint timer
        if (isMoving) sprintTimer += Time.deltaTime;
        else sprintTimer = 0.0f;
        isMoving = false;
    }

    private void FixedUpdatePaused()
    {
        if (!isPaused) return;

        // Lerp height to baseline
        float hoverPct = stats["HoverForce"] * controlledCO.baseWO.moveResist * Time.fixedDeltaTime;
        float targetY = GetHoverHeight(0.0f);
        float lerpedY = Mathf.Lerp(transform.position.y, targetY, hoverPct);
        transform.position = new Vector3(transform.position.x, lerpedY, transform.position.z);

        // Lerp rotation to baseline
        float rotAcc = stats["AimForce"] * controlledCO.baseWO.moveResist * Time.fixedDeltaTime;
        transform.rotation = Quaternion.Lerp(controlledCO.baseWO.rb.rotation, controlledCO.GetForwardRot(), rotAcc);
    }

    public override void MoveInDirection(Vector3 dir)
    {
        if (!isAssigned || !isActive || isPaused || isTransitioning) return;

        // Move in the given direction
        float force = isSprinting ? stats["MovementSprintForce"] : stats["MovementWalkForce"];
        float moveAcc = force * controlledCO.baseWO.moveResist * Time.fixedDeltaTime;
        controlledCO.baseWO.rb.AddForce(dir.normalized * moveAcc, ForceMode.VelocityChange);
        movementDir = dir;
        isMoving = true;
    }

    public override void AimAtPosition(Vector3 pos)
    {
        if (!isAssigned || !isActive || isPaused || isTransitioning) return;

        // Aim at the given position rotation
        Quaternion newRot = Quaternion.LookRotation((pos - controlledCO.baseWO.rb.position).normalized, Vector3.up);
        float adjustFactor = stats["AimForce"] * controlledCO.baseWO.moveResist;
        Quaternion rotTorque = newRot * Quaternion.Inverse(controlledCO.baseWO.rb.rotation);
        Vector3 rotTorqueVec = new Vector3(rotTorque.x, rotTorque.y, rotTorque.z) * adjustFactor;
        controlledCO.baseWO.rb.AddTorque(rotTorqueVec, ForceMode.Acceleration);

        //float dampenFactor = 0.8f;
        //Vector3 dampenTorqueVec = -controlledCO.baseWO.rb.angularVelocity * dampenFactor;
        //controlledCO.baseWO.rb.AddTorque(dampenTorqueVec, ForceMode.Acceleration);

        //var x = Vector3.Cross(currentDir.normalized, newDir.normalized);
        //float theta = Mathf.Asin(x.magnitude);
        //var w = x.normalized * theta / Time.fixedDeltaTime;
        //var q = transform.rotation * rigidbody.inertiaTensorRotation;
        //var t = q * Vector3.Scale(rigidbody.inertiaTensor, Quaternion.Inverse(q) * w);
        //rigidbody.AddTorque(t - rigidbody.angularVelocity, ForceMode.Impulse);
    }

    private IEnumerator Sfx_FadeOut(AudioSource src, float duration)
    {
        // Fade out source volume
        float startVolume = src.volume;
        for (float t = duration; t > 0.0f;)
        {
            src.volume = startVolume * (t / duration);
            t -= Time.deltaTime;
            if (t < 0.0f) src.volume = 0.0f;
            yield return null;
        }
    }


    private float GetHoverHeight(float pct)
    {
        if (!isAssigned) return 0.0f;

        // Calculate current hover height based on pct
        float targetY = controlledCO.baseWO.maxExtent * (1.0f + 2.0f * stats["HoverHeight"]);
        targetY += Mathf.Sin(pct) * stats["HoverSinRange"];
        return targetY;
    }

    private float GetMaxHoverHeight() => GetHoverHeight(1.0f);

    private float GetCurrentHoverHeight() => GetHoverHeight(Time.time * stats["HoverSinFrequency"] * (2 * Mathf.PI));

    public override bool SetActive(bool isActive_)
    {
        if (!base.SetActive(isActive_)) return false;

        // Update state
        if (isActive) controlledCO.SetControlledBy(this);
        else controlledCO.SetControlledBy(null);

        // Update physics and play sfx
        controlledCO.baseWO.isLoose = true;
        controlledCO.baseWO.isFloating = isActive;
        if (!isActive) StartCoroutine(Sfx_FadeOut(hoverAudio, 0.15f));
        return true;
    }

    public override bool SetPaused(bool isPaused_)
    {
        if (!base.SetPaused(isPaused_)) return false;

        // Update physics
        controlledCO.baseWO.isLoose = !isPaused;
        controlledCO.baseWO.isFloating = true;
        return true;
    }

    #endregion


    #region ConstructCoreMovement

    [Header("References")]
    [SerializeField] private AudioClip coreAttachSFX;
    [SerializeField] private AudioClip coreChargeSFX;


    private void CoreAwake()
    {
        // Initialize shape
        shapeCoreAttachment = gameObject.AddComponent<ShapeHoverAttachment>();
    }


    protected override IEnumerator IE_RunAttach(ConstructObject targetCO)
    {
        // Turn off physics / colliders, update state
        controlledCC.baseWO.isFloating = true;
        controlledCC.baseWO.isLoose = false;
        controlledCC.baseWO.isColliding = false;
        if (coreChargeSFX != null) sfxAudio.PlayOneShot(coreChargeSFX);

        // Move backwards, start spinning and point at targetCO
        Vector3 targetPos = PlayerController.instance.currentHover.hoveredPos;
        Coroutine moveBackwardsCR = StartCoroutine(IE_AttachMoveBackwards(targetCO, targetPos));
        Coroutine lookAtCR = StartCoroutine(IE_AttachLookAt(targetCO, targetPos));
        yield return moveBackwardsCR;
        yield return lookAtCR;

        // Jab forwards into targetCO
        Coroutine jabIntoCR = StartCoroutine(IE_AttachJabInto(targetCO, targetPos));
        yield return jabIntoCR;

        // Update variables, Play VFX (chromatic aberration / camera shake) and play SFX
        StartCoroutine(CameraEffects.instance.Vfx_Shake(0.15f, 0.08f));
        StartCoroutine(CameraEffects.instance.Vfx_Chromatic(0.4f, 0.65f));
        if (coreAttachSFX != null) sfxAudio.PlayOneShot(coreAttachSFX);

        // Create core attachment shape
        shapeCoreAttachment.SetAttachingCC(controlledCC);
        shapeCoreAttachment.SetAttachedCO(targetCO);
    }

    private IEnumerator IE_AttachMoveBackwards(ConstructObject targetCO, Vector3 targetPos)
    {
        // Initialize variables
        Vector3 rawOffset = Quaternion.Inverse(targetCO.transform.rotation) * (targetPos - targetCO.transform.position);
        float startDist = (targetPos - controlledCC.transform.position).magnitude;
        Vector3 dir, start, end;

        // Move towards a point which is start + 1.0 distance away
        for (float t = 0, movePct; t < 0.65f;)
        {
            Vector3 newTargetPos = targetCO.transform.position + targetCO.transform.rotation * rawOffset;
            dir = newTargetPos - controlledCC.transform.position;
            start = newTargetPos + -dir.normalized * startDist;
            end = start + -dir.normalized * 1.0f;

            movePct = Util.Easing.EaseOutSine(Mathf.Min(t, 0.65f) / 0.65f);
            controlledCC.baseWO.transform.position = Vector3.Lerp(start, end, movePct);

            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator IE_AttachLookAt(ConstructObject targetCO, Vector3 targetPos)
    {
        // Initialize variables
        Vector3 rawOffset = Quaternion.Inverse(targetCO.transform.rotation) * (targetPos - targetCO.transform.position);
        Vector3 dir, startUp = controlledCC.baseWO.transform.up;

        // Lerp rotate local y towards targetCO, lerp rotate around local y
        for (float t = 0, aimPct, spinPct; t < 0.85f;)
        {
            Vector3 newTargetPos = targetCO.transform.position + targetCO.transform.rotation * rawOffset;
            dir = newTargetPos - controlledCC.transform.position;

            aimPct = Util.Easing.EaseOutSine(Mathf.Min(t / 0.65f, 1.0f));
            spinPct = Util.Easing.EaseInSine(Mathf.Min(t / 0.85f, 1.0f));
            controlledCC.baseWO.transform.up = Vector3.Lerp(startUp, dir, aimPct);
            controlledCC.baseWO.transform.rotation *= Quaternion.AngleAxis(360 * spinPct, Vector3.up);

            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator IE_AttachJabInto(ConstructObject targetCO, Vector3 targetPos)
    {
        // Initialize variables
        Vector3 rawOffset = Quaternion.Inverse(targetCO.transform.rotation) * (targetPos - targetCO.transform.position);
        Vector3 dir;
        float speed;

        // Raycast then move towards targetCO
        while (true)
        {
            Vector3 newTargetPos = targetCO.transform.position + targetCO.transform.rotation * rawOffset;
            dir = newTargetPos - controlledCC.transform.position;

            speed = 12.0f * Time.deltaTime;
            bool reached = dir.magnitude < speed;
            controlledCC.baseWO.transform.position += dir.normalized * Mathf.Min(dir.magnitude, speed);

            if (reached) break;
            yield return null;
        }
    }

    protected override IEnumerator IE_RunDetach()
    {
        // Detach but without control
        Vector3 popDir = (controlledCC.baseWO.transform.position - controlledCC.shapeCoreAttachment.attachedCO.transform.position).normalized;
        controlledCC.baseWO.isFloating = false;
        controlledCC.baseWO.isLoose = true;
        controlledCC.baseWO.isColliding = true;

        // Apply popping force and torque and wait 0.5s
        float prevDrag = controlledCC.baseWO.rb.angularDrag;
        controlledCC.baseWO.rb.angularDrag = 0.0f;
        controlledCC.baseWO.rb.AddForce(popDir * 2.5f, ForceMode.VelocityChange);
        controlledCC.baseWO.rb.AddTorque(controlledCC.transform.right * 15.0f, ForceMode.VelocityChange); // FIX
        yield return new WaitForSeconds(0.5f);
        controlledCC.baseWO.rb.angularDrag = prevDrag;
    }

    #endregion
}
