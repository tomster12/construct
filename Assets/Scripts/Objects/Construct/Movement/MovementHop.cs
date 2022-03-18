
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovementHop : MonoBehaviour, MovementI {

  // #region - Setup

  // Declare static, references, variables
  private static float JUMP_Z_PCT = 0.75f;

  [SerializeField] private GameObject rockParticleGeneratorPfb;
  [SerializeField] private GameObject speedParticleGeneratorPfb;
  private WorldObject WJ;

  private ParticleSystem speedParticleGenerator;
  [SerializeField] private float particleLimit = 0.9f;
  [SerializeField] private StatList stats = new StatList() {
    ["MovementStrength"] = 5.0f,
    ["JumpCooldown"] = 1.0f,
    ["AttackStrength"] = 10.0f,
    ["AttackDuration"] = 0.5f,
    ["AttackCooldown"] = 2.0f,
    ["AimLerp"] = 2.0f };
  private bool active = false;
  private bool isGrounded = true;
  private bool isAttacking = false;
  private float jumpTimer = 0.0f;
  private float attackTimer = 0.0f;
  private float attackCooldown = 0.0f;
  private Vector3 aimedDirection;
  private Vector3 attackPoint;


  public void Awake() {
    // Initialize references
    WJ = GetComponent<WorldObject>();

    speedParticleGenerator = Instantiate(speedParticleGeneratorPfb).GetComponent<ParticleSystem>();
    speedParticleGenerator.Stop();
  }

  // #endregion


  // #region - Main

  public void Update() {
    // Update timers
    if (isAttacking) {
      attackTimer = Mathf.Max(attackTimer - Time.deltaTime, 0.0f);
      if (attackTimer <= 0.0f) isAttacking = false;
    } else {
      attackCooldown = Mathf.Max(attackCooldown - Time.deltaTime, 0.0f);
      if (isGrounded) jumpTimer = Mathf.Max(jumpTimer - Time.deltaTime, 0.0f);
    }

    // Aim in pointing direction while airborne
    if (!isGrounded) {
      Vector3 dir = aimedDirection;
      float aimStrength = WJ.moveResist * stats["AimLerp"] * Time.deltaTime;
      if (attackCooldown > 0.0f) {
        dir = (attackPoint - transform.position).normalized;
        aimStrength *= 0.5f;
      } else if (isAttacking) aimStrength *= 2.0f;
      Quaternion dirRot = Quaternion.LookRotation(dir, transform.up);
      transform.rotation = Quaternion.Lerp(transform.rotation, dirRot, aimStrength);
    }

    // Activate speed particle effect when in the air for attacking
    if (isAttacking && !isGrounded) {
      Quaternion speedDir = Quaternion.LookRotation(-WJ.rb.velocity, Vector3.up);
      speedParticleGenerator.transform.position = transform.position;
      speedParticleGenerator.transform.rotation = speedDir;
      if (!speedParticleGenerator.isPlaying) speedParticleGenerator.Play();
    } else if (speedParticleGenerator.isPlaying) speedParticleGenerator.Stop();

    // Aim and move towards attack point
    if (isAttacking) {
      Vector3 dir = (attackPoint - transform.position).normalized;
      float jumpStrength = 2.0f * stats["AttackStrength"] * WJ.moveResist * Time.deltaTime;
      float aimStrength = WJ.moveResist * 2.0f * stats["AimLerp"] * Time.deltaTime;
      WJ.rb.velocity = WJ.rb.velocity + dir * jumpStrength;
    }
  }


  public void moveInDirection(Vector3 dir, float force) {
    // If grounded, not attacking, and can jump
    if (canMove(dir)) {

      // Hop in the given direction
      float jumpStrength = force * stats["MovementStrength"] * WJ.moveResist;
      WJ.rb.velocity = WJ.rb.velocity + new Vector3(0.0f, jumpStrength * JUMP_Z_PCT, 0.0f);
      WJ.rb.velocity = WJ.rb.velocity + dir * jumpStrength;

      // Update variables
      jumpTimer = stats["JumpCooldown"];
      isGrounded = false;
    }
  }

  public void aimAtPosition(Vector3 pos, float strength) {
    // Aim towards target point
    aimedDirection = pos - transform.position;
  }

  public void attack(WorldObject targetWJ, Vector3 aimedPos) {
    // If grounded, and can attack
    if (canAttack(targetWJ, aimedPos)) {

      // Jump towards target
      Vector3 dir = (aimedPos - transform.position).normalized;
      float jumpStrength = stats["AttackStrength"] * WJ.moveResist;
      WJ.rb.velocity = WJ.rb.velocity + dir * jumpStrength;

      // Update variables
      attackPoint = aimedPos;
      attackTimer = stats["AttackDuration"];
      attackCooldown = stats["AttackCooldown"];
      jumpTimer = stats["JumpCooldown"];
      isGrounded = false;
      isAttacking = true;


      // Setup particle generator
      if (!speedParticleGenerator.isPlaying) speedParticleGenerator.Play();
    }
  }


  public bool canMove(Vector3 moveDir) => isGrounded && !isAttacking && jumpTimer <= 0.0f;

  public bool canAttack(WorldObject targetWJ, Vector3 aimedPos) => isGrounded && !isAttacking && attackCooldown <= 0.0f;

  public void setActive(bool active_) => active = active_;

  public bool getActive() => active;

  public StatList getStats() => stats;


  public void OnCollisionEnter(Collision collision) {
    // When hit ground becoming grounded
    if (collision.gameObject.tag == "Terrain") isGrounded = true;

    // Create particles
    if (WJ.rb.velocity.magnitude >= particleLimit) {
      float mult = Mathf.Min(0.5f, WJ.rb.velocity.magnitude / particleLimit - 1f) * (0.2f / 0.5f) + 0.8f;
      GameObject particles = Instantiate(rockParticleGeneratorPfb);
      particles.transform.position = collision.contacts[0].point;
      particles.transform.localScale = Vector3.one * mult * (isAttacking ? 1.0f : 0.35f);
      particles.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal, Vector3.up);
    }

    // If hit something while attacking then stop
    if (isAttacking) {
      isGrounded = true;
      isAttacking = false;
    }
  }

  public void OnCollisionStay(Collision collision) {
    // If dragging along ground while attacking then stop if not moving
    if ((!isGrounded || isAttacking) && collision.gameObject.tag == "Terrain") {
      if (WJ.rb.velocity.magnitude < 0.35f) {
        isGrounded = true;
        isAttacking = false;
      }
    }
  }

  // #endregion
}
