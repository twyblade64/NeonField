using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ScoreController : MonoBehaviour {
	public static ScoreController instance;
	public TextMeshProUGUI scoreText;
	private int currentScore;


	// Use this for initialization
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
