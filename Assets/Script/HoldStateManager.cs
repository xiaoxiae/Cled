using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Information about the given hold's state when it's placed on the wall.
/// Used to smoothly continue editing it and for import/export.
/// </summary>
public class HoldState
{
    public SerializableVector3 Position;
    public SerializableVector3 Normal;

    public float Rotation;
}

/// <summary>
/// Manages the state of the holds on the wall (even the one held).
/// </summary>
public class HoldStateManager : MonoBehaviour
{
    private readonly Dictionary<GameObject, (HoldBlueprint holdBlueprint, HoldState holdState)> _placedHolds = new();

    // the held object (state)
    private GameObject _heldObject;
    private HoldBlueprint _heldObjectBlueprint;
    private HoldState _heldObjectState;

    /// <summary>
    /// The currently placed holds.
    /// </summary>
    public GameObject[] PlacedHolds => _placedHolds.Keys.ToArray();

    /// <summary>
    /// The currently held hold.
    /// </summary>
    public GameObject HeldHold => _heldObject;

    /// <summary>
    /// Start holding a hold from a hold object.
    /// If replace is specified, we're replacing the currently held one with another one, saving the state.
    /// </summary>
    public void InstantiateToHolding(HoldBlueprint holdBlueprint, bool replace = false)
    {
        if (replace)
        {
            var state = _heldObjectState;
            StopHolding();
            StartHolding(Instantiate(holdBlueprint.Model), holdBlueprint, state);
        }
        else
            StartHolding(Instantiate(holdBlueprint.Model), holdBlueprint, new HoldState());
        
    }

    /// <summary>
    /// Start holding a GameObject hold.
    /// If it was placed and we're starting to hold it, remove it from placed.
    /// </summary>
    private void StartHolding(GameObject model, HoldBlueprint holdBlueprint, HoldState holdState)
    {
        if (IsPlaced(model))
            _placedHolds.Remove(model);

        _heldObject = model;
        _heldObjectBlueprint = holdBlueprint;
        _heldObjectState = holdState;

        UpdateNormal(model, holdState.Normal, holdState.Rotation);
        UpdatePosition(model, holdState.Position);

        // ignore this object until placed
        _heldObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        EnableHeld();
    }

    /// <summary>
    /// Rotate the currently held hold by a certain amount in radians.
    /// </summary>
    public void RotateHeld(float delta)
        => _heldObjectState.Rotation = (_heldObjectState.Rotation + delta) % (2 * (float)Math.PI);

    /// <summary>
    /// Stop holding the currently held hold, destroying it in the process.
    /// </summary>
    public void StopHolding()
    {
        DestroyImmediate(_heldObject);

        _heldObject = null;
        _heldObjectBlueprint = null;
        _heldObjectState = null;
    }

    /// <summary>
    /// Disable the currently held item.
    /// </summary>
    public void DisableHeld() => _heldObject.SetActive(false);

    /// <summary>
    /// Enable the currently held item.
    /// </summary>
    public void EnableHeld() => _heldObject.SetActive(true);

    /// <summary>
    /// Place the currently held hold.
    /// </summary>
    public void PutDown() => Place(_heldObject, _heldObjectBlueprint, _heldObjectState);

    /// <summary>
    /// Pick up a currently placed hold.
    /// </summary>
    public void PickUp(GameObject model) => StartHolding(model, GetHoldBlueprint(model), GetHoldState(model));

    /// <summary>
    /// Place a new hold from the hold blueprint.
    /// </summary>
    public GameObject InstantiatePlace(HoldBlueprint holdBlueprint, HoldState holdState)
    {
        var instance = Instantiate(holdBlueprint.Model);
        Place(instance, holdBlueprint, holdState);
        return instance;
    }

    /// <summary>
    /// Place the given hold.
    /// </summary>
    public void Place(GameObject model, HoldBlueprint holdBlueprint, HoldState holdState)
    {
        // if we're placing the held object, stop holding it
        if (model == _heldObject)
        {
            _heldObject = null;
            _heldObjectBlueprint = null;
            _heldObjectState = null;
        }

        _placedHolds.Add(model, (holdBlueprint, holdState));

        model.layer = LayerMask.NameToLayer("Default");

        // update the hold normal and the hold position
        UpdateNormal(model, holdState.Normal, holdState.Rotation);
        UpdatePosition(model, holdState.Position);

        model.SetActive(true);
    }

    /// <summary>
    /// Remove the hold from the state manager, no matter if it's being held or if it's placed down.
    /// </summary>
    public void Remove(GameObject model, bool destroy = false)
    {
        if (_heldObject == model)
            _heldObject = null;
        else
            _placedHolds.Remove(model);

        if (destroy)
            DestroyImmediate(model);
    }

    /// <summary>
    /// Get the state of the given model object.
    /// </summary>
    public HoldState GetHoldState(GameObject model)
        => model == _heldObject ? _heldObjectState : _placedHolds[model].holdState;

    /// <summary>
    /// Get the holdBlueprint of the given model object.
    /// </summary>
    public HoldBlueprint GetHoldBlueprint(GameObject model)
        => model == _heldObject ? _heldObjectBlueprint : _placedHolds[model].holdBlueprint;

    /// <summary>
    /// Return True if this game object is placed.
    /// </summary>
    public bool IsPlaced(GameObject model) => _placedHolds.ContainsKey(model);

    /// <summary>
    /// Move the currently held hold to the specified point.
    /// </summary>
    private void UpdateHeldPosition(Vector3 point)
    {
        UpdatePosition(_heldObject, point);
        _heldObjectState.Position = point;
    }

    /// <summary>
    /// Update the currently held hold normal to the specified vector and rotation about it.
    /// </summary>
    private void UpdateHeldNormal(Vector3 normal)
    {
        UpdateNormal(_heldObject, normal, _heldObjectState.Rotation);
        _heldObjectState.Normal = normal;
    }

    /// <summary>
    /// Update the gameobject's position to the given position.
    /// </summary>
    private void UpdatePosition(GameObject gameObject, Vector3 position) =>
        gameObject.transform.position = position;

    /// <summary>
    /// Update the gameobject to turn towards the given vector and rotate in the given float (in radians).
    /// </summary>
    private void UpdateNormal(GameObject gameObject, Vector3 normal, float rotation)
    {
        // rotate the world "up" around the hit normal by some degrees
        var upVector = Quaternion.AngleAxis(Mathf.Rad2Deg * rotation, normal) * Vector3.up;
        gameObject.transform.LookAt(normal + gameObject.transform.position, upVector);
    }

    /// <summary>
    /// Move the currently held hold to the raycast hit.
    /// </summary>
    public void MoveHeldToHit(RaycastHit hit)
    {
        UpdateHeldPosition(hit.point);
        UpdateHeldNormal(hit.normal);
    }

    /// <summary>
    /// Clear all of the instances of the holds, destroying them in the process.
    /// </summary>
    public void Clear()
    {
        foreach (var (placedHold, _) in _placedHolds)
            DestroyImmediate(placedHold);

        _placedHolds.Clear();

        _heldObject = null;
        _heldObjectBlueprint = null;
        _heldObjectState = null;
    }
}