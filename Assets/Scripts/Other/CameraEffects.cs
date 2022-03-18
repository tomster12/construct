
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


class CameraEffects : MonoBehaviour {

  // Initialize references
  private Volume vol;
  private Vector3 shakeOffset;


  private void Awake() {
    // Initialize references
    vol = GetComponent<Volume>();
  }


  private void Update() {
    transform.localPosition = shakeOffset;
  }


  public IEnumerator vfxShake(float time, float strength) {
    // Shake camera over time
    for (float t = time; t > 0.0f;) {
      shakeOffset = Random.insideUnitSphere * (t / time) * strength;
      t -= Time.deltaTime;
      yield return null;
    }
  }


  public IEnumerator vfxChromatic(float time, float strength) {
    // Get chromatic aberration from player volume
    ChromaticAberration c;
    vol.profile.TryGet(out c);

    // Pop and then ease out
    for (float t = time; t > 0.0f;) {
      c.intensity.value = Easing.easeOutSine(t / time) * strength;
      t -= Time.deltaTime;
      yield return null;
    }
  }
}