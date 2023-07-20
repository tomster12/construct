
using System.Collections.Generic;
using UnityEngine;


public class SkillBindings
{
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

    public void UpdateInput()
    {
        // Check each key / mouse button
        foreach (string key in bindableButtons)
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

 
    public Skill GetSkill(string button)
    {
        // Call binded action for given button
        if (!bindedButtons.ContainsKey(button)) return null;
        return bindedButtons[button];
    }
}
