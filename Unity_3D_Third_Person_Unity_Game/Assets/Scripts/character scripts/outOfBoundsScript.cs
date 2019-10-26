using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class outOfBoundsScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerExit(Collider other)
    {
        if(other.transform.tag == "Player")
        {
            // kill player
            Debug.Log("kill Player");
        }
        else if(other.transform.tag != "MainCamera")
        {
            // deactivate this object
            other.gameObject.SetActive(false);
            Debug.Log(other.transform.name + " has been deactivated");
        }
    }
}
