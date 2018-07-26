using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fpsCounterController : MonoBehaviour {
	private Text text;
	private float fps = 1f/60;

	// Use this for initialization
	void Start () {
		text = GetComponent<Text>();
		QualitySettings.vSyncCount = 0;
	}
	
	// Update is called once per frame
	void Update () {
		fps = Mathf.Lerp(fps, Time.deltaTime, .1f);
		text.text = string.Format("FPS: {0:0.00}", + 1f/fps);
	}

}
