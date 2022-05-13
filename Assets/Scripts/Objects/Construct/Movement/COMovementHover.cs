
using System.Collections;
using UnityEngine;


public class COMovementHover : MonoBehaviour, ICOMovement
{
    // Declare references, config, variables
    [SerializeField] private AudioClip hoverSFX;
    private AudioSource hoverAudio;
    private ConstructObject baseCO;

    [SerializeField] protected StatList stats = new StatList()
    {
        ["hoverHeight"] = 2.0f,
        ["hoverSpeed"] = 3.0f,
        ["hoverSinRange"] = 0.2f,
        ["hoverSinFrequency"] = 0.4f,
        ["MovementStrength"] = 7f,
        ["MovementFriction"] = 0.96f,
        ["aimStrength"] = 4f
    };

    protected bool isControlled = false;
    protected bool overrideControl = false;


    protected virtual void Awake()
    {
        // Initialize references
        SetConstructObject(GetComponent<ConstructObject>());
        hoverAudio = gameObject.AddComponent<AudioSource>();

        // Setup hover SRC
        if (hoverSFX != null)
        {
            hoverAudio.clip = hoverSFX;
            hoverAudio.volume = 0.0f;
            hoverAudio.loop = true;
            hoverAudio.Play();
        }
    }


    private void FixedUpdate() => UpdateHover();

    private void UpdateHover()
    {
        if (!isControlled || overrideControl) return;
        
        // Raycast downwards to find best matching hit
        float targetY, hoverStrength;
        int bestHit = -1;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, GetMaxHoverHeight() * 1.5f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == transform) continue;
            if (bestHit == -1 || hits[i].distance < hits[bestHit].distance) bestHit = i;
        }

        // No valid hit so float downwards
        if (bestHit == -1)
        {
            targetY = transform.position.y - 1.0f;
            hoverStrength = baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        }

        // Oscillate above closest reasonable surface
        else
        {
            targetY = hits[bestHit].point.y + GetHoverHeight();
            hoverStrength = stats["hoverSpeed"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        }

        // Lerp position, and apply friction to movement
        float lerpedY = Mathf.Lerp(transform.position.y, targetY, hoverStrength);
        transform.position = new Vector3(transform.position.x, lerpedY, transform.position.z);
        baseCO.baseWO.rb.velocity = baseCO.baseWO.rb.velocity * stats["MovementFriction"];

        // Update hover SFX volume
        float targetHoverVolume = Mathf.Min(baseCO.baseWO.rb.velocity.magnitude / 3f, 1.0f) * 0.25f + 0.15f;
        hoverAudio.volume += (targetHoverVolume - hoverAudio.volume) * 0.08f;
    }


    public void MoveInDirection(Vector3 dir)
    {
        if (!isControlled || overrideControl) return;

        // Move in the given direction
        float movePct = stats["MovementStrength"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        baseCO.baseWO.rb.velocity = baseCO.baseWO.rb.velocity + dir * movePct;
    }

    public void AimAtPosition(Vector3 pos)
    {
        if (!isControlled || overrideControl) return;

        // Aim at the given position rotation
        Vector3 dir = (pos - baseCO.baseWO.rb.position).normalized;
        Quaternion dirRot = Quaternion.LookRotation(dir, Vector3.up);
        float aimPct = stats["aimStrength"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        baseCO.baseWO.rb.rotation = Quaternion.Lerp(baseCO.baseWO.rb.rotation, dirRot, aimPct);
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


    private float GetMaxHoverHeight()
    {
        float targetY = baseCO.baseWO.GetMaxExtent() * (1.0f + 2.0f * stats["hoverHeight"]);
        targetY += 1.0f * stats["hoverSinRange"];
        return targetY;
    }

    private float GetHoverHeight()
    {
        float targetY = baseCO.baseWO.GetMaxExtent() * (1.0f + 2.0f * stats["hoverHeight"]);
        targetY += Mathf.Sin(Time.time * stats["hoverSinFrequency"] * (2 * Mathf.PI)) * stats["hoverSinRange"];
        return targetY;
    }

    public bool GetControlled() => isControlled;
    

    public void SetControlled(bool isControlled_)
    {
        // Set isControlled and apply vfx
        isControlled = isControlled_;
        baseCO.baseWO.rb.useGravity = !isControlled;
        if (!isControlled) StartCoroutine(Sfx_FadeOut(hoverAudio, 0.15f));
    }

    protected void SetConstructObject(ConstructObject baseCO_) { baseCO = baseCO_; }
}
