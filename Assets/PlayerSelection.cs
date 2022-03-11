using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PlayerSelection : MonoBehaviour
{
    public PlayerControl PlayerControl;
    public HoldManager HoldManager;
    public StateManager StateManager;

    private string[] _selected;
    private int _selectedIndex;

    private GameObject pointerObject = null;
    private Vector3 previousLook = Vector3.zero;

    private bool wasSeen; 
    void Start()
    {
        // TODO: remove this, temporarily selects all of the holds
        _selected = HoldManager.Filter(hold => true);
        
        pointerObject = HoldManager.ToGameObject(_selected[_selectedIndex]);
        pointerObject.layer = 2;  // ignore this object until placed
    }

    void Update()
    {
        // TODO: only do this when in edit mode
        if (Input.GetMouseButtonDown(0))
        {
            pointerObject.layer = 0;
            StateManager.AddHold(pointerObject);
            pointerObject = Instantiate(pointerObject);
            pointerObject.layer = 2;
        }
        
        // in insert mode, do stuff with placing the hold
        if (PlayerControl.CurrentMode == Mode.Edit)
        {
            // cast a ray directly in front of the camera
            var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
        
            if (Physics.Raycast(ray, out hit))
            {
                pointerObject.transform.Rotate(0.1f, 0f, 0f);
                
                // for smooth movement of the object that we currently have selected
                pointerObject.transform.position = Vector3.Lerp(pointerObject.transform.position, hit.point,
                    pointerObject.activeSelf ? 0.55f : 1f);
                
                // TODO: store the rotation internally
                // rotate the vector around the axis of the hit normal
                
                // https://docs.unity3d.com/ScriptReference/Transform.LookAt.html
                var currentLook = Vector3.Lerp(previousLook, hit.point + hit.normal * 100, 0.3f);
                
                pointerObject.transform.LookAt(currentLook, Vector3.up);
                pointerObject.SetActive(true);

                previousLook = currentLook;
            }
            else
            {
                pointerObject.SetActive(false);
            }
        }
        
        // in normal mode, highlight selected hold
        else
        {
            pointerObject.SetActive(false);
        }
    }
}