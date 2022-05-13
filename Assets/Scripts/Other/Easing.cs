
using UnityEngine;


class Easing
{
    public static float EaseOutSine(float x) => Mathf.Sin((x * Mathf.PI) / 2);
    public static float EaseInSine(float x) => 1 - Mathf.Cos((x * Mathf.PI) / 2);
}
