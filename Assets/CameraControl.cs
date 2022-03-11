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
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		if (Input.GetKey(KeyCode.LeftControl))
		{
			mouseX *= SlowMultiplier;
			mouseY *= SlowMultiplier;
		}
		
		yRotation -= mouseY;
		yRotation = Mathf.Clamp(yRotation, -90f, +90f);

		// don't move when shift is pressed in edit mode (holds turn)
		if (Input.GetKey(KeyCode.LeftShift) && PlayerControl.CurrentMode == Mode.Edit) return;
		
		transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
		playerBody.Rotate(Vector3.up * mouseX);
    }
}
