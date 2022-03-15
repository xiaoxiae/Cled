using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    readonly List<GameObject> _placedHolds = new List<GameObject>();

    public void AddHold(GameObject hold)
    {
        _placedHolds.Add(hold);
    }

    public bool IsHold(GameObject hold) => _placedHolds.Contains(hold);
}