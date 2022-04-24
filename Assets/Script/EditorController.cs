using UnityEngine;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;

public enum Mode
{
    Normal, // the usual mode that we're in
    Holding, // when we're holding a hold
    Route, // when we're looking at an entire route
}

public class EditorController : MonoBehaviour
{
    public HoldStateManager HoldStateManager;
    public CameraControl CameraControl;
    public HighlightManager HighlightManager;
    public RouteManager RouteManager;
    public ToolbarMenuManager ToolbarMenuManager;
    public PopupManager PopupManager;
    public HoldPickerManager HoldPickerManager;
    public RouteSettingsMenuManager RouteSettingsMenuManager;

    private Label _currentModeLabel;

    public Mode currentMode = Mode.Normal;

    private HoldBlueprint _currentlyPickedHold;

    public float mouseSensitivity = 1f;

    void Awake()
    {
        _currentModeLabel = ToolbarMenuManager.GetComponent<UIDocument>().rootVisualElement
            .Q<Label>("current-mode-label");

        SetCurrentMode(Mode.Normal);
    }

    /// <summary>
    /// Set the current mode, updating the UI mode label in the process.
    /// </summary>
    private void SetCurrentMode(Mode mode)
    {
        currentMode = mode;
        _currentModeLabel.text = mode.ToString().ToUpper();

        if (mode != Mode.Route)
            RouteManager.DeselectRoute();
    }

    /// <summary>
    /// Ensure that the currently picked hold is still picked.
    /// If it isn't, pick some other one from the selected ones.
    /// 
    /// If there are no selected ones, return false.
    /// </summary>
    private bool EnsurePickedHold()
    {
        var pickedHolds = HoldPickerManager.GetPickedHolds();

        if (pickedHolds.Count == 0)
            return false;

        if (!pickedHolds.Contains(_currentlyPickedHold))
            _currentlyPickedHold = pickedHolds[0];

        return true;
    }

    /// <summary>
    /// Called each update at the very beginning.
    /// </summary>
    private void CombinedBehaviorBefore()
    {
        // toggle between holding and normal with e press (or from route to holding)
        // also go from Route to Holding
        if (Input.GetKeyDown(KeyCode.E))
        {
            // however, if the would-be-held hold is not picked any more, don't switch to holding mode
            // and display a warning message instead (what would we even pick?)
            if (currentMode != Mode.Holding && !EnsurePickedHold())
                PopupManager.CreateInfoPopup("No holds selected, can't start holding!");
            else
            {
                SetCurrentMode(currentMode == Mode.Holding ? Mode.Normal : Mode.Holding);

                if (currentMode == Mode.Normal)
                {
                    RouteManager.RemoveHold(HoldStateManager.GetHeld());
                    HoldStateManager.SetUnheld(true);
                }
                else
                {
                    if (EnsurePickedHold())
                        HoldStateManager.SetHeld(_currentlyPickedHold);
                }
            }
        }
    }

    /// <summary>
    /// Called each update when nothing was hit.
    /// </summary>
    void NoHitBehavior()
    {
        switch (currentMode)
        {
            case Mode.Holding:
                HighlightManager.UnhighlightAll();
                HoldStateManager.DisableHeld();
                break;
            case Mode.Normal:
                HighlightManager.UnhighlightAll();
                break;
            case Mode.Route:
                HighlightManager.UnhighlightAll(HighlightType.Primary);
                break;
        }
    }

    /// <summary>
    /// Called each update when the wall was hit.
    /// </summary>
    void WallHitBehavior(RaycastHit hit)
    {
        switch (currentMode)
        {
            case Mode.Holding:
                HighlightManager.UnhighlightAll();
                HoldingHitControls(hit);
                break;
            case Mode.Normal:
                HighlightManager.UnhighlightAll();
                break;
            case Mode.Route:
                HighlightManager.UnhighlightAll(HighlightType.Primary);
                break;
        }
    }

