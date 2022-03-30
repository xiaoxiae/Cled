using UnityEngine;

public class MovementControl : MonoBehaviour
{
    public CharacterController controller;
    public EditorController editorController;
    public CameraControl cameraControl;

    public float speed = 5f;

    public float gravityMultiplier = 1;

    private float _gravity;

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // don't move when shift is pressed in edit mode -- holds turn
        Vector3 move;
        if (Input.GetKey(KeyCode.LeftShift) && editorController.currentMode == Mode.Holding)
            move = Vector3.zero;
        else
            move = transform.right * x + transform.forward * z;

        // ensure that we're pointing towards where the camera is 
        // not elegant but functional
        move = cameraControl.transform.TransformDirection(move);
        var mag = move.magnitude;
        move.y = 0;
        move = move.normalized * mag;

        _gravity += 0.981f * Time.deltaTime;

        // reset gravity if grounded
        if (controller.isGrounded)
            _gravity = 0;

        controller.Move(move * (speed * Time.deltaTime) + Vector3.down * (_gravity * gravityMultiplier));
    }
}