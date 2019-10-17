using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlatform : MonoBehaviour
{
	public float worldRotationSpeed = 5.0f;
	public Vector3 world = new Vector3(0, 0, 0);

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void FixedUpdate()
	{
		transform.Rotate(world * worldRotationSpeed, Space.World);
	}

	public Vector3 TranslateCharacter(Vector3 characterPosition){
		Vector3 displacement = characterPosition - transform.position;
		displacement = Quaternion.Euler(world * worldRotationSpeed) * displacement;
		Vector3 translate = displacement + transform.position - characterPosition;
		return translate;
	}
}
