using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour {
	public GameObject spawnPrefab;
	public float spawnTime;
	public float spawnAmmount;
	public Rect spawnArea;

	// Use this for initialization
	void Start () {
		StartCoroutine(Spawn());
	}

	IEnumerator Spawn() {
		while (true) {
			for (int i = 0; i < spawnAmmount; ++i) {
				Vector3 pos = new Vector3(spawnArea.x + Random.value * spawnArea.width, 0 , spawnArea.x + Random.value * spawnArea.height);
				Instantiate(spawnPrefab, pos, Quaternion.identity);
			}
			yield return new WaitForSeconds(spawnTime);
		}
	}
}
