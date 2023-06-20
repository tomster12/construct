
using System.Collections.Generic;
using UnityEngine;


public class SkillBindings
{
    // Declare static, variables
    public List<string> bindableButtons { get; private set; } = new List<string>() { "_0", "_1", "1", "2", "3", "4" };
    private Dictionary<string, Skill> bindedButtons = new Dictionary<string, Skill>();

    public bool isBlocking => false;


    public SkillBindings(List<string> bindableButtons_)
    {
        // Initialize variables
        if (bindableButtons_ != null) bindableButtons = bindableButtons_;
    }


    public void UpdateSkills()
    {
        // Update all binded skills
        foreach (KeyValuePair<string, Skill> entry in bindedButtons) entry.Value.Update();
    }


    public void TryUseAll()
    {
        foreach (string key in  bindableButtons)
        {
            if (key.StartsWith("_"))
            {
                if (Input.GetMouseButtonDown(key[1] - '0')) Use(key);
            }
            else if (Input.GetKeyDown(key)) Use(key);
        }
    }

    public void Use(string button)
    {
        // Call binded action for given button
        if (!bindedButtons.ContainsKey(button)) return;
        if (!bindedButtons[button].isUsable) return;
        bindedButtons[button].Use();
    }

    public Skill GetSkill(string button)
    {
        // Call binded action for given button
        if (!bindedButtons.ContainsKey(button)) return null;
        return bindedButtons[button];
    }


    public bool RequestBinding(Skill skill, string button, bool force = false)
    {
        // Ensure is bindable and has not already been binded
        if (!bindableButtons.Contains(button)) return false;
        if (bindedButtons.ContainsKey(button))
        {
            if (force) Unbind(button);
            else return false;
        }
        bindedButtons.Add(button, skill);
        skill.Bind(this);
        return true;
    }

    public void Unbind(string button)
    {
        // Unbind button if bound
        if (bindedButtons.ContainsKey(button))
        {
            bindedButtons[button].Unbind();
            bindedButtons.Remove(button);
        }
    }

    public void Unbind(Skill skill)
    {
        // Find button for the skill
        string button = null;
        foreach (KeyValuePair<string, Skill> entry in bindedButtons)
        {
            if (entry.Value == skill) button = entry.Key;
        }

        // Unbind skill if bound
        if (button != null)
        {
            skill.Unbind();
            bindedButtons.Remove(button);
        }
    }

    public void Clear() => bindedButtons.Clear();
}


public abstract class Skill : MonoBehaviour
{
    // Declare variables
    public SkillBindings bindings { get; private set; }
    public bool isBinded { get; private set; } = false;
    public bool isUsable => !isActive && !isCooldown;
    public bool isActive { get; protected set; } = false;
    public bool isCooldown { get; protected set; } = false;
    public float cooldownTimer { get; protected set; } = 0.0f;
    public float cooldownTimerMax { get; protected set; } = 1.0f;


    public virtual void Bind(SkillBindings bindings) => isBinded = true;

    public virtual void Unbind() => isBinded = false;


    public virtual void Update()
    {
        // Update cooldown timer
        if (isCooldown)
        {
            cooldownTimer = Mathf.Max(0.0f, cooldownTimer - Time.deltaTime);
            if (cooldownTimer == 0.0f) isCooldown = false;
        }
    }
    

    public virtual void Use() { }


    protected virtual void SetActive(bool isActive_) => isActive = isActive_;

    protected virtual void StartCooldown()
    {
        // Start the cooldown
        if (cooldownTimerMax != 0.0f)
        {
            isCooldown = true;
            cooldownTimer = cooldownTimerMax;
        }
    }
}


//    private class AttackSkill : Skill
//    {
//        // Declare variables
//        private COMovementHop movement;
//        private ParticleSystem speedParticleGenerator;
//        public Vector3 attackPoint { get; private set; }
//        private float attackTimer;
//        private float attackTimerMax;


//        public AttackSkill(COMovementHop movement_) : base(movement_.stats["AttackCooldown"])
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
//            float jumpStrength = movement.stats["AttackStrength"] * movement.controlledCO.baseWO.moveResist;
//            movement.controlledCO.baseWO.rb.velocity = movement.controlledCO.baseWO.rb.velocity + dir * jumpStrength;

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
//                        float aimStrength = 0.5f * movement.controlledCO.baseWO.moveResist * movement.stats["AimLerp"] * Time.deltaTime;
//                        float jumpStrength = 2.0f * movement.stats["AttackStrength"] * movement.controlledCO.baseWO.moveResist * Time.deltaTime;
//                        Quaternion dirRot = Quaternion.LookRotation(dir, movement.transform.up);
//                        movement.transform.rotation = Quaternion.Lerp(movement.transform.rotation, dirRot, aimStrength);
//                        movement.controlledCO.baseWO.rb.velocity = movement.controlledCO.baseWO.rb.velocity + dir * jumpStrength;
//                    }

//                    // Activate speed particle effects
//                    Quaternion speedDir = Quaternion.LookRotation(-movement.controlledCO.baseWO.rb.velocity, Vector3.up);
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