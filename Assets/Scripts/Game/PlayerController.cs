using System.Collections;
using UnityEngine;

/// <summary>
/// Class controlling the player's ship movement and fire logic.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>
public class PlayerController : MonoBehaviour {
	/// <summary>
	/// Reference to the bullet prefab
	/// </summary>
	public GameObject bulletPrefab;

	/// <summary>
	/// Ship's acceleration over time.
	/// </summary>
	public float accel;

	/// <summary>
	/// Ship's max speed.
	/// </summary>
	public float maxSpeed;

	/// <summary>
	/// Factor in wich to decrease in ship's speed.
	/// </summary>
	public float damp;

	/// <summary>
	/// Speed of each ship's fired bullet.
	/// </summary>
	public float shotSpeed;

	/// <summary>
	/// Damage of each ship's fired bullet.
	/// </summary>
	public float shotDamage;

	/// <summary>
	/// Vertical offset of the ship's bullets.
	/// </summary>
	public float shootVSep;

	/// <summary>
	/// Horizontal offset of the ship's bullets.
	/// Since the ship has two guns, this is used to alternate between those.
	/// </summary>
	public float shootHSep;

	/// <summary>
	/// Delay between each shot
	/// </summary>
	public float shootTime;

	/// <summary>
	/// Deadzone for the ship's movement
	/// </summary>
	public float movementDeadzone;

	/// <summary>
	/// Factor used to smooth input vector.
	/// /// </summary>
	public float smoothInputFactor;

	/// <summary>
	/// Factor used to smooth the ship's steer when moving. 
	/// Graphical effect only, has no effect on actual steering.
	/// </summary>
	public float smoothMovementSteerFactor;

	/// <summary>
	/// Factor used to smooth the ship's steer when firing. 
	/// Graphical effect only, has no effect on actual steering.
	/// </summary>
	public float smoothFiringSteerFactor;

	/// Reference to the shoot coroutine. Used to know if the ship can shoot again.
	Coroutine shootCoroutine = null;

	/// Shoot from the right cannon? Otherwise shoot from the left one.
	private bool shootRight = false;

	/// If any fire input is beign recieved
	private bool isFiring;

	/// Ships rigidbody
	private Rigidbody rb;

	/// Self-managed velocity vector
	private Vector2 vel;

	/// Smoothed input vector
	private Vector2 smoothInput;

	void Awake() {
		rb = GetComponent<Rigidbody>();
		smoothInput = Vector2.zero;
		isFiring = false;
	}

	/// <summary>
	/// Handle the ship's fire.
	/// </summary>
	void Update() {
		isFiring = false;

		Vector2 shootDir = new Vector2(Input.GetAxis("ShootX"), Input.GetAxis("ShootY"));
		if (shootDir.sqrMagnitude > 0) {
			isFiring = true;

			shootDir.Normalize();
			Quaternion rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(shootDir.x, 0, shootDir.y), Vector3.up), smoothFiringSteerFactor);
			transform.rotation = rotation;

			if (shootCoroutine == null) shootCoroutine = StartCoroutine(Shoot(new Vector2(transform.forward.x, transform.forward.z).normalized));
		}
	}

	/// <summary>
	/// Handle the ship's movement.
	/// </summary>
	void FixedUpdate() {
		// Get initial circular input.
		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

		// Apply deadzone.
		input.x = Mathf.Abs(input.x) > movementDeadzone ? input.x : 0;
		input.y = Mathf.Abs(input.y) > movementDeadzone ? input.y : 0;

		// Clamp input magnitude (for keyboard).
		input = Vector2.ClampMagnitude(input, 1);

		// Smooth input changes over time.
		smoothInput = Vector2.Lerp(this.smoothInput, input, smoothInputFactor);

		// Decrease previous velocity vector.
		vel *= damp;

		// Apply acceleration to velocity.
		vel += smoothInput * accel * Time.fixedDeltaTime;

		// Limit velocity to maxSpeed vector.
		vel = Vector2.ClampMagnitude(vel, maxSpeed);

		// Apply velocity.
		rb.velocity = new Vector3(vel.x, 0, vel.y);

		// Rotate ship to the movement direction if not firing
		if (!isFiring && input.sqrMagnitude > 0.01) {
			Quaternion rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(input.x, 0, input.y), Vector3.up), smoothMovementSteerFactor);
			transform.rotation = rotation;
		}
	}

	/// <summary>
	/// Shoot a bullet in a specified direction. The coroutine manages a 
	///  reference to itself so it is known when another bullet can be shot.
	/// </summary>
	/// <param name="dir">The direction to shoot the bullet. It is expected to be a normalized vector.</param>
	IEnumerator Shoot(Vector2 dir) {
		BulletController bc = Instantiate(bulletPrefab).GetComponent<BulletController>();
		bc.transform.position = transform.position +
			new Vector3(dir.x, 0, dir.y) * shootVSep +
			new Vector3(-dir.y, 0, dir.x) * shootHSep * (shootRight ? -1 : 1);
		bc.speed = shotSpeed;
		bc.damage = shotDamage;
		bc.direction = dir;

		shootRight = !shootRight;

		yield return new WaitForSeconds(shootTime);
		shootCoroutine = null;
	}
}