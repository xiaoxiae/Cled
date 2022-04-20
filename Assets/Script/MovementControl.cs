using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MovementControl : MonoBehaviour
{
    public CharacterController controller;
    public EditorController editorController;
    public CameraControl cameraControl;

    private const float ForwardBackwardSpeed = 5;
    private const float SideSpeed = 4;

    // flying-related variables
    private bool _flying;
    private float _flyingSpeed;  // normalized from 0 to 1
    private const float FlyingSmoothness = 0.15f;
    private const float FlyingMultiplier = 0.1f;

    // gravity-related things
    private const float GravityMultiplier = 0.25f;
    private float _gravity;

    /// <summary>
    /// Get the position the player.
    /// </summary>
    public Vector3 GetPosition() => transform.position;

    /// <summary>
    /// Set the position the player.
    /// </summary>
    public void SetPosition(Vector3 position) => transform.position = position;

    void Update()
    {
        float x = Input.GetAxis("Horizontal") * SideSpeed;
        float z = Input.GetAxis("Vertical") * ForwardBackwardSpeed;

        // flying-related stuff
        if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift))
        {
            _flying = true;
            _flyingSpeed = Mathf.Lerp(_flyingSpeed, 0, FlyingSmoothness);
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            _flying = true;
            _flyingSpeed = Mathf.Lerp(_flyingSpeed, 1, FlyingSmoothness);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            _flying = true;
            _flyingSpeed = Mathf.Lerp(_flyingSpeed, -1, FlyingSmoothness);
        }
        else
        {
            _flyingSpeed = Mathf.Lerp(_flyingSpeed, 0, FlyingSmoothness);
        }

        // when grounded, reset flying (but only if we're not trying to lift off)
        if (!Input.GetKey(KeyCode.Space) && controller.isGrounded)
        {
            _flying = false;
            _gravity = 0;
        }

        // don't move when middle button is pressed in edit mode (holds turn) and when ctrl is pressed (ctrl + s)
        if (Input.GetMouseButton(2) || Input.GetKey(KeyCode.LeftControl))
        {
            controller.Move(Vector3.zero);
            return;
        }

        Vector3 move = transform.right * x + transform.forward * z;

        // ensure that we're pointing towards where the camera is 
        // not elegant but functional
        move = cameraControl.transform.TransformDirection(move);
        var mag = move.magnitude;
        move.y = 0;
        move = move.normalized * mag;

        if (!_flying)
        {
            _gravity += 0.981f * Time.deltaTime;
            controller.Move(move * Time.deltaTime + Vector3.down * (_gravity * GravityMultiplier));
        }
        else
            controller.Move(move * Time.deltaTime + Vector3.up * _flyingSpeed * FlyingMultiplier);
    }
}