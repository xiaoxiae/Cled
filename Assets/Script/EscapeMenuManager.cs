using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class EscapeMenuManager : MonoBehaviour
{
    public GameObject EscapeMenu;
    
    private bool _forceSave;
    private bool _forceSaveAs;

    /// <summary>
    /// Forces a save when either main menu or quit is called.
    /// Should ideally be called after the wall has been modified and the user wanted to quit before it was saved.
    /// </summary>
    private void ForceSave() => _forceSave = true;
    
    /// <summary>
    /// Forces a save when either main menu or quit is called.
    /// Should be called if the wall is not saved at all.
    /// </summary>
    private void ForceSaveAs() => _forceSaveAs = true;
    
    void Start()
    {
        EscapeMenu.SetActive(false);
	    Cursor.lockState = CursorLockMode.Locked;
        
        var root = EscapeMenu.GetComponent<UIDocument>().rootVisualElement;
        
        // TODO: disable if _forceSave is false
        // TODO: should set _forceSave to true
        var saveButton = root.Q<Button>("save-button");
        
        // TODO: should set _forceSaveAs to true
        var saveAsButton = root.Q<Button>("save-as-button");
        
        var mainMenuButton = root.Q<Button>("main-menu-button");
        
        var quitButton = root.Q<Button>("quit-button");
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
