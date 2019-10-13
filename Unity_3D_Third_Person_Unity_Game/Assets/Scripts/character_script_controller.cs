using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character_script_controller : MonoBehaviour
{
	Transform cameraT;

	Rigidbody rb;

	private Animator anim;
	private float speed = 0.0f;
	public float interval = 0.001f;

	protected CharacterState characterState = CharacterState.STATIONARY;

	public float walkSpeed = 2;
	public float runSpeed = 6;

	public float magnitudeJumpAcceptance = 1.0f;
	public float groundDistanceCheck = 0.1f;

	public float turnSmoothTime = 0.2f;
	float turnSmoothVelocity;

	public float speedSmoothTime = 0.1f;
	float speedSmoothVelocity;
	float currentSpeed;

	Vector3 PrevPos;
	Vector3 NewPos;
	Vector3 ObjVelocity;

	public float distToGround;

	public float initialJumpVelocity = 1.0f;
	public float initialHighJumpVelocity = 1.0f;
	public Vector2 initialLongJumpVelocity = new Vector2(1.0f, 1.0f); // length and height


	// input variables
	Vector2 input;
	float jump;
	float specialJump;

	// secondary input variables
	public bool running;

    // Start is called before the first frame update
    void Start(){
		PrevPos = transform.position;
		NewPos = transform.position;

		cameraT = Camera.main.transform;
		rb = GetComponent<Rigidbody>();
		distToGround = GetComponent<Collider>().bounds.extents.y;

		anim = gameObject.GetComponentInChildren<Animator>();
		anim.SetFloat("speed", speed);
    }

    // Update is called once per frame
    void Update(){

		NewPos = transform.position;
		ObjVelocity = (NewPos - PrevPos)/Time.deltaTime;
		PrevPos = NewPos;

		// first we check for "no input status"
		// then collect the inputs from the player
		// enter the function call from current character state
		// and then change the state respsectively, followed by
		// acting on the input values
		if(characterState == CharacterState.NO_INPUT){
			// act like no inputs being made
			input = new Vector2(0, 0);
			jump = 0;
			specialJump = 0;
		}
		else{
			// inputs are allowed
			input = new Vector2 (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			input = input.normalized;
			jump = Input.GetAxisRaw("Jump");
			specialJump = Input.GetAxisRaw("Special Jump");
		}
		// inputs are collected
		if(characterState == CharacterState.STATIONARY){
			StationaryState();
		}
		else if(characterState == CharacterState.RUNNING){
			RunningState();
		}
		else if(characterState == CharacterState.JUMPING_LOW){
			RegularJump();
		}
		else if(characterState == CharacterState.JUMPING_LONG){
			LongJump();
		}
		else if(characterState == CharacterState.JUMPING_HIGH){
			HighJump();
		}
		else if(characterState == CharacterState.FALLING){
			
		}
		else if(characterState == CharacterState.FALLING_NO_CONTROL){
			
		}

    }

	void StationaryState(){
		// this function is called if player originally is in the stationary state
		// from the stationary state our character can enter a running or jumping state

		if (input != Vector2.zero) {
			Debug.Log("input");
			// must exit the stationary state
			characterState = CharacterState.RUNNING;
			// notice we can swap into and then out of running state if horizontal and jump
			// input, this means the character will jump rather than start running from
			// stationary pose, which means the character can't do a long jump from stationary
		}

		// now check if a jump has been called
		if(jump > 0 && IsGrounded()){
			// later come up with a keyboard click tester (don't let holding down button work as jump input)
			// now determine what kind of jump
			if(specialJump > 0){
				// since we are in the stationary state we will do a high jump
				characterState = CharacterState.JUMPING_HIGH;
			}
			else{
				characterState = CharacterState.JUMPING_LOW;
			}
		}
		// if no input was registered we remain in the stationary state, and animation will reflect that
	}

	void RunningState(){
		// running covers any horizontal movement, so check speed to do a long jump
		if (input != Vector2.zero) {
			float targetRotation = Mathf.Atan2 (input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
			transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
		}

		float targetSpeed = ((running) ? runSpeed : walkSpeed) * input.magnitude;
		currentSpeed = Mathf.SmoothDamp (currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

		transform.Translate (transform.forward * currentSpeed * Time.deltaTime, Space.World);
		/*
		if(ObjVelocity.magnitude > magnitudeJumpAcceptance){
			characterState = CharacterState.JUMPING_LONG;
		}
		else{
			// no long jump just regular
		}
		*/
	}

	void LongJump(){
		Vector3 jump = new Vector3(transform.forward.x * initialLongJumpVelocity.x, initialLongJumpVelocity.y, transform.forward.z * initialLongJumpVelocity.x);
		rb.AddForce(jump, ForceMode.Impulse);
	}

	void HighJump(){
		rb.AddForce(new Vector3(0, initialHighJumpVelocity, 0), ForceMode.Impulse);
	}

	void RegularJump(){
		rb.AddForce(new Vector3(0, initialJumpVelocity, 0), ForceMode.Impulse);
	}

	bool IsGrounded(){
		// test if the character controller is grounded function is sufficent
		return Physics.Raycast(transform.position, -Vector3.up, groundDistanceCheck);

	}



	void extra(){
		speed += interval;
		anim.SetFloat("speed", speed);
	}

	protected enum CharacterState{
		// if a state switch is detected we finish up the orignal function and swap the state,
		// and on the NEXT FRAME the new state function will be called (avoid infinite loops)

		// all states can transition to falling
		// all jumping states transition to falling
		STATIONARY, // can transition to running, jumping_low, jumping_high
		RUNNING, // can transition to stationary, jumping_low, jumping_long
		JUMPING_LOW,
		JUMPING_LONG,
		JUMPING_HIGH,
		FALLING, // upon ground detection will transition to stationary
		FALLING_NO_CONTROL, // when fall starts without a jump (i.e. bumped off edge)
		NO_INPUT // used when doing cutscene or other stuff when character shouldn't move,
		// a function is called to set this value and must clean up a few values before forcing a no input senario

	}

	public void SwitchToNoInput(){
		// this function is called when we need to deactivate inputs,
		// must clean up a few things before setting the state to no inputs
	}


	/*
	Player movement needs to be broken down into states such that one state can transition
	to another through change in terrain/input. So far states are stationary, running, jumping,
	falling. Stationary is described as no movement input and can transition into any other
	state. Running is allowed when a floor is detected and can transition into the other states.
	Jumping is broken down into what the previous state was, and is subdivided into 3 smaller
	states. Long jump, regular jump, and high jump. Falling is done either after the initial
	jumping phase or when a floor is not detected. On each update call the game must first test
	whether the character is jumping before checking the slope of the floor. This way in impulse
	to snap the player to the floor is only used if the player is not jumping. Otherwise if the
	player is jumping then the slope is not checked and an impulse upwards is used instead.
	During the jumping phase (which is tested each frame by the velocity of the rigid body) a
	small impulse is added to effectively reduce the effect of gravity during the initial ascent
	of the jump, gravity is restored to regular value when over the initial jump (and capped off
	with another value). Another type of impulse is used during the course of a long jump. A 
	long jump will begin using regular gravity when over the initial height jump of the long
	jump duration.
	 */
}
