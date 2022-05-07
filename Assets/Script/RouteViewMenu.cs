using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class RouteViewMenu : MonoBehaviour, IClosable
{
    public RouteManager routeManager;
    public RouteSettingsMenu routeSettingsMenu;
    public HighlightManager highlightManager;
    public EditorModeManager editorModeManager;

    private ListView _listView;
    private VisualElement _root;

    // Start is called before the first frame update
    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        Utilities.DisableElementFocusable(_root);

        // close by default
        Close();

        VisualElement MakeItem()
        {
            return new Label();
        }

        void BindItem(VisualElement e, int i)
        {
            var route = GetSortedRoutes()[i];
            var l = (Label)e;

            l.text = route.Name ?? "Unnamed Route";

            if (_listView.selectedIndex == i)
                l.style.unityFontStyleAndWeight = FontStyle.Bold;

            if (!string.IsNullOrWhiteSpace(route.Zone))
                l.text = $"({route.Zone}) {l.text}";
        }

        _listView = _root.Q<ListView>("route-list-view");
        _listView.makeItem = MakeItem;
        _listView.bindItem = BindItem;

        _listView.onItemsChosen += ChosenItemCallback;

        routeManager.AddRoutesChangedCallback(Rebuild);
        routeManager.AddSelectedRouteChangedCallback(Rebuild);
    }

    public void Close()
    {
        _root.visible = false;
    }

    /// <summary>
    ///     A function that gets called when an item from the list view is clicked on.
    /// </summary>
    private void ChosenItemCallback(IEnumerable<object> items)
    {
        foreach (var route in items.Select(item => (Route)item))
        {
            if (routeManager.SelectedRoute == route)
                routeSettingsMenu.Show();

            // this is done to prevent an infinite loop between selecting 
            routeManager.SelectRoute(route, false);

            highlightManager.UnhighlightAll();
            highlightManager.HighlightRoute(route, true);
            editorModeManager.CurrentMode = EditorModeManager.Mode.Route;
        }

        Rebuild();
    }

    /// <summary>
    ///     Sort the given routes to be listed in the list view - by zones.
    /// </summary>
    private List<Route> GetSortedRoutes()
    {
        return routeManager.GetUsableRoutes().OrderBy(route => route.Zone).ToList();
    }

    /// <summary>
    ///     Rebuild the route view from the current state of the route manager.
    /// </summary>
    private void Rebuild()
    {
        _listView.itemsSource = GetSortedRoutes();

        var selectedIndex = _listView.itemsSource.IndexOf(routeManager.SelectedRoute);

        // make sure that the current route is selected
        _listView.SetSelectionWithoutNotify(selectedIndex >= 0
            ? new List<int> { selectedIndex }
            : new List<int>());

        _listView.Rebuild();

        if (selectedIndex >= 0)
            _listView.ScrollToId(selectedIndex);
    }

    public void Show()
    {
        _root.visible = true;
    }
}