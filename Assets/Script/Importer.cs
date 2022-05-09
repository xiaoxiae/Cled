using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

/// <summary>
///     The class that takes care of importing and exporting the editor state.
/// </summary>
public class Importer : MonoBehaviour, IResetable
{
    public HoldStateManager holdStateManager;
    public HoldPickerMenu holdPickerMenu;
    public HoldLoader holdLoader;
    public RouteManager routeManager;
    public WallLoader wallLoader;
    public LightManager lightManager;
    public PauseMenu pauseMenu;
    public PopupMenu popupMenu;
    public BottomBar bottomBar;
    public LoadingScreenMenu loadingScreenMenu;

    public MovementController movementController;
    public CameraController cameraController;

    /// <summary>
    ///     Reset the editor state.
    /// </summary>
    public void Reset()
    {
        holdStateManager.Reset();
        holdLoader.Reset();
        routeManager.Reset();
        wallLoader.Reset();
        holdPickerMenu.Reset();
        lightManager.Reset();
        pauseMenu.Reset();
        movementController.Reset();
        cameraController.Reset();
    }

    /// <summary>
    ///     Return the deserialized state object, given its path.
    /// </summary>
    private static SerializableState Deserialize(string path)
    {
        var deserializer = new Deserializer();

        using var reader = new StreamReader(path);
        return deserializer.Deserialize<SerializableState>(reader);
    }

    /// <summary>
    ///     Import the preferences (path to holds and to wall), returning true if successful.
    /// </summary>
    public bool ImportPreferences(string path)
    {
        try
        {
            var obj = Deserialize(path);

            Preferences.CurrentWallModelPath = obj.WallModelPath;
            Preferences.CurrentHoldModelsPath = obj.HoldModelsPath;
        }
        catch (Exception e)
        {
            popupMenu.CreateInfoPopup($"The following exception occurred while importing the project: {e.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Close the loading screen with a given exception during an action.
    /// </summary>
    private void CloseWithException(string action, string errorMessage)
    {
        Reset();
        Preferences.Initialized = false;

        popupMenu.CreateInfoPopup(
            $"The following exception occurred while {action}: {errorMessage}",
            loadingScreenMenu.Close
        );
    }

    /// <summary>
    ///     Asynchronously import state while showing the loading screen.
    /// </summary>
    private IEnumerator ImportStateAsync(string path)
    {
        loadingScreenMenu.Show("Clearing current state...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Reset();

        loadingScreenMenu.Show("Loading the wall...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            // initialize wall
            wallLoader.Initialize(Preferences.CurrentWallModelPath);
        }
        catch (Exception e)
        {
            CloseWithException("loading the wall", e.Message);
            yield break;
        }

        loadingScreenMenu.Show("Loading the holds...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            // initialize holds
            holdLoader.Initialize(Preferences.CurrentHoldModelsPath);
        }
        catch (Exception e)
        {
            CloseWithException("loading the holds", e.Message);
            yield break;
        }

        loadingScreenMenu.Show("Populating the wall...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        SerializableState obj;
        try
        {
            obj = Deserialize(path);

            var holds = new Dictionary<string, GameObject>();

            // import holds
            foreach (var (id, serializableHold) in obj.Holds)
            {
                var hold = holdStateManager.InstantiatePlace(
                    holdLoader.GetHoldBlueprint(serializableHold.BlueprintId), serializableHold.State);
                holds[id] = hold;
            }

            // import routes
            foreach (var serializableRoute in obj.Routes)
            {
                var route = routeManager.CreateRoute();

                foreach (var hold in serializableRoute.HoldIDs.Select(x => holds[x]))
                    routeManager.ToggleHold(route, hold, holdStateManager.GetHoldBlueprint(hold));

                route.Name = serializableRoute.Name;
                route.Grade = serializableRoute.Grade;
                route.Setter = serializableRoute.Setter;
                route.Zone = serializableRoute.Zone;
            }

            // import starting hold 
            foreach (var hold in obj.StartingHoldIDs.Select(x => holds[x]))
                routeManager.ToggleStarting(hold, holdStateManager.GetHoldBlueprint(hold));

            // import ending holds
            foreach (var hold in obj.EndingHoldIDs.Select(x => holds[x]))
                routeManager.ToggleEnding(hold, holdStateManager.GetHoldBlueprint(hold));
        }
        catch (Exception e)
        {
            CloseWithException("populating the wall", e.Message);
            yield break;
        }

        loadingScreenMenu.Show("Configuring...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            // set player position
            movementController.Position = obj.Player.Position;
            cameraController.Orientation = obj.Player.Orientation;
            movementController.Flying = obj.Player.Flying;

            // capture image settings
            Preferences.CaptureImagePath = obj.CaptureSettings.ImagePath;
            Preferences.ImageSupersize = obj.CaptureSettings.ImageSupersize;

            // import lights
            Preferences.LightIntensity = obj.Lights.Intensity;
            Preferences.ShadowStrength = obj.Lights.ShadowStrength;

            lightManager.UpdateLightIntensity();
            lightManager.UpdateShadowStrength();

            foreach (Vector3 position in obj.Lights.Positions)
                lightManager.AddLight(position);

            lightManager.PlayerLightEnabled = obj.Player.Light;

            holdPickerMenu.Initialize();

            foreach (var blueprintId in obj.SelectedHoldBlueprintIDs)
                holdPickerMenu.Select(holdLoader.GetHoldBlueprint(blueprintId));

            Preferences.Initialized = true;
        }
        catch (Exception e)
        {
            CloseWithException("configuring", e.Message);
            yield break;
        }

        loadingScreenMenu.Close();
    }

    /// <summary>
    ///     Import the state from the given path.
    ///     Must be called after ImportPreferences and after the Awake functions of managers were called.
    ///     Return true if successful, else false.
    /// </summary>
    public void ImportState(string path)
    {
        StartCoroutine(ImportStateAsync(path));
    }

    /// <summary>
    ///     Asynchronously import the project from the given paths.
    /// </summary>
    private IEnumerator ImportFromNewAsync(string currentWallModelPath, string currentHoldModelsPath)
    {
        loadingScreenMenu.Show("Clearing current state...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Reset();

        loadingScreenMenu.Show("Loading the wall...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            // initialize wall
            wallLoader.Initialize(currentWallModelPath);
        }
        catch (Exception e)
        {
            CloseWithException("loading the wall", e.Message);
            yield break;
        }

        loadingScreenMenu.Show("Loading the holds...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            // initialize holds
            holdLoader.Initialize(currentHoldModelsPath);
        }
        catch (Exception e)
        {
            CloseWithException("loading the holds", e.Message);
            yield break;
        }

        loadingScreenMenu.Show("Configuring...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            holdPickerMenu.Initialize();

            movementController.Reset();
            cameraController.Reset();

            Preferences.SetToDefault();

            Preferences.Initialized = true;
        }
        catch (Exception e)
        {
            CloseWithException("configuring", e.Message);
            yield break;
        }

        loadingScreenMenu.Close();
    }

    /// <summary>
    ///     Initialize a new state from a wall model and holds folder.
    ///     Returns true if successful, else false.
    /// </summary>
    public void ImportFromNew(string currentWallModelPath, string currentHoldModelsPath)
    {
        StartCoroutine(ImportFromNewAsync(currentWallModelPath, currentHoldModelsPath));
    }
}