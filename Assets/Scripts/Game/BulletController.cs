using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {
	public float speed;
	public float damage;
	public Vector2 direction;
	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);
		transform.rotation = rotation;
		rb.velocity = new Vector3(direction.x, 0, direction.y)*speed;
		StartCoroutine(CheckBounds());
	}
	
	IEnumerator CheckBounds() {
		while (true) {
			if (transform.position.sqrMagnitude > 20*20) {
				Destroy(this.gameObject);
				break;
			}
			yield return new WaitForSeconds(1f);
		}
	}
}
