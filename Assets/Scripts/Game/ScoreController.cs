using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Class managing the score in the game
/// 
/// - Raul Vera 2018
/// </summary>
public class ScoreController : MonoBehaviour {
	/// <summary>
	/// Lazy static reference to an instance of this class.
	/// </summary>
	public static ScoreController instance;

	/// <summary>
	/// Reference to a text mesh used to display the current score number.
	/// </summary>
	public TextMeshProUGUI scoreText;

	private int currentScore;
	
	void Start () {
		instance = this;
		UpdateScore();
	}

	public void AddScore(int score) {
		currentScore += score;
		UpdateScore();
	}

	public void UpdateScore() {
		scoreText.text = String.Format("{0:00000000.}", currentScore);
	}
}
