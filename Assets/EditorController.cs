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
    public HoldManager HoldManager;
    public StateManager StateManager;
    public CameraControl CameraControl;
    public HighlightManager HighlightManager;

    public RouteManager RouteManager;

    public Mode CurrentMode = Mode.Normal;

    // TODO: do this a different way
    private HoldBlueprint[] _selected;

    public float mouseSensitivity = 1f;

    void tmpSelectHolds()
    {
        _selected = HoldManager.Filter(hold => true);
    }

    void Start()
    {
        Invoke("tmpSelectHolds", 1);
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
            CurrentMode = CurrentMode == Mode.Holding ? Mode.Normal : Mode.Holding;

            if (CurrentMode == Mode.Normal)
                StateManager.SetUnheld(true);
            else
            {
                StateManager.SetHeld(_selected[0]);
                RouteManager.DeselectRoute();  // TODO: this is not pretty
            }
        }

        // always secondarily highlight the route
        if (CurrentMode == Mode.Route)
            HighlightManager.Highlight(RouteManager.SelectedRoute, HighlightType.Secondary);
    }

    /// <summary>
    /// Called each update when nothing was hit.
    /// </summary>
    void NoHitBehavior()
    {
        switch (CurrentMode)
        {
            case Mode.Holding:
                HighlightManager.UnhighlightAll();
                StateManager.DisableHeld();
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
        switch (CurrentMode)
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
        switch (CurrentMode)
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
                
                // CTRL + left click toggles a hold to be in the route
                if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
                    RouteManager.ToggleHold(RouteManager.SelectedRoute, hold);

                break;
        }
    }

    /// <summary>
    /// Called each update at the very end.
    /// </summary>
    void CombinedBehaviorAfter()
    {
        if (CurrentMode == Mode.Holding)
        {
            // rotate hold on shift press
            if (Input.GetKey(KeyCode.LeftShift))
                StateManager.RotateHeld(Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Controls for holding mode when a hold/wall is hit.
    /// </summary>
    void HoldingHitControls(RaycastHit hit)
    {
        // make sure that the hold is enabled when holding
        StateManager.EnableHeld();

        // when in holding mode, move the held hold accordingly
        StateManager.InterpolateHeldToHit(hit);

        // left click: place held hold and go to normal mode 
        if (Input.GetMouseButtonDown(0))
        {
            StateManager.InterpolateHeldToHit(hit, 0);

            StateManager.PutDown();
            CurrentMode = Mode.Normal;
        }
        
        // r/del - delete the held hold and switch to normal mode
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Delete))
        {
            StateManager.SetUnheld(true);
            CurrentMode = Mode.Normal;
        }
    }

    /// <summary>
    /// Called when normal and route hit a hold.
    /// </summary>
    void NormalRouteHoldHitControls(GameObject hold)
    {
        // highlight the hold we're looking at
        HighlightManager.Highlight(hold, HighlightType.Primary);

        // when left clicking, snap back to holding mode and pick it up
        // CTRL + left click behaves differently in route mode, so it's forbidden altogether
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftControl))
        {
            CurrentMode = Mode.Holding;
            RouteManager.DeselectRoute();  // TODO: this is not pretty

            StateManager.PickUp(hold);

            CameraControl.LookAt(hold.transform.position);
        }

        Route route = RouteManager.GetRouteWithHold(hold);

        // b/t for bottom/top marks
        if (Input.GetKeyDown(KeyCode.B))
            route.ToggleStarting(hold);

        if (Input.GetKeyDown(KeyCode.T))
            route.ToggleEnding(hold);
        
        // r/del - delete hold
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Delete))
            StateManager.Unplace(hold, true);
        
        // if we delete the current hold and the route has no more holds, switch to normal mode
        if (route.IsEmpty())
            CurrentMode = Mode.Normal;
        
        // right click for route mode
        if (Input.GetMouseButtonDown(1))
        {
            var clickedRoute = RouteManager.GetRouteWithHold(hold);

            if (RouteManager.SelectedRoute != clickedRoute)
            {
                RouteManager.SelectRoute(clickedRoute);
                HighlightManager.UnhighlightAll();
                CurrentMode = Mode.Route;
            }
        }
    }

    void Update()
    {
        CombinedBehaviorBefore();
        
        var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var hitObject = hit.collider.gameObject;

            if (StateManager.IsPlacedHold(hitObject))
                HoldHitBehavior(hit, hitObject);
            else
                WallHitBehavior(hit);
        }
        else
            NoHitBehavior();

        CombinedBehaviorAfter();
    }
}