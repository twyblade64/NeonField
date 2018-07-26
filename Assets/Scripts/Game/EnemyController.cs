using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {
	public GameObject particlesPrefab;
	public GameObject explosionPrefab;
	public float creationForce;
	public float creationDuration;
	public float creationDistance;
	public float hitForce;
	public float hitDuration;
	public float hitDistance;
	public float deathForce;
	public float deathDuration;
	public float deathDistance;

	public float accel;
	public float maxSpeed;
	private Vector3 vel;
	public float life;

	private Rigidbody rb;
	private PlayerController player;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

		ForceExplosionController explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity).GetComponent<ForceExplosionController>();
		explosion.force = creationForce;
		explosion.duration = creationDuration;
		explosion.distance = creationDistance;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 dir = (player.transform.position - transform.position).normalized;
		vel += dir * accel * Time.deltaTime;
		vel = Vector3.ClampMagnitude(vel, maxSpeed);
		rb.velocity = vel;
	}

	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Bullet")) {
			BulletController bullet = other.GetComponent<BulletController>();
			life -= bullet.damage;
			Destroy(bullet.gameObject);

			ForceExplosionController explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity).GetComponent<ForceExplosionController>();
			if (life > 0) {
				explosion.force = hitForce;
				explosion.duration = hitDuration;
				explosion.distance = hitDistance;
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
			} else {
				explosion.force = deathForce;
				explosion.duration = deathDuration;
				explosion.distance = deathDistance;
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Instantiate(particlesPrefab, transform.position, particlesPrefab.transform.rotation);
				Destroy(gameObject);
			}
		}
	}
}
