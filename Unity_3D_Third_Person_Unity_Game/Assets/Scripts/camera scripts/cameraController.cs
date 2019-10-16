using UnityEngine;
using System.Collections;

public class cameraController : MonoBehaviour {

	public bool lockCursor;
	public bool invertPitch = false;
	public float yawSensitivity = 5;
	public float pitchSensitivity = 5;
	public Transform target;
	public float dstFromTarget = 2;

	public bool useRegularPitch = true;

	public Vector2 pitchMinMax = new Vector2 (-40, 85);
	public Vector2 mazePitchMinMax = new Vector2(50, 85);


	public Vector3 offset = new Vector3(0, 0, 0);

	public float rotationSmoothTime = .12f;
	Vector3 rotationSmoothVelocity;
	Vector3 currentRotation;

	float yaw;
	float pitch;



	void Start() {
		if (lockCursor) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	void LateUpdate () {

		// xbox 360 controller input
		//XboxControllerInput();

		// keyboard input
		KeyBoardInput();



		if(useRegularPitch){
			pitch = Mathf.Clamp (pitch, pitchMinMax.x, pitchMinMax.y);
		}
		else{
			pitch = Mathf.Clamp (pitch, mazePitchMinMax.x, mazePitchMinMax.y);
		}

		currentRotation = Vector3.SmoothDamp (currentRotation, new Vector3 (pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
		transform.eulerAngles = currentRotation;
		transform.position = target.position + offset - transform.forward * dstFromTarget;
	}

	void KeyBoardInput(){
		yaw += Input.GetAxis ("Mouse X") * yawSensitivity;
		if(invertPitch){
			pitch += Input.GetAxis ("Mouse Y") * pitchSensitivity;
		}
		else{
			pitch -= Input.GetAxis ("Mouse Y") * pitchSensitivity;
		}
	}

	void XboxControllerInput(){
		yaw += Input.GetAxis ("camera x") * yawSensitivity;
		if(invertPitch){
			pitch += Input.GetAxis ("camera y") * pitchSensitivity;
		}
		else{
			pitch -= Input.GetAxis ("camera y") * pitchSensitivity;
		}
	}

}