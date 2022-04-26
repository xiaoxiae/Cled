using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A class for a collection of holds that form it.
/// </summary>
public class Route
{
    private readonly Dictionary<GameObject, HoldBlueprint> _holds = new();

    private readonly GameObject _startMarkerPrefab;
    private readonly GameObject _endMarkerPrefab;

    public string Name;

    public string Grade;
    public string Zone;
    public string Setter;

    private readonly HashSet<GameObject> _starting = new();
    private readonly HashSet<GameObject> _ending = new();

    public Route(GameObject startMarkerPrefab, GameObject endMarkerPrefab)
    {
        _startMarkerPrefab = startMarkerPrefab;
        _endMarkerPrefab = endMarkerPrefab;
    }

    /// <summary>
    /// Get the holds of the route.
    /// </summary>
    public GameObject[] Holds => _holds.Keys.ToArray();

    /// <summary>
    /// Get the starting holds of the route.
    /// </summary>
    public GameObject[] StartingHolds => _starting.ToArray();

    /// <summary>
    /// Get the ending holds of the route.
    /// </summary>
    public GameObject[] EndingHolds => _ending.ToArray();

    /// <summary>
    /// Add a hold to the route.
    /// </summary>
    public void AddHold(GameObject hold, HoldBlueprint blueprint, bool isStarting = false, bool isEnding = false)
    {
        _holds[hold] = blueprint;

        if (isStarting) _starting.Add(hold);
        if (isEnding) _ending.Add(hold);
    }

    /// <summary>
    /// Remove a hold from the route.
    /// </summary>
    public void RemoveHold(GameObject hold)
    {
        _holds.Remove(hold);
        _starting.Remove(hold);
        _ending.Remove(hold);
    }

    /// <summary>
    /// Toggle a hold being in this route.
    /// </summary>
    public void ToggleHold(GameObject hold, HoldBlueprint blueprint, bool isStarting = false, bool isEnding = false)
    {
        if (ContainsHold(hold))
            RemoveHold(hold);
        else
            AddHold(hold, blueprint, isStarting, isEnding);
    }

    /// <summary>
    /// Return true if the route contains this hold, else return false.
    /// </summary>
    public bool ContainsHold(GameObject hold) => _holds.ContainsKey(hold);

    /// <summary>
    /// Toggle a starting hold of the route.
    /// </summary>
    public void ToggleStarting(GameObject hold)
    {
        Utilities.ToggleInCollection(hold, _starting);

        if (_starting.Contains(hold))
            AddMarker(hold, _startMarkerPrefab);
        else
            RemoveMarker(hold);
    }

    /// <summary>
    /// Toggle a starting hold of the route.
    /// </summary>
    public void ToggleEnding(GameObject hold)
    {
        Utilities.ToggleInCollection(hold, _ending);

        if (_ending.Contains(hold))
            AddMarker(hold, _endMarkerPrefab);
        else
            RemoveMarker(hold);
    }
    
    /// <summary>
    /// A component for updating the marker.
    /// It remembers the last hold position and rotation to only update the marker if necessary.
    /// </summary>
    public class MarkerUpdate : MonoBehaviour
    {
        public event System.Action OnUpdate;
        
        public Vector3 LastPosition;
        public Quaternion LastRotation;

        void Update() => OnUpdate?.Invoke();
    }
    
    /// <summary>
    /// Add the marker to the hold.
    /// </summary>
    void AddMarker(GameObject hold, GameObject marker)
    {
        GameObject markerInstance = Object.Instantiate(marker);
        markerInstance.SetActive(true);
        markerInstance.transform.parent = hold.transform;
        markerInstance.transform.localPosition = Vector3.zero;

        var customUpdate = markerInstance.AddComponent<MarkerUpdate>();
        customUpdate.OnUpdate += () =>
        {
            // only update when hold position or rotation changed
            if (customUpdate.LastPosition == hold.transform.position &&
                customUpdate.LastRotation == hold.transform.rotation)
                return;

            var holdPosition = hold.transform.position;

            customUpdate.LastPosition = holdPosition;
            customUpdate.LastRotation = hold.transform.rotation;
            
            var c1 = hold.GetComponent<MeshCollider>();
            var c2 = markerInstance.transform.GetChild(0).GetComponent<MeshCollider>();
            
            markerInstance.transform.position = holdPosition;
            markerInstance.transform.LookAt(hold.transform.forward + holdPosition, Vector3.up);

            float step = 0.001f;
            float stepsBack = 30;
            
            // a little dangerous but whatever
            while (Physics.ComputePenetration(c1, c1.transform.position, c1.transform.rotation, c2,
                                        c2.transform.position, c2.transform.rotation, out _, out _))
                markerInstance.transform.position -= markerInstance.transform.up * step;
            
            markerInstance.transform.position += markerInstance.transform.up * step * stepsBack;
        };
    }

