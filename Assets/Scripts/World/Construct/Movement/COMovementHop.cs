
using UnityEngine;


public class COMovementHop : MonoBehaviour, ICOMovement
{
    // Declare references, config, variables
    private static float JUMP_Z_PCT = 0.75f;

    [Header("References")]
    [SerializeField] private GameObject rockParticleGeneratorPfb;
    [SerializeField] private GameObject speedParticleGeneratorPfb;
    private ConstructObject baseCO;

    [Header("Config")]
    [SerializeField] private float particleLimit = 0.9f;
    [SerializeField]
    private StatList stats = new StatList()
    {
        ["MovementForce"] = 5.0f,
        ["AimForce"] = 2.0f,
        ["MovementCooldown"] = 1.0f,
        ["AttackStrength"] = 10.0f,
        ["AttackDuration"] = 0.5f,
        ["AttackCooldown"] = 2.0f
    };

    private AttackSkill attackSkill;
    private bool isControlled = false;
    protected bool isForging = false;
    private bool isGrounded = true;
    private float jumpTimer = 0.0f;
    private Vector3 aimedDirection;


    public void Awake()
    {
        // Initialize references, variables
        SetCO(GetComponent<ConstructObject>());
        attackSkill = new AttackSkill(this);
    }


    public void Update()
    {
        if (isForging) UpdateForging();
        else UpdateIngame();
    }

    private void UpdateIngame()
    {
        // Update timers
        if (isGrounded) jumpTimer = Mathf.Max(jumpTimer - Time.deltaTime, 0.0f);

        // Aim in pointing direction while airborne
        if (!attackSkill.isActive && !isGrounded && aimedDirection != Vector3.zero)
        {
            Vector3 dir = aimedDirection;
            float rotAcc = baseCO.baseWO.moveResist * stats["AimForce"] * Time.deltaTime;
            Quaternion dirRot = Quaternion.LookRotation(dir, transform.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, dirRot, rotAcc);
        }
    }

    private void UpdateForging() { }


    public void MoveInDirection(Vector3 dir)
    {
        if (!GetCanMove()) return;

        // Hop in the given direction
        float hopVelocity = 1.0f * stats["MovementForce"] * baseCO.baseWO.moveResist; // dV = dT * F / M
        baseCO.baseWO.rb.velocity = baseCO.baseWO.rb.velocity + new Vector3(0.0f, hopVelocity * JUMP_Z_PCT, 0.0f);
        baseCO.baseWO.rb.velocity = baseCO.baseWO.rb.velocity + dir * hopVelocity;

        // Update variables
        jumpTimer = stats["MovementCooldown"];
        isGrounded = false;
    }

    public void AimAtPosition(Vector3 pos)
    {
        if (!GetCanMove()) return;

        // Update aimed direction
        aimedDirection = pos - transform.position;
    }


    public bool GetControlled() => isControlled;

    public bool GetCanForge() => isGrounded && !attackSkill.isActive;

    public bool GetCanMove() => isControlled && isGrounded && !attackSkill.isActive && jumpTimer <= 0.0f;


    public void SetCO(ConstructObject baseCO_) { baseCO = baseCO_; }

    public void SetControlled(bool isControlled_)
    {
        // Update variable and bind skills
        isControlled = isControlled_;
        baseCO.SetLoose(true);
        baseCO.SetFloating(false);
        if (isControlled && !attackSkill.isBinded) baseCO.construct.skills.RequestBinding(attackSkill, baseCO.construct.abilityButtons);
        else if (attackSkill.isBinded) baseCO.construct.skills.Unbind(attackSkill);
    }

    public virtual void SetForging(bool isForging_)
    {
        // Set forging and apply offset
        isForging = isForging_;
        if (isForging) baseCO.baseWO.transform.position += Vector3.up * baseCO.baseWO.GetMaxExtent() * 1.5f;
        else baseCO.baseWO.transform.position -= Vector3.up * baseCO.baseWO.GetMaxExtent() * 1.5f;
    }


