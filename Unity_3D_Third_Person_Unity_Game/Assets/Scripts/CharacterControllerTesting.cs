using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerTesting : MonoBehaviour
{

	Transform cameraT;
    public float distToGround;
    public bool isGrounded = true;
    public Vector3 slopeVector = new Vector3(0, 0, 0); // on each frame we get the normal of the ground, if no ground then (0, 0, 0)
    public float forwardHeightTop = 1.0f;
    public float forwardHeightMiddle = 1.0f;
    public float forwardHeightBottom = 1.0f;
    public float downwardThickness = 0.1f; // the difference between the ground and the position of the character
    public float downwardHeight = 2.0f; // the height the ray cast starts from (up from position)
    public float upwardHeight = 2.0f; // the height the ray cast starts from (up from position)
    public float forwardThickness = 0.2f; // this need to be have the diameter of the total player


    // a wall is defined as any surface with normal.y between -0.174 and 0.174
    public float wallUpperLimitNormalY = 0.174f; // <=
    public float wallLowerLimitNormalY = -0.174f; // >=
    // a regular floor is defined as any surface with normal.y between 0.707 and 1.0
    public float floorUpperLimitNormalY = 1.0f; // <=
    public float floorLowerLimitNormalY = 0.707f; // >=
    // a slippery floor is defined as any surface with normal.y between 0.5 and 0.707
    public float slipperyFloorUpperLimitNormalY = 0.707f; // <
    public float slipperyFloorLowerLimitNormalY = 0.5f; // >=
    // a ceiling is defined as any surface with normal.y between -1.0 and -0.5
    public float ceilingUpperLimitNormalY = -0.5f; // <=
    // -1.0f is the minimum value for normal.y so we don't need to use this value for ceilings
    


    // Bit shift the index of the layer (8) to get a bit mask
    public int layerMask = 1 << 8;
    // This would cast rays only against colliders in layer 8.

    private Animator anim;
	public float animMoveSpeed = 0.0f;
	public float animDeltaSpeed = 0.01f;
	public float animIdleSpeed = 0.0f;
	public float animWalkSpeed = 0.6f;
	public float animRunSpeed = 1.0f;

	public CharacterState characterState = CharacterState.STATIONARY;

	public float walkSpeed = 0.1f;
	public float runSpeed = 0.2f;

    // the current gravity value is contained in the universal vector, gravity increment is added to its y component
    public float gravityIncrement = -0.0981f; // this is the amount each frame the gravity value changes
	public float maxFallVelocity = -0.1f; // the maximum value to displace the character

	public float groundDistanceCheck = 0.1f;

	public float turnSmoothTime = 0.0f;
	float turnSmoothVelocity = 0.0f;

	public float speedSmoothTime = 0.1f;
	float speedSmoothVelocity;
	float currentSpeed;



	public float initialJumpVelocity = 1.0f;
	public float initialHighJumpVelocity = 2.0f;
	public Vector2 initialLongJumpVelocity = new Vector2(1.0f, 1.0f); // length and height
    
    
    

	// input variables
	Vector2 input;
	float jump;
	float specialJump;
	float groundPound;

	// secondary input variables
	public bool running;




	// this is the vector by which the player moves, all inputs and movements are added to this vector and then
	// this vector is applied to the player each update, so to jump you apply a single up value and on each frame
	// that value will be used to move the player up, and gravity will reduce it
	// when grounded the y component will be set to zero
	public Vector3 universalMovementVector = new Vector3(0, 0, 0);
	//public float slopeSlipSpeed = 0.1f;
	public bool InputAccepted = true;
	public float groundPoundTimer = 0.0f;
	public float groundPoundTimeLimit = 4.0f;

    // Start is called before the first frame update
    void Start(){
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

		cameraT = Camera.main.transform;

		anim = gameObject.GetComponentInChildren<Animator>();
		anim.SetFloat("speed", animIdleSpeed);
	}

	// Update is called once per frame
	void FixedUpdate(){
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
        // no matter what state we are in we must check the collisions with the ray cast
        // thus we do the collision testing after the state function
        // each raycast will set values of their own (downward raycast will set isgrounded if it detects a ground)
        // thus checking for isgrounded is done by variable rather than function
        // the downward raycast must check for ground and a rotating body, if rotating body is found then
        // add it to the universalTranslationVector
        // also DO NOT USE DELTA TIME, only use motion as frame by frame Application.targetFrameRate = 30
        

        bool xzModified = false;
        bool yModified = false;

        Vector3 XZ = new Vector3(universalMovementVector.x, 0, universalMovementVector.z);
        Vector3 Y = new Vector3(0, universalMovementVector.y, 0);




        RaycastHit hitDownward;
        // remember the distance is based off the universalMovementVector
        // we only want the x and z components of the universalMovementVector, x and z limits are identical
        // we want the player's thickness included in the raycast

        // a tiny 0.01f offset to ensure the raycast doesn't just miss the floor on next frame after moving character
        // to the floor
        float trans = -(Y.y - downwardThickness) + downwardHeight + 0.01f;
        slopeVector = Vector3.zero; // reset the slope vector to zero each frame
        if (Physics.Raycast(transform.position + new Vector3(0, downwardHeight, 0), -transform.up, out hitDownward, trans, layerMask))
        {
            // we found something but the normal.y might be >= 0.09
            //Debug.Log(hitDownward.normal);
            if (hitDownward.normal.y >= 0.5f && hitDownward.normal.y < slipperyFloorUpperLimitNormalY)
            {
                // a slippery floor is detected
                Debug.Log("slippery floor is hit");
                float translator = hitDownward.point.y + downwardThickness;
                transform.position = new Vector3(transform.position.x, translator, transform.position.z);
                yModified = true;
                isGrounded = true;
                slopeVector = hitDownward.normal;
                if (hitDownward.transform.tag == "rotatingPlatform")
                {
                    universalMovementVector += hitDownward.collider.gameObject.GetComponent<RotatePlatform>().TranslateCharacter(transform.position);

                }
            }
            else if (hitDownward.normal.y >= slipperyFloorUpperLimitNormalY)
            {
                // a regular floor is detected
                Debug.Log("floor is hit");
                float translator = hitDownward.point.y + downwardThickness;
                transform.position = new Vector3(transform.position.x, translator, transform.position.z);
                yModified = true;
                isGrounded = true;
                if(hitDownward.transform.tag == "rotatingPlatform")
                {
                    universalMovementVector += hitDownward.collider.gameObject.GetComponent<RotatePlatform>().TranslateCharacter(transform.position);

                }
            }
            else
            {
                // a surface was found but it does not meet the definition of a regular floor or a slippery floor
                isGrounded = false;
            }
        }
        else
        {
            Debug.Log("not found");
            isGrounded = false;
        }




        if (XZ == Vector3.zero && Y != Vector3.zero)
        {
            // we are moving vertically but not horizontally thus we have no direction for wall collisions
            // must use the transform.forward, and do a vector in all four directions
            RaycastHit noDirection;
            if (Physics.Raycast(transform.position, transform.forward, out noDirection, forwardThickness + Mathf.Sin(wallUpperLimitNormalY) * maxFallVelocity, layerMask))
            {
                if (noDirection.normal.y >= wallLowerLimitNormalY && noDirection.normal.y <= wallUpperLimitNormalY)
                {
                    Debug.Log("wall is hit");
                    Vector3 translator = noDirection.point + noDirection.normal * forwardThickness;
                    transform.position = new Vector3(translator.x, transform.position.y, translator.z);
                    xzModified = true;
                    //universalMovementVector = Vector3.zero;
                }
                else
                {
                    // a surface was found but it does not meet the definition of a wall
                }
            }
            if (Physics.Raycast(transform.position, transform.right, out noDirection, forwardThickness + Mathf.Sin(wallUpperLimitNormalY) * maxFallVelocity, layerMask))
            {
                if (noDirection.normal.y >= wallLowerLimitNormalY && noDirection.normal.y <= wallUpperLimitNormalY)
                {
                    Debug.Log("wall is hit");
                    Vector3 translator = noDirection.point + noDirection.normal * forwardThickness;
                    transform.position = new Vector3(translator.x, transform.position.y, translator.z);
                    xzModified = true;
                    //universalMovementVector = Vector3.zero;
                }
                else
                {
                    // a surface was found but it does not meet the definition of a wall
                }
            }
            if (Physics.Raycast(transform.position, -transform.right, out noDirection, forwardThickness + Mathf.Sin(wallUpperLimitNormalY) * maxFallVelocity, layerMask))
            {
                if (noDirection.normal.y >= wallLowerLimitNormalY && noDirection.normal.y <= wallUpperLimitNormalY)
                {
                    Debug.Log("wall is hit");
                    Vector3 translator = noDirection.point + noDirection.normal * forwardThickness;
                    transform.position = new Vector3(translator.x, transform.position.y, translator.z);
                    xzModified = true;
                    //universalMovementVector = Vector3.zero;
                }
                else
                {
                    // a surface was found but it does not meet the definition of a wall
                }
            }
            if (Physics.Raycast(transform.position, -transform.forward, out noDirection, forwardThickness + Mathf.Sin(wallUpperLimitNormalY) * maxFallVelocity, layerMask))
            {
                if (noDirection.normal.y >= wallLowerLimitNormalY && noDirection.normal.y <= wallUpperLimitNormalY)
                {
                    Debug.Log("wall is hit");
                    Vector3 translator = noDirection.point + noDirection.normal * forwardThickness;
                    transform.position = new Vector3(translator.x, transform.position.y, translator.z);
                    xzModified = true;
                    //universalMovementVector = Vector3.zero;
                }
                else
                {
                    // a surface was found but it does not meet the definition of a wall
                }
            }
        }
        else
        {
            // these three ray casts must be done at the varying heights of the character
            RaycastHit hitForwardTop;
            // remember the distance is based off the universalMovementVector
            // we only want the x and z components of the universalMovementVector, x and z limits are identical
            // can use either for distance, and then set the temps based of which is smaller the current temps or the
            // new calculated temps
            // we want the player's thickness included in the raycast
            if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop, 0), XZ, out hitForwardTop, XZ.magnitude + forwardThickness, layerMask))
            {
                if(hitForwardTop.normal.y >= wallLowerLimitNormalY && hitForwardTop.normal.y <= wallUpperLimitNormalY)
                {
                    Debug.Log("wall is hit");
                    Vector3 translator = hitForwardTop.point + hitForwardTop.normal * forwardThickness;
                    transform.position = new Vector3(translator.x, transform.position.y, translator.z);
                    xzModified = true;
                    //universalMovementVector = Vector3.zero;
                }
                else
                {
                    // a surface was found but it does not meet the definition of a wall
                }
            }



            RaycastHit hitForwardMiddle;
            // remember the distance is based off the universalMovementVector
            // we only want the x and z components of the universalMovementVector, x and z limits are identical
            // can use either for distance, and then set the temps based of which is smaller the current temps or the
            // new calculated temps
            // we want the player's thickness included in the raycast
            if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightMiddle, 0), XZ, out hitForwardMiddle, XZ.magnitude + forwardThickness, layerMask))
            {
                if(hitForwardMiddle.normal.y >= wallLowerLimitNormalY && hitForwardMiddle.normal.y <= wallUpperLimitNormalY)
                {
                    Debug.Log("wall is hit");
                    Vector3 translator = hitForwardMiddle.point + hitForwardMiddle.normal * forwardThickness;
                    transform.position = new Vector3(translator.x, transform.position.y, translator.z);
                    xzModified = true;
                    //universalMovementVector = Vector3.zero;
                }
                else
                {
                    // a surface was found but it does not meet the definition of a wall
                }
            }


            RaycastHit hitForwardBottom;
            // remember the distance is based off the universalMovementVector
            // we only want the x and z components of the universalMovementVector, x and z limits are identical
            // can use either for distance, and then set the temps based of which is smaller the current temps or the
            // new calculated temps
            // we want the player's thickness included in the raycast
            if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightBottom, 0), XZ, out hitForwardBottom, XZ.magnitude + forwardThickness, layerMask))
            {
                if(hitForwardBottom.normal.y >= wallLowerLimitNormalY && hitForwardBottom.normal.y <= wallUpperLimitNormalY)
                {
                    Debug.Log("wall is hit");
                    Vector3 translator = hitForwardBottom.point + hitForwardBottom.normal * forwardThickness;
                    transform.position = new Vector3(translator.x, transform.position.y, translator.z);
                    xzModified = true;
                    //universalMovementVector = Vector3.zero;
                }
                else
                {
                    // a surface was found but it does not meet the definition of a wall
                }
            }
        }
        


        // only if not modified will this function move the player in that coordinate
        MoveCharacter(universalMovementVector, xzModified, yModified);
        Debug.DrawRay(transform.position + new Vector3(0, forwardHeightTop, 0), transform.forward * 10, Color.red);
        Debug.DrawRay(transform.position + new Vector3(0, forwardHeightMiddle, 0), transform.forward * 10, Color.red);
        Debug.DrawRay(transform.position + new Vector3(0, forwardHeightBottom, 0), transform.forward * 10, Color.red);
    }

    
    void MoveCharacter(Vector3 movement, bool xz, bool y)
    {
        // do not use Time.deltaTime
        if (xz)
        {
            movement.x = 0;
            movement.z = 0;
            if (!isGrounded)
            {
                // we have modified x and z and are not grounded thus we are in the air and hit a wall
                // so we should smack the wall
                universalMovementVector.x = 0;
                universalMovementVector.z = 0;
            }
        }
        if (y)
        {
            movement.y = 0;
        }
        transform.Translate(movement, Space.World);
    }

	void StationaryState(){
        universalMovementVector = Vector3.zero;
		if(!isGrounded){
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
			// now check if a jump has been called
			// notice how we go to a jump state rather than running state if both running and jumping
			if(jump > 0 && isGrounded){
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
		if(!isGrounded){
			// we are in the running state but no ground is present, we swap the state
			// and skip the input
			characterState = CharacterState.FALLING;
		}
		else{

			if (input != Vector2.zero) {
				// all this is to rotate the character in the desired running direction
				float targetRotation = Mathf.Atan2 (input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
                transform.eulerAngles = Vector3.up * targetRotation;// Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
			}
			else{
				// no input so we move back to stationary
				characterState = CharacterState.STATIONARY;
			}

			// determine the speed at which to move the player
			float targetSpeed = ((running || Input.GetKey(KeyCode.E)) ? runSpeed : walkSpeed) * input.magnitude;
			currentSpeed = Mathf.SmoothDamp (currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);
			universalMovementVector = transform.forward * targetSpeed;
			
			if(jump > 0){
				// later come up with a keyboard click tester (don't let holding down button work as jump input)
				// now determine what kind of jump
				if(specialJump > 0){
					if(running || Input.GetKey(KeyCode.E)){
						// since we are in the running state we will do a high jump
						LongJump();
					}
					else{
						HighJump(); // later we should restrict this to the stationary state
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
        // also since in long jump we dont want to change the character's rotation
        universalMovementVector.y += gravityIncrement;
		universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

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

        universalMovementVector.x = transform.forward.x * currentSpeed;
        universalMovementVector.z = transform.forward.z * currentSpeed;


		universalMovementVector.y += gravityIncrement;
		universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

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

        universalMovementVector.x = transform.forward.x * currentSpeed;
        universalMovementVector.z = transform.forward.z * currentSpeed;


		universalMovementVector.y += gravityIncrement;
		universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

		if(universalMovementVector.y <= 0.0f){
			characterState = CharacterState.FALLING;
		}

		if(groundPound > 0){
			characterState = CharacterState.GROUND_POUND;
		}
	}

	void FallingState(){
		if(isGrounded){
            //Debug.Log("grounded");
			characterState = CharacterState.RUNNING; // see if stationary is more appropriate
		}
		else{
            float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);

            float targetSpeed = ((running || Input.GetKey(KeyCode.E)) ? runSpeed : walkSpeed) * input.magnitude;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

            universalMovementVector.x = transform.forward.x * currentSpeed;
            universalMovementVector.z = transform.forward.z * currentSpeed;


            universalMovementVector.y += gravityIncrement;
			universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);
			
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
        // the slope has been determined by the raycast and it will have saved it in a vector
        // we move by this vector, might want to adjust speed by the angle of the slope (60 = 0 and 90 = max)
		//universalMovementVector = slopeVector * slopeSlipSpeed;
        
	}

	void GroundPoundState(){
		// in the ground pound state we halt all motion and start the ground pound animation
		// a timer is started which will cover the length of the animation plus a little delay
		// then the rest of the code is allowed to process and the character falls at maximum
		// velocity. once ground is detected we revert back to stationary
		if(groundPoundTimer >= groundPoundTimeLimit){
			// times up proceed with falling
			if(isGrounded){
				characterState = CharacterState.STATIONARY;
				groundPoundTimer = 0.0f;
			}
			else{
                // we are falling during ground pound
				//controller.Move(new Vector3(0, maxFallVelocity, 0) * Time.deltaTime);
				universalMovementVector.y = maxFallVelocity;
				//controller.Move(universalMovementVectorDelta);
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
		//universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		//controller.Move(universalMovementVectorDelta);


		characterState = CharacterState.JUMPING_LONG;
	}

	void HighJump(){
        // keep the x and z as we want to continue to move (but they might be zero anyway since our character has to be stationary)
        universalMovementVector.y = initialHighJumpVelocity;
		characterState = CharacterState.JUMPING_HIGH;
	}

	void RegularJump(){
        // keep the x and z as we want to continue to move
        universalMovementVector.y = initialJumpVelocity;
		characterState = CharacterState.JUMPING_LOW;
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
}