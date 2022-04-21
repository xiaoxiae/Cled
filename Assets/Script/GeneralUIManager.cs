using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GeneralUIManager : MonoBehaviour
{
    public HoldPickerManager holdPickerManager;
    public RouteSettingsMenuManager routeSettingsMenuManager;
    public PopupManager popupManager;
    
    public PauseManager pauseManager;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // if we're not paused at all, pause
            if (pauseManager.IsUnpaused())
            {
                pauseManager.Pause(PauseType.Normal);
                return;
            }
            
            // else close popups first
            if (pauseManager.IsPaused(PauseType.Popup)) {
                pauseManager.Unpause(PauseType.Popup);
                popupManager.Close();
                return;
            }
            
            // then hold picker
            if (pauseManager.IsPaused(PauseType.HoldPicker)) {
                pauseManager.Unpause(PauseType.HoldPicker);
                holdPickerManager.Close();
                return;
            }
                
            // then route settings
            if (pauseManager.IsPaused(PauseType.RouteSettings)) {
                pauseManager.Unpause(PauseType.RouteSettings);
                routeSettingsMenuManager.Close();
                return;
            }
            
            // then normal pause
            if (pauseManager.IsPaused(PauseType.Normal)) {
                pauseManager.Unpause(PauseType.Normal);
                return;
            }
        }
    }
}
