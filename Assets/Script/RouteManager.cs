using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// Add this to any GameObject then assign
/// parameter-less functions to OnUpdate.
public class CustomUpdate : MonoBehaviour
{
    public event System.Action OnUpdate;

    void Update() => OnUpdate?.Invoke();
}

/// <summary>
/// A class for a collection of holds that form it.
/// </summary>
public class Route
{
    private readonly Dictionary<GameObject, HoldBlueprint> _holds = new();

    private readonly GameObject _startingMarkerPrefab;
    private readonly GameObject _endingMarkerPrefab;

    public int Id;
    public string Name;

    // this should be enums somewhere
    public string Grade;
    public string Zone;
    public string Setter;

    private readonly HashSet<GameObject> _starting = new();
    private readonly HashSet<GameObject> _ending = new();

    public Route(GameObject startingMarkerPrefab, GameObject endingMarkerPrefab)
    {
        _startingMarkerPrefab = startingMarkerPrefab;
        _endingMarkerPrefab = endingMarkerPrefab;
    }

    /// <summary>
    /// Get the holds of the route.
    /// </summary>
    public GameObject[] Holds => _holds.Keys.ToArray();

    /// <summary>
    /// Add a hold to the route.
    /// </summary>
    public void AddHold(GameObject hold, HoldBlueprint blueprint) => _holds[hold] = blueprint;

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
    public void ToggleHold(GameObject hold, HoldBlueprint blueprint)
    {
        if (ContainsHold(hold))
            RemoveHold(hold);
        else
            AddHold(hold, blueprint);
    }

    /// <summary>
    /// Return true if the route contains this hold, else return false.
    /// </summary>
    public bool ContainsHold(GameObject hold) => _holds.ContainsKey(hold);

    /// <summary>
    /// Return true if the hold is a starting one, else false.
    /// </summary>
    public bool IsStarting(GameObject hold) => _starting.Contains(hold);

    /// <summary>
    /// Return true if the hold is an ending one, else false.
    /// </summary>
    public bool IsEnding(GameObject hold) => _ending.Contains(hold);

    /// <summary>
    /// Toggle a starting hold of the route.
    /// </summary>
    public void ToggleStarting(GameObject hold)
    {
        ToggleInCollection(hold, _starting);

        if (IsStarting(hold))
            AddMarker(hold, _startingMarkerPrefab);
        else
            RemoveMarker(hold);
    }

    /// <summary>
    /// Toggle a starting hold of the route.
    /// </summary>
    public void ToggleEnding(GameObject hold)
    {
        ToggleInCollection(hold, _ending);

        if (IsStarting(hold))
            AddMarker(hold, _endingMarkerPrefab);
        else
            RemoveMarker(hold);
    }

    /// <summary>
    /// Add the marker to the hold.
    /// </summary>
    void AddMarker(GameObject hold, GameObject marker)
    {
        GameObject child = Object.Instantiate(marker);
        child.SetActive(true);
        child.transform.parent = hold.transform;
        child.transform.localPosition = Vector3.zero;

        var customUpdate = child.AddComponent<CustomUpdate>();
        customUpdate.OnUpdate += () =>
            child.transform.LookAt(hold.transform.forward + hold.transform.position, Vector3.up);
    }
    
    /// <summary>
    /// Remove the hold's marker.
    /// </summary>
    void RemoveMarker(GameObject hold)
        => Object.DestroyImmediate(hold.transform.GetChild(hold.transform.childCount - 1).gameObject);

    /// <summary>
    /// Toggle the object being in the given set.
    /// </summary>
    private void ToggleInCollection<T>(T obj, HashSet<T> set)
    {
        if (set.Contains(obj))
            set.Remove(obj);
        else
            set.Add(obj);
    }

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
    private HashSet<Route> _routes = new();

    public Route SelectedRoute;

    public GameObject StartingMarkerPrefab;
    public GameObject EndingMarkerPrefab;

    /// <summary>
    /// Clear out the route manager, NOT destroying any holds in the process.
    /// </summary>
    public void Clear()
    {
        _routes = new HashSet<Route>();
        SelectedRoute = null;
    }

    /// <summary>
    /// Select the given route.
    /// </summary>
    public void SelectRoute(Route route) => SelectedRoute = route;

    /// <summary>
    /// Deselect the currently selected route.
    /// </summary>
    public void DeselectRoute() => SelectedRoute = null;

    /// <summary>
    /// Toggle a hold being in a route, possibly removing it from other routes.
    /// </summary>
    public void ToggleHold(Route route, GameObject hold, HoldBlueprint blueprint)
    {
        Route originalRoute = GetRouteWithHold(hold);

        // if it is in no route, create one and add it
        if (originalRoute == null)
        {
            route = CreateRoute();
            route.AddHold(hold, blueprint);
            return;
        }

        // if we're removing it, simply toggle it
        if (route == originalRoute)
            route.ToggleHold(hold, blueprint);

        // TODO: starting/ending markers

        // if we're adding it to a different one, remove it from the original and add it to the new
        else
        {
            originalRoute.RemoveHold(hold);
            route.AddHold(hold, blueprint);

            // if the original route is now empty, remove it altogether
            if (originalRoute.IsEmpty())
                RemoveRoute(originalRoute);

            // TODO: starting/ending markers
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
        var newRoute = new Route(StartingMarkerPrefab, EndingMarkerPrefab);
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
    /// <returns></returns>
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
}