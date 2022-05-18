
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


class CameraEffects : MonoBehaviour
{

    // Initialize references
    [Header("References")]
    [SerializeField] private Volume vol;


    public IEnumerator Vfx_Shake(float time, float strength)
    {
        // Shake camera inside random sphere
        for (float t = time; t > 0.0f;)
        {
            transform.localPosition = Random.insideUnitSphere * (t / time) * strength;
            t -= Time.deltaTime;
            yield return null;
        }
    }


    public IEnumerator Vfx_Chromatic(float time, float strength)
    {
        // Apply chromatic then ease out
        ChromaticAberration c;
        vol.profile.TryGet(out c);
        for (float t = time; t > 0.0f;)
        {
            c.intensity.value = Easing.EaseOutSine(t / time) * strength;
            t -= Time.deltaTime;
            yield return null;
        }
    }
}
