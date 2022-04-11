using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Information about the given hold's state when it's placed on the wall.
/// Used by various classes to smoothly continue editing it.
/// </summary>
public struct HoldState
{
    public float Rotation;
}

/// <summary>
/// Manages the state of the holds on the wall (even the one held).
/// </summary>
public class HoldStateManager : MonoBehaviour
{
    private readonly Dictionary<GameObject, HoldState> _placedHolds = new();

    // the held object (state)
    private GameObject _heldObject;
    private HoldState _heldObjectState;

    // where the held object pointed previously (for smooth movement)
    private Vector3 _previousHeldObjectNormal = Vector3.zero;

    /// <summary>
    /// Clear out the hold state manager, destroying all objects in the process.
    /// </summary>
    public void Clear()
    {
        if (_heldObject)
        {
            DestroyImmediate(_heldObject);
            _heldObject = null;
        }
        
        _heldObjectState = new HoldState();
        
        foreach(GameObject key in _placedHolds.Keys)
            DestroyImmediate(key);
        
        _placedHolds.Clear();
    }

    /// <summary>
    /// Start holding a hold from a hold object (copying it over).
    /// </summary>
    public void SetHeld(HoldBlueprint holdBlueprint, HoldState holdState = new())
    {
        SetHeld(Instantiate(holdBlueprint.Model), holdState);
    }

    /// <summary>
    /// Start holding a GameObject directly (no copying over).
    /// </summary>
    public void SetHeld(GameObject model, HoldState holdState = new())
    {
        _heldObject = model;
        _heldObjectState = holdState;
        _previousHeldObjectNormal = model.transform.forward;

        // ignore this object until placed
        _heldObject.layer = 2;
        _heldObject.SetActive(true);
    }

    /// <summary>
    /// Rotate the currently held hold by a certain amount in radians.
    /// </summary>
    public void RotateHeld(float delta)
    {
        _heldObjectState.Rotation = (_heldObjectState.Rotation + delta) % (2 * (float)Math.PI);
    }

    /// <summary>
    /// Stop holding the hold.
    /// </summary>
    public void SetUnheld(bool destroy = false)
    {
        if (destroy)
            DestroyImmediate(_heldObject);

        _heldObject = null;
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
    public void PutDown()
    {
        Place(_heldObject, _heldObjectState);
        SetUnheld();
    }

    /// <summary>
    /// Pick up the currently placed hold.
    /// </summary>
    /// <param name="model"></param>
    public void PickUp(GameObject model)
    {
        SetHeld(model, GetHoldState(model));
        Unplace(model);
    }

    /// <summary>
    /// Add a hold to the state manager.
    /// </summary>
    public void Place(GameObject model, HoldState state)
    {
        _placedHolds.Add(model, state);
        model.layer = 0; // don't ignore the object when placed
    }

    /// <summary>
    /// Remove the hold from the state manager.
    /// </summary>
    public void Unplace(GameObject model, bool destroy = false)
    {
        _placedHolds.Remove(model);
        
        if (destroy)
            DestroyImmediate(model);
    }

    /// <summary>
    /// Get the state of the given model object.
    /// </summary>
    private HoldState GetHoldState(GameObject model) => _placedHolds[model];

    /// <summary>
    /// Return True if this game object is placed.
    /// </summary>
    public bool IsPlacedHold(GameObject model) => _placedHolds.ContainsKey(model);

    /// <summary>
    /// Smoothly move the currently held hold to the specified point.
    /// </summary>
    private void MoveHeldToPoint(Vector3 point, float smoothness)
    {
        _heldObject.transform.position = Vector3.Lerp(_heldObject.transform.position, point, 1 - smoothness);
    }

    /// <summary>
    /// Smoothly update the currently held hold normal to the specified vector and rotation about it.
    /// </summary>
    private void UpdateHeldNormal(Vector3 normal, float smoothness)
    {
        // calculate the normal smoothly
        var currentNormal = Vector3.Lerp(_previousHeldObjectNormal, normal, 1 - smoothness);

        // rotate the world "up" around the hit normal by some degrees
        var upVector = Quaternion.AngleAxis(Mathf.Rad2Deg * _heldObjectState.Rotation, normal) * Vector3.up;

        _heldObject.transform.LookAt(currentNormal + _heldObject.transform.position, upVector);

        _previousHeldObjectNormal = currentNormal;
    }

    /// <summary>
    /// Move the currently held hold to the raycast hit.
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="smoothness"></param>
    public void InterpolateHeldToHit(RaycastHit hit, float smoothness = 0.5f)
    {
        MoveHeldToPoint(hit.point, smoothness);
        UpdateHeldNormal(hit.normal, smoothness);
    }
}