
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


class CameraEffects : MonoBehaviour
{
    public static CameraEffects instance { get; private set; }

    [Header("References")]
    [SerializeField] private Volume vol;


    private void Awake()
    {
        instance = this;
    }


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
            c.intensity.value = Util.Easing.EaseOutSine(t / time) * strength;
            t -= Time.deltaTime;
            yield return null;
        }
    }
}
