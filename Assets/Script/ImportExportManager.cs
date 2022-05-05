using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

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
/// The object that gets serialized when exporting.
/// </summary>
public class SerializableState
{
    public string Version;

    // player
    public SerializablePlayer Player;

    // paths to wall and models
    public string WallModelPath;
    public string HoldModelsPath;

    // given a hold instance, store its id and state
    public Dictionary<string, SerializableHold> Holds;

    // all routes
    public List<SerializableRoute> Routes;

    // starting/ending holds
    public List<string> StartingHoldIDs;
    public List<string> EndingHoldIDs;

    // selected holds
    public List<string> SelectedHoldBlueprintIDs;

    // lights
    public SerializableLights Lights;

    // capture
    public SerializableCaptureSettings CaptureSettings;
}

/// <summary>
/// Image capture settings.
/// </summary>
public class SerializableCaptureSettings
{
    public string ImagePath;
    public int ImageSupersize;
}

/// <summary>
/// A route that can be serialized.
/// </summary>
public class SerializableRoute
{
    public List<string> HoldIDs;

    public string Name;
    public string Grade;
    public string Zone;
    public string Setter;
}

/// <summary>
/// A hold that can be serialized.
/// </summary>
public class SerializableHold
{
    public string BlueprintId;
    public HoldState State;
}

/// <summary>
/// Lights.
/// </summary>
public class SerializableLights
{
    // positions of the lights
    public List<SerializableVector3> Positions;

    // light parameters
    public float Intensity;
    public float ShadowStrength;
}

/// <summary>
/// Player.
/// </summary>
public class SerializablePlayer
{
    // the player position and orientation
    public SerializableVector3 Position { get; set; }
    public SerializableVector2 Orientation { get; set; }

    // whether the player is flying
    public bool Flying { get; set; }

    // whether the player's flashlight is on
    public bool Light;
}

/// <summary>
/// The class that takes care of importing and exporting the editor state.
/// </summary>
public class ImportExportManager : MonoBehaviour
{
    public HoldStateManager holdStateManager;
    public HoldPickerMenu holdPickerMenu;
    public HoldLoader holdLoader;
    public RouteManager routeManager;
    public WallLoader wallLoader;
    public LightManager lightManager;
    public PauseMenu pauseMenu;
    public PopupMenu popupMenu;
    public LoadingScreenMenu loadingScreenMenu;

    public PlayerController playerController;
    public CameraController cameraController;

    /// <summary>
    /// Return the deserialized state object, given its path.
    /// </summary>
    private static SerializableState Deserialize(string path)
    {
        var deserializer = new Deserializer();

        using var reader = new StreamReader(path);
        return deserializer.Deserialize<SerializableState>(reader);
    }

    /// <summary>
    /// Clear the editor state.
    /// </summary>
    private void Clear()
    {
        holdStateManager.Clear();
        holdLoader.Clear();
        routeManager.Clear();
        wallLoader.Clear();
        holdPickerMenu.Clear();
        lightManager.Clear();

        playerController.ResetPosition();
        cameraController.ResetOrientation();

        pauseMenu.UnpauseAll();
    }

    /// <summary>
    /// Import the preferences (path to holds and to wall), returning true if successful.
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
    /// Close the loading screen with a given exception during an action.
    /// </summary>
    private void CloseWithException(string action, string errorMessage)
    {
        Clear();
        Preferences.Initialized = false;
        
        popupMenu.CreateInfoPopup(
            $"The following exception occurred while {action}: {errorMessage}",
            loadingScreenMenu.Close
        );
    }

