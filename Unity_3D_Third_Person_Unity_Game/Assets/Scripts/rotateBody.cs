using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateBody : MonoBehaviour
{
	public float worldRotationSpeed = 5.0f;
	public float localRotationSpeed = 5.0f;
	//public Vector3 axis = new Vector3(0, 0, 0);
	public Vector3 world = new Vector3(0, 0, 0);
	public Vector3 local = new Vector3(0, 0, 0);

	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		// on each update we want to adjust the ring rotation by a defined axis
		transform.Rotate(world * worldRotationSpeed, Space.World);
		transform.Rotate(local * localRotationSpeed, Space.Self);
	}
}
