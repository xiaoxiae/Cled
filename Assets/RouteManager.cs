using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// A class for a collection of holds that form it.
/// </summary>
public class Route
{
    public readonly List<GameObject> Holds = new List<GameObject>();
    
    // TODO: some route name/id/label thingy

    private readonly List<GameObject> _starting = new List<GameObject>();
    private readonly List<GameObject> _ending = new List<GameObject>();
    
    /// <summary>
    /// Add a hold to the route.
    /// </summary>
    public void AddHold(GameObject hold) => Holds.Add(hold);

    /// <summary>
    /// Remove a hold from the route.
    /// </summary>
    public void RemoveHold(GameObject hold)
    {
        Holds.Remove(hold);
        _starting.Remove(hold);
        _ending.Remove(hold);
    }

    // TODO: starting and ending methods
}

/// <summary>
/// A class for managing all currently active routes.
/// </summary>
public class RouteManager : MonoBehaviour
{
    private List<Route> _routes = new List<Route>();

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
