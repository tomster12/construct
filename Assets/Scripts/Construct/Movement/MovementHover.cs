
using System.Collections;
using UnityEngine;


public class MovementHover : ConstructCoreMovement
{
    private void Awake()
    {
        ObjectAwake();
        CoreAwake();
    }


    #region ConstructPartMovement

    [Header("References")]
    [SerializeField] protected ConstructPart _controlledPart;
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
    public bool IsSprinting() => sprintTimer >= stats["MovementSprintTimer"];
    
    protected IConstructPart controlledIPart => _controlledPart;
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
            hoverPct = stats["HoverForce"] * controlledIPart.GetObject().moveResist * Time.fixedDeltaTime;
        }

        // No valid hit so float downwards
        else
        {
            isGrounded = false;
            groundPosition = Vector3.zero;
            targetY = transform.position.y - 1.0f;
            hoverPct = controlledIPart.GetObject().moveResist * Time.fixedDeltaTime;
        }

        // Lerp height and apply drag
        float lerpedY = Mathf.Lerp(transform.position.y, targetY, hoverPct);
        controlledIPart.GetObject().rb.position = new Vector3(transform.position.x, lerpedY, transform.position.z);
        controlledIPart.GetObject().rb.AddForce(-controlledIPart.GetObject().rb.velocity * stats["MovementDrag"] * Time.fixedDeltaTime, ForceMode.VelocityChange);

        // Update hover SFX volume
        float targetHoverVolume = Mathf.Min(controlledIPart.GetObject().rb.velocity.magnitude / 3f, 1.0f) * 0.25f + 0.15f;
        hoverAudio.volume += (targetHoverVolume - hoverAudio.volume) * 0.08f;

        // Lean with a sprint
        if (isMoving)
        {
            float tiltAmount = IsSprinting() ? stats["SprintTilt"] : stats["WalkTilt"];
            Vector3 axis = -Vector3.Cross(movementDir, Vector3.up).normalized;
            float torqueAmount = stats["TiltForce"] * tiltAmount * controlledIPart.GetObject().moveResist;
            controlledIPart.GetObject().rb.AddTorque(axis * torqueAmount, ForceMode.Acceleration);
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
        float hoverPct = stats["HoverForce"] * controlledIPart.GetObject().moveResist * Time.fixedDeltaTime;
        float targetY = GetHoverHeight(0.0f);
        float lerpedY = Mathf.Lerp(transform.position.y, targetY, hoverPct);
        transform.position = new Vector3(transform.position.x, lerpedY, transform.position.z);

        // Lerp rotation to baseline
        float rotAcc = stats["AimForce"] * controlledIPart.GetObject().moveResist * Time.fixedDeltaTime;
        transform.rotation = Quaternion.Lerp(controlledIPart.GetObject().rb.rotation, controlledIPart.GetForwardRot(), rotAcc);
    }

    public override void MoveInDirection(Vector3 dir)
    {
        if (!isAssigned || !isActive || isPaused || isTransitioning) return;

        // Move in the given direction
        float force = IsSprinting() ? stats["MovementSprintForce"] : stats["MovementWalkForce"];
        float moveAcc = force * controlledIPart.GetObject().moveResist * Time.fixedDeltaTime;
        controlledIPart.GetObject().rb.AddForce(dir.normalized * moveAcc, ForceMode.VelocityChange);
        movementDir = dir;
        isMoving = true;
    }

    public override void AimAtPosition(Vector3 pos)
    {
        if (!isAssigned || !isActive || isPaused || isTransitioning) return;

        // Aim at the given position rotation
        Quaternion newRot = Quaternion.LookRotation((pos - controlledIPart.GetObject().rb.position).normalized, Vector3.up);
        float adjustFactor = stats["AimForce"] * controlledIPart.GetObject().moveResist;
        Quaternion rotTorque = newRot * Quaternion.Inverse(controlledIPart.GetObject().rb.rotation);
        Vector3 rotTorqueVec = new Vector3(rotTorque.x, rotTorque.y, rotTorque.z) * adjustFactor;
        controlledIPart.GetObject().rb.AddTorque(rotTorqueVec, ForceMode.Acceleration);

        //float dampenFactor = 0.8f;
        //Vector3 dampenTorqueVec = -controlledIPart.GetObject().rb.angularVelocity * dampenFactor;
        //controlledIPart.GetObject().rb.AddTorque(dampenTorqueVec, ForceMode.Acceleration);

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
        float targetY = controlledIPart.GetObject().maxExtent * (1.0f + 2.0f * stats["HoverHeight"]);
        targetY += Mathf.Sin(pct) * stats["HoverSinRange"];
        return targetY;
    }

    private float GetMaxHoverHeight() => GetHoverHeight(1.0f);

    private float GetCurrentHoverHeight() => GetHoverHeight(Time.time * stats["HoverSinFrequency"] * (2 * Mathf.PI));

    public override bool SetActive(bool isActive_)
    {
        if (!base.SetActive(isActive_)) return false;

        // Update state
        if (isActive) controlledIPart.SetControlledBy(this);
        else controlledIPart.SetControlledBy(null);

        // Update physics and play sfx
        controlledIPart.GetObject().isLoose = true;
        controlledIPart.GetObject().isFloating = isActive;
        if (!isActive) StartCoroutine(Sfx_FadeOut(hoverAudio, 0.15f));
        return true;
    }

    public override bool SetPaused(bool isPaused_)
    {
        if (!base.SetPaused(isPaused_)) return false;

        // Update physics
        controlledIPart.GetObject().isLoose = !isPaused;
        controlledIPart.GetObject().isFloating = true;
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
        attachmentShape = gameObject.AddComponent<CoreAttachmentShapeHover>();
    }


    protected override IEnumerator IEAttachImpl(IConstructPart IPart)
    {
        // Turn off physics / colliders, update state
        controlledICore.GetObject().isFloating = true;
        controlledICore.GetObject().isLoose = false;
        controlledICore.GetObject().isColliding = false;
        if (coreChargeSFX != null) sfxAudio.PlayOneShot(coreChargeSFX);

        // Move backwards, start spinning and point at IPart
        Vector3 targetPos = PlayerController.instance.currentHover.hoveredPos;
        Coroutine moveBackwardsCR = StartCoroutine(IEAttachMoveBackwards(IPart, targetPos));
        Coroutine lookAtCR = StartCoroutine(IEAttachLookAt(IPart, targetPos));
        yield return moveBackwardsCR;
        yield return lookAtCR;

        // Jab forwards into IPart
        Coroutine jabIntoCR = StartCoroutine(IEAttachJabInto(IPart, targetPos));
        yield return jabIntoCR;

        // Update variables, Play VFX (chromatic aberration / camera shake) and play SFX
        StartCoroutine(CameraEffects.instance.Vfx_Shake(0.15f, 0.08f));
        StartCoroutine(CameraEffects.instance.Vfx_Chromatic(0.4f, 0.65f));
        if (coreAttachSFX != null) sfxAudio.PlayOneShot(coreAttachSFX);

        // Create core attachment shape
        attachmentShape.SetAttachingICore(controlledICore);
        attachmentShape.SetAttachedIPart(IPart);
    }

    private IEnumerator IEAttachMoveBackwards(IConstructPart IPart, Vector3 targetPos)
    {
        // Initialize variables
        Vector3 rawOffset = Quaternion.Inverse(IPart.GetTransform().rotation) * (targetPos - IPart.GetTransform().position);
        float startDist = (targetPos - controlledICore.GetTransform().position).magnitude;
        Vector3 dir, start, end;

        // Move towards a point which is start + 1.0 distance away
        for (float t = 0, movePct; t < 0.65f;)
        {
            Vector3 newTargetPos = IPart.GetTransform().position + IPart.GetTransform().rotation * rawOffset;
            dir = newTargetPos - controlledICore.GetTransform().position;
            start = newTargetPos + -dir.normalized * startDist;
            end = start + -dir.normalized * 1.0f;

            movePct = Util.Easing.EaseOutSine(Mathf.Min(t, 0.65f) / 0.65f);
            controlledICore.GetObject().transform.position = Vector3.Lerp(start, end, movePct);

            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator IEAttachLookAt(IConstructPart IPart, Vector3 targetPos)
    {
        // Initialize variables
        Vector3 rawOffset = Quaternion.Inverse(IPart.GetTransform().rotation) * (targetPos - IPart.GetTransform().position);
        Vector3 dir, startUp = controlledICore.GetObject().transform.up;

        // Lerp rotate local y towards IPart, lerp rotate around local y
        for (float t = 0, aimPct, spinPct; t < 0.85f;)
        {
            Vector3 newTargetPos = IPart.GetTransform().position + IPart.GetTransform().rotation * rawOffset;
            dir = newTargetPos - controlledICore.GetTransform().position;

            aimPct = Util.Easing.EaseOutSine(Mathf.Min(t / 0.65f, 1.0f));
            spinPct = Util.Easing.EaseInSine(Mathf.Min(t / 0.85f, 1.0f));
            controlledICore.GetObject().transform.up = Vector3.Lerp(startUp, dir, aimPct);
            controlledICore.GetObject().transform.rotation *= Quaternion.AngleAxis(360 * spinPct, Vector3.up);

            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator IEAttachJabInto(IConstructPart IPart, Vector3 targetPos)
    {
        // Initialize variables
        Vector3 rawOffset = Quaternion.Inverse(IPart.GetTransform().rotation) * (targetPos - IPart.GetTransform().position);
        Vector3 dir;
        float speed;

        // Raycast then move towards IPart
        while (true)
        {
            Vector3 newTargetPos = IPart.GetTransform().position + IPart.GetTransform().rotation * rawOffset;
            dir = newTargetPos - controlledICore.GetTransform().position;

            speed = 12.0f * Time.deltaTime;
            bool reached = dir.magnitude < speed;
            controlledICore.GetObject().transform.position += dir.normalized * Mathf.Min(dir.magnitude, speed);

            if (reached) break;
            yield return null;
        }
    }

    protected override IEnumerator IEDetachImpl()
    {
        // Detach but without control
        Vector3 popDir = (controlledICore.GetPosition() - controlledICore.GetAttachmentShape().attachedIPart.GetPosition()).normalized;
        controlledICore.GetObject().isFloating = false;
        controlledICore.GetObject().isLoose = true;
        controlledICore.GetObject().isColliding = true;

        // Apply popping force and torque and wait 0.5s
        float prevDrag = controlledICore.GetObject().rb.angularDrag;
        controlledICore.GetObject().rb.angularDrag = 0.0f;
        controlledICore.GetObject().rb.AddForce(popDir * 2.5f, ForceMode.VelocityChange);
        controlledICore.GetObject().rb.AddTorque(controlledICore.GetTransform().right * 15.0f, ForceMode.VelocityChange); // FIX
        yield return new WaitForSeconds(0.5f);
        controlledICore.GetObject().rb.angularDrag = prevDrag;
    }

    #endregion
}
