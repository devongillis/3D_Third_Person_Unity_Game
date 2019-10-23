using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlatform : MonoBehaviour
{
    public Vector3 translate = new Vector3(0, 0, 0);
    public float speed = 0.0f;
    // Start is called before the first frame update
    // for platforms that move up and down you want it
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(translate * speed);
    }
    
}