    /// <summary>
    /// Remove the hold's marker.
    /// </summary>
    void RemoveMarker(GameObject hold)
        => Object.DestroyImmediate(hold.transform.GetChild(hold.transform.childCount - 1).gameObject);

    /// <summary>
    /// Return True if the given route is empty.
    /// </summary>
    public bool IsEmpty() => Holds.Length == 0;
}

/// <summary>
/// A class for managing all currently active routes.
///
/// The routes themselves can contain any number of holds.
/// However, if a hold is right click selected, it will automatically create a route of one hold.
/// This route will, however, not be a real route, even if it is implemented as one, since lone holds don't substitute routes.
/// </summary>
public class RouteManager : MonoBehaviour
{
    private readonly HashSet<Route> _routes = new();

    public Route SelectedRoute;

    public GameObject StartMarkerPrefab;
    public GameObject EndMarkerPrefab;

    public readonly HashSet<GameObject> StartingHolds = new();
    public readonly HashSet<GameObject> EndingHolds = new();

    /// <summary>
    /// Get all routes.
    /// </summary>
    /// <returns></returns>
    public Route[] GetRoutes() => _routes.ToArray();

    /// <summary>
    /// Select the given route.
    /// </summary>
    public void SelectRoute(Route route) => SelectedRoute = route;

    /// <summary>
    /// Deselect the currently selected route.
    /// </summary>
    public void DeselectRoute() => SelectedRoute = null;

    /// <summary>
    /// Toggle a starting hold of the route.
    /// </summary>
    public void ToggleStarting(GameObject hold, HoldBlueprint blueprint)
    {
        Route originalRoute = GetOrCreateRouteWithHold(hold, blueprint);

        // if it's already ending, make it not so
        if (EndingHolds.Contains(hold))
            ToggleEnding(hold, blueprint);

        originalRoute.ToggleStarting(hold);

        if (StartingHolds.Contains(hold))
            StartingHolds.Remove(hold);
        else
            StartingHolds.Add(hold);
    }

    /// <summary>
    /// Toggle a starting hold of the route.
    /// </summary>
    public void ToggleEnding(GameObject hold, HoldBlueprint blueprint)
    {
        Route originalRoute = GetOrCreateRouteWithHold(hold, blueprint);

        // if it's already starting, make it not so
        if (StartingHolds.Contains(hold))
            ToggleStarting(hold, blueprint);

        originalRoute.ToggleEnding(hold);

        if (EndingHolds.Contains(hold))
            EndingHolds.Remove(hold);
        else
            EndingHolds.Add(hold);
    }

    /// <summary>
    /// Toggle a hold being in a route, possibly removing it from other routes.
    /// </summary>
    public void ToggleHold(Route route, GameObject hold, HoldBlueprint blueprint)
    {
        // the route the hold was originally in
        Route originalRoute = GetRouteWithHold(hold);

        bool isStarting = StartingHolds.Contains(hold);
        bool isEnding = EndingHolds.Contains(hold);

        // if we're removing it from a route, simply toggle it
        if (route == originalRoute)
            route.ToggleHold(hold, blueprint, isStarting, isEnding);

        // if we're adding it to a different one, remove it from the original (if it was in one) and add it to the new
        else
        {
            if (originalRoute != null)
            {
                RemoveHold(hold);

                if (originalRoute.IsEmpty())
                    RemoveRoute(originalRoute);
            }

            route.AddHold(hold, blueprint, isStarting, isEnding);
        }
    }

    /// <summary>
    /// Remove the given hold from the route it is in (if it is in one).
    /// </summary>
    public void RemoveHold(GameObject hold)
    {
        Route route = GetRouteWithHold(hold);

        if (route == null)
            return;

        route.RemoveHold(hold);

        if (route.IsEmpty())
            RemoveRoute(route);
    }

    /// <summary>
    /// Create a new route, adding it to the manager.
    /// </summary>
    public Route CreateRoute()
    {
        var newRoute = new Route(StartMarkerPrefab, EndMarkerPrefab);
        _routes.Add(newRoute);

        return newRoute;
    }

    /// <summary>
    /// Return the route with the given hold, returning null if no such route exists.
    /// </summary>
    public Route GetRouteWithHold(GameObject hold)
        => _routes.FirstOrDefault(route => route.ContainsHold(hold));

    /// <summary>
    /// Return the route with the given hold, or create one if no such exists.
    /// </summary>
    public Route GetOrCreateRouteWithHold(GameObject hold, HoldBlueprint blueprint)
    {
        var route = GetRouteWithHold(hold);

        if (route != null)
            return route;

        route = CreateRoute();
        route.AddHold(hold, blueprint);

        return route;
    }

    /// <summary>
    /// Remove a route from the manager.
    /// </summary>
    private void RemoveRoute(Route route) => _routes.Remove(route);

    public void Clear()
    {
        _routes.Clear();

        SelectedRoute = null;

        StartingHolds.Clear();
        EndingHolds.Clear();
    }
}