using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class energyRingScript : MonoBehaviour
{

	public float rotationSpeed = 5.0f;
	public Vector3 axis = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
		//axis.Normalize();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // on each update we want to adjust the ring rotation by a defined axis
		transform.Rotate(axis * rotationSpeed, Space.Self);
		//transform.Rotate(new Vector3(0, 0.5f, 0) * rotationSpeed, Space.World);

	}
}
