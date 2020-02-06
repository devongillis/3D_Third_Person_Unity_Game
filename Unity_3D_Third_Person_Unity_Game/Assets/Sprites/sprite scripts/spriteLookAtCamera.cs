using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spriteLookAtCamera : MonoBehaviour
{
    public GameObject theCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.LookAt(theCamera.transform, Vector3.up);
        transform.forward = new Vector3(theCamera.transform.position.x - transform.position.x, 0, theCamera.transform.position.z - transform.position.z);
    }
}
