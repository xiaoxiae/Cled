using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlayerSelection : MonoBehaviour
{
    public PlayerControl PlayerControl;
    public HoldManager HoldManager;
    public StateManager StateManager;
    public CameraControl CameraControl;

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
    private GameObject previouslyHighlightedObject;

    void Start()
    {
        // TODO: remove this, temporarily selects all of the holds
        _selected = HoldManager.Filter(hold => true);

        _selectedObject = HoldManager.ToGameObject(_selected[_selectedIndex]);
        _selectedObject.layer = 2; // ignore this object until placed
    }

    private void PlaceHold()
    {
        _selectedObject.layer = 0; // don't ignore the object any more
        StateManager.AddHold(_selectedObject, _selectedObjectRotation);
        _selectedObject = Instantiate(_selectedObject);
        _selectedObject.layer = 2;
    }

    /// <summary>
    /// Rotate the currently held hold by a certain amount in radians.
    /// </summary>
    private void RotateHold(float delta)
    {
        _selectedObjectRotation = (_selectedObjectRotation + delta) % (2 * (float)Math.PI);
    }

    /// <summary>
    /// Smoothly move the currently held hold to the specified point.
    /// </summary>
    private void MoveHoldToHit(Vector3 point, float smoothness = 0.5f)
    {
        // move to destination smoothly
        _selectedObject.transform.position = Vector3.Lerp(
            _selectedObject.transform.position,
            point,
            _selectedObject.activeSelf ? smoothness : 1f
        );
    }

    /// <summary>
    /// Smoothly update the currently held hold normal to the specified vector and rotation about it.
    /// </summary>
    private void UpdateHoldNormal(Vector3 normal, float smoothness = 0.5f)
    {
        // calculate the normal smoothly
        var currentNormal = Vector3.Lerp(_previousSelectedObjectNormal, normal, smoothness);

        // rotate the world "up" around the hit normal by some degrees
        var upVector = Quaternion.AngleAxis(Mathf.Rad2Deg * _selectedObjectRotation, normal) * Vector3.up;

        _selectedObject.transform.LookAt(currentNormal + _selectedObject.transform.position, upVector);

        _previousSelectedObjectNormal = currentNormal;
    }

    /// <summary>
    /// Add the outline to a hold and note down that it's highlighted, if it doesn't have it.
    /// </summary>
    private void TryAddOutline(GameObject hold)
    {
        if (hold.GetComponent<Outline>()) return;

        var outline = hold.AddComponent<Outline>();

        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = 15f;

        previouslyHighlightedObject = hold;
    }


    /// <summary>
    /// Remove the hold outline (if it is a hold and has one).
    /// </summary>
    private void TryRemoveOutline(GameObject hold)
    {
        if (hold != null)
        {
            var outline = hold.GetComponent<Outline>();

            if (!outline) return;

            Destroy(outline);
            previouslyHighlightedObject = null;
        }
    }

    void Update()
    {
        // cast a ray directly in front of the camera
        var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        
        // used to prevent swapping back and forth in one update
        // is a bit dirty but works
        bool swappedMode = false;

        if (Physics.Raycast(ray, out hit))
        {
            var hitObject = hit.collider.gameObject;

            // if we hit an object that isn't highlighted or we're in edit mode, remove outlines
            if (previouslyHighlightedObject != hitObject || PlayerControl.CurrentMode == Mode.Edit)
                TryRemoveOutline(previouslyHighlightedObject);

            switch (PlayerControl.CurrentMode)
            {
                case Mode.Edit:
                    // when in edit mode, move the selected hold accordingly
                    MoveHoldToHit(hit.point);
                    UpdateHoldNormal(hit.normal);
                    _selectedObject.SetActive(true);
                    
                    break;
                case Mode.Normal:
                {
                    if (StateManager.IsHold(hitObject))
                    {
                        // attempt to outline hold if in normal mode
                        TryAddOutline(hitObject);

                        // when left clicking a hold in normal mode, snap back to edit mode
                        if (Input.GetMouseButtonDown(0))
                        {
                            PlayerControl.CurrentMode = Mode.Edit;

                            HoldState state = StateManager.GetHoldState(hitObject);
                            
                            if (_selectedObject)
                                Destroy(_selectedObject);

                            StateManager.RemoveHold(hitObject);
                            _selectedObject = hitObject;
                            _selectedObject.layer = 2;

                            CameraControl.transform.LookAt(_selectedObject.transform.position);
                            _selectedObjectRotation = state.PlacedAngle;
                            
                            UpdateHoldNormal(_selectedObject.transform.forward, 1);

                            swappedMode = true;
                        }
                    }

                    break;
                }
            }
        }
        
        // even when the ray didn't hit anything, do some stuff
        if (PlayerControl.CurrentMode == Mode.Edit)
        {
            if (Input.GetMouseButtonDown(0) && !swappedMode)
            {
                PlaceHold();
                PlayerControl.CurrentMode = Mode.Normal;
            }

            if (Input.GetKey(KeyCode.LeftShift))
                RotateHold(Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime);
        }
        else if (PlayerControl.CurrentMode == Mode.Normal)
        {
            _selectedObject.SetActive(false);
        }
    }
}