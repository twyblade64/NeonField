using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionParticleController : MonoBehaviour {
	private ParticleSystem partSys;

	void Awake() {
		partSys = GetComponent<ParticleSystem>();
	}

	void Start() {
		StartCoroutine(Kill());
	}
	

	IEnumerator Kill() {
		yield return new WaitForSeconds(partSys.main.duration);
		Destroy(gameObject);
	}
}
