
using UnityEngine;


public class MovementHop : ConstructObjectMovement
{
    private static float JUMP_Z_PCT = 0.75f;

    [Header("References")]
    [SerializeField] protected ConstructObject controlledCO;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject rockParticleGeneratorPfb;
    [SerializeField] private GameObject speedParticleGeneratorPfb;

    [Header("Config")]
    [SerializeField] private float particleLimit = 0.9f;
    [SerializeField] private StatList stats = new StatList()
    {
        ["MovementForce"] = 5.0f,
        ["AimForce"] = 2.0f,
        ["MovementCooldown"] = 1.0f,
        ["AttackStrength"] = 10.0f,
        ["AttackDuration"] = 0.5f,
        ["AttackCooldown"] = 2.0f
    };

    public override bool isBlocking => !isGrounded;

    private float jumpTimer = 0.0f;
    private bool isGrounded = true;
    private Vector3 aimedDirection;


    public void Update()
    {
        if (!isAssigned || !isActive) return;

        // Update timers
        if (isGrounded) jumpTimer = Mathf.Max(jumpTimer - Time.deltaTime, 0.0f);

        // Aim in pointing direction while airborne
        if (!isGrounded && aimedDirection != Vector3.zero)
        {
            Vector3 dir = aimedDirection;
            float rotAcc = controlledCO.baseWO.moveResist * stats["AimForce"] * Time.deltaTime;
            Quaternion dirRot = Quaternion.LookRotation(dir, transform.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, dirRot, rotAcc);
        }
    }

    public override void MoveInDirection(Vector3 dir)
    {
        if (!isAssigned || !isActive || isPaused || isBlocking || jumpTimer > 0.0f) return;

        // Hop in the given direction
        float hopVelocity = 1.0f * stats["MovementForce"] * controlledCO.baseWO.moveResist; // dV = dT * F / M
        controlledCO.baseWO.rb.velocity = controlledCO.baseWO.rb.velocity + new Vector3(0.0f, hopVelocity * JUMP_Z_PCT, 0.0f);
        controlledCO.baseWO.rb.velocity = controlledCO.baseWO.rb.velocity + dir * hopVelocity;

        // Update variables
        jumpTimer = stats["MovementCooldown"];
        isGrounded = false;
    }

    public override void AimAtPosition(Vector3 pos)
    {
        if (!isAssigned || !isActive || isPaused) return;

        // Update aimed direction
        aimedDirection = pos - transform.position;
    }


    public void OnCollisionEnter(Collision collision)
    {
        // When hit ground becoming grounded
        if (collision.gameObject.layer == LayerMask.NameToLayer("Environment")) isGrounded = true;

        // Create particles
        if (isAssigned && controlledCO.baseWO.rb.velocity.magnitude >= particleLimit)
        {
            float mult = Mathf.Min(0.5f, controlledCO.baseWO.rb.velocity.magnitude / particleLimit - 1f) * (0.2f / 0.5f) + 0.8f;
            GameObject particles = Instantiate(rockParticleGeneratorPfb);
            particles.transform.position = collision.contacts[0].point;
            particles.transform.localScale = Vector3.one * mult * 0.4f;
            particles.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal, Vector3.up);
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        // If dragging along ground while attacking then stop if not moving
        if (
            !isGrounded
            && collision.gameObject.layer == LayerMask.NameToLayer("Environment")
            && controlledCO.baseWO.rb.velocity.magnitude < 0.35f)
        {
            isGrounded = true;
        }
    }


    public override bool SetActive(bool isActive_)
    {
        if (!base.SetActive(isActive_)) return false;

        // Update state
        if (isActive) controlledCO.SetControlledBy(this);
        else controlledCO.SetControlledBy(null);

        // Update physics
        controlledCO.baseWO.isLoose = true;
        controlledCO.baseWO.isFloating = false;
        return true;
    }

    public override bool SetPaused(bool isPaused_)
    {
        if (!base.SetPaused(isPaused_)) return false;

        // Offset upwards
        if (isPaused) controlledCO.baseWO.transform.position += Vector3.up * controlledCO.baseWO.maxExtent * 1.5f;
        else controlledCO.baseWO.transform.position -= Vector3.up * controlledCO.baseWO.maxExtent * 1.5f;
        controlledCO.baseWO.isLoose = !isPaused;
        controlledCO.baseWO.isFloating = isPaused;
        return true;
    }
}
