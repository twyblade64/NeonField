using UnityEngine;

/// <summary>
/// Class used to have a dynamic camera movement following an
/// specified object.
/// 
/// The movement done is 2-dimensional in the Z plane and it uses 2 bounds:
/// - Target bounds: The space where the followed object is theorically limited
/// - Camera bounds: The space where the camera is limited to move to.
/// 
/// The class first finds the position of the object within the Target bounds and
/// then transforms it to the Camera bounds. If the camera limit is activated, the
/// camera will not go out of the Camera bounds.
/// 
/// - Raul Vera 2018
/// </summary>
public class CameraController : MonoBehaviour {
    /// <summary>
    /// The reference bounds that enclose the target movement.
    /// </summary>
    [SerializeField] public Vector2 _targetBounds;

    /// <summary>
    /// The reference bounds that enclose the camera movement.
    /// </summary>
    [SerializeField] Vector2 _cameraBounds;

    /// <summary>
    /// If true, the camera movement will be limited to the camera bounds.
    /// </summary>
    [SerializeField] bool _limitCamera;

    /// <summary>
    /// The target to follow.
    /// </summary>
    [SerializeField] GameObject _followTarget;

    /// Camera offset if we want the camera to be anywhere else than the origin
    Vector3 _startPosition;

    /// Reference to the camera
    Camera _camera;

    /// <summary>
    /// Setup some initial parameters
    /// </summary>
    void Awake() {
        _camera = gameObject.GetComponent<Camera>();
        _startPosition = transform.position;
    }

    /// <summary>
    /// Update the camera position.
    /// It is in LateUpdate so it moves to the final target position
    /// </summary>
    void LateUpdate() {
        Vector2 relativePos;

        // Get the relative position of the follow target within the target bounds space
        relativePos.x = (_followTarget.transform.position.x - _startPosition.x) / _targetBounds.x;
        relativePos.y = (_followTarget.transform.position.z - _startPosition.z) / _targetBounds.y;

        // Clamp ratio between [-1,1]
        if (_limitCamera) {
            relativePos.x = Mathf.Max(-1, Mathf.Min(relativePos.x, 1));
            relativePos.y = Mathf.Max(-1, Mathf.Min(relativePos.y, 1));
        }

        // Lets move the relative position to the camera bounds space
        relativePos.x *= _cameraBounds.x;
        relativePos.y *= _cameraBounds.y;

        // Update the current camera position
        Vector3 pos = _startPosition;
        pos.x += relativePos.x;
        pos.z += relativePos.y;
        transform.position = pos;
    }
}