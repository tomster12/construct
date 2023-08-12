//    private class AttackSkill : Skill
//    {
//        // Declare variables
//        private PartMovementHop movement;
//        private ParticleSystem speedParticleGenerator;
//        public Vector3 attackPoint { get; private set; }
//        private float attackTimer;
//        private float attackTimerMax;


//        public AttackSkill(PartMovementHop movement_) : base(movement_.stats["AttackCooldown"])
//        {
//            // Initialize variables
//            movement = movement_;
//            speedParticleGenerator = Instantiate(movement.speedParticleGeneratorPfb).GetComponent<ParticleSystem>();
//        }


//        public override void Use()
//        {
//            if (!GetUsable()) return;

//            // Jump towards target
//            Vector3 dir = (PlayerController.instance.hovered.pos - movement.transform.position).normalized;
//            float jumpStrength = movement.stats["AttackStrength"] * movement.controlledPart.GetObject().moveResist;
//            movement.controlledPart.GetObject().rb.velocity = movement.controlledPart.GetObject().rb.velocity + dir * jumpStrength;

//            // Update variables
//            attackPoint = PlayerController.instance.hovered.pos;
//            attackTimer = movement.stats["AttackDuration"];
//            movement.isGrounded = false;
//            SetActive(true);

//            // Setup particle generator
//            if (!speedParticleGenerator.isPlaying) speedParticleGenerator.Play();

//        }


//        public override void Update()
//        {
//            base.Update();

//            // Update cooldown timers
//            attackTimerMax = movement.stats["AttackDuration"];
//            cooldownTimerMax = movement.stats["AttackCooldown"];
//            attackTimer = Mathf.Max(attackTimer - Time.deltaTime, 0.0f);

//            // Handle active
//            if (isActive)
//            {
//                // Check if grounded or finished attacking
//                if (movement.isGrounded || attackTimer <= 0.0f) SetActive(false);

//                // While still in the air and attacking
//                else
//                {
//                    // Aim / move towards attack point
//                    if (movement.aimedDirection != Vector3.zero)
//                    {
//                        Vector3 dir = (attackPoint - movement.transform.position).normalized;
//                        float aimStrength = 0.5f * movement.controlledPart.GetObject().moveResist * movement.stats["AimLerp"] * Time.deltaTime;
//                        float jumpStrength = 2.0f * movement.stats["AttackStrength"] * movement.controlledPart.GetObject().moveResist * Time.deltaTime;
//                        Quaternion dirRot = Quaternion.LookRotation(dir, movement.transform.up);
//                        movement.transform.rotation = Quaternion.Lerp(movement.transform.rotation, dirRot, aimStrength);
//                        movement.controlledPart.GetObject().rb.velocity = movement.controlledPart.GetObject().rb.velocity + dir * jumpStrength;
//                    }

//                    // Activate speed particle effects
//                    Quaternion speedDir = Quaternion.LookRotation(-movement.controlledPart.GetObject().rb.velocity, Vector3.up);
//                    speedParticleGenerator.transform.position = movement.transform.position;
//                    speedParticleGenerator.transform.rotation = speedDir;
//                    if (!speedParticleGenerator.isPlaying) speedParticleGenerator.Play();
//                }
//            }

//            // Disable movement particles
//            else if (speedParticleGenerator.isPlaying) speedParticleGenerator.Stop();
//        }


//        protected override bool GetUsable() => !isActive && !isCooldown && movement.isGrounded;


//        protected override void SetActive(bool isActive_)
//        {
//            base.SetActive(isActive_);

//            // Update timers
//            if (isActive_) attackTimer = attackTimerMax;

//            // Update particles
//            if (!isActive_) speedParticleGenerator.Stop();
//        }
//    }
//}