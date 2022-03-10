using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController Controller;
    public PlayerControl PlayerControl;
    
    public float Speed = 5f;
    
    public float GravityMultiplier = 1;
    public float SlowMultiplier = 0.6f;
    
    private float gravity;

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // don't move when shift is pressed in edit mode -- holds turn
        Vector3 move;
        if (Input.GetKey(KeyCode.LeftShift) && PlayerControl.CurrentMode == Mode.Edit)
            move = Vector3.zero;
        else
            move = transform.right * x + transform.forward * z;

        gravity += 0.981f * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftControl))
            move *= SlowMultiplier;
        
        // reset gravity if grounded
        if (Controller.isGrounded)
            gravity = 0;
        
        Controller.Move(move * Speed * Time.deltaTime + gravity * Vector3.down * GravityMultiplier);
    }
}