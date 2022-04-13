using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using YamlDotNet.Serialization;


/// <summary>
/// The object that gets serialized when exporting.
/// </summary>
public class SerializableWallState
{
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

        wallManager.InitializeFromPath(PreferencesManager.CurrentWallModelPath);

        return true;
    }

    /// <summary>
    /// Return an UID of a GameObject that is a string.
    /// </summary>
    private static string GetObjectId(GameObject obj) => Sha256(obj.GetInstanceID().ToString())[..12];

    /// <summary>
    /// Return a sha256 hash from a string.
    /// </summary>
    private static string Sha256(string randomString)
    {
        var crypt = new System.Security.Cryptography.SHA256Managed();
        var hash = new System.Text.StringBuilder();
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
        foreach (byte theByte in crypto)
        {
            hash.Append(theByte.ToString("x2"));
        }

        return hash.ToString();
    }

    /// <summary>
    /// Export the state to the given path.
    /// 
    /// Return true if successful, else false.
    /// </summary>
    public bool Export(string path)
    {
        var ext = Path.GetExtension(path);

        // if it doesn't contain an extension, add it
        // if it does, the data will still be YAML, but it's less invasive than changing it
        if (ext == "")
            path += ".yaml";

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

                holds[GetObjectId(hold)] = new SerializableHold
                    { BlueprintId = holdBlueprint.Id, State = holdState };
            }

            // save only routes that either contain one or more holds, or contain a starting/ending hold
            var routes = new List<SerializableRoute>();
            foreach (Route route in routeManager.GetRoutes())
                if (route.Holds.Length > 1 || route.StartingHolds.Length != 0 || route.EndingHolds.Length != 0)
                    routes.Add(new SerializableRoute
                        { HoldIDs = route.Holds.Select(GetObjectId).ToList() });

            serializer.Serialize(writer,
                new SerializableWallState
                {
                    WallModelPath = PreferencesManager.CurrentWallModelPath,
                    HoldModelsPath = PreferencesManager.CurrentHoldModelsPath,
                    HoldStates = holds,
                    Routes = routes,
                    StartingHoldIDs = routeManager._startingHolds.Select(GetObjectId).ToList(),
                    EndingHoldIDs = routeManager._endingHolds.Select(GetObjectId).ToList(),
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