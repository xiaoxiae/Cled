using UnityEngine;

/// <summary>
///     A class that controls the player's movement.
/// </summary>
public class MovementController : MonoBehaviour, IResetable
{
    private const float ForwardBackwardSpeed = 5;
    private const float SideSpeed = 4;

    private const float FallThreshold = -10;
    private const float FlyingSmoothness = 0.15f;
    private const float FlyingMultiplier = 0.1f;

    // gravity-related things
    private const float GravityMultiplier = 0.25f;
    public CharacterController controller;

    public CameraController cameraController;
    public EditorModeManager editorModeManager;
    public PauseMenu pauseMenu;

    // flying-related variables
    private bool _flying;
    private float _flyingSpeed; // normalized from 0 to 1
    private float _gravity;

    /// <summary>
    ///     The position of the player.
    /// </summary>
    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    /// <summary>
    ///     Whether the player is flying or not.
    /// </summary>
    public bool Flying
    {
        get => _flying;
        set
        {
            if (value)
                _gravity = 0;

            _flying = value;
        }
    }

    private void Update()
    {
        if (!Preferences.Initialized)
            return;

        // don't move when we're paused
        if (pauseMenu.IsAnyPaused())
            return;

        // reset position when falling out of the map
        if (transform.position.y < FallThreshold)
        {
            Reset();
            cameraController.Reset();
            return;
        }

        var x = Input.GetAxis("Horizontal") * SideSpeed;
        var z = Input.GetAxis("Vertical") * ForwardBackwardSpeed;

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

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
            _flyingSpeed = 0;

        // don't move when middle button is pressed in edit mode (holds turn)
        if (Input.GetMouseButton(2) && editorModeManager.CurrentMode == EditorModeManager.Mode.Holding)
        {
            controller.velocity.Set(0, 0, 0);
            _flyingSpeed = 0;
            return;
        }

        var move = transform.right * x + transform.forward * z;

        // ensure that we're pointing towards where the camera
        // not elegant but functional
        move = cameraController.transform.TransformDirection(move);
        var mag = move.magnitude;
        move.y = 0;
        move = move.normalized * mag;

        if (!_flying)
        {
            _gravity += 0.981f * Time.deltaTime;
            controller.Move(move * Time.deltaTime + Vector3.down * (_gravity * GravityMultiplier));
        }
        else
        {
            controller.Move(move * Time.deltaTime + Vector3.up * _flyingSpeed * FlyingMultiplier);
        }
    }

    /// <summary>
    ///     Reset the position of the player
    /// </summary>
    public void Reset()
    {
        // TODO: this is a bit ugly, but the player is 1.8m long and will likely never change...
        Position = new Vector3(0, 0.9f, 0);
        Flying = false;
    }
}