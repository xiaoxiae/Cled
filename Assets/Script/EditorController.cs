using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public enum Mode
{
    Normal, // the usual mode that we're in
    Holding, // when we're holding a hold
    Route, // when we're looking at an entire route
}

public class EditorController : MonoBehaviour
{
    public HoldManager holdManager;
    public HoldStateManager HoldStateManager;
    public CameraControl cameraControl;
    public HighlightManager highlightManager;
    public RouteManager routeManager;
    public WallManager WallManager;
    public StateImportExportManager StateImportExportManager;
    public EscapeMenuManager EscapeMenuManager;

    public Mode currentMode = Mode.Normal;

    // TODO: do this a different way
    private HoldBlueprint[] _selected;

    // a flag for when hold started to be held
    // used to immediately move it to hit ray
    private bool _startedBeingHeld;

    public float mouseSensitivity = 1f;

    void tmpSelectHolds()
    {
        _selected = holdManager.Filter(_ => true);
    }

    void Start()
    {
        Invoke("tmpSelectHolds", 1);

        // initialize the states from preference manager
        // TODO: this should be a different class
        if (PreferencesManager.LastOpenWallPath != "")
            StateImportExportManager.Import(PreferencesManager.LastOpenWallPath);
        else if (PreferencesManager.CurrentWallModelPath != "")
        {
            WallManager.InitializeFromPath(PreferencesManager.CurrentWallModelPath);
            EscapeMenuManager.ForceSaveAs();
        }
    }

    /// <summary>
    /// Called each update at the very beginning.
    /// </summary>
    void CombinedBehaviorBefore()
    {
        // toggle between holding and normal with e press
        // also go from Route to Holding
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentMode = currentMode == Mode.Holding ? Mode.Normal : Mode.Holding;

            if (currentMode == Mode.Normal)
                HoldStateManager.SetUnheld(true);
            else
            {
                HoldStateManager.SetHeld(_selected[0]);
                routeManager.DeselectRoute(); // TODO: this is not pretty
                _startedBeingHeld = true;
            }
        }

        // always secondarily highlight the route
        if (currentMode == Mode.Route)
            highlightManager.Highlight(routeManager.SelectedRoute, HighlightType.Secondary);
    }

    /// <summary>
    /// Called each update when nothing was hit.
    /// </summary>
    void NoHitBehavior()
    {
        switch (currentMode)
        {
            case Mode.Holding:
                highlightManager.UnhighlightAll();
                HoldStateManager.DisableHeld();
                break;
            case Mode.Normal:
                highlightManager.UnhighlightAll();
                break;
            case Mode.Route:
                highlightManager.UnhighlightAll(HighlightType.Primary);
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
                highlightManager.UnhighlightAll();
                HoldingHitControls(hit);
                break;
            case Mode.Normal:
                highlightManager.UnhighlightAll();
                break;
            case Mode.Route:
                highlightManager.UnhighlightAll(HighlightType.Primary);
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
                highlightManager.UnhighlightAll();
                HoldingHitControls(hit);
                break;
            case Mode.Normal:
                // if some other hold is highlighted, unhighlight it
                if (!highlightManager.IsHighlighted(hold))
                    highlightManager.UnhighlightAll();

                NormalRouteHoldHitControls(hold);
                break;
            case Mode.Route:
                // if some other hold is highlighted, unhighlight it
                if (!highlightManager.IsHighlighted(hold))
                    highlightManager.UnhighlightAll(HighlightType.Primary);

                NormalRouteHoldHitControls(hold);

                // CTRL+LMB or SHIFT+LMB click toggles a hold to be in the route
                if (Input.GetMouseButtonDown(0) &&
                    (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift)))
                    routeManager.ToggleHold(routeManager.SelectedRoute, hold);

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
            // rotate hold on shift press
            if (Input.GetKey(KeyCode.LeftShift))
                HoldStateManager.RotateHeld(Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime);
        }
    }

    /// <summary>
    /// Controls for holding mode when a hold/wall is hit.
    /// </summary>
    void HoldingHitControls(RaycastHit hit)
    {
        // make sure that the hold is enabled when holding
        HoldStateManager.EnableHeld();

        // when in holding mode, move the held hold accordingly
        if (_startedBeingHeld)
        {
            HoldStateManager.InterpolateHeldToHit(hit, 0);
            _startedBeingHeld = false;
        }
        else
            HoldStateManager.InterpolateHeldToHit(hit);

        // left click: place held hold and go to normal mode 
        if (Input.GetMouseButtonDown(0))
        {
            HoldStateManager.InterpolateHeldToHit(hit, 0);

            HoldStateManager.PutDown();
            currentMode = Mode.Normal;
        }

        // r/del - delete the held hold and switch to normal mode
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Delete))
        {
            HoldStateManager.SetUnheld(true);
            currentMode = Mode.Normal;
        }
    }

    /// <summary>
    /// Called when normal and route hit a hold.
    /// </summary>
    void NormalRouteHoldHitControls(GameObject hold)
    {
        // highlight the hold we're looking at
        highlightManager.Highlight(hold, HighlightType.Primary);

        // when left clicking, snap back to holding mode and pick it up
        // CTRL+LMB and SHIFT+LMB click behaves differently in route mode, so it's forbidden altogether
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
        {
            currentMode = Mode.Holding;
            routeManager.DeselectRoute(); // TODO: this is not pretty

            HoldStateManager.PickUp(hold);

            cameraControl.LookAt(hold.transform.position);
        }

        Route route = routeManager.GetRouteWithHold(hold);

        // b/t for bottom/top marks
        if (Input.GetKeyDown(KeyCode.B))
            route.ToggleStarting(hold);

        if (Input.GetKeyDown(KeyCode.T))
            route.ToggleEnding(hold);

        // r/del - delete hold
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Delete))
            HoldStateManager.Unplace(hold, true);

        // if we delete the current hold and the route has no more holds, switch to normal mode
        if (route.IsEmpty())
            currentMode = Mode.Normal;

        // right click for route mode
        if (Input.GetMouseButtonDown(1))
        {
            var clickedRoute = routeManager.GetRouteWithHold(hold);

            if (routeManager.SelectedRoute != clickedRoute)
            {
                routeManager.SelectRoute(clickedRoute);
                highlightManager.UnhighlightAll();
                currentMode = Mode.Route;
            }
        }
    }

    void FixedUpdate()
    {
        CombinedBehaviorBefore();

        var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var hitObject = hit.collider.gameObject;

            if (HoldStateManager.IsPlacedHold(hitObject))
                HoldHitBehavior(hit, hitObject);
            else
                WallHitBehavior(hit);
        }
        else
            NoHitBehavior();

        CombinedBehaviorAfter();
    }
}