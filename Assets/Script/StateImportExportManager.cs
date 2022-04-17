using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;


/// <summary>
/// The object that gets serialized when exporting.
/// </summary>
public class SerializableWallState
{
    // the player position and orientation
    public SerializableVector3 PlayerPosition { get; set; }
    public SerializableQuaternion PlayerOrientation { get; set; }
        
    // paths to wall and models
    public string WallModelPath { get; set; }
    public string HoldModelsPath { get; set; }

    // given a hold instance, store its id and state
    public Dictionary<string, SerializableHold> HoldStates { get; set; }

    // also store all routes
    public List<SerializableRoute> Routes { get; set; }

    // and starting/ending markers
    public List<string> StartingHoldIDs { get; set; }
    public List<string> EndingHoldIDs { get; set; }
}

/// <summary>
/// A route that can be serialized.
/// </summary>
public class SerializableRoute
{
    public List<string> HoldIDs { get; set; }
    
    // TODO: other route attributes
}

/// <summary>
/// A hold that can be serialized.
/// </summary>
public class SerializableHold
{
    public string BlueprintId;
    public HoldState State;
}


public class StateImportExportManager : MonoBehaviour
{
    public HoldStateManager holdStateManager;
    public HoldManager holdManager;
    public RouteManager routeManager;
    public WallManager wallManager;

    public MovementControl movementControl;
    public CameraControl cameraControl;

    private static SerializableWallState Deserialize(string path)
    {
        var deserializer = new Deserializer();

        using var reader = new StreamReader(path);
        return deserializer.Deserialize<SerializableWallState>(reader);
    }

    /// <summary>
    /// Import the preferences (path to holds and to wall).
    /// </summary>
    public static bool ImportPreferences(string path)
    {
        try
        {
            var obj = Deserialize(path);

            PreferencesManager.CurrentWallModelPath = obj.WallModelPath;
            PreferencesManager.CurrentHoldModelsPath = obj.HoldModelsPath;
        }
        catch (Exception e)
        {
            Debug.Log(e);

            // TODO display message
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
        var obj = Deserialize(path);

        holdStateManager.Clear();
        routeManager.Clear();

        var holds = new Dictionary<string, GameObject>();

        // import holds
        foreach (var (id, serializableHold) in obj.HoldStates)
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
        }

        // import starting hold 
        foreach (GameObject hold in obj.StartingHoldIDs.Select(x => holds[x]))
            routeManager.ToggleStarting(hold, holdStateManager.GetHoldBlueprint(hold));

        // import ending holds
        foreach (GameObject hold in obj.EndingHoldIDs.Select(x => holds[x]))
            routeManager.ToggleEnding(hold, holdStateManager.GetHoldBlueprint(hold));
        
        // initialize wall
        wallManager.InitializeFromPath(PreferencesManager.CurrentWallModelPath);
        
        // set player position
        movementControl.SetPosition(obj.PlayerPosition);
        cameraControl.SetOrientation(obj.PlayerOrientation);

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
                        { HoldIDs = route.Holds.Select(Utilities.GetObjectId).ToList() });

            serializer.Serialize(writer,
                new SerializableWallState
                {
                    PlayerPosition = movementControl.GetPosition(),
                    PlayerOrientation = cameraControl.GetOrientation(),
                    WallModelPath = PreferencesManager.CurrentWallModelPath,
                    HoldModelsPath = PreferencesManager.CurrentHoldModelsPath,
                    HoldStates = holds,
                    Routes = routes,
                    StartingHoldIDs = routeManager._startingHolds.Select(Utilities.GetObjectId).ToList(),
                    EndingHoldIDs = routeManager._endingHolds.Select(Utilities.GetObjectId).ToList(),
                });
        }
        catch (Exception e)
        {
            Debug.Log(e);

            // TODO display message
            return false;
        }

        return true;
    }
}