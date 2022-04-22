using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// By default, serialization emits all public fields, which includes things like magnitude and normalized form.
/// This class ensures that only x, y and z coordinates of the Vector3 are stored and the serialization looks nice.
/// There is probably a different way to do this, but this works well enough.
/// </summary>
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    /// <summary>
    /// Automatic conversion from SerializableVector3 to Vector3.
    /// </summary>
    public static implicit operator Vector3(SerializableVector3 rValue) => new(rValue.x, rValue.y, rValue.z);

    /// <summary>
    /// Automatic conversion from Vector3 to SerializableVector3.
    /// </summary>
    public static implicit operator SerializableVector3(Vector3 rValue) => new(rValue.x, rValue.y, rValue.z);
}

/// <summary>
/// Same as SerializableVector2.
/// </summary>
public struct SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2(float rX, float rY)
    {
        x = rX;
        y = rY;
    }

    /// <summary>
    /// Automatic conversion from SerializableVector3 to Vector3.
    /// </summary>
    public static implicit operator Vector2(SerializableVector2 rValue) => new(rValue.x, rValue.y);

    /// <summary>
    /// Automatic conversion from Vector3 to SerializableVector3.
    /// </summary>
    public static implicit operator SerializableVector2(Vector2 rValue) => new(rValue.x, rValue.y);
}

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
    /// Get all currently placed holds.
    /// </summary>
    public GameObject[] GetAllHolds() => _placedHolds.Keys.ToArray();

    public GameObject GetHeld() => _heldObject;

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

        foreach (GameObject key in _placedHolds.Keys)
            DestroyImmediate(key);

        _placedHolds.Clear();
    }

    /// <summary>
    /// Start holding a hold from a hold object (instantiating it).
    /// </summary>
    public void SetHeld(HoldBlueprint holdBlueprint) => SetHeld(holdBlueprint, new HoldState());

    /// <summary>
    /// Start holding a hold from a hold object (instantiating it) and given hold state.
    /// </summary>
    public void SetHeld(HoldBlueprint holdBlueprint, HoldState holdState)
        => SetHeld(Instantiate(holdBlueprint.Model), holdBlueprint, holdState);

    /// <summary>
    /// Start holding a GameObject directly (no copying over).
    /// </summary>
    public void SetHeld(GameObject model, HoldBlueprint holdBlueprint, HoldState holdState)
    {
        _heldObject = model;
        _heldObjectBlueprint = holdBlueprint;
        _heldObjectState = holdState;

        // ignore this object until placed
        _heldObject.layer = 2;
        _heldObject.SetActive(true);
    }

    /// <summary>
    /// Rotate the currently held hold by a certain amount in radians.
    /// </summary>
    public void RotateHeld(float delta)
        => _heldObjectState.Rotation = (_heldObjectState.Rotation + delta) % (2 * (float)Math.PI);

    /// <summary>
    /// Stop holding the hold.
    /// </summary>
    public void SetUnheld(bool destroy = false)
    {
        if (destroy)
            DestroyImmediate(_heldObject);

        _heldObject = null;
        _heldObjectBlueprint = null;
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
        Place(_heldObject, _heldObjectBlueprint, _heldObjectState);
        _heldObject.layer = 0; // don't ignore the object when placed

        SetUnheld();
    }

    /// <summary>
    /// Pick up a currently placed hold.
    /// </summary>
    public void PickUp(GameObject model)
    {
        SetHeld(model, GetHoldBlueprint(model), GetHoldState(model));
        Unplace(model);
    }

    /// <summary>
    /// Place a new hold from the hold blueprint.
    /// </summary>
    public GameObject Place(HoldBlueprint holdBlueprint, HoldState holdState)
    {
        var instance = Instantiate(holdBlueprint.Model);
        Place(instance, holdBlueprint, holdState);
        return instance;
    }

    /// <summary>
    /// Place the given hold.
    /// </summary>
    public void Place(GameObject gameObject, HoldBlueprint holdBlueprint, HoldState holdState)
    {
        _placedHolds.Add(gameObject, (holdBlueprint, holdState));

        UpdateNormal(holdState.Normal, holdState.Rotation, gameObject);
        UpdatePosition(holdState.Position, gameObject);

        gameObject.SetActive(true);
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
    /// Smoothly move the currently held hold to the specified point.
    /// </summary>
    private void UpdateHeldPosition(Vector3 point)
    {
        UpdatePosition(point, _heldObject);
        _heldObjectState.Position = _heldObject.transform.position;
    }

    private void UpdatePosition(Vector3 point, GameObject gameObject)
    {
        gameObject.transform.position = point;
    }

    /// <summary>
    /// Smoothly update the currently held hold normal to the specified vector and rotation about it.
    /// </summary>
    private void UpdateHeldNormal(Vector3 normal)
    {
        UpdateNormal(normal, _heldObjectState.Rotation, _heldObject);
        _heldObjectState.Normal = normal;
    }

    private void UpdateNormal(Vector3 normal, float rotation, GameObject gameObject)
    {
        // rotate the world "up" around the hit normal by some degrees
        var upVector = Quaternion.AngleAxis(Mathf.Rad2Deg * rotation, normal) * Vector3.up;
        gameObject.transform.LookAt(normal + gameObject.transform.position, upVector);
    }

    /// <summary>
    /// Move the currently held hold to the raycast hit.
    /// </summary>
    public void InterpolateHeldToHit(RaycastHit hit)
    {
        UpdateHeldPosition(hit.point);
        UpdateHeldNormal(hit.normal);
    }
}