
using System.Collections;
using UnityEngine;


public class MovementHover : ConstructCoreMovement
{
    #region Object Movement

    [Header("References")]
    [SerializeField] private AudioClip hoverSFX;
    private AudioSource hoverAudio;
    private AudioSource sfxAudio;

    [Header("Config")]
    [SerializeField] protected StatList stats = new StatList()
    {
        ["MovementForce"] = 7.0f,
        ["AimForce"] = 4.0f,
        ["MovementDrag"] = 2.5f,
        ["HoverHeight"] = 2.0f,
        ["HoverSinRange"] = 0.2f,
        ["HoverSinFrequency"] = 0.4f,
        ["HoverForce"] = 3.0f
    };

    private Vector3 groundPosition;
    public bool isGrounded { get; private set; } = false;


    private void Awake()
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
        if (!isConstructed || !isActive || isTransitioning) return;
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
        controlledCO.baseWO.rb.velocity -= (controlledCO.baseWO.rb.velocity * stats["MovementDrag"]) * Time.fixedDeltaTime;

        // Update hover SFX volume
        float targetHoverVolume = Mathf.Min(controlledCO.baseWO.rb.velocity.magnitude / 3f, 1.0f) * 0.25f + 0.15f;
        hoverAudio.volume += (targetHoverVolume - hoverAudio.volume) * 0.08f;
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
        if (!isConstructed || !isActive || isPaused || isTransitioning) return;

        // Move in the given direction
        float moveAcc = stats["MovementForce"] * controlledCO.baseWO.moveResist * Time.fixedDeltaTime;
        controlledCO.baseWO.rb.velocity = controlledCO.baseWO.rb.velocity + dir.normalized * moveAcc;
    }

    public override void AimAtPosition(Vector3 pos)
    {
        if (!isConstructed || !isActive || isPaused || isTransitioning) return;

        // Aim at the given position rotation
        float rotAcc = stats["AimForce"] * controlledCO.baseWO.moveResist * Time.fixedDeltaTime;
        Vector3 dir = (pos - controlledCO.baseWO.rb.position).normalized;
        Quaternion dirRot = Quaternion.LookRotation(dir, Vector3.up);
        controlledCO.baseWO.rb.rotation = Quaternion.Lerp(controlledCO.baseWO.rb.rotation, dirRot, rotAcc);
    }

    
    private float GetHoverHeight(float pct)
    {
        if (!isConstructed) return 0.0f;

        // Calculate current hover height based on pct
        float targetY = controlledCO.baseWO.GetMaxExtent() * (1.0f + 2.0f * stats["HoverHeight"]);
        targetY += Mathf.Sin(pct) * stats["HoverSinRange"];
        return targetY;
    }
    
    private float GetMaxHoverHeight() => GetHoverHeight(1.0f);
    
    private float GetCurrentHoverHeight() => GetHoverHeight(Time.time * stats["HoverSinFrequency"] * (2 * Mathf.PI));


    public override void SetActive(bool isActive_)
    {
        // Set to loose and floating
        base.SetActive(isActive_);
        controlledCO.SetLoose(true);
        controlledCO.SetFloating(isActive);
        if (!isActive) StartCoroutine(Sfx_FadeOut(hoverAudio, 0.15f));
    }

    public override void SetPaused(bool isPaused_)
    {
        // Set back to active state
        base.SetPaused(isPaused_);
        controlledCO.SetLoose(!isPaused);
        controlledCO.SetFloating(true);
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

    #endregion


    #region Core Movement

    [Header("References")]
    [SerializeField] private CameraEffects camFX;
    [SerializeField] private AudioClip coreAttachSFX;
    [SerializeField] private AudioClip coreChargeSFX;


    public override IEnumerator IE_Attach(ConstructObject targetCO)
    {
        if (isBlocking) yield break;

        // Turn off physics / colliders, update state
        SetTransitioning(true);
        controlledCC.SetFloating(true);
        controlledCC.SetLoose(false);
        controlledCC.SetColliding(false);
        if (coreChargeSFX != null) sfxAudio.PlayOneShot(coreChargeSFX);

        // Move backwards, start spinning and point at targetCO
        Vector3 targetPos = PlayerController.instance.hovered.pos;
        Coroutine moveBackwardsCR = StartCoroutine(_AttachCoreIEMoveBackwards(targetCO, targetPos));
        Coroutine lookAtCR = StartCoroutine(_AttachCoreIELookAt(targetCO, targetPos));
        yield return moveBackwardsCR;
        yield return lookAtCR;

        // Jab forwards into targetCO
        Coroutine jabIntoCR = StartCoroutine(_AttachCoreIEJabInto(targetCO, targetPos));
        yield return jabIntoCR;

        // Update variables, Play VFX (chromatic aberration / camera shake) and play SFX
        SetTransitioning(false);
        SetActive(false);
        StartCoroutine(camFX.Vfx_Shake(0.15f, 0.05f));
        StartCoroutine(camFX.Vfx_Chromatic(0.4f, 0.65f));
        if (coreAttachSFX != null) sfxAudio.PlayOneShot(coreAttachSFX);

        // Deactivate movement
        SetActive(false);
    }

    private IEnumerator _AttachCoreIEMoveBackwards(ConstructObject targetCO, Vector3 targetPos)
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

    private IEnumerator _AttachCoreIELookAt(ConstructObject targetCO, Vector3 targetPos)
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
            dir = newTargetPos - controlledCC.transform.position;

            speed = 12.0f * Time.deltaTime;
            bool reached = dir.magnitude < speed;
            controlledCC.baseWO.transform.position += dir.normalized * Mathf.Min(dir.magnitude, speed);

            if (reached) break;
            yield return null;
        }
    }


    public override IEnumerator IE_Detach()
    {
        if (isBlocking) yield break;

        // Detach but without control
        Vector3 popDir = (controlledCC.baseWO.transform.position - controlledCC.attachedCO.transform.position).normalized;
        SetTransitioning(true);
        controlledCC.SetFloating(false);
        controlledCC.SetLoose(true);
        controlledCC.SetColliding(true);

        // Apply popping force and torque and wait 0.5s
        float prevDrag = controlledCC.baseWO.rb.angularDrag;
        controlledCC.baseWO.rb.angularDrag = 0.0f;
        controlledCC.baseWO.rb.AddForce(popDir * 2.5f, ForceMode.VelocityChange);
        controlledCC.baseWO.rb.AddTorque(controlledCC.transform.right * 15.0f, ForceMode.VelocityChange); // FIX
        yield return new WaitForSeconds(0.5f);
        controlledCC.baseWO.rb.angularDrag = prevDrag;

        // Reactive movement
        SetTransitioning(false);
        SetActive(true);
    }

    #endregion
}
