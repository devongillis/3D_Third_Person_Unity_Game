using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class castShadow : MonoBehaviour
{
    // Start is called before the first frame update
    // the max size for the shadow is scale of 7
    public Vector3 scale;
    public float maxRenderDistance = 10.0f;

    void Start()
    {
        scale = transform.localScale;
    }

    void LateUpdate()
    {
        Debug.DrawRay(transform.position + new Vector3(0, 0.1f, 0), -transform.up * 10.0f, Color.red);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, maxRenderDistance))
        {
            //float factor = (maxRenderDistance - (transform.position.y - hit.point.y)) / maxRenderDistance;
            //transform.localScale = scale * factor;
            //Quaternion rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
            //transform.rotation = rotation;
            transform.position = hit.point;
        }
    }
}
