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

	public float rotationSmoothTime = 0.12f;
    public float translationSmoothTime = 0.12f;
    public float translationSmoothValue;
    public float translationSmoothDecrement = 0.01f;
	Vector3 rotationSmoothVelocity;
    Vector3 translationSmoothVelocity;
	Vector3 currentRotation;

	float yaw;
	float pitch;

    public int smoothCounter;

    public GameObject masterObject;
    public KeyBindScript keyBindScript;

	void Start() {
        keyBindScript = masterObject.GetComponent<KeyBindScript>();
		if (lockCursor) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

    public void useSmooth(int time)
    {
        //Debug.Log("hello");
        // time is how many frames we have to transition from our current position to the desired position
        // since smooth time is a value in seconds for how long the camera takes we can
        smoothCounter = time;
        translationSmoothValue = translationSmoothTime;
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
		//transform.position = target.position + offset - transform.forward * dstFromTarget;
        if(smoothCounter > 0)
        {
            transform.position = Vector3.SmoothDamp(transform.position, target.position + offset - transform.forward * dstFromTarget, ref translationSmoothVelocity, translationSmoothValue);
            smoothCounter--;
            translationSmoothValue -= translationSmoothDecrement;
        }
        else
        {
            transform.position = target.position + offset - transform.forward * dstFromTarget;
        }
    }

	void KeyBoardInput(){
		//yaw += Input.GetAxis ("Mouse X") * yawSensitivity;
        yaw += keyBindScript.CVHinput.x * yawSensitivity;
		if(invertPitch){
			//pitch += Input.GetAxis ("Mouse Y") * pitchSensitivity;
            pitch += keyBindScript.CVHinput.y * pitchSensitivity;
		}
		else{
			//pitch -= Input.GetAxis ("Mouse Y") * pitchSensitivity;
            pitch -= keyBindScript.CVHinput.y * pitchSensitivity;
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