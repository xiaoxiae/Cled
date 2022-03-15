using System;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
	public PlayerControl PlayerControl;
	public float mouseSensitivity = 100f;
	public Transform playerBody;

	private float yRotation;
    public float SlowMultiplier = 0.6f;

    void Start()
    {
	    Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
		// don't move when shift is pressed in edit mode (holds turn)
		if (Input.GetKey(KeyCode.LeftShift) && PlayerControl.CurrentMode == Mode.Edit) return;
		
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		if (Input.GetKey(KeyCode.LeftControl))
		{
			mouseX *= SlowMultiplier;
			mouseY *= SlowMultiplier;
		}
		
		transform.Rotate(-Vector3.right * mouseY, Space.Self);
		playerBody.Rotate(Vector3.up * mouseX);
    }
}
