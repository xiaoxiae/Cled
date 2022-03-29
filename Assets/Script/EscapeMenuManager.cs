using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class EscapeMenuManager : MonoBehaviour
{
    public GameObject EscapeMenu;
    
    void Start()
    {
        EscapeMenu.SetActive(false);
	    Cursor.lockState = CursorLockMode.Locked;
        
        var root = EscapeMenu.GetComponent<UIDocument>().rootVisualElement;
        
        // TODO :)
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
                EscapeMenu.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Time.timeScale = 0;
                EscapeMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
