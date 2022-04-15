using SFB;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class ToolbarMenuManager : MonoBehaviour
{
    public StateImportExportManager StateImportExportManager;

    private bool _forceSave;
    private bool _forceSaveAs;

    private VisualElement _root;

    /// <summary>
    /// Forces a save when either main menu or quit is called.
    /// Should ideally be called after the wall has been modified and the user wanted to quit before it was saved.
    /// </summary>
    public void ForceSave() => SetForceSave(true);

    /// <summary>
    /// Set the forceSave attribute to a given state, along with possibly enabling/disabling the save button.
    /// </summary>
    private void SetForceSave(bool state)
    {
        _forceSave = state;

        // only enable save button if save as is not
        if (state)
        {
            if (!_forceSaveAs)
                _saveButton.SetEnabled(true);
        }
        else
        {
            _saveButton.SetEnabled(false);
        }
    }

    /// <summary>
    /// Forces a save as when either main menu or quit is called.
    /// Should be called if the wall is not saved at all after loading the editor scene.
    /// </summary>
    public void ForceSaveAs() => _forceSaveAs = true;

    private Button _saveButton;
    private Button _saveAsButton;
    private Button _mainMenuButton;
    private Button _quitButton;

    void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        
        var openButton = _root.Q<Button>("open-button");
        MenuUtilities.AddOpenButtonOperation(openButton);
        
        var newButton = _root.Q<Button>("new-button");
        MenuUtilities.AddNewButtonOperation(newButton);

        _saveButton = _root.Q<Button>("save-button");
        _saveButton.SetEnabled(false);
        _saveButton.clicked += () => Save();

        _saveAsButton = _root.Q<Button>("save-as-button");
        _saveAsButton.clicked += () => SaveAs();

        _quitButton = _root.Q<Button>("quit-button");
        _quitButton.clicked += Quit;
    }

    /// <summary>
    /// Attempt to save.
    /// </summary>
    private bool Save()
    {
        if (!StateImportExportManager.Export(PreferencesManager.LastOpenWallPath))
            return false;

        SetForceSave(false);
        return true;
    }

    /// <summary>
    /// Attempt to save as.
    /// </summary>
    private bool SaveAs()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Save As", "", "",
            new[] { new ExtensionFilter("Cled Data Files (.yaml)", "yaml") });

        if (string.IsNullOrEmpty(path))
            return false;
        
        if (!StateImportExportManager.Export(path))
            return false;

        PreferencesManager.LastOpenWallPath = path;

        SetForceSave(false);
        _forceSaveAs = false;
        return true;
    }

    /// <summary>
    /// Attempt to return to the main menu, possibly warning about things not being saved.
    /// </summary>
    private void MainMenu()
    {
        if (EnsureSaved())
            SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Attempt to quit.
    /// </summary>
    private void Quit()
    {
        if (EnsureSaved())
            Application.Quit();
    }

    /// <summary>
    /// Ensure that things are saved by prompting either save or save as dialogues.
    ///
    /// Return true if they are after the function call.
    /// </summary>
    /// <returns></returns>
    private bool EnsureSaved()
    {
        if (_forceSaveAs)
        {
            // TODO: popup here
            
            return SaveAs();
        }

        if (_forceSave)
        {
            // TODO: popup here
            
            return Save();
        }

        return true;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            EnsureSaved();
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}