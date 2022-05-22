
using System.Collections;
using UnityEngine;


public class COMovementHover : MonoBehaviour, ICOMovement
{
    // Declare references, config, variables
    [Header("References")]
    [SerializeField] private AudioClip hoverSFX;
    private AudioSource hoverAudio;

    [Header("Config")]
    [SerializeField] protected StatList stats = new StatList()
    {
        ["hoverHeight"] = 2.0f,
        ["hoverSpeed"] = 3.0f,
        ["hoverSinRange"] = 0.2f,
        ["hoverSinFrequency"] = 0.4f,
        ["MovementStrength"] = 7f,
        ["MovementFriction"] = 2.5f,
        ["aimStrength"] = 4f
    };

    private ConstructObject baseCO;
    protected bool isControlled = false;
    protected bool isGrounded = false;
    protected bool overrideControl = false;


    protected virtual void Awake()
    {
        // Initialize references
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
        if (!GetCanMove()) return;

        // Oscillate above closest reasonable surface
        float targetY, hoverStrength;
        LayerMask layer = LayerMask.GetMask("Environment");
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, GetMaxHoverHeight() * 1.5f, layer))
        {
            isGrounded = true;
            targetY = hit.point.y + GetHoverHeight();
            hoverStrength = stats["hoverSpeed"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        }

        // No valid hit so float downwards
        else
        {
            isGrounded = false;
            targetY = transform.position.y - 1.0f;
            hoverStrength = baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        }

        // Lerp position, and apply friction to movement
        float lerpedY = Mathf.Lerp(transform.position.y, targetY, hoverStrength);
        transform.position = new Vector3(transform.position.x, lerpedY, transform.position.z);
        baseCO.baseWO.rb.velocity -= baseCO.baseWO.rb.velocity * stats["MovementFriction"] * Time.fixedDeltaTime;

        // Update hover SFX volume
        float targetHoverVolume = Mathf.Min(baseCO.baseWO.rb.velocity.magnitude / 3f, 1.0f) * 0.25f + 0.15f;
        hoverAudio.volume += (targetHoverVolume - hoverAudio.volume) * 0.08f;
    }


    public void MoveInDirection(Vector3 dir)
    {
        if (!GetCanMove()) return;

        // Move in the given direction
        float movePct = stats["MovementStrength"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        baseCO.baseWO.rb.velocity = baseCO.baseWO.rb.velocity + dir * movePct;
    }

    public void AimAtPosition(Vector3 pos)
    {
        if (!GetCanMove()) return;

        // Aim at the given position rotation
        Vector3 dir = (pos - baseCO.baseWO.rb.position).normalized;
        Quaternion dirRot = Quaternion.LookRotation(dir, Vector3.up);
        float aimPct = stats["aimStrength"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        baseCO.baseWO.rb.rotation = Quaternion.Lerp(baseCO.baseWO.rb.rotation, dirRot, aimPct);
    }


    public bool GetControlled() => isControlled;

    public bool GetCanForge() => isGrounded;

    private float GetMaxHoverHeight()
    {
        // Calculate max hover height based on stats
        float targetY = baseCO.baseWO.GetMaxExtent() * (1.0f + 2.0f * stats["hoverHeight"]);
        targetY += 1.0f * stats["hoverSinRange"];
        return targetY;
    }

    private float GetHoverHeight()
    {
        // Calculate current hover height
        float targetY = baseCO.baseWO.GetMaxExtent() * (1.0f + 2.0f * stats["hoverHeight"]);
        targetY += Mathf.Sin(Time.time * stats["hoverSinFrequency"] * (2 * Mathf.PI)) * stats["hoverSinRange"];
        return targetY;
    }

    public bool GetCanMove() => !overrideControl && isControlled && !baseCO.construct.isForging;


    public void SetCO(ConstructObject baseCO_) { baseCO = baseCO_; }

    public void SetControlled(bool isControlled_)
    {
        // Set isControlled and apply vfx
        isControlled = isControlled_;
        baseCO.SetLoose(true);
        baseCO.SetFloating(isControlled);
        if (!isControlled) StartCoroutine(Sfx_FadeOut(hoverAudio, 0.15f));
    }

    public void SetForging(bool isForging_) { } // TODO: Currently no forging positioning


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
}
