
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovementHover : MonoBehaviour, CoreMovementI
{

    // #region - Setup

    // Declare references, variables
    private Object selfWJ;
    [SerializeField] private AudioClip hoverSFX;
    private AudioSource hoverSRC;

    [SerializeField]
    private StatList stats = new StatList()
    {
        ["hoverHeight"] = 2.0f,
        ["hoverSpeed"] = 3.0f,
        ["hoverSinRange"] = 0.2f,
        ["hoverSinFrequency"] = 0.4f,
        ["MovementStrength"] = 7f,
        ["MovementFriction"] = 0.96f,
        ["aimStrength"] = 4f
    };
    private bool active = false;


    public void Awake()
    {
        // Initialize references
        selfWJ = GetComponent<Object>();
        hoverSRC = gameObject.AddComponent<AudioSource>();

        // Initialize hoverSRC
        hoverSRC.clip = hoverSFX;
        hoverSRC.volume = 0.0f;
        hoverSRC.loop = true;
        hoverSRC.Play();

        // Awake core
        coreAwake();
    }

    // #endregion


    // #region - Main

    public void FixedUpdate()
    {
        // If currently being controlled
        if (active)
        {

            // Raycast downwards and find first hit
            float targetY, hoverStrength;
            int firstHit = -1;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, getMaxHoverHeight() * 1.5f);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform != transform && (firstHit == -1 || hits[i].distance < hits[firstHit].distance)) firstHit = i;
            }

            // Set target as first hit + hover height
            if (firstHit != -1)
            {
                targetY = hits[firstHit].point.y + getHoverHeight();
                hoverStrength = stats["hoverSpeed"] * selfWJ.moveResist * Time.fixedDeltaTime;

                // Too high so hover downwards
            }
            else
            {
                targetY = transform.position.y - 1.0f;
                hoverStrength = selfWJ.moveResist * Time.fixedDeltaTime;
            }

            // Lerp towards target with acceleration
            float lerpedY = Mathf.Lerp(transform.position.y, targetY, hoverStrength);
            transform.position = new Vector3(transform.position.x, lerpedY, transform.position.z);

            // Slow down Movement
            selfWJ.rb.velocity = selfWJ.rb.velocity * stats["MovementFriction"];

            // Update hover SFX volume
            float targetHoverVolume = Mathf.Min(selfWJ.rb.velocity.magnitude / 3f, 1.0f) * 0.25f + 0.15f;
            hoverSRC.volume += (targetHoverVolume - hoverSRC.volume) * 0.08f;
        }
    }


    private float getMaxHoverHeight()
    {
        float targetY = selfWJ.maxExtent * (1.0f + 2.0f * stats["hoverHeight"]);
        targetY += 1.0f * stats["hoverSinRange"];
        return targetY;
    }

    private float getHoverHeight()
    {
        float targetY = selfWJ.maxExtent * (1.0f + 2.0f * stats["hoverHeight"]);
        targetY += Mathf.Sin(Time.time * stats["hoverSinFrequency"] * (2 * Mathf.PI)) * stats["hoverSinRange"];
        return targetY;
    }


    private IEnumerator fadeOutSFX(AudioSource src, float duration)
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


    public void aimAtPosition(Vector3 pos, float force)
    {
        if (active)
        {

            // Aim at the given position rotation
            Vector3 dir = (pos - selfWJ.rb.position).normalized;
            Quaternion dirRot = Quaternion.LookRotation(dir, Vector3.up);
            float aimPct = force * stats["aimStrength"] * selfWJ.moveResist * Time.fixedDeltaTime;
            selfWJ.rb.rotation = Quaternion.Lerp(selfWJ.rb.rotation, dirRot, aimPct);
        }
    }

    public void moveInDirection(Vector3 dir, float force)
    {
        if (active)
        {

            // Move in the given direction
            float movePct = force * stats["MovementStrength"] * selfWJ.moveResist * Time.fixedDeltaTime;
            selfWJ.rb.velocity = selfWJ.rb.velocity + dir * movePct;
        }
    }

    public void attack(Object targetWJ, Vector3 aimedPos) { }


    public bool canAttack(Object targetWJ, Vector3 aimedPos) => false;

    public void setActive(bool active_)
    {
        // Set active
        active = active_;
        selfWJ.rb.useGravity = !active;

        // Fade out hover sfx
        if (!active) StartCoroutine(fadeOutSFX(hoverSRC, 0.15f));
    }

    public bool getActive() => active;

    public StatList getStats() => stats;

    // #endregion


    // #region - Core

    // Declare references, variables
    [SerializeField] private AudioClip coreAttachSFX;
    [SerializeField] private AudioClip coreChargeSFX;
    private AudioSource coreAudioSRC;
    private CameraEffects camFX;

    private CoreAttachmentState coreAttachmentState;
    private Object attachedWJ;


    private void coreAwake()
    {
        // Initialize references
        coreAudioSRC = gameObject.AddComponent<AudioSource>();
        camFX = Camera.main.GetComponent<CameraEffects>();

        // Initialize variables
        coreAttachmentState = CoreAttachmentState.Detached;
    }


    public void attachCore(Object targetWJ) { StartCoroutine(attachCoreIE(targetWJ)); }

    public IEnumerator attachCoreIE(Object targetWJ)
    {
        if (coreAttachmentState == CoreAttachmentState.Detached)
        {

            // Turn off physics / colliders, update state
            setActive(false);
            selfWJ.rb.useGravity = false;
            selfWJ.rb.isKinematic = true;
            selfWJ.cl.enabled = false;
            coreAttachmentState = CoreAttachmentState.Attaching;
            coreAudioSRC.PlayOneShot(coreChargeSFX);

            // Move backwards, start spinning and point at targetWJ
            Coroutine moveBackwardsCR = StartCoroutine(_attachCoreIEMoveBackwards(targetWJ));
            Coroutine lookAtCR = StartCoroutine(_attachCoreIELookAt(targetWJ));
            yield return moveBackwardsCR;
            yield return lookAtCR;

            // Jab forwards into targetWJ
            Coroutine jabIntoCR = StartCoroutine(_attachCoreIEJabInto(targetWJ));
            yield return jabIntoCR;

            // Play VFX (chromatic aberration / camera shake) and play SFX
            StartCoroutine(camFX.VfxShake(0.15f, 0.05f));
            StartCoroutine(camFX.VfxChromatic(0.4f, 0.65f));
            coreAudioSRC.PlayOneShot(coreAttachSFX);

            // Update parent object, pass over control, update state
            selfWJ.transform.parent = targetWJ.transform;
            coreAttachmentState = CoreAttachmentState.Attached;
            attachedWJ = targetWJ;
        }
    }

    private IEnumerator _attachCoreIEMoveBackwards(Object targetWJ)
    {
        // Initialize variables
        float startDist = (targetWJ.transform.position - selfWJ.transform.position).magnitude;
        Vector3 dir, start, end;
        float movePct;

        // Move towards a point which is start + 1.0 distance away
        for (float t = 0; t < 0.65f;)
        {
            dir = targetWJ.transform.position - selfWJ.transform.position;
            start = targetWJ.transform.position + -dir.normalized * startDist;
            end = start + -dir.normalized * 1.0f;
            movePct = Easing.EaseOutSine(Mathf.Min(t, 0.65f) / 0.65f);
            selfWJ.transform.position = Vector3.Lerp(start, end, movePct);

            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator _attachCoreIELookAt(Object targetWJ)
    {
        // Initialize variables
        Vector3 dir, startUp = selfWJ.transform.up;
        float aimPct, spinPct;

        // Lerp rotate local y towards targetWJ, lerp rotate around local y
        for (float t = 0; t < 0.85f;)
        {
            dir = targetWJ.transform.position - selfWJ.transform.position;
            aimPct = Easing.EaseOutSine(Mathf.Min(t / 0.65f, 1.0f));
            spinPct = Easing.EaseInSine(Mathf.Min(t / 0.85f, 1.0f));
            selfWJ.transform.up = Vector3.Lerp(startUp, dir, aimPct);
            selfWJ.transform.rotation *= Quaternion.AngleAxis(360 * spinPct, Vector3.up);

            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator _attachCoreIEJabInto(Object targetWJ)
    {
        // Initialize variables
        Vector3 dir;
        bool hasHit;
        RaycastHit hit;
        float speed;

        // Raycast then move towards targetWJ
        while (true)
        {
            dir = targetWJ.transform.position - selfWJ.transform.position;
            hasHit = Physics.Raycast(selfWJ.transform.position, dir, out hit, dir.magnitude);

            speed = 12.0f * Time.deltaTime;
            bool reached = hit.distance < speed;
            selfWJ.transform.position += dir.normalized * Mathf.Min(hit.distance, speed);

            if (reached) break;
            yield return null;
        }
    }


    public void detachCore() { StartCoroutine(detachCoreIE()); }

    public IEnumerator detachCoreIE()
    {
        Vector3 popDir = (selfWJ.transform.position - attachedWJ.transform.position).normalized;

        // Detach but without control
        selfWJ.rb.isKinematic = false;
        selfWJ.rb.useGravity = true;
        selfWJ.cl.enabled = true;
        coreAttachmentState = CoreAttachmentState.Detaching;
        attachedWJ = null;

        // Apply popping force and torque and wait 0.5s
        float prevDrag = selfWJ.rb.angularDrag;
        selfWJ.rb.angularDrag = 0.0f;
        selfWJ.rb.AddForce(popDir * 2.5f, ForceMode.VelocityChange);
        selfWJ.rb.AddTorque(transform.right * 15.0f, ForceMode.VelocityChange); // FIX
        yield return new WaitForSeconds(0.5f);

        // Reactive moveset and angular drag
        selfWJ.rb.angularDrag = prevDrag;
        coreAttachmentState = CoreAttachmentState.Detached;
        setActive(true);
    }


    public CoreAttachmentState getCoreAttachmentState()
    {
        // Return current coreAttachmentState
        return coreAttachmentState;
    }

    // #endregion
}
