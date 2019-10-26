using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class poleOnCollisionEnter : MonoBehaviour
{
    public float poleThickness = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<UpdatedCharacterControllerScript>().PoleDetected(transform.position, poleThickness);
        }
    }
}
