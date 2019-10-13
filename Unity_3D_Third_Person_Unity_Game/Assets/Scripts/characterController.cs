using UnityEngine;
using System.Collections;

public class characterController : MonoBehaviour {

	public float walkSpeed = 2;
	public float runSpeed = 6;

	public float turnSmoothTime = 0.2f;
	float turnSmoothVelocity;

	public float speedSmoothTime = 0.1f;
	float speedSmoothVelocity;
	float currentSpeed;


	Rigidbody rb;

	// jump physics
	public float initialJumpVelocity = 1.0f;
	public float initialHighJumpVelocity = 1.0f;
	public Vector2 initialLongJumpVelocity = new Vector2(1.0f, 1.0f); // length and height
	public float distToGround;
	public int frameTimer;
	public int frameCount = 10;

	public bool controlEnabled = true;

	public float magnitudeAcceptance = 1.0f;

	public float groundDistanceCheck = 0.1f;


	Vector3 PrevPos;
	Vector3 NewPos;
	Vector3 ObjVelocity;



	// slope snap values
	public float slopeForce = 1.0f;
	public float slopeForceRayLength = 1.0f;














	//Animator animator;
	Transform cameraT;

	void Start () {

		PrevPos = transform.position;
		NewPos = transform.position;


		//animator = GetComponent<Animator> ();
		cameraT = Camera.main.transform;
		rb = GetComponent<Rigidbody>();
		distToGround = GetComponent<Collider>().bounds.extents.y;
	}

	void Update () {

		NewPos = transform.position;
		ObjVelocity = (NewPos - PrevPos)/Time.deltaTime;
		PrevPos = NewPos;



		//Debug.Log(rb.velocity);
		if(frameTimer > 0){
			frameTimer--;
		}
		Vector2 input;
		Vector2 inputDir;
		float jump;
		float specialJump;
		if(controlEnabled){
			input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
			inputDir = input.normalized;
			jump = Input.GetAxisRaw("Jump");
			specialJump = Input.GetAxisRaw("Left Trigger");
		}
		else{
			input = new Vector2(0, 0);
			inputDir = new Vector2(0, 0);
			jump = 0;
			specialJump = 0;
		}
		if(jump > 0 && frameTimer <= 0){// first check if jump input is even allowed, this is because IsGrounded() is an expensive function
			if(IsGrounded()){
				// we have confirmed a jump has been called and allowed, now determine what kind of jump
				// we also need to disable the slope snap test for a few frames to allow clearance during initial jump
				if(specialJump > 0){
					if(ObjVelocity.magnitude > magnitudeAcceptance){
						LongJump();
					}
					else{
						Debug.Log(ObjVelocity.magnitude);
						HighJump();
					}
					//LongJump();
				}
				else{
					RegularJump();
				}
				//rb.AddForce(new Vector3(0, initialJumpVelocity, 0), ForceMode.Impulse);
			}
			frameTimer = frameCount;
		}

		if (inputDir != Vector2.zero) {
			float targetRotation = Mathf.Atan2 (inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
			transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
		}

		bool running = Input.GetKey (KeyCode.LeftShift);
		float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
		currentSpeed = Mathf.SmoothDamp (currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

		transform.Translate (transform.forward * currentSpeed * Time.deltaTime, Space.World);

		//float animationSpeedPercent = ((running) ? 1 : .5f) * inputDir.magnitude;
		//animator.SetFloat ("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
		/*
		if(currentSpeed != 0 && frameTimer == 0 && OnSlope()){
			// moving, not jumping and on slope
			transform.Translate(Vector3.down * groundDistanceCheck * slopeForceRayLength, Space.World);
			//rb.AddForce(new Vector3(0, initialJumpVelocity, 0), ForceMode.Impulse);
		}
*/

	}

	private bool OnSlope(){
		RaycastHit hit;
		if(Physics.Raycast(transform.position, Vector3.down, out hit, groundDistanceCheck * slopeForceRayLength)){
			if(hit.normal != Vector3.up){
				Debug.Log(transform.position - hit.point);
				return true;
			}
		}
		return false;	
	}
		

	bool IsGrounded(){
		//Debug.DrawRay(transform.position, -Vector3.up, Color.yellow);
		//transform.Translate(0, -groundDistanceCheck, 0);
		return Physics.Raycast(transform.position + new Vector3(0, 0, 0), -Vector3.up, groundDistanceCheck);

	}

	void RegularJump(){
		rb.AddForce(new Vector3(0, initialJumpVelocity, 0), ForceMode.Impulse);
	}

	void HighJump(){
		rb.AddForce(new Vector3(0, initialHighJumpVelocity, 0), ForceMode.Impulse);
	}

	void LongJump(){
		Vector3 jump = new Vector3(transform.forward.x * initialLongJumpVelocity.x, initialLongJumpVelocity.y, transform.forward.z * initialLongJumpVelocity.x);
		//jump.x 
		rb.AddForce(jump, ForceMode.Impulse);
	}
}