    public void OnCollisionEnter(Collision collision)
    {
        // When hit ground becoming grounded
        if (collision.gameObject.layer == LayerMask.NameToLayer("Environment")) isGrounded = true;

        // Create particles
        if (baseCO.baseWO.rb.velocity.magnitude >= particleLimit)
        {
            float mult = Mathf.Min(0.5f, baseCO.baseWO.rb.velocity.magnitude / particleLimit - 1f) * (0.2f / 0.5f) + 0.8f;
            GameObject particles = Instantiate(rockParticleGeneratorPfb);
            particles.transform.position = collision.contacts[0].point;
            particles.transform.localScale = Vector3.one * mult * (attackSkill.isActive ? 1.0f : 0.35f);
            particles.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal, Vector3.up);
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        // If dragging along ground while attacking then stop if not moving
        if (
            !isGrounded
            && collision.gameObject.layer == LayerMask.NameToLayer("Environment")
            && baseCO.baseWO.rb.velocity.magnitude < 0.35f)
        {
            isGrounded = true;
        }
    }


    private class AttackSkill : Skill
    {
        // Declare variables
        private COMovementHop movement;
        private ParticleSystem speedParticleGenerator;
        public Vector3 attackPoint { get; private set; }
        private float attackTimer;
        private float attackTimerMax;


        public AttackSkill(COMovementHop movement_) : base(movement_.stats["AttackCooldown"])
        {
            // Initialize variables
            movement = movement_;
            speedParticleGenerator = Instantiate(movement.speedParticleGeneratorPfb).GetComponent<ParticleSystem>();
        }


        public override void Use()
        {
            if (!GetUsable()) return;

            // Jump towards target
            Vector3 dir = (PlayerController.instance.aimedPos - movement.transform.position).normalized;
            float jumpStrength = movement.stats["AttackStrength"] * movement.baseCO.baseWO.moveResist;
            movement.baseCO.baseWO.rb.velocity = movement.baseCO.baseWO.rb.velocity + dir * jumpStrength;

            // Update variables
            attackPoint = PlayerController.instance.aimedPos;
            attackTimer = movement.stats["AttackDuration"];
            movement.isGrounded = false;
            SetActive(true);

            // Setup particle generator
            if (!speedParticleGenerator.isPlaying) speedParticleGenerator.Play();

        }


        public override void Update()
        {
            base.Update();

            // Update cooldown timers
            attackTimerMax = movement.stats["AttackDuration"];
            cooldownTimerMax = movement.stats["AttackCooldown"];
            attackTimer = Mathf.Max(attackTimer - Time.deltaTime, 0.0f);

            // Handle active
            if (isActive)
            {
                // Check if grounded or finished attacking
                if (movement.isGrounded || attackTimer <= 0.0f) SetActive(false);

                // While still in the air and attacking
                else
                {
                    // Aim / move towards attack point
                    if (movement.aimedDirection != Vector3.zero)
                    {
                        Vector3 dir = (attackPoint - movement.transform.position).normalized;
                        float aimStrength = 0.5f * movement.baseCO.baseWO.moveResist * movement.stats["AimLerp"] * Time.deltaTime;
                        float jumpStrength = 2.0f * movement.stats["AttackStrength"] * movement.baseCO.baseWO.moveResist * Time.deltaTime;
                        Quaternion dirRot = Quaternion.LookRotation(dir, movement.transform.up);
                        movement.transform.rotation = Quaternion.Lerp(movement.transform.rotation, dirRot, aimStrength);
                        movement.baseCO.baseWO.rb.velocity = movement.baseCO.baseWO.rb.velocity + dir * jumpStrength;
                    }

                    // Activate speed particle effects
                    Quaternion speedDir = Quaternion.LookRotation(-movement.baseCO.baseWO.rb.velocity, Vector3.up);
                    speedParticleGenerator.transform.position = movement.transform.position;
                    speedParticleGenerator.transform.rotation = speedDir;
                    if (!speedParticleGenerator.isPlaying) speedParticleGenerator.Play();
                }
            }

            // Disable movement particles
            else if (speedParticleGenerator.isPlaying) speedParticleGenerator.Stop();
        }


        protected override bool GetUsable() => !isActive && !isCooldown && movement.isGrounded;


        protected override void SetActive(bool isActive_)
        {
            base.SetActive(isActive_);

            // Update timers
            if (isActive_) attackTimer = attackTimerMax;

            // Update particles
            if (!isActive_) speedParticleGenerator.Stop();
        }
    }
}
