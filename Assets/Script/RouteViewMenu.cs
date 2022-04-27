using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class RouteViewMenu : MonoBehaviour
{
    public RouteManager routeManager;
    public RouteSettingsMenu routeSettingsMenu;

    private ListView _listView;
    private VisualElement _root;

    // Start is called before the first frame update
    void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        Utilities.DisableElementFocusable(_root);
        
        // close by default
        Close();

        VisualElement MakeItem() => new Label();

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
        
        _listView.onItemsChosen += items =>
        {
            foreach (var route in items.Select(item => (Route)item))
            {
                if (routeManager.SelectedRoute == route)
                    routeSettingsMenu.Show();
                
                foreach (var callback in _selectedRouteCallbacks)
                    callback(route);
            }
        };

        _listView.onSelectionChange += items =>
        {
            foreach (var route in items.Select(item => (Route)item))
            {
                if (routeManager.SelectedRoute == route)
                    routeSettingsMenu.Show();
                
                foreach (var callback in _selectedRouteCallbacks)
                    callback(route);
            }
        };

        routeManager.AddRoutesChangedCallback(Rebuild);
        routeManager.AddSelectedRouteChangedCallback(Rebuild);
    }

    List<Route> GetSortedRoutes() => routeManager.GetUsableRoutes().OrderBy(route => route.Zone).ToList();

    void Rebuild()
    {
        _listView.itemsSource = GetSortedRoutes();
        
        // make sure that the current route is selected
        if (routeManager.SelectedRoute != null)
            _listView.SetSelectionWithoutNotify(new List<int> {_listView.itemsSource.IndexOf(routeManager.SelectedRoute)});
        else
            _listView.SetSelectionWithoutNotify(new List<int>());
        
        _listView.Rebuild();
    }

    private readonly List<Action<Route>> _selectedRouteCallbacks = new();

    /// <summary>
    /// Add a callback that gets executed when a route is clicked on.
    /// </summary>
    public void AddSelectedRouteCallback(Action<Route> route) => _selectedRouteCallbacks.Add(route);

    public void Show() => _root.visible = true;

    public void Close() => _root.visible = false;
}