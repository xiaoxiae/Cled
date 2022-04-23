using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;


/// <summary>
/// The object that gets serialized when exporting.
/// </summary>
public class SerializableState
{
    public string Version;
    
    // player
    public SerializablePlayer Player;

    // paths to wall and models
    public string WallModelPath { get; set; }
    public string HoldModelsPath { get; set; }

    // given a hold instance, store its id and state
    public Dictionary<string, SerializableHold> Holds { get; set; }

    // all routes
    public List<SerializableRoute> Routes { get; set; }

    // starting/ending markers
    public List<string> StartingHoldIDs { get; set; }
    public List<string> EndingHoldIDs { get; set; }

    // lights
    public SerializableLights Lights { get; set; }
}

/// <summary>
/// A route that can be serialized.
/// </summary>
public class SerializableRoute
{
    public List<string> HoldIDs { get; set; }

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

public class StateImportExportManager : MonoBehaviour
{
    public HoldStateManager holdStateManager;
    public HoldPickerManager holdPickerManager;
    public HoldManager holdManager;
    public RouteManager routeManager;
    public WallManager wallManager;
    public LightManager lightManager;
    public PauseManager pauseManager;

    public PopupManager popupManager;
    
    public MovementControl movementControl;
    public CameraControl cameraControl;

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
    /// Clear appropriate manager states.
    /// </summary>
    private void Clear()
    {
        holdStateManager.Clear();
        holdManager.Clear();
        routeManager.Clear();
        wallManager.Clear();
        holdPickerManager.Clear();
        lightManager.Clear();
        
        movementControl.Position = Vector3.zero;
        cameraControl.Orientation = Vector3.forward;
        
        pauseManager.UnpauseAll();
    }
    
    /// <summary>
    /// Import the preferences (path to holds and to wall).
    ///
    /// Since this is a static function called either from the main menu or ingame, a popup manager object
    /// must be passed for the popup to properly show.
    /// </summary>
    public bool ImportPreferences(string path)
    {
        try
        {
            var obj = Deserialize(path);

            PreferencesManager.CurrentWallModelPath = obj.WallModelPath;
            PreferencesManager.CurrentHoldModelsPath = obj.HoldModelsPath;
        }
        catch (Exception e)
        {
            popupManager.CreateInfoPopup($"The following exception occurred while exporting the project:\n\n{e}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Import the state from the given path.
    /// Must be called after ImportPreferences and after the Awake functions of managers were called.
    /// 
    /// Return true if successful, else false.
    /// </summary>
    public bool ImportState(string path)
    {
        try
        {
            Clear();

            // initialize wall
            wallManager.Initialize(PreferencesManager.CurrentWallModelPath);
            
            // initialize holds
            holdManager.Initialize(PreferencesManager.CurrentHoldModelsPath);
            
            var obj = Deserialize(path);

            var holds = new Dictionary<string, GameObject>();

            // import holds
            foreach (var (id, serializableHold) in obj.Holds)
            {
                var hold = holdStateManager.Place(
                    holdManager.GetHoldBlueprint(serializableHold.BlueprintId), serializableHold.State);
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

            // set player position
            movementControl.Position = obj.Player.Position;
            cameraControl.Orientation = obj.Player.Orientation;
            movementControl.Flying = obj.Player.Flying;

            // import lights
            foreach (Vector3 position in obj.Lights.Positions)
                lightManager.AddLight(position);

            lightManager.Intensity = obj.Lights.Intensity;
            lightManager.ShadowStrength = obj.Lights.ShadowStrength;
            lightManager.PlayerLightEnabled = obj.Player.Light;

            holdPickerManager.Initialize();
        }
        catch (Exception e)
        {
            popupManager.CreateInfoPopup($"The following exception occurred while importing the project:\n\n{e}");
            return false;
        }

        return true;
    }

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
            foreach (GameObject hold in holdStateManager.GetAllHolds())
            {
                var holdBlueprint = holdStateManager.GetHoldBlueprint(hold);
                var holdState = holdStateManager.GetHoldState(hold);

                holds[Utilities.GetObjectId(hold)] = new SerializableHold
                    { BlueprintId = holdBlueprint.Id, State = holdState };
            }

            // save only routes that either contain one or more holds, or contain a starting/ending hold
            var routes = new List<SerializableRoute>();
            foreach (Route route in routeManager.GetRoutes())
                if (route.Holds.Length > 1 || route.StartingHolds.Length != 0 || route.EndingHolds.Length != 0)
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
                        Position = movementControl.Position,
                        Orientation = cameraControl.Orientation,
                        Flying = movementControl.Flying,
                        Light = lightManager.PlayerLightEnabled
                    },
                    WallModelPath = PreferencesManager.CurrentWallModelPath,
                    HoldModelsPath = PreferencesManager.CurrentHoldModelsPath,
                    Holds = holds,
                    Routes = routes,
                    StartingHoldIDs = routeManager.StartingHolds.Select(Utilities.GetObjectId).ToList(),
                    EndingHoldIDs = routeManager.EndingHolds.Select(Utilities.GetObjectId).ToList(),
                    Lights = new SerializableLights
                    {
                        Positions = lightManager.GetPositions().Select<Vector3, SerializableVector3>(x => x).ToList(),
                        Intensity = lightManager.Intensity,
                        ShadowStrength = lightManager.ShadowStrength,
                    }
                });

            return true;
        }
        catch (Exception e)
        {
            popupManager.CreateInfoPopup($"The following exception occurred while exporting the project:\n\n{e}");
            return false;
        }
    }

    /// <summary>
    /// Initialize a new state from a wall model and holds folder.
    /// </summary>
    public bool ImportFromNew(string currentWallModelPath, string currentHoldModelsPath)
    {
        try
        {
            Clear();

            wallManager.Initialize(currentWallModelPath);
            holdManager.Initialize(currentHoldModelsPath);
            
            holdPickerManager.Initialize();

            return true;
        }
        catch (Exception e)
        {
            popupManager.CreateInfoPopup($"The following exception occurred while exporting the project:\n\n{e}");
            return false;
        }
    }
}