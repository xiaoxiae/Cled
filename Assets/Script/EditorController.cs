using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// The general controller of picking up and placing down holds, cycling through them, etc.
/// </summary>
public class EditorController : MonoBehaviour
{
    public HoldStateManager holdStateManager;
    public CameraController cameraController;
    public HighlightManager highlightManager;
    public RouteManager routeManager;
    public PopupMenu popupMenu;
    public HoldPickerMenu holdPickerMenu;
    public RouteSettingsMenu routeSettingsMenu;
    public EditorModeManager editorModeManager;
    public RouteViewMenu routeViewMenu;

    private const float HoldRotationSensitivity = 5f;

    void Awake()
    {
        // deselect routes when mode changes from route
        editorModeManager.AddModeChangeCallback(mode =>
        {
            if (mode != EditorModeManager.Mode.Route)
                routeManager.DeselectRoute();
        });
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
            if (editorModeManager.CurrentMode != EditorModeManager.Mode.Holding &&
                holdPickerMenu.GetPickedHolds().Count == 0)
                popupMenu.CreateInfoPopup("No holds selected, can't start holding!");
            else
            {
                editorModeManager.CurrentMode = editorModeManager.CurrentMode == EditorModeManager.Mode.Holding
                    ? EditorModeManager.Mode.Normal
                    : EditorModeManager.Mode.Holding;

                if (editorModeManager.CurrentMode == EditorModeManager.Mode.Normal)
                {
                    routeManager.RemoveHold(holdStateManager.HeldHold);
                    holdStateManager.StopHolding();
                }
                else
                {
                    holdStateManager.InstantiateToHolding(holdPickerMenu.CurrentlySelectedHold);
                }
            }
        }
    }

    /// <summary>
    /// Called each update when nothing was hit.
    /// </summary>
    void NoHitBehavior()
    {
        switch (editorModeManager.CurrentMode)
        {
            case EditorModeManager.Mode.Holding:
                highlightManager.UnhighlightAll();
                holdStateManager.DisableHeld();
                break;
            case EditorModeManager.Mode.Normal:
                highlightManager.UnhighlightAll();
                break;
            case EditorModeManager.Mode.Route:
                highlightManager.UnhighlightAll(HighlightType.Main);
                break;
        }
    }

    /// <summary>
    /// Called each update when the wall was hit.
    /// </summary>
    void WallHitBehavior(RaycastHit hit)
    {
        switch (editorModeManager.CurrentMode)
        {
            case EditorModeManager.Mode.Holding:
                highlightManager.UnhighlightAll();
                HoldingHitControls(hit);
                break;
            case EditorModeManager.Mode.Normal:
                highlightManager.UnhighlightAll();
                break;
            case EditorModeManager.Mode.Route:
                highlightManager.UnhighlightAll(HighlightType.Main);
                break;
        }
    }

    /// <summary>
    /// Called each update when a hold was hit
    /// </summary>
    void HoldHitBehavior(RaycastHit hit, GameObject hold)
    {
        switch (editorModeManager.CurrentMode)
        {
            case EditorModeManager.Mode.Holding:
                highlightManager.UnhighlightAll();
                HoldingHitControls(hit);
                break;
            case EditorModeManager.Mode.Normal:
                // if some other hold is highlighted, unhighlight it
                if (!highlightManager.IsHighlighted(hold))
                    highlightManager.UnhighlightAll();

                NormalRouteHoldHitControls(hold);
                break;
            case EditorModeManager.Mode.Route:
                // if some other hold is highlighted, unhighlight it
                if (!highlightManager.IsHighlighted(hold))
                    highlightManager.UnhighlightAll(HighlightType.Main);

                NormalRouteHoldHitControls(hold);

                // CTRL+LMB or SHIFT+LMB click toggles a hold to be in the route
                if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && Input.GetKey(KeyCode.LeftControl))
                {
                    routeManager.ToggleHold(routeManager.SelectedRoute, hold, holdStateManager.GetHoldBlueprint(hold));

                    // if the route has no more holds, switch to normal mode
                    if (routeManager.SelectedRoute.IsEmpty())
                        editorModeManager.CurrentMode = EditorModeManager.Mode.Normal;
                }

                break;
        }
    }

    /// <summary>
    /// Called each update at the very end.
    /// </summary>
    void CombinedBehaviorAfter()
    {
        if (editorModeManager.CurrentMode == EditorModeManager.Mode.Holding)
        {
            if (Input.GetKeyDown(KeyCode.B))
                routeManager.ToggleStarting(holdStateManager.HeldHold,
                    holdStateManager.GetHoldBlueprint(holdStateManager.HeldHold));

            if (Input.GetKeyDown(KeyCode.T))
                routeManager.ToggleEnding(holdStateManager.HeldHold,
                    holdStateManager.GetHoldBlueprint(holdStateManager.HeldHold));

            // rotate hold on middle mouse button
            if (Input.GetMouseButton(2))
                holdStateManager.RotateHeld(Input.GetAxis("Mouse X") * HoldRotationSensitivity * Time.deltaTime);

        }

        if (editorModeManager.CurrentMode is EditorModeManager.Mode.Holding or EditorModeManager.Mode.Normal) {
            var mouseDelta = Input.mouseScrollDelta.y;

            if (mouseDelta != 0)
            {
                if (mouseDelta < 0)
                    holdPickerMenu.MoveToPreviousHold();
                else
                    holdPickerMenu.MoveToNextHold();

                // only swap if more than one hold is selected (it is pointless to swap to itself)
                if (holdPickerMenu.GetPickedHolds().Count != 1)
                {
                    routeManager.RemoveHold(holdStateManager.HeldHold);
                    holdStateManager.InstantiateToHolding(holdPickerMenu.CurrentlySelectedHold, true);
                }
            }
        }

        // always highlight the current route
        if (editorModeManager.CurrentMode == EditorModeManager.Mode.Route)
            highlightManager.HighlightRoute(routeManager.SelectedRoute, true);
    }

    /// <summary>
    /// Controls for holding mode when a hold/wall is hit.
    /// </summary>
    void HoldingHitControls(RaycastHit hit)
    {
        // make sure that the hold is enabled when holding
        holdStateManager.EnableHeld();

        // when in holding mode, move the held hold accordingly
        holdStateManager.MoveHeldToHit(hit);

        // left click: place held hold and go to normal mode 
        if (Input.GetMouseButtonDown(0))
        {
            holdStateManager.PutDown();
            editorModeManager.CurrentMode = EditorModeManager.Mode.Normal;
        }

        // ctrl + r/del - delete the entire route
        if (Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Delete)))
        {
            var hold = holdStateManager.HeldHold;

            Route route = routeManager.GetRouteWithHold(hold);

            foreach (var routeHold in route.Holds)
            {
                routeManager.RemoveHold(routeHold);
                holdStateManager.Remove(routeHold, true);
            }

            editorModeManager.CurrentMode = EditorModeManager.Mode.Normal;
        }
        // r/del - delete the held hold and switch to normal mode
        else if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Delete))
        {
            routeManager.RemoveHold(holdStateManager.HeldHold);

            holdStateManager.StopHolding();
            editorModeManager.CurrentMode = EditorModeManager.Mode.Normal;
        }
        
        // h - flip hold
        else if (Input.GetKeyDown(KeyCode.H))
            holdStateManager.FlipHold(holdStateManager.HeldHold);
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
            editorModeManager.CurrentMode = EditorModeManager.Mode.Holding;

            holdStateManager.PickUp(hold);

            cameraController.PointCameraAt(hold.transform.position);
        }

        if (Input.GetKeyDown(KeyCode.B))
            routeManager.ToggleStarting(hold, holdStateManager.GetHoldBlueprint(hold));

        if (Input.GetKeyDown(KeyCode.T))
            routeManager.ToggleEnding(hold, holdStateManager.GetHoldBlueprint(hold));

        Route route = routeManager.GetOrCreateRouteWithHold(hold, holdStateManager.GetHoldBlueprint(hold));

        // ctrl + r/del - delete the entire route
        if (Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Delete)))
        {
            foreach (var routeHold in route.Holds)
            {
                routeManager.RemoveHold(routeHold);
                holdStateManager.Remove(routeHold, true);
            }
        }
        
        // r/del - delete hold
        else if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Delete))
        {
            routeManager.RemoveHold(hold);
            holdStateManager.Remove(hold, true);
        }
        
        // h - flip hold
        else if (Input.GetKeyDown(KeyCode.H))
            holdStateManager.FlipHold(hold);

        // if we delete the current hold and the route has no more holds, switch to normal mode
        if (routeManager.SelectedRoute != null && routeManager.SelectedRoute.IsEmpty())
            editorModeManager.CurrentMode = EditorModeManager.Mode.Normal;

        // right click for selecting a route (unless already in route mode, in which ctrl + click toggles a hold)
        if (Input.GetMouseButtonDown(1) && !(editorModeManager.CurrentMode == EditorModeManager.Mode.Route &&
                                             Input.GetKey(KeyCode.LeftControl)))
        {
            var clickedRoute = routeManager.GetOrCreateRouteWithHold(hold, holdStateManager.GetHoldBlueprint(hold));

            if (routeManager.SelectedRoute != clickedRoute)
            {
                routeManager.SelectRoute(clickedRoute);
                highlightManager.UnhighlightAll();
                editorModeManager.CurrentMode = EditorModeManager.Mode.Route;
            }
            else
            {
                routeSettingsMenu.Show();
            }
        }

        // secondary highlight the hold we're looking at
        highlightManager.HighlightRoute(route);

        // highlight the hold we're looking at
        highlightManager.Highlight(hold, HighlightType.Main);
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

            if (holdStateManager.IsPlaced(hitObject))
                HoldHitBehavior(hit, hitObject);
            else
                WallHitBehavior(hit);
        }
        else
            NoHitBehavior();

        CombinedBehaviorAfter();
    }
}