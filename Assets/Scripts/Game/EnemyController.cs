﻿using System.Collections;
using UnityEngine;

/// <summary>
/// Class containing basic behaviour of an enemy.
/// Enemies try to follow the player and can be destroyed if shoot.
/// Explosions are generated on the following events:
/// 	- On creation
/// 	- On hit by bullet
/// 	- On destruction
/// 
/// - Raúl Vera Ortega 2018
/// </summary>
public class EnemyController : MonoBehaviour {
	/// Hit particles prefab
	public GameObject particlesPrefab;

	/// Explosion object prefab
	public GameObject explosionPrefab;

	/// Creation explosion parameters
	public float creationForce;
	public float creationDuration;
	public float creationDistance;

	/// Hit explosion parameters
	public float hitForce;
	public float hitDuration;
	public float hitDistance;

	/// Death explosion parameters
	public float deathForce;
	public float deathDuration;
	public float deathDistance;

	/// Enemy parameteres
	public float accel;
	public float maxSpeed;
	public float life;

	private Vector3 vel;
	private Rigidbody rb;
	private PlayerController player;

	/// <summary>
	/// Setup references and make creation explosion. 
	/// </summary>
	void Start() {
		rb = GetComponent<Rigidbody>();
		player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

		StartCoroutine(LateExplosionCreation(creationForce, creationDuration, creationDistance));
	}

	/// <summary>
	/// Move the enemy
	/// </summary>
	void FixedUpdate() {
		Vector3 dir = (player.transform.position - transform.position).normalized;
		vel += dir * accel * Time.fixedDeltaTime;
		vel = Vector3.ClampMagnitude(vel, maxSpeed);
		rb.velocity = vel;
	}

	/// <summary>
	/// Check bullet collisions and create hit explosions and effects.
	/// Also destroy self if life reached zero.
	/// </summary>
	void OnTriggerEnter(Collider other) {
		if (life <= 0) return;
		if (other.CompareTag("Bullet")) {
			BulletController bullet = other.GetComponent<BulletController>();
			life -= bullet.damage;
			Destroy(bullet.gameObject);

			if (life > 0) {
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				StartCoroutine(LateExplosionCreation(hitForce, hitDuration, hitDistance));
			} else {
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				StartCoroutine(LateExplosionCreationAndDestruction(deathForce, deathDuration, deathDistance));
			}
		}
	}

	IEnumerator LateExplosionCreation(float force, float duration, float distance) {
		yield return new WaitForEndOfFrame();
		ForceExplosionController explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity).GetComponent<ForceExplosionController>();
		explosion.force = force;
		explosion.duration = duration;
		explosion.distance = distance;
	}

	IEnumerator LateExplosionCreationAndDestruction(float force, float duration, float distance) {
		enabled = false;
		yield return new WaitForEndOfFrame();
		ForceExplosionController explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity).GetComponent<ForceExplosionController>();
		explosion.force = force;
		explosion.duration = duration;
		explosion.distance = distance;
		Destroy(gameObject);
	}
}