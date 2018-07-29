using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    public float spaceRatio;
    public float marginRatio;
    public GameObject followObject;
    private Vector3 startPosition;

    private Camera m_camera;
	// Use this for initialization
	void Start () {
        m_camera = gameObject.GetComponent<Camera>();
        startPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        float ratioX = followObject.transform.position.x/(m_camera.orthographicSize * marginRatio * m_camera.aspect);
        float ratioY = followObject.transform.position.z/(m_camera.orthographicSize * marginRatio);

        ratioX = Mathf.Max(Mathf.Min(ratioX, 1), -1) * (spaceRatio-1);
        ratioY = Mathf.Max(Mathf.Min(ratioY, 1), -1) * (spaceRatio-1);

        Vector3 pos = startPosition;
        pos.x += m_camera.orthographicSize * ratioX * m_camera.aspect;
        pos.z += m_camera.orthographicSize * ratioY;
        transform.position = pos;
    }
}
