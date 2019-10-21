using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterControllerScript_withCharacterControllerAttribute : MonoBehaviour
{
	Transform cameraT;

	private Animator anim;
	//private float speed = 0.0f;
	public float animMoveSpeed = 0.0f;
	public float animDeltaSpeed = 0.01f;
	public float animIdleSpeed = 0.0f;
	public float animWalkSpeed = 0.6f;
	public float animRunSpeed = 1.0f;

	public CharacterState characterState = CharacterState.STATIONARY;

	public float walkSpeed = 2;
	public float runSpeed = 6;

	public float gravity = -9.81f;
	public float maxFallVelocity = -10.0f;

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
	public float jumpDirectionalMomentumDecay = 0.99f;

	// input variables
	Vector2 input;
	float jump;
	float specialJump;
	float groundPound;

	// secondary input variables
	public bool running;

	private CharacterController controller;



	// this is the vector by which the player moves, all inputs and movements are added to this vector and then
	// this vector is applied to the player each update, so to jump you apply a single up value and on each frame
	// that value will be used to move the player up, and gravity will reduce it
	// when grounded the y component will be set to zero
	public Vector3 universalMovementVector = new Vector3(0, 0, 0);
	public Vector3 universalMovementVectorDelta = new Vector3(0, 0, 0);


	//bool forcedFall = false;
	public float moveBackGradient = 1.0f;
	Vector3 moveBack;

	public float slopeSlipSpeed = 0.1f;


	public bool InputAccepted = true;


	public Vector3 extraRotationTranslationVector = Vector3.zero;





	public float groundPoundTimer = 0.0f;
	public float groundPoundTimeLimit = 10.0f;


	public float forwardDistanceCheck = 0.1f;
	//public float moveBackValue = 0.1f;
	public Vector3 fallingMoveBack = new Vector3(0, 0, 0);

	// Start is called before the first frame update
	void Start(){
		controller = GetComponent<CharacterController>();

		PrevPos = transform.position;
		NewPos = transform.position;

		cameraT = Camera.main.transform;
		distToGround = GetComponent<Collider>().bounds.extents.y;

		anim = gameObject.GetComponentInChildren<Animator>();
		anim.SetFloat("speed", animIdleSpeed);
	}

	// Update is called once per frame
	void FixedUpdate(){
		//Debug.Log(characterState);
		NewPos = transform.position;
		ObjVelocity = (NewPos - PrevPos)/Time.deltaTime;
		PrevPos = NewPos;

		// first we check for "no input status"
		// then collect the inputs from the player
		// enter the function call from current character state
		// and then change the state respsectively, followed by
		// acting on the input values
		if(!InputAccepted){
			// act like no inputs being made
			input = new Vector2(0, 0);
			jump = 0;
			specialJump = 0;
			groundPound = 0;
		}
		else{
			// inputs are allowed
			input = new Vector2 (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			input = input.normalized;
			jump = Input.GetAxisRaw("Jump");
			specialJump = Input.GetAxisRaw("Special Jump");
			groundPound = Input.GetAxisRaw("Ground Pound");
		}
		// inputs are collected
		if(characterState == CharacterState.STATIONARY){
			StationaryState();
		}
		else if(characterState == CharacterState.RUNNING){
			RunningState();
		}
		else if(characterState == CharacterState.JUMPING_LOW){
			LowJumpingState();
		}
		else if(characterState == CharacterState.JUMPING_LONG){
			LongJumpingState();
		}
		else if(characterState == CharacterState.JUMPING_HIGH){
			HighJumpingState();
		}
		else if(characterState == CharacterState.FALLING){
			FallingState();
		}
		else if(characterState == CharacterState.FALLING_NO_CONTROL){

		}
		else if(characterState == CharacterState.SLIPPING_NO_CONTROL){
			SlippingState();
		}
		else if(characterState == CharacterState.GROUND_POUND){
			GroundPoundState();
		}

	}

	void StationaryState(){
		
		if(!IsGrounded()){
			// we are in the stationary state but the floor is not present, (got pushed off the edge?)
			// we swap the state and skip the input
			characterState = CharacterState.FALLING;
		}
		else{
			if (input != Vector2.zero) {
				// we have input for running
				// must exit the stationary state
				characterState = CharacterState.RUNNING;
				// notice we can swap into and then out of running state if horizontal and jump
				// input, this means the character will jump rather than start running from
				// stationary pose, which means the character can't do a long jump from stationary
			}

			CalculateExtraRotationTranslationVector();
			if(extraRotationTranslationVector != Vector3.zero){
				universalMovementVectorDelta = extraRotationTranslationVector;
				controller.Move(universalMovementVectorDelta);
			}
			// now check if a jump has been called
			// notice how we go to a jump state rather than running state if both running and jumping
			if(jump > 0 && IsGrounded()){
				// later come up with a keyboard click tester (don't let holding down button work as jump input)
				// now determine what kind of jump
				if(specialJump > 0){
					// since we are in the stationary state we will do a high jump
					HighJump();
				}
				else{
					RegularJump();
				}
			}
			// if no input was registered we remain in the stationary state, and animation will reflect that

			if(animMoveSpeed <= animIdleSpeed){
				animMoveSpeed = animIdleSpeed;
			}
			else{
				animMoveSpeed -= animDeltaSpeed;
			}

			anim.SetFloat("speed", animMoveSpeed);
		}
	}

	void RunningState(){
		
		// running covers any horizontal movement, so check speed to do a long jump
		if(!IsGrounded()){
			// we are in the running state but no ground is present, we swap the state
			// and skip the input
			characterState = CharacterState.FALLING;
		}
		else{

			if (input != Vector2.zero) {
				// all this is to rotate the character in the desired running direction
				float targetRotation = Mathf.Atan2 (input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
				transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
			}
			else{
				// no input so we move back to stationary
				characterState = CharacterState.STATIONARY;
			}

			// determine the speed at which to move the player
			float targetSpeed = ((running || Input.GetKey(KeyCode.E)) ? runSpeed : walkSpeed) * input.magnitude;
			currentSpeed = Mathf.SmoothDamp (currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

			// now apply gravity, gravity is m/s^2 so multiply by Time.deltaTime twice (only during a jump or fall)
			universalMovementVector = transform.forward * currentSpeed;
			CalculateExtraRotationTranslationVector();

			universalMovementVectorDelta = universalMovementVector * Time.deltaTime + extraRotationTranslationVector;
			controller.Move(universalMovementVectorDelta);

			if(jump > 0){
				// later come up with a keyboard click tester (don't let holding down button work as jump input)
				// now determine what kind of jump
				if(specialJump > 0){
					if(running || Input.GetKey(KeyCode.E)){
						// since we are in the running state we will do a high jump
						LongJump();
					}
					else{
						HighJump();
					}
				}
				else{
					RegularJump();
				}
			}

			if(running || Input.GetKey(KeyCode.E)){
				if(animMoveSpeed < animRunSpeed){
					animMoveSpeed += animDeltaSpeed * 5;
				}
				else{
					animMoveSpeed -= animDeltaSpeed * 5;
				}
				anim.SetFloat("speed", animMoveSpeed);
			}
			else{
				if(animMoveSpeed < animWalkSpeed){
					animMoveSpeed += animDeltaSpeed;
				}
				else{
					animMoveSpeed -= animDeltaSpeed;
				}
				anim.SetFloat("speed", animWalkSpeed);
			}
		}
	}

	void LongJumpingState(){
		
		float targetRotation = Mathf.Atan2 (input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
		transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);

		universalMovementVector.x = ObjVelocity.x;
		universalMovementVector.z = ObjVelocity.z;
		universalMovementVector.y += gravity * Time.deltaTime;
		universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

		universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		controller.Move(universalMovementVectorDelta);

		if(universalMovementVector.y <= 0.0f){
			characterState = CharacterState.FALLING;
		}

		if(groundPound > 0){
			characterState = CharacterState.GROUND_POUND;
		}
	}

	void HighJumpingState(){
		
		float targetRotation = Mathf.Atan2 (input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
		transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);

		float targetSpeed = ((running || Input.GetKey(KeyCode.E)) ? runSpeed : walkSpeed) * input.magnitude;
		currentSpeed = Mathf.SmoothDamp (currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

		universalMovementVector = new Vector3(transform.forward.x * currentSpeed, universalMovementVector.y, transform.forward.z * currentSpeed);
		universalMovementVector.y += gravity * Time.deltaTime;
		universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

		universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		controller.Move(universalMovementVectorDelta);

		if(universalMovementVector.y <= 0.0f){
			characterState = CharacterState.FALLING;
		}

		if(groundPound > 0){
			characterState = CharacterState.GROUND_POUND;
		}
	}

	void LowJumpingState(){
		
		float targetRotation = Mathf.Atan2 (input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
		transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);

		float targetSpeed = ((running || Input.GetKey(KeyCode.E)) ? runSpeed : walkSpeed) * input.magnitude;
		currentSpeed = Mathf.SmoothDamp (currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

		universalMovementVector = new Vector3(transform.forward.x * currentSpeed, universalMovementVector.y, transform.forward.z * currentSpeed);

		universalMovementVector.y += gravity * Time.deltaTime;
		universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

		universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		controller.Move(universalMovementVectorDelta);

		if(universalMovementVector.y <= 0.0f){
			characterState = CharacterState.FALLING;
		}

		if(groundPound > 0){
			characterState = CharacterState.GROUND_POUND;
		}
	}

	void FallingState(){
		if(IsGrounded()){
			characterState = CharacterState.RUNNING; // see if stationary is more appropriate
		}
		else{
			universalMovementVector.y += gravity * Time.deltaTime;
			universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);
			if(fallingMoveBack.magnitude != 0){
				universalMovementVector = Vector3.zero;
			}

			//controller.Move(universalMovementVector * Time.deltaTime /*+ fallingMoveBack*/);
			//universalMovementVectorDelta = universalMovementVector * Time.deltaTime + fallingMoveBack;
			universalMovementVectorDelta = universalMovementVector * Time.deltaTime + new Vector3(fallingMoveBack.x, 0, fallingMoveBack.z);
			controller.Move(universalMovementVectorDelta);
			//universalMovementVectorDelta = new Vector3(0, fallingMoveBack.y, 0);
			//controller.Move(universalMovementVectorDelta);

			/*
			universalMovementVectorDelta = new Vector3(moveBack.x, 0, moveBack.z) * slopeSlipSpeed * Time.deltaTime;
			controller.Move(universalMovementVectorDelta);
			
			universalMovementVectorDelta = new Vector3(0, moveBack.y * 20, 0) * slopeSlipSpeed * Time.deltaTime;
			controller.Move(universalMovementVectorDelta);
			*/


			//Debug.Log("not grounded");
			fallingMoveBack = Vector3.zero;
			if(groundPound > 0){
				characterState = CharacterState.GROUND_POUND;
			}
		}
	}

	void SlippingState(){
		// slipping state will put the character back and down the slope by the moveback vector
		// it will not accept inputs and will set the state back to stationary, if still on slope,
		// the collision function will send us back to slipping for a 0 net change
		// we want all initial movement zeroed out so don't "add" to but rather "set" the universal vector
		// to avoid issues with collider not allowing movement due to y component forcing character into mesh (maybe)
		// apply the x and z first and then the y
		// intially from another state the character moves and the OnControllerColliderHit function is called within after moving
		// this should then determine a vector to move back and down by and set the state to slipping, on the first slipping call
		// we move by the moveback vector and thus call the collision function again it will decide wether to swap the state

		characterState = CharacterState.STATIONARY;

		universalMovementVector = Vector3.zero;


		universalMovementVectorDelta = new Vector3(moveBack.x, 0, moveBack.z) * slopeSlipSpeed * Time.deltaTime;
		controller.Move(universalMovementVectorDelta);

		universalMovementVectorDelta = new Vector3(0, moveBack.y * 20, 0) * slopeSlipSpeed * Time.deltaTime;
		controller.Move(universalMovementVectorDelta);

		//Debug.Log(moveBack);
	}

	void GroundPoundState(){
		// in the ground pound state we halt all motion and start the ground pound animation
		// a timer is started which will cover the length of the animation plus a little delay
		// then the rest of the code is allowed to process and the character falls at maximum
		// velocity. once ground is detected we revert back to stationary
		if(groundPoundTimer >= groundPoundTimeLimit){
			// times up proceed with falling
			if(IsGrounded()){
				characterState = CharacterState.STATIONARY;
				groundPoundTimer = 0.0f;
			}
			else{
				//controller.Move(new Vector3(0, maxFallVelocity, 0) * Time.deltaTime);
				universalMovementVectorDelta = new Vector3(0, maxFallVelocity, 0) * Time.deltaTime;
				controller.Move(universalMovementVectorDelta);
			}
		}
		else{
			groundPoundTimer += Time.deltaTime;
		}
	}
	/*
	public bool IsGroundPounding(){
		Debug.Log(characterState);
		if(characterState == CharacterState.GROUND_POUND){
			return true;
		}
		else{
			return false;
		}
	}
	*/
	void LongJump(){
		
		universalMovementVector = transform.forward * initialLongJumpVelocity.x + new Vector3(0, initialLongJumpVelocity.y, 0);

		//controller.Move(universalMovementVector * Time.deltaTime);
		universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		controller.Move(universalMovementVectorDelta);


		characterState = CharacterState.JUMPING_LONG;
	}

	void HighJump(){
		
		universalMovementVector = new Vector3(0, initialHighJumpVelocity, 0);

		//controller.Move(universalMovementVector * Time.deltaTime);
		universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		controller.Move(universalMovementVectorDelta);

		characterState = CharacterState.JUMPING_HIGH;
	}

	void RegularJump(){
		
		universalMovementVector = new Vector3(0, initialJumpVelocity, 0);

		//controller.Move(universalMovementVector * Time.deltaTime);
		universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		controller.Move(universalMovementVectorDelta);

		characterState = CharacterState.JUMPING_LOW;
	}

	bool IsGrounded(){
		if(Physics.Raycast(transform.position, -Vector3.up, groundDistanceCheck)){
			//controller.Move(new Vector3(0, -groundDistanceCheck, 0));
			universalMovementVectorDelta = new Vector3(0, -groundDistanceCheck, 0);
			controller.Move(universalMovementVectorDelta);

			return true;
		}
		else{
			return false;
		}
	}

	public enum CharacterState{
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
		SLIPPING_NO_CONTROL,
		GROUND_POUND
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


	void CalculateExtraRotationTranslationVector(){
		// this function will ray cast downward for a rotating body
		// if object is tagged ith rotation access its script for
		// a vector to translate by, the player is grounded when this
		// function is called

		Vector3 start = transform.position;
		Vector3 direction = Vector3.down;
		RaycastHit hit;
		extraRotationTranslationVector = Vector3.zero;
		if(Physics.Raycast(start, direction, out hit)){
			if(hit.transform.tag == "rotatingPlatform"){
				extraRotationTranslationVector = hit.collider.gameObject.GetComponent<RotatePlatform>().TranslateCharacter(transform.position);
			}
		}
	}

	void OnControllerColliderHit(ControllerColliderHit hit){ // this function in unity is called inside characterController.move()
		// there are 4 surfaces that can be found here, floor, slippery slope, wall, ceiling
		// we ray cast in the direction of the hit from position to find the surface hit
		// 

		if(hit.collider.tag == "rotatingPlatform"){
			return;
		}
		else if(hit.collider.tag == "switch" && characterState == CharacterState.GROUND_POUND){
			hit.collider.gameObject.GetComponent<buttonCollapse>().Collapse();
			return;
		}




		// raycast code to get the normal of the surface
		RaycastHit ray1;
		Vector3 direction = hit.point - transform.position;
		if(Physics.Raycast(transform.position, direction, out ray1, direction.magnitude + forwardDistanceCheck)){
			
		}
		else{
			Debug.Log("error did not find surface");
		}




		if(ray1.normal.y >= 0.5f){
			// floor, we do nothing
		}
		else if(ray1.normal.y < 0.5f && ray1.normal.y > 0.09f){
			// slippery slope
			// gravity wont pull us down we need to move backward first and then down
			// we might have jumped into the slope and thus are still moving up (in the jumping state)
			// in which case we ignore the slope, it only applies when grounded or falling
			if(universalMovementVector.y > 0.0f){
				return;
			}
			else{
				// we are not falling so now we slide down the slope

				// slippery surface, greater than 60 degrees
				// we must set the player into the slip state
				// with a backward/downward vector that is proportional
				// to the slope of the slipper surface

				// the downward/backward vector is the normal vector with the
				// value of sqrt(x^2 + z^2) switched with y
				// so x1 = mx, z1 = mz, and m = y/sqrt(x^2 + z^2)
				// and y = sqrt(x^2 + z^2)
				// make y negative to reflect gravity
				// and move x and z first then y
				if(characterState != CharacterState.SLIPPING_NO_CONTROL){
					characterState = CharacterState.SLIPPING_NO_CONTROL;
					//Debug.Log(hit.normal);
					// we have not executed a slip
					float y1 = Mathf.Sqrt(ray1.normal.x * ray1.normal.x + ray1.normal.z * ray1.normal.z);
					float m = ray1.normal.y/y1;
					float x1 = ray1.normal.x * m;
					float z1 = ray1.normal.z * m;
					// the new vector is ready
					moveBack = new Vector3(x1, -y1, z1); // this is a unit vector (magnitude = 1)
				}
				else{
					// we are waiting for update to execute our moveback vector, so do nothing
				}

			}

		}
		else if(ray1.normal.y < 0.09f && ray1.normal.y > -0.09f){
			Debug.Log("found a wall" + universalMovementVector.y);
			// we have found a wall, once again we do nothing if we are in the jumping state
			// otherwise we must force the player to fall by measuring the slope and creating a moveback
			// vector and fall down by the amount we lost from the collision which is the current
			// universalmovementvectorDelta.y - (PrevPos.y - transform.position.y)
			// apply this directly as is
			if(universalMovementVector.y > 0.0f){
				return;
			}
			else{
				// found a wall and not jumping

				fallingMoveBack = transform.position - hit.point;
				fallingMoveBack.y = 0;
				Debug.Log(fallingMoveBack);

				/*
				float y1 = Mathf.Sqrt(ray1.normal.x * ray1.normal.x + ray1.normal.z * ray1.normal.z);
				float m = ray1.normal.y/y1;
				float x1 = ray1.normal.x * m;
				float z1 = ray1.normal.z * m;
				// the new vector is ready

				fallingMoveBack = new Vector3(-x1, -Mathf.Abs(y1), z1); // this is a unit vector (magnitude = 1)
				// now multiply it by the value lost
				// DOUBLE CHECK THIS ITS MAKING Y BECOME POSITIVE
				Debug.Log("falling move back" + fallingMoveBack + " " + universalMovementVectorDelta.y + " " + PrevPos.y + " " + transform.position.y);
				fallingMoveBack *= 1.0f;//Mathf.Abs(universalMovementVectorDelta.y - (PrevPos.y - transform.position.y));
				Debug.Log("falling move back" + fallingMoveBack);
				*/
			}
		}
		else{
			// ceiling do nothing yet, later cancel the momentum and begin falling	
		}













		/*

		if(hit.collider.tag == "rotatingPlatform"){
			return;
		}
		else if(hit.collider.tag == "switch" && characterState == CharacterState.GROUND_POUND){
			hit.collider.gameObject.GetComponent<buttonCollapse>().Collapse();
			return;
		}

		RaycastHit ray;
		if(Physics.Raycast(transform.position, Vector3.down, out ray, groundDistanceCheck)){
			//Debug.Log(ray.normal);


			if(ray.normal.y < -0.09f){
				// ceiling
			}
			else if(ray.normal.y < 0.09f){
				//Debug.Log("wall");
			}
			else if(ray.normal.y < 0.5f){
				// slippery surface, greater than 60 degrees
				// we must set the player into the slip state
				// with a backward/downward vector that is proportional
				// to the slope of the slipper surface

				// the downward/backward vector is the normal vector with the
				// value of sqrt(x^2 + z^2) switched with y
				// so x1 = mx, z1 = mz, and m = y/sqrt(x^2 + z^2)
				// and y = sqrt(x^2 + z^2)
				// make y negative to reflect gravity
				// and move x and z first then y
				if(characterState != CharacterState.SLIPPING_NO_CONTROL){
					characterState = CharacterState.SLIPPING_NO_CONTROL;
					//Debug.Log(hit.normal);
					// we have not executed a slip
					float y1 = Mathf.Sqrt(ray.normal.x * ray.normal.x + ray.normal.z * ray.normal.z);
					float m = ray.normal.y/y1;
					float x1 = ray.normal.x * m;
					float z1 = ray.normal.z * m;
					// the new vector is ready
					moveBack = new Vector3(x1, -y1, z1); // this is a unit vector (magnitude = 1)
				}
				else{
					// we are waiting for update to execute our moveback vector, so do nothing
				}
			}
			else{
				// regular floor
			}


		}
		else{
			//Debug.Log("no floor found");
			// we did collide with something but it is not a floor, might be stuck on it
			RaycastHit ray2;
			Vector3 direction = hit.point - transform.position;
			if(Physics.Raycast(transform.position, direction, out ray2, direction.magnitude + forwardDistanceCheck)){
				Debug.Log("wall found");
				fallingMoveBack = new Vector3(-direction.x, 0, -direction.z) * moveBackValue;
				Debug.Log(fallingMoveBack);
			}
		}
		*/
	}

}
