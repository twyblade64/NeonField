using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePositionController : MonoBehaviour {
	// Update is called once per frame
	void Update () {
		Ray mouseScreenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Plane plane = new Plane(Vector3.up, Vector3.zero);
		float rayPlaneInter;
		if (plane.Raycast(mouseScreenRay, out rayPlaneInter)) {
			transform.position = mouseScreenRay.origin + mouseScreenRay.direction * rayPlaneInter;
		}
	}
}
