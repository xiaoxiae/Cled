using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 7f;

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool spacePressed = Input.GetKey(KeyCode.Space);
        bool ctrlPressed = Input.GetKey(KeyCode.LeftControl);

        float upDownSpeed = speed * 0.03f;

        float adjustedSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift))
            adjustedSpeed *= 1.6f;

        Vector3 move = transform.right * x + transform.forward * z +
                       transform.up * ((spacePressed ? upDownSpeed : 0) + (ctrlPressed ? -upDownSpeed : 0));

        controller.Move(move * (adjustedSpeed * Time.deltaTime));
    }
}