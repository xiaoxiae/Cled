using UnityEngine;

/// <summary>
///     Controls UI components with some common key bindings (like Escape or Enter).
/// </summary>
public class UIKeyboardController : MonoBehaviour
{
    public HoldPickerMenu holdPickerMenu;
    public RouteSettingsMenu routeSettingsMenu;
    public SettingsMenu settingsMenu;
    public PopupMenu popupMenu;

    public PauseMenu pauseMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // else close popups first
            if (pauseMenu.IsTypePaused(PauseType.Popup))
            {
                popupMenu.Accept();
                return;
            }

            // then route settings
            if (pauseMenu.IsTypePaused(PauseType.RouteSettings))
            {
                routeSettingsMenu.Accept();
                return;
            }

            // then general settings
            if (pauseMenu.IsTypePaused(PauseType.Settings)) settingsMenu.Accept();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            // if we're not paused at all, pause
            if (pauseMenu.IsAllUnpaused())
            {
                pauseMenu.PauseType(PauseType.Normal);
                return;
            }

            // else close popups first
            if (pauseMenu.IsTypePaused(PauseType.Popup))
            {
                popupMenu.Close();
                return;
            }

            // then hold picker
            if (pauseMenu.IsTypePaused(PauseType.HoldPicker))
            {
                holdPickerMenu.Close();
                return;
            }

            // then route settings
            if (pauseMenu.IsTypePaused(PauseType.RouteSettings))
            {
                routeSettingsMenu.Close();
                return;
            }

            // then general settings
            if (pauseMenu.IsTypePaused(PauseType.Settings))
            {
                settingsMenu.Close();
                return;
            }

            // then normal pause
            if (pauseMenu.IsTypePaused(PauseType.Normal)) pauseMenu.UnpauseType(PauseType.Normal);
        }
    }
}