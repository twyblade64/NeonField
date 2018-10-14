using System.Collections;
using UnityEngine;

/// <summary>
/// Standard script for moving an object in a direction.
/// It is destroyed automatically when reaching the specified destruction distance.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>
public class BulletController : MonoBehaviour {
	public float speed;
	public float damage;
	public Vector2 direction;
	public float destructionDistance;
	private Rigidbody rb;

	void Start() {
		rb = GetComponent<Rigidbody>();
		Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);
		transform.rotation = rotation;
		rb.velocity = new Vector3(direction.x, 0, direction.y) * speed;
		StartCoroutine(CheckBounds());
	}

	IEnumerator CheckBounds() {
		while (true) {
			if (transform.position.sqrMagnitude > destructionDistance * destructionDistance) {
				Destroy(this.gameObject);
				break;
			}
			yield return new WaitForSeconds(1f);
		}
	}
}