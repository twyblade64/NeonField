using System.Collections;
using UnityEngine;

/// <summary>
/// Contains parameters for the ForceGeneratorComponent and
/// automatically destroys it after the specified time.
/// - Raul Vera 2018
/// </summary>
public class ForceExplosionController : MonoBehaviour {
	public float force;
	public float distance;
	public float duration;

	void Start () {
		StartCoroutine(DelayedDestroy(duration));
	}

	IEnumerator DelayedDestroy(float time) {
		yield return new WaitForSeconds(time);
		Destroy(gameObject);
	}
}