    /// <summary>
    /// Asynchronously import state while showing the loading screen.
    /// </summary>
    private IEnumerator ImportStateAsync(string path)
    {
        loadingScreenMenu.Show("Clearing current state...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Clear();

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

                foreach (GameObject hold in serializableRoute.HoldIDs.Select(x => holds[x]))
                    routeManager.ToggleHold(route, hold, holdStateManager.GetHoldBlueprint(hold));

                route.Name = serializableRoute.Name;
                route.Grade = serializableRoute.Grade;
                route.Setter = serializableRoute.Setter;
                route.Zone = serializableRoute.Zone;
            }

            // import starting hold 
            foreach (GameObject hold in obj.StartingHoldIDs.Select(x => holds[x]))
                routeManager.ToggleStarting(hold, holdStateManager.GetHoldBlueprint(hold));

            // import ending holds
            foreach (GameObject hold in obj.EndingHoldIDs.Select(x => holds[x]))
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
            playerController.Position = obj.Player.Position;
            cameraController.Orientation = obj.Player.Orientation;
            playerController.Flying = obj.Player.Flying;

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

            foreach (string blueprintId in obj.SelectedHoldBlueprintIDs)
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
    /// Import the state from the given path.
    /// Must be called after ImportPreferences and after the Awake functions of managers were called.
    /// 
    /// Return true if successful, else false.
    /// </summary>
    public void ImportState(string path) => StartCoroutine(ImportStateAsync(path));

    /// <summary>
    /// Export the state to the given path.
    /// 
    /// Return true if successful, else false.
    /// </summary>
    public bool Export(string path)
    {
        try
        {
            using StreamWriter writer = new StreamWriter(path);

            var serializer = new SerializerBuilder().DisableAliases().Build();

            // save holds
            var holds = new Dictionary<string, SerializableHold>();
            foreach (GameObject hold in holdStateManager.PlacedHolds)
            {
                var holdBlueprint = holdStateManager.GetHoldBlueprint(hold);
                var holdState = holdStateManager.GetHoldState(hold);

                holds[Utilities.GetObjectId(hold)] = new SerializableHold
                    { BlueprintId = holdBlueprint.Id, State = holdState };
            }

            // save only routes that either contain one or more holds, or contain a starting/ending hold
            var routes = new List<SerializableRoute>();
            foreach (Route route in routeManager.GetUsableRoutes())
                routes.Add(new SerializableRoute
                {
                    HoldIDs = route.Holds.Select(Utilities.GetObjectId).ToList(),
                    Name = route.Name,
                    Setter = route.Setter,
                    Zone = route.Zone,
                    Grade = route.Grade,
                });

            serializer.Serialize(writer,
                new SerializableState
                {
                    Version = Application.version,
                    Player = new SerializablePlayer
                    {
                        Position = playerController.Position,
                        Orientation = cameraController.Orientation,
                        Flying = playerController.Flying,
                        Light = lightManager.PlayerLightEnabled
                    },
                    WallModelPath = Preferences.CurrentWallModelPath,
                    HoldModelsPath = Preferences.CurrentHoldModelsPath,
                    Holds = holds,
                    Routes = routes,
                    StartingHoldIDs = routeManager.StartingHolds.Select(Utilities.GetObjectId).ToList(),
                    EndingHoldIDs = routeManager.EndingHolds.Select(Utilities.GetObjectId).ToList(),
                    SelectedHoldBlueprintIDs = holdPickerMenu.GetPickedHolds().Select(val => val.Id).ToList(),
                    Lights = new SerializableLights
                    {
                        Positions = lightManager.GetPositions().Select<Vector3, SerializableVector3>(x => x).ToList(),
                        Intensity = Preferences.LightIntensity,
                        ShadowStrength = Preferences.LightIntensity,
                    },
                    CaptureSettings = new SerializableCaptureSettings
                    {
                        ImagePath = Preferences.CaptureImagePath,
                        ImageSupersize = Preferences.ImageSupersize,
                    }
                });

            return true;
        }
        catch (Exception e)
        {
            popupMenu.CreateInfoPopup($"The following exception occurred while exporting the project: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Asynchronously import the project from the given paths.
    /// </summary>
    private IEnumerator ImportFromNewAsync(string currentWallModelPath, string currentHoldModelsPath)
    {
        loadingScreenMenu.Show("Clearing current state...");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Clear();

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
            
            playerController.ResetPosition();
            cameraController.ResetOrientation();

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
    /// Initialize a new state from a wall model and holds folder.
    ///
    /// Returns true if successful, else false.
    /// </summary>
    public void ImportFromNew(string currentWallModelPath, string currentHoldModelsPath) =>
        StartCoroutine(ImportFromNewAsync(currentWallModelPath, currentHoldModelsPath));
}