using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// A class for a collection of holds that form it.
/// </summary>
public class Route
{
    private readonly HashSet<GameObject> _holds = new HashSet<GameObject>();

    // TODO: some route name/id/label thingy

    private readonly HashSet<GameObject> _starting = new HashSet<GameObject>();
    private readonly HashSet<GameObject> _ending = new HashSet<GameObject>();
    
    public List<GameObject> Holds => _holds.ToList();

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
        
    // TODO: starting and ending methods
}

/// <summary>
/// A class for managing all currently active routes.
/// </summary>
public class RouteManager : MonoBehaviour
{
    private HashSet<Route> _routes = new HashSet<Route>();

    /// <summary>
    /// Return the route with the given hold.
    /// </summary>
    public Route GetRouteWithHold(GameObject hold)
    {
        foreach (Route route in _routes)
            if (route.Holds.Contains(hold))
                return route;

        // if no such route exists, create it
        var newRoute = new Route();
        newRoute.AddHold(hold);
        _routes.Add(newRoute);

        return newRoute;
    }
}