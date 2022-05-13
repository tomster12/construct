
using UnityEngine;


public class ParticleSystemRemover : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake() => ps = GetComponent<ParticleSystem>();
    private void Update() { if (!ps.IsAlive()) Destroy(gameObject); }
}
