using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    public GameObject pointerObject;
    public Transform pointerObjectTransform;

    private bool wasSeen;

    void Update()
    {
        int layerMask = 1 << 8;

        layerMask = ~layerMask;

        var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            pointerObjectTransform.position = Vector3.Lerp(pointerObjectTransform.position, hit.point, pointerObject.activeSelf ? 0.55f : 1f);
            pointerObjectTransform.LookAt(hit.point + hit.normal * 100);
            pointerObject.SetActive(true);
        }
        else
        {
            pointerObject.SetActive(false);
        }

    }
}