using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public enum Mode
{
    Normal, // the usual mode that we're in
    Holding, // when we're holding a hold
    Route, // when we're looking at an entire route
}

public class PlayerSelection : MonoBehaviour
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

    void Start()
    {
        _selected = HoldManager.Filter(hold => true);
    }

    void Update()
    {
        // toggle between holding and normal with e press
        if (Input.GetKeyDown(KeyCode.E))
        {
            CurrentMode = CurrentMode == Mode.Holding ? Mode.Normal : Mode.Holding;

            if (CurrentMode == Mode.Normal)
                StateManager.SetUnheld(true);
            else
                StateManager.SetHeld(_selected[0]);
        }

        // cast a ray directly in front of the camera
        var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // TODO: split commands to hit -- wall, hit -- route and no hit
        if (Physics.Raycast(ray, out hit))
        {
            var hitObject = hit.collider.gameObject;

            switch (CurrentMode)
            {
                case Mode.Holding:
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

                    break;

                case Mode.Normal:
                    // when in normal mode and not hovering a hold, deselect all
                    if (!HighlightManager.IsHighlighted(hitObject))
                        HighlightManager.UnhighlightAll();

                    // if hovering a hold
                    if (StateManager.IsPlacedHold(hitObject))
                    {
                        // highlight hold if in normal mode
                        HighlightManager.Highlight(hitObject, HighlightType.Primary);

                        // when left clicking, snap back to holding mode
                        if (Input.GetMouseButtonDown(0))
                        {
                            CurrentMode = Mode.Holding;

                            StateManager.PickUp(hitObject);
                            HighlightManager.Unhighlight(hitObject);

                            CameraControl.LookAt(hitObject.transform.position);
                        }

                        Route route = RouteManager.GetRouteWithHold(hitObject);

                        // b/t for bottom/top marks
                        if (Input.GetKey(KeyCode.B))
                            route.ToggleStarting(hitObject);

                        if (Input.GetKey(KeyCode.T))
                            route.ToggleEnding(hitObject);

                        // r/del for deleting holds
                        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.Delete))
                            StateManager.Unplace(hitObject, true);

                        // right click for route mode
                        if (Input.GetMouseButtonDown(1))
                        {
                            _selectedRoute = RouteManager.GetRouteWithHold(hitObject);
                            CurrentMode = Mode.Route;

                            HighlightManager.Highlight(_selectedRoute, HighlightType.Secondary);
                        }
                    }

                    break;

                case Mode.Route:
                    // if hovering a hold
                    // TODO: duplicated code
                    if (StateManager.IsPlacedHold(hitObject))
                    {
                        // highlight hold if in normal mode
                        HighlightManager.Highlight(hitObject, HighlightType.Primary);

                        // when left clicking, snap back to holding mode
                        if (Input.GetMouseButtonDown(0))
                        {
                            CurrentMode = Mode.Holding;

                            StateManager.PickUp(hitObject);
                            HighlightManager.Unhighlight(hitObject);

                            CameraControl.LookAt(hitObject.transform.position);
                        }

                        Route route = RouteManager.GetRouteWithHold(hitObject);

                        // b/t for bottom/top marks
                        if (Input.GetKey(KeyCode.B))
                            route.ToggleStarting(hitObject);

                        if (Input.GetKey(KeyCode.T))
                            route.ToggleEnding(hitObject);

                        // r/del for deleting holds
                        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.Delete))
                            StateManager.Unplace(hitObject, true);

                        // right click for route mode
                        if (Input.GetMouseButtonDown(1))
                        {
                            _selectedRoute = RouteManager.GetRouteWithHold(hitObject);
                            CurrentMode = Mode.Route;
                        }
                    }

                    break;
            }
        }
        else
        {
            switch (CurrentMode)
            {
                case Mode.Holding:
                    StateManager.DisableHeld();
                    break;
                case Mode.Route:
                    HighlightManager.UnhighlightAll(HighlightType.Primary);
                    break;
            }
        }

        // even when the ray didn't hit anything, do some stuff
        if (CurrentMode == Mode.Holding)
        {
            // rotate hold on shift press
            if (Input.GetKey(KeyCode.LeftShift))
                StateManager.RotateHeld(Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime);
        }
        else if (CurrentMode == Mode.Route)
        {
            // TODO: duplicit code
            HighlightManager.UnhighlightAll(HighlightType.Primary);
            HighlightManager.Highlight(_selectedRoute, HighlightType.Secondary);
        }
    }
}