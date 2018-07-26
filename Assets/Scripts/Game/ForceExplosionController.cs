using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceExplosionController : MonoBehaviour {
	public float force;
	public float distance;
	public float duration;

	// Use this for initialization
	void Start () {
		StartCoroutine(DelayedDestroy(duration));
	}

	IEnumerator DelayedDestroy(float time) {
		yield return new WaitForSeconds(time);
		Destroy(gameObject);
	}
}
