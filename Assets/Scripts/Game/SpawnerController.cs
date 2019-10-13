using System.Collections;
using UnityEngine;

/// <summary>
/// Prefab spawning utility used to
/// spawn enemies within specified bounds.
/// 
/// The spawning is made on the Y-aligned plane with center on the origin
/// 
/// - Raúl Vera Ortega 2018
/// </summary>
public class SpawnerController : MonoBehaviour {
	/// <summary>
	/// The prefab to spawn.
	/// </summary>
	public GameObject spawnPrefab;

	/// <summary>
	/// Delay between prefab spawn.
	/// </summary>
	public float spawnTime;

	/// <summary>
	/// Ammount of prefabs to spawn each time.
	/// </summary>
	public float spawnAmmount;

	/// <summary>
	/// A rectangle defining where to spawn the prefab.
	/// </summary>
	public Rect spawnArea;

	void Start() {
		StartCoroutine(Spawn());
	}

	IEnumerator Spawn() {
		while (true) {
			yield return new WaitForEndOfFrame();
			for (int i = 0; i < spawnAmmount; ++i) {
				// Random position within spawn area.
				Vector3 pos = new Vector3(spawnArea.x + Random.value * spawnArea.width, 0, spawnArea.x + Random.value * spawnArea.height);
				
				Instantiate(spawnPrefab, pos, spawnPrefab.transform.rotation);
			}
			yield return new WaitForSeconds(spawnTime);
		}
	}
}