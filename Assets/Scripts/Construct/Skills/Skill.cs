
using UnityEngine;


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
