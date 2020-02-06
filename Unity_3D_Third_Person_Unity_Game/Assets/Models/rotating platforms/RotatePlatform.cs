using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlatform : MonoBehaviour
{
	public float rotationSpeed = 1.0f;
	public Vector3 axis = new Vector3(0, 0, 0);
    public bool local = true;
    public bool childrenCopyRotation = true;

    // later check if the platform can rotate in world coords with player still on it (bowser flips platform in second battle)

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void FixedUpdate()
	{
        if (local)
        {
            transform.Rotate(axis * rotationSpeed, Space.Self);
            if (!childrenCopyRotation)
            {
                for(int i = 0; i < transform.childCount; i++){
                    transform.GetChild(i).transform.Rotate(-axis * rotationSpeed, Space.Self);
                }
            }
        }
        else
        {
            transform.Rotate(axis * rotationSpeed, Space.World);
            if (!childrenCopyRotation)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).transform.Rotate(-axis * rotationSpeed, Space.World);
                }
            }
        }
	}

}
