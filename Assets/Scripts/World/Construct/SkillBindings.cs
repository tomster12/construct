
using System.Collections.Generic;
using UnityEngine;


public class SkillBindings
{
    // Declare static, variables
    public List<string> bindableButtons { get; private set; } = new List<string>() { "_0", "_1", "1", "2", "3", "4" };
    private Dictionary<string, Skill> bindedButtons = new Dictionary<string, Skill>();


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
