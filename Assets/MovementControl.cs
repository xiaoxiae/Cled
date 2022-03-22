using UnityEngine;

public class MovementControl : MonoBehaviour
{
    public CharacterController Controller;
    public EditorController EditorController;
    public CameraControl CameraControl;

    public float Speed = 5f;

    public float GravityMultiplier = 1;

    private float gravity;

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // don't move when shift is pressed in edit mode -- holds turn
        Vector3 move;
        if (Input.GetKey(KeyCode.LeftShift) && EditorController.CurrentMode == Mode.Holding)
            move = Vector3.zero;
        else
            move = transform.right * x + transform.forward * z;

        // ensure that we're pointing towards where the camera is 
        // not elegant but functional
        move = CameraControl.transform.TransformDirection(move);
        var mag = move.magnitude;
        move.y = 0;
        move = move.normalized * mag;

        gravity += 0.981f * Time.deltaTime;

        // reset gravity if grounded
        if (Controller.isGrounded)
            gravity = 0;

        Controller.Move(move * Speed * Time.deltaTime + gravity * Vector3.down * GravityMultiplier);
    }
}