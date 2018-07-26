using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public GameObject bulletPrefab;
	public float accel;
	public float maxSpeed;
	public float drag;
	public float shotSpeed;
	public float shotDamage;
	public float shootSep;
	private Vector2 vel;

	public float shootTime;
	Coroutine shootCoroutine = null;

	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 shootDir = new Vector2(Input.GetAxis("ShootX"), Input.GetAxis("ShootY"));
		if (shootDir.sqrMagnitude > 0) {
			Debug.Log("Shoot! "+shootDir);
			shootDir.Normalize();
			Quaternion rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(shootDir.x, 0, shootDir.y), Vector3.up), .5f);
			//rotation.w = 0;
			//rotation.z = 0;
			transform.rotation = rotation;

			if (shootCoroutine == null){
				shootCoroutine = StartCoroutine(Shoot(new Vector2(transform.forward.x,transform.forward.z).normalized));
			}
		}
	}

	void FixedUpdate() {
		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) ;
		//input *= 1.42f;
		input.x = Mathf.Min(Mathf.Max(input.x, -1), 1);
		input.y = Mathf.Min(Mathf.Max(input.y, -1), 1);

		vel += input*accel*Time.deltaTime;
		vel = Vector2.ClampMagnitude(vel, maxSpeed);
		vel.x = Mathf.Min(Mathf.Max(vel.x, -maxSpeed), maxSpeed);
		vel.y = Mathf.Min(Mathf.Max(vel.y, -maxSpeed), maxSpeed);

		rb.velocity = new Vector3(vel.x, 0, vel.y);
		vel.x = vel.x * ((drag)*(1-Mathf.Abs(input.x)) + (Mathf.Abs(input.x)));
		vel.y = vel.y * ((drag)*(1-Mathf.Abs(input.y)) + (Mathf.Abs(input.y)));
	}

	IEnumerator Shoot(Vector2 dir) {
		BulletController bc = Instantiate(bulletPrefab).GetComponent<BulletController>();
		bc.transform.position = transform.position + new Vector3(dir.x, 0, dir.y)*shootSep;
		bc.speed = shotSpeed;
		bc.damage = shotDamage;
		bc.direction = dir;
		yield return new WaitForSeconds(shootTime);
		shootCoroutine = null;
	}
}
