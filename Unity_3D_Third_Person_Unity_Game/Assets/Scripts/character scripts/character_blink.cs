using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character_blink : MonoBehaviour
{
	private Animator anim;
	public int upper = 10;
	// Start is called before the first frame update
	void Start()
	{
		anim = gameObject.GetComponentInChildren<Animator>();
	}

	// Update is called once per frame
	void Update()
	{
		//int digit = Random.Range(0, 10);
		//Debug.Log(digit);
		if(Random.Range(0, upper) == 0){
			anim.SetTrigger("blink");
		}
	}
}
