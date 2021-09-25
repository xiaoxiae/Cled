using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 10f;

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        float adjustedSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift))
            adjustedSpeed *= 1.5f;

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * (adjustedSpeed * Time.deltaTime));
    }
}
