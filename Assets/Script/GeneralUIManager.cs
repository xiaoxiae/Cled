using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GeneralUIManager : MonoBehaviour
{
    public HoldPickerManager holdPickerManager;
    public RouteSettingsMenuManager routeSettingsMenuManager;
    public SettingsMenuManager settingsMenuManager;
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
            if (pauseManager.IsAllUnpaused())
            {
                pauseManager.PauseType(PauseType.Normal);
                return;
            }
            
            // else close popups first
            if (pauseManager.IsTypePaused(PauseType.Popup)) {
                pauseManager.UnpauseType(PauseType.Popup);
                popupManager.Close();
                return;
            }
            
            // then hold picker
            if (pauseManager.IsTypePaused(PauseType.HoldPicker)) {
                pauseManager.UnpauseType(PauseType.HoldPicker);
                holdPickerManager.Close();
                return;
            }
                
            // then route settings
            if (pauseManager.IsTypePaused(PauseType.RouteSettings)) {
                pauseManager.UnpauseType(PauseType.RouteSettings);
                routeSettingsMenuManager.Close();
                return;
            }
            
            // then general settings
            if (pauseManager.IsTypePaused(PauseType.Settings)) {
                pauseManager.UnpauseType(PauseType.Settings);
                settingsMenuManager.Close();
                return;
            }
            
            // then normal pause
            if (pauseManager.IsTypePaused(PauseType.Normal)) {
                pauseManager.UnpauseType(PauseType.Normal);
                return;
            }
        }
    }
}
