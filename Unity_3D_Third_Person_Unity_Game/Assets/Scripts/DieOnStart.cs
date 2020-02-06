using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieOnStart : MonoBehaviour
{
    // this script is for deleting parent objects upon starting a scene
    // its to allow better hierarchy sorting during scene edtiting
    // Start is called before the first frame update
    void Start()
    {
        int children = transform.childCount;
        while(transform.childCount > 0) {
            transform.GetChild(0).SetParent(transform.parent);
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
