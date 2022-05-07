using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Information about the given hold's state when it's placed on the wall.
///     Used to smoothly continue editing it and for import/export.
/// </summary>
public class HoldState
{
    /// <summary>
    ///     Whether the hold is horizontally flipped.
    /// </summary>
    public bool Flipped;

    /// <summary>
    ///     The hold's normal.
    /// </summary>
    public SerializableVector3 Normal;

    /// <summary>
    ///     The hold's position.
    /// </summary>
    public SerializableVector3 Position;

    /// <summary>
    ///     The hold's rotation relative to its normal on the wall.
    /// </summary>
    public float Rotation;
}

/// <summary>
///     Manages the state of the holds on the wall (even the one held).
/// </summary>
public class HoldStateManager : MonoBehaviour, IResetable
{
    private readonly Dictionary<GameObject, (HoldBlueprint holdBlueprint, HoldState holdState)> _placedHolds = new();

    // the held object (state)
    private HoldBlueprint _heldObjectBlueprint;
    private HoldState _heldObjectState;

    /// <summary>
    ///     The currently placed holds.
    /// </summary>
    public IEnumerable<GameObject> PlacedHolds => _placedHolds.Keys;

    /// <summary>
    ///     The currently held hold.
    /// </summary>
    public GameObject HeldHold { get; private set; }

    /// <summary>
    ///     Clear all of the instances of the holds, destroying them in the process.
    /// </summary>
    public void Reset()
    {
        foreach (var (placedHold, _) in _placedHolds)
            DestroyImmediate(placedHold);

        _placedHolds.Clear();

        HeldHold = null;
        _heldObjectBlueprint = null;
        _heldObjectState = null;
    }

    /// <summary>
    ///     Start holding a hold from a hold object.
    ///     If replace is specified, we're replacing the currently held one with another one, saving the state.
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
        {
            StartHolding(Instantiate(holdBlueprint.Model), holdBlueprint, new HoldState());
        }
    }

    /// <summary>
    ///     Start holding a GameObject hold.
    ///     If it was placed and we're starting to hold it, remove it from placed.
    /// </summary>
    private void StartHolding(GameObject model, HoldBlueprint holdBlueprint, HoldState holdState)
    {
        if (IsPlaced(model))
            _placedHolds.Remove(model);

        HeldHold = model;
        _heldObjectBlueprint = holdBlueprint;
        _heldObjectState = holdState;

        ApplyHoldState(model, holdState);

        // ignore this object until placed
        HeldHold.layer = LayerMask.NameToLayer("Ignore Raycast");

        EnableHeld();
    }

    /// <summary>
    ///     Rotate the currently held hold by a certain amount in radians.
    /// </summary>
    public void RotateHeld(float delta)
    {
        _heldObjectState.Rotation = (_heldObjectState.Rotation + delta) % (2 * (float)Math.PI);
    }

    /// <summary>
    ///     Stop holding the currently held hold, destroying it in the process.
    /// </summary>
    public void StopHolding()
    {
        DestroyImmediate(HeldHold);

        HeldHold = null;
        _heldObjectBlueprint = null;
        _heldObjectState = null;
    }

    /// <summary>
    ///     Disable the currently held item.
    /// </summary>
    public void DisableHeld()
    {
        HeldHold.SetActive(false);
    }

    /// <summary>
    ///     Enable the currently held item.
    /// </summary>
    public void EnableHeld()
    {
        HeldHold.SetActive(true);
    }

    /// <summary>
    ///     Place the currently held hold.
    /// </summary>
    public void PutDown()
    {
        Place(HeldHold, _heldObjectBlueprint, _heldObjectState);
    }

    /// <summary>
    ///     Pick up a currently placed hold.
    /// </summary>
    public void PickUp(GameObject model)
    {
        StartHolding(model, GetHoldBlueprint(model), GetHoldState(model));
    }

    /// <summary>
    ///     Place a new hold from the hold blueprint.
    /// </summary>
    public GameObject InstantiatePlace(HoldBlueprint holdBlueprint, HoldState holdState)
    {
        var instance = Instantiate(holdBlueprint.Model);
        Place(instance, holdBlueprint, holdState);
        return instance;
    }

    /// <summary>
    ///     Place the given hold.
    /// </summary>
    public void Place(GameObject model, HoldBlueprint holdBlueprint, HoldState holdState)
    {
        // if we're placing the held object, stop holding it
        if (model == HeldHold)
        {
            HeldHold = null;
            _heldObjectBlueprint = null;
            _heldObjectState = null;
        }

        _placedHolds.Add(model, (holdBlueprint, holdState));

        model.layer = LayerMask.NameToLayer("Default");

        ApplyHoldState(model, holdState);

        model.SetActive(true);
    }

    /// <summary>
    ///     Apply the hold state to the given hold.
    /// </summary>
    private void ApplyHoldState(GameObject hold, HoldState state)
    {
        // update the hold normal and the hold position
        UpdateNormal(hold, state.Normal, state.Rotation);
        UpdatePosition(hold, state.Position);

        // update whether the hold is flipped
        hold.transform.localScale = state.Flipped ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
    }

    /// <summary>
    ///     Update the gameObject to turn towards the given vector and rotate in the given float (in radians).
    /// </summary>
    private void UpdateNormal(GameObject gameObject, Vector3 normal, float rotation)
    {
        // rotate the world "up" around the hit normal by some degrees
        var upVector = Quaternion.AngleAxis(Mathf.Rad2Deg * rotation, normal) * Vector3.up;
        gameObject.transform.LookAt(normal + gameObject.transform.position, upVector);
    }

    /// <summary>
    ///     Update the gameObject's position to the given position.
    /// </summary>
    private void UpdatePosition(GameObject gameObject, Vector3 position)
    {
        gameObject.transform.position = position;
    }

    /// <summary>
    ///     Remove the hold from the state manager, no matter if it's being held or if it's placed down.
    /// </summary>
    public void Remove(GameObject model, bool destroy = false)
    {
        if (HeldHold == model)
            HeldHold = null;
        else
            _placedHolds.Remove(model);

        if (destroy)
            DestroyImmediate(model);
    }

    /// <summary>
    ///     Get the state of the given model object.
    /// </summary>
    public HoldState GetHoldState(GameObject model)
    {
        return model == HeldHold ? _heldObjectState : _placedHolds[model].holdState;
    }

    /// <summary>
    ///     Get the holdBlueprint of the given model object.
    /// </summary>
    public HoldBlueprint GetHoldBlueprint(GameObject model)
    {
        return model == HeldHold ? _heldObjectBlueprint : _placedHolds[model].holdBlueprint;
    }

    /// <summary>
    ///     Return True if this game object is placed.
    /// </summary>
    public bool IsPlaced(GameObject model)
    {
        return _placedHolds.ContainsKey(model);
    }

    /// <summary>
    ///     Move the currently held hold to the raycast hit.
    /// </summary>
    public void MoveHeldToHit(RaycastHit hit)
    {
        _heldObjectState.Position = hit.point;
        _heldObjectState.Normal = hit.normal;

        ApplyHoldState(HeldHold, _heldObjectState);
    }

    /// <summary>
    ///     Horizontally flip the given hold.
    /// </summary>
    public void FlipHold(GameObject gameObject)
    {
        var state = GetHoldState(gameObject);
        state.Flipped = !state.Flipped;

        // we have to flip the rotation too to actually make it horizontally flipped
        state.Rotation = (float)(2 * Math.PI - state.Rotation);

        ApplyHoldState(gameObject, state);
    }
}