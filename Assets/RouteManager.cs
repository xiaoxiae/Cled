using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// A class for a collection of holds that form it.
/// </summary>
public class Route
{
    private readonly HashSet<GameObject> _holds = new();

    // TODO: some route name/id/label thingy

    private readonly HashSet<GameObject> _starting = new();
    private readonly HashSet<GameObject> _ending = new();
    
    public GameObject[] Holds {
        get
        {
            foreach(var hold in _holds.ToArray()) {
                if (!hold)
                    RemoveHold(hold);
            }
            
            return _holds.ToArray();
        }
    }

    /// <summary>
    /// Add a hold to the route.
    /// </summary>
    public void AddHold(GameObject hold) => _holds.Add(hold);

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
    public void ToggleHold(GameObject hold)
    {
        if (ContainsHold(hold))
            RemoveHold(hold);
        else
            AddHold(hold);
    }

    /// <summary>
    /// Return true if the route contains this hold, else return false.
    /// </summary>
    public bool ContainsHold(GameObject hold) => _holds.Contains(hold);

    /// <summary>
    /// Toggle a starting hold of the route.
    /// </summary>
    public void ToggleStarting(GameObject hold) => ToggleInCollection(hold, _starting);
    
    /// <summary>
    /// Toggle a starting hold of the route.
    /// </summary>
    public void ToggleEnding(GameObject hold) => ToggleInCollection(hold, _ending);

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
    public bool IsEmpty() => _holds.Count == 0;
}

/// <summary>
/// A class for managing all currently active routes.
/// </summary>
public class RouteManager : MonoBehaviour
{
    private HashSet<Route> _routes = new HashSet<Route>();

    public Route SelectedRoute;

    /// <summary>
    /// Select the given route.
    /// </summary>
    public void SelectRoute(Route route) => SelectedRoute = route;
    
    /// <summary>
    /// Deselect the currently selected route.
    /// </summary>
    public void DeselectRoute() => SelectedRoute = null;
    
    /// <summary>
    /// Toggle a hold being in this route, possibly removing it from other routes.
    /// </summary>
    public void ToggleHold(Route route, GameObject hold)
    {
        Route originalRoute = GetRouteWithHold(hold);
        
        // if we're removing it, simply toggle it
        if (route == originalRoute)
            route.ToggleHold(hold);

        // if we're adding it, remove it from the route that it was in
        else
        {
            originalRoute.ToggleHold(hold);
            route.ToggleHold(hold);

            if (originalRoute.IsEmpty())
                RemoveRoute(originalRoute);
        }
    }

    /// <summary>
    /// Return the route with the given hold.
    /// </summary>
    public Route GetRouteWithHold(GameObject hold)
    {
        foreach (Route route in _routes)
            if (route.ContainsHold(hold))
                return route;

        // if no such route exists, create it
        var newRoute = new Route();
        newRoute.AddHold(hold);
        _routes.Add(newRoute);

        return newRoute;
    }
    
    /// <summary>
    /// Remove a route from the manager.
    /// </summary>
    private void RemoveRoute(Route route) => _routes.Remove(route);
}