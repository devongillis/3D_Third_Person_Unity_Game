using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerTesting : MonoBehaviour
{
	Transform cameraT;
    public float distToGround;
    public bool isGrounded = true;
    public Vector3 slopeVector = new Vector3(0, 0, 0);
    public float height = 1.0f; // this needs to be half the value of the total player height
    public float thickness = 1.0f; // this need to be hald the diameter of the total player

    // Bit shift the index of the layer (8) to get a bit mask
    public int layerMask = 1 << 8;
    // This would cast rays only against colliders in layer 8.

    private Animator anim;
	//private float speed = 0.0f;
	public float animMoveSpeed = 0.0f;
	public float animDeltaSpeed = 0.01f;
	public float animIdleSpeed = 0.0f;
	public float animWalkSpeed = 0.6f;
	public float animRunSpeed = 1.0f;

	public CharacterState characterState = CharacterState.STATIONARY;

	public float walkSpeed = 0.1f;
	public float runSpeed = 0.2f;

	public float gravity = -0.0981f;
	public float maxFallVelocity = -0.1f;

	//public float magnitudeJumpAcceptance = 1.0f;
	public float groundDistanceCheck = 0.1f;

	public float turnSmoothTime = 0.0f;
	float turnSmoothVelocity;

	public float speedSmoothTime = 0.1f;
	float speedSmoothVelocity;
	float currentSpeed;

	Vector3 PrevPos;
	Vector3 NewPos;
	Vector3 ObjVelocity;



	public float initialJumpVelocity = 1.0f;
	public float initialHighJumpVelocity = 2.0f;
	public Vector2 initialLongJumpVelocity = new Vector2(1.0f, 1.0f); // length and height
    //public float jumpDirectionalMomentumDecay = 0.99f;
    
    
    

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
	//public Vector3 universalMovementVectorDelta = new Vector3(0, 0, 0);


	//bool forcedFall = false;
	//public float moveBackGradient = 1.0f;
	//Vector3 moveBack;

	public float slopeSlipSpeed = 0.1f;


	public bool InputAccepted = true;


	public Vector3 extraRotationTranslationVector = Vector3.zero;





	public float groundPoundTimer = 0.0f;
	public float groundPoundTimeLimit = 4.0f;


    float limitXP;
    float limitXN;
    float limitYP;
    float limitYN;
    float limitZP;
    float limitZN;


    //public float forwardDistanceCheck = 0.1f;
    //public float moveBackValue = 0.1f;
    //public Vector3 fallingMoveBack = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start(){
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        limitXP = initialLongJumpVelocity.x;
        limitXN = -initialLongJumpVelocity.x;
        limitYP = initialHighJumpVelocity;
        limitYN = maxFallVelocity;
        limitZP = initialLongJumpVelocity.x;
        limitZN = -initialLongJumpVelocity.x;

        PrevPos = transform.position;
		NewPos = transform.position;

		cameraT = Camera.main.transform;

		anim = gameObject.GetComponentInChildren<Animator>();
		anim.SetFloat("speed", animIdleSpeed);
	}

	// Update is called once per frame
	void Update(){
		//Debug.Log(characterState);
        /*
		NewPos = transform.position;
		ObjVelocity = (NewPos - PrevPos)/Time.deltaTime;
		PrevPos = NewPos;
        */
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
        // set the addtional translation vector
        // also we must break down movement into parts to fit with the raycasts
        // that means the translations must be calulated in each state but not executed
        // make sure the translation parts are smaller than the smallest raycast used and then raycast for each movement
        // or we can break down the movement vector into 3 parts x, y, z and move each one and raycast based of the values
        // so raycast in the x at the maximum value allowed to move forward and then move the player the smaller of the two values
        // in the x (desired movement or the limited movement), the repeat for z and then y
        // also check for rotation translation during the raycast, each state only added their own translation, stationary
        // doesn't do anything so it can't check for rotating bodies
        // also dont let any state determine movement using time.deltaTime, we will account for that at the end
        // also cap off the gravity velocity
        // also DO NOT USE DELTA TIME, only use motion as frame by frame Application.targetFrameRate = 30
        // it is important that we check the y raycast last as if we are moving down and in a direction (slope) we
        // want to move down the slope and should do x and z first
        // since we are breaking down the universalMovementVector into its x, y, z components we can limit the raycasts being used
        // first assign default values for x, y, z and for each raycast we begin to set limits for their values
        // once the limits are determined we need to include an offset for the player size, this is done to all 6 values
        // after all the raycasts are done and before the movement is executed
        // say the forward is initial limited by 10.0f and the player is 1.0f units radius, if a wall is found
        // that is 9.0f units infront, then the limit is set to 9.0f and then set to 8.0f for the thickness of the player
        // thus the player only moves 8.0f units, this is because the wall is 9.0f away from the player center but 8.0f away
        // from the player's front radius, we want the front of the player right on the wall
        // then after all limits are determined we move the player in the x, y, z by smaller of the two values (limits and vector)

        float limitXPt = limitXP; // P = positive, N = negative, t = temporary
        float limitXNt = limitXN;
        float limitYPt = limitYP;
        float limitYNt = limitYN;
        float limitZPt = limitZP;
        float limitZNt = limitZN;

        /*
        // upward raycast
        // the upward ray cast must react to any normal.y < -0.09 (5 degrees)
        RaycastHit hitUpward;
        // remember the distance is based off the universalMovementVector
        // we only want the x and z components of the universalMovementVector, x and z limits are identical
        // can use either for distance, and then set the temps based of which is smaller the current temps or the
        // new calculated temps
        // we want the player's thickness included in the raycast
        if (Physics.Raycast(transform.position, transform.up, out hitUpward, limitXP + height, layerMask))
        {
            // we found something but the normal.y might be >= 0.09
            if (hitUpward.normal.y <= -0.09f) // a ceiling is detected
            {
                Debug.Log("ceiling within range, determine limits to move by");
                // the y value is less than 0.09 which means its a wall
                // must set the limits for the x and z values, forward means maximum value added to x
                // but we could be moving in the -x direction
                // we don't care about y so don't need to account for the height of the character
                Vector3 initialTranslation = hitForward.point - transform.position;
                // we take the hitpoint as the absolute maximum the player can travel in the forward direction
                // toward that point and remove the thickness, thus limit actually means a position, not a translation
                Vector3 playerTranslationVector = initialTranslation - (initialTranslation.normalized * thickness);
                // we now have a translation convert this to limits
                // the vector has x and z components if they are negative
                if (initialTranslation.x >= 0)
                {
                    // moving in the +x direction
                    limitXPt = Mathf.Min(limitXPt, playerTranslationVector.x);
                }
                else
                {
                    limitXNt = Mathf.Max(limitXNt, playerTranslationVector.x);
                }
                if (initialTranslation.z >= 0)
                {
                    // moving in the -x direction
                    limitZPt = Mathf.Min(limitZPt, playerTranslationVector.z);
                }
                else
                {
                    limitZNt = Mathf.Max(limitZNt, playerTranslationVector.z);
                }
            }
            // the limits in the x and z directions regarding what the forward vector finds are set
        }
        else
        {
            //Debug.DrawRay(transform.position, transform.forward * 1000, Color.white);
            //Debug.Log("Did not Hit");
            // there is nothing so do nothing
        }
        */



        // downward raycast
        // the downward raycast will check if the character is grounded and also if the slope is within range
        // must react to any normal.y > 0.09


        // these four ray casts must be done at the height of the character

        // backward raycast
        // this is mostly used for when the character is moving backward by terrain or enemy
        // the backward raycast will check if there is a wall behind the player by its normal
        // if the normal.y >= 0.09 then we have a floor/slippery floor thus we do not react by the backward raycast
        // that will be for the downward raycast to check
        RaycastHit hitBackward;
        // remember the distance is based off the universalMovementVector
        // we only want the x and z components of the universalMovementVector, x and z limits are identical
        // can use either for distance, and then set the temps based of which is smaller the current temps or the
        // new calculated temps
        // we want the player's thickness included in the raycast
        if (Physics.Raycast(transform.position + new Vector3(0, height, 0), transform.forward, out hitBackward, limitXP + thickness, layerMask))
        {
            // we found something but the normal.y might be >= 0.09
            if (hitBackward.normal.y <= -0.09f)
            {
                // a ceiling is detected do nothing, that is for the hitUpward
            }
            else if (hitBackward.normal.y >= 0.09f)
            {
                // a floor is detected do nothing, that is for the hitDownward
            }
            else
            {
                Debug.Log("wall within range, determine limits to move by");
                // the y value is less than 0.09 which means its a wall
                // must set the limits for the x and z values, forward means maximum value added to x
                // but we could be moving in the -x direction
                // we don't care about y so don't need to account for the height of the character
                Vector3 initialTranslation = hitBackward.point - transform.position;
                // we take the hitpoint as the absolute maximum the player can travel in the forward direction
                // toward that point and remove the thickness, thus limit actually means a position, not a translation
                Vector3 playerTranslationVector = initialTranslation - (initialTranslation.normalized * thickness);
                // we now have a translation convert this to limits
                // the vector has x and z components if they are negative
                if (initialTranslation.x >= 0)
                {
                    // moving in the +x direction
                    limitXPt = Mathf.Min(limitXPt, playerTranslationVector.x);
                }
                else
                {
                    limitXNt = Mathf.Max(limitXNt, playerTranslationVector.x);
                }
                if (initialTranslation.z >= 0)
                {
                    // moving in the -x direction
                    limitZPt = Mathf.Min(limitZPt, playerTranslationVector.z);
                }
                else
                {
                    limitZNt = Mathf.Max(limitZNt, playerTranslationVector.z);
                }
            }
            // the limits in the x and z directions regarding what the forward vector finds are set
        }
        else
        {
            //Debug.DrawRay(transform.position, transform.forward * 1000, Color.white);
            //Debug.Log("Did not Hit");
            // there is nothing so do nothing
        }






        // forward raycast
        // the forward raycast will check if there is a wall infront of the player by its normal
        // if the normal.y >= 0.09 then we have a floor/slippery floor thus we do not react by the forward raycast
        // that will be for the downward raycast to check
        RaycastHit hitForward;
        // remember the distance is based off the universalMovementVector
        // we only want the x and z components of the universalMovementVector, x and z limits are identical
        // can use either for distance, and then set the temps based of which is smaller the current temps or the
        // new calculated temps
        // we want the player's thickness included in the raycast
        if (Physics.Raycast(transform.position + new Vector3(0, height, 0), transform.forward, out hitForward, limitXP + thickness, layerMask))
        {
            // we found something but the normal.y might be >= 0.09
            if (hitForward.normal.y <= -0.09f)
            {
                // a ceiling is detected do nothing, that is for the hitUpward
            }
            else if (hitForward.normal.y >= 0.09f)
            {
                // a floor is detected do nothing, that is for the hitDownward
            }
            else
            {
                Debug.Log("wall within range, determine limits to move by");
                // the y value is less than 0.09 which means its a wall
                // must set the limits for the x and z values, forward means maximum value added to x
                // but we could be moving in the -x direction
                // we don't care about y so don't need to account for the height of the character
                Vector3 initialTranslation = hitForward.point - transform.position;
                // we take the hitpoint as the absolute maximum the player can travel in the forward direction
                // toward that point and remove the thickness, thus limit actually means a position, not a translation
                Vector3 playerTranslationVector = initialTranslation - (initialTranslation.normalized * thickness);
                // we now have a translation convert this to limits
                // the vector has x and z components if they are negative
                if(initialTranslation.x >= 0)
                {
                    // moving in the +x direction
                    limitXPt = Mathf.Min(limitXPt, playerTranslationVector.x);
                }
                else
                {
                    limitXNt = Mathf.Max(limitXNt, playerTranslationVector.x);
                }
                if (initialTranslation.z >= 0)
                {
                    // moving in the -x direction
                    limitZPt = Mathf.Min(limitZPt, playerTranslationVector.z);
                }
                else
                {
                    limitZNt = Mathf.Max(limitZNt, playerTranslationVector.z);
                }
            }
            // the limits in the x and z directions regarding what the forward vector finds are set
        }
        else
        {
            //Debug.DrawRay(transform.position, transform.forward * 1000, Color.white);
            //Debug.Log("Did not Hit");
            // there is nothing so do nothing
        }







        // right raycast
        // the right raycast will check if there is a wall beside of the player by its normal
        // if the normal.y >= 0.09 then we have a floor/slippery floor thus we do not react by the right raycast
        // that will be for the downward raycast to check
        RaycastHit hitRight;
        // remember the distance is based off the universalMovementVector
        // we only want the x and z components of the universalMovementVector, x and z limits are identical
        // can use either for distance, and then set the temps based of which is smaller the current temps or the
        // new calculated temps
        // we want the player's thickness included in the raycast
        if (Physics.Raycast(transform.position + new Vector3(0, height, 0), transform.right, out hitRight, limitXP + thickness, layerMask))
        {
            // we found something but the normal.y might be >= 0.09
            if (hitRight.normal.y <= -0.09f)
            {
                // a ceiling is detected do nothing, that is for the hitUpward
            }
            else if (hitRight.normal.y >= 0.09f)
            {
                // a floor is detected do nothing, that is for the hitDownward
            }
            else
            {
                Debug.Log("wall within range, determine limits to move by");
                // the y value is less than 0.09 which means its a wall
                // must set the limits for the x and z values, forward means maximum value added to x
                // but we could be moving in the -x direction
                // we don't care about y so don't need to account for the height of the character
                Vector3 initialTranslation = hitRight.point - transform.position;
                // we take the hitpoint as the absolute maximum the player can travel in the forward direction
                // toward that point and remove the thickness, thus limit actually means a position, not a translation
                Vector3 playerTranslationVector = initialTranslation - (initialTranslation.normalized * thickness);
                // we now have a translation convert this to limits
                // the vector has x and z components if they are negative
                if (initialTranslation.x >= 0)
                {
                    // moving in the +x direction
                    limitXPt = Mathf.Min(limitXPt, playerTranslationVector.x);
                }
                else
                {
                    limitXNt = Mathf.Max(limitXNt, playerTranslationVector.x);
                }
                if (initialTranslation.z >= 0)
                {
                    // moving in the -x direction
                    limitZPt = Mathf.Min(limitZPt, playerTranslationVector.z);
                }
                else
                {
                    limitZNt = Mathf.Max(limitZNt, playerTranslationVector.z);
                }
            }
            // the limits in the x and z directions regarding what the forward vector finds are set
        }
        else
        {
            //Debug.DrawRay(transform.position, transform.forward * 1000, Color.white);
            //Debug.Log("Did not Hit");
            // there is nothing so do nothing
        }






        
        // left raycast
        // the left raycast will check if there is a wall beside of the player by its normal
        // if the normal.y >= 0.09 then we have a floor/slippery floor thus we do not react by the left raycast
        // that will be for the downward raycast to check
        RaycastHit hitLeft;
        // remember the distance is based off the universalMovementVector
        // we only want the x and z components of the universalMovementVector, x and z limits are identical
        // can use either for distance, and then set the temps based of which is smaller the current temps or the
        // new calculated temps
        // we want the player's thickness included in the raycast
        if (Physics.Raycast(transform.position + new Vector3(0, height, 0), -transform.right, out hitLeft, limitXP + thickness, layerMask))
        {
            // we found something but the normal.y might be >= 0.09
            if (hitLeft.normal.y <= -0.09f)
            {
                // a ceiling is detected do nothing, that is for the hitUpward
            }
            else if (hitLeft.normal.y >= 0.09f)
            {
                // a floor is detected do nothing, that is for the hitDownward
            }
            else
            {
                Debug.Log("wall within range, determine limits to move by");
                // the y value is less than 0.09 which means its a wall
                // must set the limits for the x and z values, forward means maximum value added to x
                // but we could be moving in the -x direction
                // we don't care about y so don't need to account for the height of the character
                Vector3 initialTranslation = hitLeft.point - transform.position;
                // we take the hitpoint as the absolute maximum the player can travel in the forward direction
                // toward that point and remove the thickness, thus limit actually means a position, not a translation
                Vector3 playerTranslationVector = initialTranslation - (initialTranslation.normalized * thickness);
                // we now have a translation convert this to limits
                // the vector has x and z components if they are negative
                if (initialTranslation.x >= 0)
                {
                    // moving in the +x direction
                    limitXPt = Mathf.Min(limitXPt, playerTranslationVector.x);
                }
                else
                {
                    limitXNt = Mathf.Max(limitXNt, playerTranslationVector.x);
                }
                if (initialTranslation.z >= 0)
                {
                    // moving in the -x direction
                    limitZPt = Mathf.Min(limitZPt, playerTranslationVector.z);
                }
                else
                {
                    limitZNt = Mathf.Max(limitZNt, playerTranslationVector.z);
                }
            }
            // the limits in the x and z directions regarding what the forward vector finds are set
        }
        else
        {
            //Debug.DrawRay(transform.position, transform.forward * 1000, Color.white);
            //Debug.Log("Did not Hit");
            // there is nothing so do nothing
        }
        






        // right here we can determine if the character is being squished, if limitPt < limitNt then the character
        // is being told to move up and down thus a squish, might not need to use this as we can be careful not to 
        // have platforms move such that squishes are possible

        universalMovementVector.x = Mathf.Min(universalMovementVector.x, limitXPt); // x can't be bigger than limitXPt
        universalMovementVector.x = Mathf.Max(universalMovementVector.x, limitXNt); // x can't be smaller than limitXNt
        universalMovementVector.z = Mathf.Min(universalMovementVector.z, limitZPt); // z can't be bigger than limitZPt
        universalMovementVector.z = Mathf.Max(universalMovementVector.z, limitZNt); // z can't be smaller than limitZNt
        universalMovementVector.y = Mathf.Min(universalMovementVector.y, limitYPt); // y can't be bigger than limitYPt
        universalMovementVector.y = Mathf.Max(universalMovementVector.y, limitYNt); // y can't be smaller than limitYNt
        MoveCharacter(universalMovementVector);



        // a ceiling is defined by any surface with normal.y < -0.09 (5 degrees)
        // a wall is defined by any surface with -0.09 <= normal.y <= 0.09
        // a slippery floor is defined by any surface with 0.09 < normal.y < 0.5 (30 degrees = 60 degree slope)
        // a floor is defined by any surface with 0.5 <= normal.y



    }

    // define later
    void MoveCharacter(Vector3 movement)
    {
        // do not use Time.deltaTime
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

			//CalculateExtraRotationTranslationVector(); // we will check this during the downward ray cast
			//if(extraRotationTranslationVector != Vector3.zero){
                // shift character by what the rotation means in translation
                // universalMovementVector += extraRotationTranslationVector;
                // we do not move the player in any state, only at the end of the fixed update function
			//}
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
			//CalculateExtraRotationTranslationVector();

			//MoveCharacter(universalMovementVector * Time.deltaTime + extraRotationTranslationVector);

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
        // also since in long jump we dont want to change the character's rotation
		//float targetRotation = Mathf.Atan2 (input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
		//transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);

        // we might not need to recalulate the x and z since universalMovementVector.xz are not changing
		universalMovementVector.x = initialLongJumpVelocity.x;
		universalMovementVector.z = initialLongJumpVelocity.y; // z here is actually the y component of this 2D vector
        universalMovementVector.y += gravity;
		universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

		//universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		//controller.Move(universalMovementVectorDelta);

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
		universalMovementVector.y += gravity;
		universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

		//universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		//controller.Move(universalMovementVectorDelta);

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
		universalMovementVector.y += gravity;
		universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

		//universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		//controller.Move(universalMovementVectorDelta);

		if(universalMovementVector.y <= 0.0f){
			characterState = CharacterState.FALLING;
		}

		if(groundPound > 0){
			characterState = CharacterState.GROUND_POUND;
		}
	}

	void FallingState(){
		if(isGrounded){
			characterState = CharacterState.RUNNING; // see if stationary is more appropriate
		}
		else{
			universalMovementVector.y += gravity;
			universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);
			//if(fallingMoveBack.magnitude != 0){
				//universalMovementVector = Vector3.zero;
			//}

			//controller.Move(universalMovementVector * Time.deltaTime /*+ fallingMoveBack*/);
			//universalMovementVectorDelta = universalMovementVector * Time.deltaTime + fallingMoveBack;
			//universalMovementVectorDelta = universalMovementVector * Time.deltaTime + new Vector3(fallingMoveBack.x, 0, fallingMoveBack.z);
			//controller.Move(universalMovementVectorDelta);
			//universalMovementVectorDelta = new Vector3(0, fallingMoveBack.y, 0);
			//controller.Move(universalMovementVectorDelta);

			/*
			universalMovementVectorDelta = new Vector3(moveBack.x, 0, moveBack.z) * slopeSlipSpeed * Time.deltaTime;
			controller.Move(universalMovementVectorDelta);
			
			universalMovementVectorDelta = new Vector3(0, moveBack.y * 20, 0) * slopeSlipSpeed * Time.deltaTime;
			controller.Move(universalMovementVectorDelta);
			*/


			//Debug.Log("not grounded");
			//fallingMoveBack = Vector3.zero;
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
		universalMovementVector = slopeVector * slopeSlipSpeed;


		//universalMovementVectorDelta = new Vector3(moveBack.x, 0, moveBack.z) * slopeSlipSpeed * Time.deltaTime;
		//controller.Move(universalMovementVectorDelta);

		//universalMovementVectorDelta = new Vector3(0, moveBack.y * 20, 0) * slopeSlipSpeed * Time.deltaTime;
		//controller.Move(universalMovementVectorDelta);

		//Debug.Log(moveBack);
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

		//controller.Move(universalMovementVector * Time.deltaTime);
		//universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		//controller.Move(universalMovementVectorDelta);

		characterState = CharacterState.JUMPING_HIGH;
	}

	void RegularJump(){
        // keep the x and z as we want to continue to move
        universalMovementVector.y = initialJumpVelocity;

		//controller.Move(universalMovementVector * Time.deltaTime);
		//universalMovementVectorDelta = universalMovementVector * Time.deltaTime;
		//controller.Move(universalMovementVectorDelta);

		characterState = CharacterState.JUMPING_LOW;
	}
    /*
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
    */
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
    /*
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
		
	}
    */
}
/*
 * {
    public float speed = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            transform.Translate(transform.forward * speed * Time.deltaTime);
        }
        else if(Input.GetAxisRaw("Horizontal") < 0)
        {
            transform.Translate(-transform.forward * speed * Time.deltaTime);
        }

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1.0f, layerMask))
        {
            //Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.yellow);
            //Debug.Log("Did Hit");
            transform.Translate(-transform.forward * (1.0f - (hit.point - transform.position).magnitude));
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * 1000, Color.white);
            Debug.Log("Did not Hit");
        }
    }
}
*/