using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonCollapse : MonoBehaviour
{
    
	public float timer = 0.0f;
	public float timeLimit = 5.0f;
	public bool timed = true;
	public bool collapsed = false;

    void Start()
    {
		//Collapse();
    }

    // Update is called once per frame
    void Update()
    {
		if(timed && collapsed){
			timer += Time.deltaTime;
			if(timer >= timeLimit){
				transform.localScale = new Vector3(1, 1.0f, 1);
				collapsed = false;
				timer = 0.0f;
			}
		}
    }

	public void Collapse(){
		// this function will collapse the button
		if(!collapsed){
			transform.localScale = new Vector3(1, 0.01f, 1);
			collapsed = true;
		}
	}
	/*
	void OnTriggerEnter(Collider other){
		if(other.tag == "Player" && other.gameObject.GetComponent<characterControllerScript_withCharacterControllerAttribute>().IsGroundPounding()){
			Collapse();
		}
	}
	*/
}
