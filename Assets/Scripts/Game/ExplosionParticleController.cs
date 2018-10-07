using System.Collections;
using UnityEngine;

/// <summary>
/// Automatically destroy particle object after
/// the end of its lifetime.
/// - Raul Vera 2018
/// </summary>
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
