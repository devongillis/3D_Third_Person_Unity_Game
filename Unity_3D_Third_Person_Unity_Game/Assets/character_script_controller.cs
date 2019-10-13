using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character_script_controller : MonoBehaviour
{

	private Animator anim;
	private float speed = 0.0f;
	public float interval = 0.001f;

    // Start is called before the first frame update
    void Start()
    {
		anim = gameObject.GetComponentInChildren<Animator>();
		anim.SetFloat("speed", speed);
    }

    // Update is called once per frame
    void Update()
    {
		//anim.speed += 0.01f;
		speed += interval;
		anim.SetFloat("speed", speed);
    }
}
