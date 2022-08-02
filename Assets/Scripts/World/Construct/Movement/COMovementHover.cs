
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
        ["MovementForce"] = 7.0f,
        ["AimForce"] = 4.0f,
        ["MovementDrag"] = 2.5f,
        ["HoverHeight"] = 2.0f,
        ["HoverSinRange"] = 0.2f,
        ["HoverSinFrequency"] = 0.4f,
        ["HoverForce"] = 3.0f
    };

    private ConstructObject baseCO;
    protected bool isControlled = false;
    protected bool isForging = false;
    protected bool isGrounded = false;
    protected bool overrideControl = false;
    protected Vector3 groundPosition;


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


    private void FixedUpdate()
    {
        // Update based on whether hovering
        if (isForging) UpdateForging();
        else UpdateIngame();
    }

    protected virtual void UpdateIngame()
    {
        if (!GetCanMove()) return;

        // Oscillate above closest reasonable surface
        float targetY, hoverPct;
        LayerMask layer = LayerMask.GetMask("Environment");
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, GetMaxHoverHeight() * 1.5f, layer))
        {
            isGrounded = true;
            groundPosition = hit.point;
            targetY = groundPosition.y + GetCurrentHoverHeight();
            hoverPct = stats["HoverForce"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        }

        // No valid hit so float downwards
        else
        {
            isGrounded = false;
            groundPosition = Vector3.zero;
            targetY = transform.position.y - 1.0f;
            hoverPct = baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        }

        // Lerp height and apply drag
        float lerpedY = Mathf.Lerp(transform.position.y, targetY, hoverPct);
        baseCO.baseWO.rb.position = new Vector3(transform.position.x, lerpedY, transform.position.z);
        baseCO.baseWO.rb.velocity -= (baseCO.baseWO.rb.velocity * stats["MovementDrag"]) * Time.fixedDeltaTime;

        // Update hover SFX volume
        float targetHoverVolume = Mathf.Min(baseCO.baseWO.rb.velocity.magnitude / 3f, 1.0f) * 0.25f + 0.15f;
        hoverAudio.volume += (targetHoverVolume - hoverAudio.volume) * 0.08f;
    }

    protected virtual void UpdateForging()
    {
        if (!isControlled) return;

        // Lerp height to baseline
        float hoverPct = stats["HoverForce"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        float targetY = GetHoverHeight(0.0f);
        float lerpedY = Mathf.Lerp(transform.position.y, targetY, hoverPct);
        transform.position = new Vector3(transform.position.x, lerpedY, transform.position.z);

        // Lerp rotation to baseline
        float rotAcc = stats["AimForce"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        transform.rotation = Quaternion.Lerp(baseCO.baseWO.rb.rotation, baseCO.GetForwardRot(), rotAcc);
    }


    public void MoveInDirection(Vector3 dir)
    {
        if (!GetCanMove()) return;

        // Move in the given direction
        float moveAcc = stats["MovementForce"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        baseCO.baseWO.rb.velocity = baseCO.baseWO.rb.velocity + dir.normalized * moveAcc;
    }

    public void AimAtPosition(Vector3 pos)
    {
        if (!GetCanMove()) return;

        // Aim at the given position rotation
        float rotAcc = stats["AimForce"] * baseCO.baseWO.moveResist * Time.fixedDeltaTime;
        Vector3 dir = (pos - baseCO.baseWO.rb.position).normalized;
        Quaternion dirRot = Quaternion.LookRotation(dir, Vector3.up);
        baseCO.baseWO.rb.rotation = Quaternion.Lerp(baseCO.baseWO.rb.rotation, dirRot, rotAcc);
    }


    public bool GetControlled() => isControlled;

    public bool GetCanForge() => isGrounded;

    private float GetHoverHeight(float pct)
    {
        // Calculate current hover height based on pct
        float targetY = baseCO.baseWO.GetMaxExtent() * (1.0f + 2.0f * stats["HoverHeight"]);
        targetY += Mathf.Sin(pct) * stats["HoverSinRange"];
        return targetY;
    }
    
    private float GetMaxHoverHeight() => GetHoverHeight(1.0f);
    
    private float GetCurrentHoverHeight() => GetHoverHeight(Time.time * stats["HoverSinFrequency"] * (2 * Mathf.PI));

    public bool GetCanMove() => !overrideControl && isControlled && !isForging;
        

    public void SetCO(ConstructObject baseCO_) => baseCO = baseCO_;

    public void SetControlled(bool isControlled_)
    {
        // Set isControlled and apply vfx
        isControlled = isControlled_;
        baseCO.SetLoose(true);
        baseCO.SetFloating(isControlled);
        if (!isControlled) StartCoroutine(Sfx_FadeOut(hoverAudio, 0.15f));
    }

    public virtual void SetForging(bool isForging_) => isForging = isForging_;


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
