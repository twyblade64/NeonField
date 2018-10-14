using UnityEngine;

/// <summary>
/// Class used to move objsects around the worldspace with the mouse.
/// It was used to have an object with a ForceGenerator component and
/// test the generated force in different locations.
/// Raúl Vera Ortega - 2018
/// </summary>
public class MousePositionController : MonoBehaviour {
	/// <summary>
	/// Find the position of the mouse in worldspace and move the current object's position there
	/// </summary>
	void Update() {
		// The current reference plane in the scene has an origin in Zero and a normal in positive Y-axis
		Plane plane = new Plane(Vector3.up, Vector3.zero);

		Ray mouseScreenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		float rayPlaneInter;
		if (plane.Raycast(mouseScreenRay, out rayPlaneInter)) {
			transform.position = mouseScreenRay.origin + mouseScreenRay.direction * rayPlaneInter;
		}
	}
}