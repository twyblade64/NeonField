using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreOnDestroy : MonoBehaviour {
	public int score;

	void OnDestroy() {
		if (ScoreController.instance != null)
			ScoreController.instance.AddScore(score);
	}
}
