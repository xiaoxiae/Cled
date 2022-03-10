using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Mode
{
    Normal,
    Edit
}

public class PlayerControl : MonoBehaviour
{
    public Mode CurrentMode = Mode.Normal;

    void Update()
    {
        // toggle between edit and normal with e press
        if (Input.GetKeyDown(KeyCode.E))
            CurrentMode = CurrentMode == Mode.Edit ? Mode.Normal : Mode.Edit;
    }
}