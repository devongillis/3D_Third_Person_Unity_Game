using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stationaryObjectFindGround : MonoBehaviour
{
    public Vector3 offset;
    // Start is called before the first frame update
    // its assumed that the object is placed within 1 unit of the ground
    // the raycast starts from 1 unit above and goes down to 1 unit below
    void Start()
    {
        RaycastHit truncate;
        if (Physics.Raycast(transform.position + new Vector3(0, 1.0f, 0) + offset, -transform.up, out truncate, 2.0f))
        {
            transform.position = truncate.point - offset;
        }
        else
        {
            Debug.Log("error object: " + gameObject.name + " did not find the ground");
        }
    }
}
