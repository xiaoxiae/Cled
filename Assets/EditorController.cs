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

    private Route _selectedRoute;

    public Mode CurrentMode = Mode.Normal;

    // TODO: do this a different way
    private Hold[] _selected;

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
    /// Called each update when nothing was hit.
    /// </summary>
    void NoHitBehavior()
    {
        switch (CurrentMode)
        {
            case Mode.Holding:
                StateManager.DisableHeld();
                break;
            case Mode.Normal:
                HighlightManager.UnhighlightAll();
                break;
            case Mode.Route:
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
            case Mode.Normal:
                HighlightManager.UnhighlightAll();
                break;
            case Mode.Holding:
                HoldingHitControls(hit);
                break;
            case Mode.Route:
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
            case Mode.Normal:
                // if some other hold is highlighted, highlight this one
                if (!HighlightManager.IsHighlighted(hold))
                    HighlightManager.UnhighlightAll();

                NormalRouteHoldHitControls(hold);

                break;
            case Mode.Holding:
                HoldingHitControls(hit);
                break;
            case Mode.Route:
                NormalRouteHoldHitControls(hold);
                
                // CTRL + left click toggles a hold to be in the route
                if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
                    RouteManager.ToggleHold(_selectedRoute, hold);
                    
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
                StateManager.SetHeld(_selected[0]);
        }

        // always unhighlight the primary and highlight the route when in route mode
        // TODO: not entirely elegant, since we're depeatedly destroying and adding the outline back
        if (CurrentMode == Mode.Route)
        {
            HighlightManager.UnhighlightAll(HighlightType.Primary);
            HighlightManager.Highlight(_selectedRoute, HighlightType.Secondary);
        }
    }

    /// <summary>
    /// Controls for holding mode when a hold is hit or not.
    /// </summary>
    void HoldingHitControls(RaycastHit hit)
    {
        // no highlighting in edit mode!
        HighlightManager.UnhighlightAll();

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
    }

    /// <summary>
    /// Normal and route mode share a large set of controls (when a hold is hit).
    /// </summary>
    void NormalRouteHoldHitControls(GameObject hold)
    {
        // highlight the hold we're looking at
        HighlightManager.Highlight(hold, HighlightType.Primary);

        // when left clicking, snap back to holding mode and pick it up
        // CTRL + left click behaves differently in route mode - not entirely elegant but works
        if (Input.GetMouseButtonDown(0) && (CurrentMode == Mode.Normal || !Input.GetKey(KeyCode.LeftControl)))
        {
            CurrentMode = Mode.Holding;

            StateManager.PickUp(hold);
            HighlightManager.Unhighlight(hold);

            CameraControl.LookAt(hold.transform.position);
        }

        Route route = RouteManager.GetRouteWithHold(hold);

        // b/t for bottom/top marks
        if (Input.GetKey(KeyCode.B))
            route.ToggleStarting(hold);

        if (Input.GetKey(KeyCode.T))
            route.ToggleEnding(hold);

        // r/del for deleting holds
        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.Delete))
            StateManager.Unplace(hold, true);

        // right click for route mode
        if (Input.GetMouseButtonDown(1))
        {
            _selectedRoute = RouteManager.GetRouteWithHold(hold);
            HighlightManager.UnhighlightAll();
            CurrentMode = Mode.Route;
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