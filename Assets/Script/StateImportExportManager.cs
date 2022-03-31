using UnityEngine;

public class StateImportExportManager : MonoBehaviour
{
    public HoldStateManager holdStateManager;
    public RouteManager routeManager;
    public WallManager wallManager;
    
    /// <summary>
    /// Import the state from the given path.
    /// 
    /// Return true if successful, else false.
    /// </summary>
    public bool Import(string path)
    {
        return false;
    }

    /// <summary>
    /// Export the state to the given path.
    /// 
    /// Return true if successful, else false.
    /// </summary>
    public bool Export(string path)
    {
        return false;
    }
}
