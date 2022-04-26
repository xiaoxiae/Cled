using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the player light and the lights placed around the room.
/// </summary>
public class LightManager : MonoBehaviour
{
    public GameObject playerLight;

    private readonly List<GameObject> _lights = new();
    private readonly List<Action<bool>> _playerLightCallbacks = new();

    private bool _playerLightEnabled;

    /// <summary>
    /// Whether the player light is enabled.
    /// </summary>
    public bool PlayerLightEnabled
    {
        get => _playerLightEnabled;
        set
        {
            _playerLightEnabled = value;

            playerLight.GetComponent<Light>().enabled = value;

            foreach (var action in _playerLightCallbacks)
                action(value);
        }
    }

    /// <summary>
    /// Add a callback function that gets called every time the player light changes.
    /// </summary>
    public void AddPlayerLightCallback(Action<bool> action) => _playerLightCallbacks.Add(action);

    /// <summary>
    /// Clear all of the lights and reset the player light.
    /// </summary>
    public void Clear()
    {
        foreach (GameObject light in _lights)
            DestroyImmediate(light);

        _lights.Clear();
        PlayerLightEnabled = true;
        
        UpdateLightIntensity();
        UpdateShadowStrength();
    }

    /// <summary>
    /// Get the position of all of the lights.
    /// </summary>
    public List<Vector3> GetPositions() => _lights.Select(x => x.transform.position).ToList();

    /// <summary>
    /// Update shadow strength according to the preferences manager.
    /// </summary>
    public void UpdateShadowStrength() =>
        _applyActionToLights(light => { light.shadowStrength = Preferences.ShadowStrength; });

    /// <summary>
    /// Update light intensity according to the preferences manager.
    /// </summary>
    public void UpdateLightIntensity() =>
        _applyActionToLights(light => { light.intensity = Preferences.LightIntensity; });

    /// <summary>
    /// Apply a given action to all of the lights.
    /// </summary>
    private void _applyActionToLights(Action<Light> lightFunction)
    {
        lightFunction(playerLight.GetComponent<Light>());

        foreach (var light in _lights)
            lightFunction(light.GetComponent<Light>());
    }

    /// <summary>
    /// Add a new light at the player's location.
    /// </summary>
    public void AddLightAtPlayer() => AddLight(transform.position);

    /// <summary>
    /// Add a new light at the given location.
    /// </summary>
    public void AddLight(Vector3 position)
    {
        // copy the player light
        GameObject lightGameObject = Instantiate(playerLight);
        lightGameObject.GetComponent<Light>().enabled = true;
        lightGameObject.transform.position = position;

        _lights.Add(lightGameObject);
    }

    void Update()
    {
        // move to player
        playerLight.transform.position = transform.position;

        // don't move the camera when time doesn't run
        if (Time.timeScale == 0)
            return;

        // place light
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
            AddLightAtPlayer();

        // toggle player light
        else if (Input.GetKeyDown(KeyCode.F))
            PlayerLightEnabled = !PlayerLightEnabled;
    }
}