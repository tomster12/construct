
using UnityEngine;


static class Util
{
    static public class Easing
    {
        public static float EaseOutSine(float x) => Mathf.Sin((x * Mathf.PI) / 2);
        public static float EaseInSine(float x) => 1 - Mathf.Cos((x * Mathf.PI) / 2);
    }


    static public float Map(float v, float a0, float a1, float b0, float b1) => b0 + (b1 - b0) * (v - a0) / (a1 - a0);

    static public float ConstrainMap(float v, float a0, float a1, float b0, float b1) => Mathf.Clamp(b0 + (b1 - b0) * (v - a0) / (a1 - a0), b0, b1);


    public static void SetLayer(Transform t, int layer)
    {
        // Recursive transform layer update
        t.gameObject.layer = layer;
        foreach (Transform c in t) SetLayer(c, layer);
    }
}