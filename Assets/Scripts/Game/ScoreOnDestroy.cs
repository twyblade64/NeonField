using UnityEngine;

/// <summary>
/// Component that adds the specified score
/// to the ScoreController when the object is destroyed.
/// 
/// - Raul Vera 2018
/// </summary>
public class ScoreOnDestroy : MonoBehaviour {
	public int score;

	void OnDestroy() {
		if (ScoreController.instance != null)
			ScoreController.instance.AddScore(score);
	}
}
