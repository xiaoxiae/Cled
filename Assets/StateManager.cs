using System.Collections.Generic;
using UnityEngine;

public class HoldState
{
    public float PlacedAngle;
    
    public HoldState(float placedAngle)
    {
        PlacedAngle = placedAngle;
    }
}

public class StateManager : MonoBehaviour
{
    private readonly Dictionary<GameObject, HoldState> _placedHolds = new Dictionary<GameObject, HoldState>();
    //readonly List<GameObject> _placedHolds = new List<GameObject>();

    /// <summary>
    /// Add a hold to the state manager.
    /// </summary>
    public void AddHold(GameObject hold, float placedAngle)
    {
        _placedHolds.Add(hold, new HoldState(placedAngle));
    }

    /// <summary>
    /// Add a hold to the state manager.
    /// </summary>
    public HoldState GetHoldState(GameObject hold) => _placedHolds[hold];
    
    /// <summary>
    /// Remove the hold from the state manager.
    /// </summary>
    public void RemoveHold(GameObject hold)
    {
        _placedHolds.Remove(hold);
    }

    public bool IsHold(GameObject hold) => _placedHolds.ContainsKey(hold);
}