using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class HoldPickerManager : MonoBehaviour
{
    private VisualElement root;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        root = GetComponent<UIDocument>().rootVisualElement;

        root.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Tab))
        {
            // TODO: if it's not toggled - some other menu might be on
            if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
                root.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Time.timeScale = 0;
                root.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
