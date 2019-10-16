using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onTriggerEnterForCamera : MonoBehaviour
{
	public GameObject mainCamera;
	cameraController script;

    // Start is called before the first frame update
    void Start()
    {
		script = mainCamera.GetComponent<cameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnTriggerEnter(Collider other){
		if(other.tag == "MainCamera"){
			script.useRegularPitch = false;
		}
	}

	void OnTriggerExit(Collider other){
		if(other.tag == "MainCamera"){
			script.useRegularPitch = true;
		}
	}
}
