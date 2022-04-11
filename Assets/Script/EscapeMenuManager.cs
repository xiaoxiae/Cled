using SFB;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class EscapeMenuManager : MonoBehaviour
{
    public StateImportExportManager StateImportExportManager;

    private bool _forceSave;
    private bool _forceSaveAs;

    private VisualElement root;

    /// <summary>
    /// Forces a save when either main menu or quit is called.
    /// Should ideally be called after the wall has been modified and the user wanted to quit before it was saved.
    /// </summary>
    public void ForceSave() => SetForceSave(true);

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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        root = GetComponent<UIDocument>().rootVisualElement;

        _saveButton = root.Q<Button>("save-button");
        _saveButton.SetEnabled(false);
        _saveButton.clicked += () => Save();

        _saveAsButton = root.Q<Button>("save-as-button");
        _saveAsButton.clicked += () => SaveAs();

        _mainMenuButton = root.Q<Button>("main-menu-button");
        _mainMenuButton.clicked += MainMenu;

        _quitButton = root.Q<Button>("quit-button");
        _quitButton.clicked += Quit;

        root.visible = false;
    }

    /// <summary>
    /// Attempt to save, returning true if it worked, else false.
    /// </summary>
    private bool Save()
    {
        if (!StateImportExportManager.Export(PreferencesManager.LastOpenWallPath))
            return false;

        SetForceSave(false);
        return true;
    }

    /// <summary>
    /// Attempt to save as, returning true if it worked, else false.
    /// </summary>
    private bool SaveAs()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Save As", "", "",
            new[] { new ExtensionFilter("Cled Data Files (.yaml)", "yaml") });

        if (path != "")
        {
            if (!StateImportExportManager.Export(path))
                return false;

            SetForceSave(false);
            _forceSaveAs = false;
            return true;
        }

        return false;
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
    /// Attempt to quit, possibly warning about things not being saved.
    /// </summary>
    private void Quit()
    {
        if (EnsureSaved())
            Application.Quit();
    }

    /// <summary>
    /// Ensure that things are saved. Return true if they are after the function call.
    /// </summary>
    /// <returns></returns>
    private bool EnsureSaved()
    {
        if (_forceSaveAs)
            return false; // TODO: dialogue for that it's not saved as anything

        if (_forceSave)
            return false; // TODO: dialogue for that there are unsaved changes

        return true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0 && root.visible)
            {
                Time.timeScale = 1;
                root.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (Time.timeScale == 1 && !root.visible)
            {
                Time.timeScale = 0;
                root.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}