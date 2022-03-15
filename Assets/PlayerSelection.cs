using System;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlayerSelection : MonoBehaviour
{
    public PlayerControl PlayerControl;
    public HoldManager HoldManager;
    public StateManager StateManager;

    private string[] _selected;
    private int _selectedIndex;

    // the current hold object
    private GameObject _selectedObject;
    
    // where the currently selected object pointed previously
    private Vector3 _previousSelectedObjectNormal = Vector3.zero;

    // the angle of the pointer object (from 0 to 2 pi)
    private float _selectedObjectRotation;

	public float mouseSensitivity = 1f;
	
    // the object that was last hovered
    // used for removing the outline when it is no longer hoverd
    private GameObject previouslyHoveredObject;
    
    void Start()
    {
        // TODO: remove this, temporarily selects all of the holds
        _selected = HoldManager.Filter(hold => true);
        
        _selectedObject = HoldManager.ToGameObject(_selected[_selectedIndex]);
        _selectedObject.layer = 2;  // ignore this object until placed
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && PlayerControl.CurrentMode == Mode.Edit)
        {
            _selectedObject.layer = 0;
            StateManager.AddHold(_selectedObject);
            _selectedObject = Instantiate(_selectedObject);
            _selectedObject.layer = 2;
        }

        if (Input.GetKey(KeyCode.LeftShift) && PlayerControl.CurrentMode == Mode.Edit)
        {
            float x = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

            _selectedObjectRotation = (_selectedObjectRotation + x) % (2 * (float)Math.PI);
        }
        
        // cast a ray directly in front of the camera
        var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
    
        if (Physics.Raycast(ray, out hit))
        {
            if (PlayerControl.CurrentMode == Mode.Edit)
            {
                _selectedObject.transform.Rotate(0.1f, 0f, 0f);
                
                // for smooth movement of the object that we currently have selected
                _selectedObject.transform.position = Vector3.Lerp(_selectedObject.transform.position, hit.point,
                    _selectedObject.activeSelf ? 0.55f : 1f);
                
                var currentNormal = Vector3.Lerp(_previousSelectedObjectNormal, hit.normal, 0.3f);
                
                // rotate the world "up" around the hit normal by some degrees
                var upVector = Quaternion.AngleAxis(Mathf.Rad2Deg * _selectedObjectRotation, hit.normal) * Vector3.up;
                
                _selectedObject.transform.LookAt(currentNormal + hit.point, upVector);
                _selectedObject.SetActive(true);

                _previousSelectedObjectNormal = currentNormal;
            }
            else
            {
                var hitObject = hit.collider.gameObject;

                if (StateManager.IsHold(hitObject))
                {
                    if (!hitObject.GetComponent<Outline>())
                    {
                        var outline = hitObject.AddComponent<Outline>();

                        outline.OutlineMode = Outline.Mode.OutlineAll;
                        outline.OutlineColor = Color.white;
                        outline.OutlineWidth = 15f;

                        previouslyHoveredObject = hitObject;
                    }
                }
                else
                {
                    // TODO: duplicite code
                    _selectedObject.SetActive(false);

                    if (previouslyHoveredObject) {
                        Destroy(previouslyHoveredObject.GetComponent<Outline>());
                        
                        previouslyHoveredObject = null;
                    }
                }
            
            }
        }
        else
        {
            // TODO: duplicite code
            _selectedObject.SetActive(false);

            if (previouslyHoveredObject) {
                Destroy(previouslyHoveredObject.GetComponent<Outline>());
                
                previouslyHoveredObject = null;
            }
        }
    }
}