    /// <summary>
    /// Called each update when a hold was hit
    /// </summary>
    void HoldHitBehavior(RaycastHit hit, GameObject hold)
    {
        switch (currentMode)
        {
            case Mode.Holding:
                HighlightManager.UnhighlightAll();
                HoldingHitControls(hit);
                break;
            case Mode.Normal:
                // if some other hold is highlighted, unhighlight it
                if (!HighlightManager.IsHighlighted(hold))
                    HighlightManager.UnhighlightAll();

                NormalRouteHoldHitControls(hold);
                break;
            case Mode.Route:
                // if some other hold is highlighted, unhighlight it
                if (!HighlightManager.IsHighlighted(hold))
                    HighlightManager.UnhighlightAll(HighlightType.Primary);

                NormalRouteHoldHitControls(hold);

                // CTRL+LMB or SHIFT+LMB click toggles a hold to be in the route
                if (Input.GetMouseButtonDown(0) &&
                    (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift)))
                {
                    RouteManager.ToggleHold(RouteManager.SelectedRoute, hold, HoldStateManager.GetHoldBlueprint(hold));

                    // if the route has no more holds, switch to normal mode
                    if (RouteManager.SelectedRoute.IsEmpty())
                        SetCurrentMode(Mode.Normal);
                }

                break;
        }
    }

    /// <summary>
    /// Called each update at the very end.
    /// </summary>
    void CombinedBehaviorAfter()
    {
        if (currentMode == Mode.Holding)
        {
            // rotate hold on middle mouse button
            if (Input.GetMouseButton(2))
                HoldStateManager.RotateHeld(Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime);

            var mouseDelta = Input.mouseScrollDelta.y;

            if (mouseDelta != 0)
            {
                var pickedHolds = HoldPickerManager.GetPickedHolds();

                // if the picked holds contain the one in hand, simply go up/down the list
                int newIndex;
                if (pickedHolds.Contains(_currentlyPickedHold))
                    newIndex = (pickedHolds.IndexOf(_currentlyPickedHold) + (mouseDelta < 0 ? -1 : 1) +
                                pickedHolds.Count) % pickedHolds.Count;
                else
                    newIndex = 0;

                // if only one hold is selected, it is quite pointless to swap the holds
                if (pickedHolds.Count != 1)
                {
                    _currentlyPickedHold = pickedHolds[newIndex];

                    RouteManager.RemoveHold(HoldStateManager.GetHeld());
                    HoldStateManager.SetUnheld(true);

                    HoldStateManager.SetHeld(_currentlyPickedHold);
                    HoldStateManager.DisableHeld();
                }
            }
        }

        // always highlight the current route
        if (currentMode == Mode.Route)
            HighlightManager.Highlight(RouteManager.SelectedRoute, true);
    }

    /// <summary>
    /// Controls for holding mode when a hold/wall is hit.
    /// </summary>
    void HoldingHitControls(RaycastHit hit)
    {
        // make sure that the hold is enabled when holding
        HoldStateManager.EnableHeld();

        // when in holding mode, move the held hold accordingly (if we just started)
        HoldStateManager.InterpolateHeldToHit(hit);

        // left click: place held hold and go to normal mode 
        if (Input.GetMouseButtonDown(0))
        {
            HoldStateManager.InterpolateHeldToHit(hit);
            HoldStateManager.PutDown();
            SetCurrentMode(Mode.Normal);
        }

        // r/del - delete the held hold and switch to normal mode
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Delete))
        {
            RouteManager.RemoveHold(HoldStateManager.GetHeld());
            HoldStateManager.SetUnheld(true);

            SetCurrentMode(Mode.Normal);
        }
    }

    /// <summary>
    /// Called when normal and route hit a hold.
    /// </summary>
    void NormalRouteHoldHitControls(GameObject hold)
    {
        // when left clicking, snap back to holding mode and pick it up
        // CTRL+LMB and SHIFT+LMB click behaves differently in route mode, so it's forbidden altogether
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
        {
            SetCurrentMode(Mode.Holding);

            HoldStateManager.PickUp(hold);

            CameraControl.PointCameraAt(hold.transform.position);
        }

        // b/t for bottom/top marks
        if (Input.GetKeyDown(KeyCode.B))
        {
            RouteManager.ToggleStarting(hold, HoldStateManager.GetHoldBlueprint(hold));
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            RouteManager.ToggleEnding(hold, HoldStateManager.GetHoldBlueprint(hold));
        }

        Route route = RouteManager.GetOrCreateRouteWithHold(hold, HoldStateManager.GetHoldBlueprint(hold));

        // r/del - delete hold
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Delete))
        {
            HoldStateManager.Unplace(hold, true);
            RouteManager.RemoveHold(hold);
        }

        // if we delete the current hold and the route has no more holds, switch to normal mode
        if (route.IsEmpty())
            SetCurrentMode(Mode.Normal);

        // right click for route mode
        if (Input.GetMouseButtonDown(1))
        {
            var clickedRoute = RouteManager.GetOrCreateRouteWithHold(hold, HoldStateManager.GetHoldBlueprint(hold));

            if (RouteManager.SelectedRoute != clickedRoute)
            {
                RouteManager.SelectRoute(clickedRoute);
                HighlightManager.UnhighlightAll();
                SetCurrentMode(Mode.Route);
            }
            else
            {
                RouteSettingsMenuManager.Show(RouteManager.SelectedRoute);
            }
        }
        
        // secondary highlight the hold we're looking at
        HighlightManager.Highlight(route);

        // highlight the hold we're looking at
        HighlightManager.Highlight(hold, HighlightType.Primary);
    }

    void Update()
    {
        // when time stops, don't do anything in the editor
        // on the other hand, this can't be a FixedUpdate method because then Inputs don't work well
        if (Time.timeScale == 0)
            return;

        CombinedBehaviorBefore();

        var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out var hit))
        {
            var hitObject = hit.collider.gameObject;

            if (HoldStateManager.IsPlaced(hitObject))
                HoldHitBehavior(hit, hitObject);
            else
                WallHitBehavior(hit);
        }
        else
            NoHitBehavior();

        CombinedBehaviorAfter();
    }
}