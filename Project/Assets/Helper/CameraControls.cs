using UnityEngine;

public class CameraControls : MonoBehaviour
{

    public float lookSpeedH = 2f;
    public float lookSpeedV = 2f;
    public float zoomSpeed = 2f;
    public float dragSpeed = 6f;

    private float yaw = 0f;
    private float pitch = 0f;

    private void Start()
    {
        transform.position = new Vector3(Environment.Instance.xWidth / 2, Environment.Instance.xWidth / 2, Environment.Instance.zWidth / 2);
        transform.LookAt(new Vector3(Environment.Instance.xWidth / 2, 0, Environment.Instance.zWidth / 2));
    }

    void Update()
    {
        //Look around with Right Mouse
        if (Input.GetMouseButton(1))
        {
            yaw += lookSpeedH * Input.GetAxis("Mouse X");
            pitch -= lookSpeedV * Input.GetAxis("Mouse Y");

            Camera.main.transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        //drag camera around with Middle Mouse
        if (Input.GetMouseButton(2))
        {
            Camera.main.transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0);
        }

        //Zoom in and out with Mouse Wheel
        Camera.main.transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, Space.Self);
    }
}