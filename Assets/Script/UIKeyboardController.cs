using System.Collections.Generic;
using Unity.VisualScripting;
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
    public NewProjectDialogue newProjectDialogue;
    public PauseMenu pauseMenu;

    private List<(IAcceptable, PauseType)> acceptingOrder;
    private List<(IClosable, PauseType)> closingOrder;

    private void Start()
    {
        acceptingOrder = new List<(IAcceptable, PauseType)>
        {
            (popupMenu, PauseType.Popup),
            (routeSettingsMenu, PauseType.RouteSettings),
            (settingsMenu, PauseType.Settings),
            (newProjectDialogue, PauseType.NewProjectDialogue),
        };

        closingOrder = new List<(IClosable, PauseType)>
        {
            (popupMenu, PauseType.Popup),
            (holdPickerMenu, PauseType.HoldPicker),
            (routeSettingsMenu, PauseType.RouteSettings),
            (settingsMenu, PauseType.Settings),
            (newProjectDialogue, PauseType.NewProjectDialogue),
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            foreach (var (acceptable, type) in acceptingOrder)
            {
                if (!pauseMenu.IsTypePaused(type)) continue;
                
                acceptable.Accept();
                return;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            // if we're not paused at all, pause
            if (pauseMenu.IsAllUnpaused())
            {
                pauseMenu.PauseType(PauseType.Normal);
                return;
            }

            foreach (var (closable, type) in closingOrder)
            {
                if (!pauseMenu.IsTypePaused(type)) continue;
                
                closable.Close();
                return;
            }
        }
    }
}