using System;
using UnityEngine;

/// <summary>
///     A class for controlling the movement of the camera.
/// </summary>
public class CameraController : MonoBehaviour, IResetable
{
    public EditorModeManager editorModeManager;
    public PauseMenu pauseMenu;

    public float mouseSensitivity = 100f;

    /// <summary>
    ///     The orientation the camera.
    ///     Since the camera only rotates around and up/down, two numbers can be used to determine it.
    /// </summary>
    public Vector2 Orientation
    {
        get => transform.rotation.eulerAngles;
        set => transform.rotation = Quaternion.Euler(value.x, value.y, 0);
    }

    private void Update()
    {
        if (!Preferences.Initialized)
            return;

        // don't move the camera when we're paused
        if (pauseMenu.IsAnyPaused())
            return;

        // don't move when middle button is pressed in edit mode (holds turn)
        if (Input.GetMouseButton(2) && editorModeManager.CurrentMode == EditorModeManager.Mode.Holding)
            return;

        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // turn up/down relative to self
        transform.Rotate(Vector3.right * -mouseY, Space.Self);

        // rotate left/right around relative to world
        transform.Rotate(Vector3.up * mouseX, Space.World);

        // clamp +- 90
        // please don't ask, I don't know
        if (Math.Abs(transform.localRotation.eulerAngles.z - 180.0) < 0.1)
        {
            var angle = Vector3.Angle(transform.forward, Vector3.down);

            if (angle > 90)
                angle = -(180 - angle);

            transform.Rotate(Vector3.right * -angle, Space.Self);
        }
    }

    /// <summary>
    ///     Reset the camera orientation.
    /// </summary>
    public void Reset()
    {
        Orientation = Vector2.zero;
    }

    /// <summary>
    ///     Look at a certain point.
    /// </summary>
    public void PointCameraAt(Vector3 point)
    {
        transform.LookAt(point);
    }
}