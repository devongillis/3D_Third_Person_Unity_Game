using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatedCharacterControllerScript : MonoBehaviour
{
    public characterStats playerStats;

    public float maximumPlayerHorizontalVelocity;

    public float verticalRaycastOffset;
    public float verticalCeilingRaycastOffset;
    public float horizontalRaycastOffset;

    Transform cameraT;
    public float distToGround;
    public bool isGrounded = true;
    public float forwardHeightTop = 4f;
    public float forwardHeightMiddle1 = 3.12f;
    public float forwardHeightMiddle2 = 2.38f;
    public float forwardHeightMiddle3 = 1.56f;
    public float forwardHeightBottom = 0.75f;
    public float downwardThickness = 0.0f; // the difference between the ground and the position of the character
    public float upwardThickness = 4.0f;
    public float forwardThickness = 0.5f; // this need to be half the diameter of the total player, keep above 1.0


    // all of these values are the angle of the normal from the vertical plane,clockwise
    public float regularFloorStart = 0;
    public float regularFloorSemiSlipperyFloor = 30; // anything below is normal, above is semi slippery
    public float SemiSlipperyFloorSlipperyFloor = 60; // anything below is semi slippery, above is slippery
    public float SlipperyFloorWall = 80; // anything below is slippery, above is wall
    public float wallCeiling = 100; // anything below is wall, above is ceiling
    public float ceiling = 180;

    public float regularFloorStartArc;
    public float regularFloorSemiSlipperyFloorArc;
    public float semiSlipperyFloorSlipperyFloorArc;
    public float SlipperyFloorWallArc;
    public float wallCeilingArc;
    public float ceilingArc;

    public float arcRatio = (2 * Mathf.PI / 360);




    // a regular floor is defined as any surface with normal.y between 0.866 and 1.0
    public float floorUpperLimitNormalY;// = 1.0f; // <=
    public float floorLowerLimitNormalY;// = 0.866f; // >=
    // a semi slippery floor is defined as any surface with normal.y between 0.5 and 0.866
    public float semiSlipperyFloorUpperLimitNormalY; // = 0.866f; // <
    public float semiSlipperyFloorLowerLimitNormalY; // = 0.5f; // <
    // a slippery floor is defined as any surface with normal.y between 0.174 and 0.5
    public float slipperyFloorUpperLimitNormalY;// = 0.5f; // <
    public float slipperyFloorLowerLimitNormalY;// = 0.174f; // >=
    // a wall is defined as any surface with normal.y between -0.174 and 0.174
    public float wallUpperLimitNormalY;// = 0.174f; // <=
    public float wallLowerLimitNormalY;// = -0.174f; // >=
    // a ceiling is defined as any surface with normal.y between -1.0 and -0.174
    public float ceilingUpperLimitNormalY;// = -0.174; // <=
    // -1.0f is the minimum value for normal.y so we don't need to use this value for ceilings



    public float slopeDownwardCheckDistance = 0.0f; // set in start, this is for checking if the player should be truncated to the floor by the maximum slope value
    //public float slopeWallMinimumForwardSpeed = 0.0f; // set in start

    // Bit shift the index of the layer (8) to get a bit mask
    public int layerMask = ~(1 << 8);
    // This would cast rays only against colliders in layer 8.

    private Animator anim;

    public CharacterState characterState = CharacterState.STATIONARY;

    public float walkSpeed = 0.15f;
    public float runSpeed = 0.3f;
    public float slipSpeed = 0.05f;

    public bool noFriction = false; // if on a no friction floor then player slides across like ice
    public float slipFactor = 100f;


    // swimming physics
    public float swimmingCoastSpeed = 0.05f;
    public float swimmingCoastGroundedSpeed = 0.07f;
    public Vector2 swimmingStrokeForwardInitialSpeed = new Vector2(0.15f, 0.1f);
    public float swimmingStrokeUpwardInitialSpeed = 0.20f;
    public float swimmingGravityIncrement = -0.01f;
    public float swimmingGravitySinkIncrement = -0.05f;
    public float swimmingMaxFallSpeed = -0.1f;
    public float swimmingJumpSpeed = 0.1f;
    public float swimmingTurnSmoothTime = 1.0f;
    public float swimmingSpeedSmoothTime = 3.0f;
    public float swimBodyHeight; // the top of the swimming medium in world quardinates
    public float swimSpeedDecay = 0.97f;

    // the current gravity value is contained in the universal vector, gravity increment is added to its y component
    public float gravityIncrement = -0.01f; // this is the amount each frame the gravity value changes
    public float maxFallVelocity = -1.0f; // the maximum value to displace the character

    public float groundDistanceCheck = 0.1f;

    public float turnSmoothTime = 1.0f;
    float turnSmoothVelocity = 1.0f;

    public float JumpingSpeedSmoothTime = 0.1f; // also used for falling
    public float runningSpeedSmoothTime = 0.5f;
    float speedSmoothVelocity;


    float currentSpeed;
    bool touchingWall = false;


    public float initialJumpVelocity = 0.35f;
    public float initialHighJumpVelocity = 0.45f;
    public Vector2 initialLongJumpVelocity = new Vector2(0.6f, 0.25f); // length and height




    // input variables
    Vector2 input;
    Vector2 inputRaw;

    float jump;
    float specialJump;
    float groundPound;
    float speedUp;
    float strokeForward;
    // used to make sure button is applied when clicked and not when just held down
    bool jumpEnable = false;
    bool groundPoundEnable = false;
    bool strokeForwardEnable = false;



    // secondary input variables
    public bool running;

    public bool fallingWithInput = true;


    // this is the vector by which the player moves, all inputs and movements are added to this vector and then
    // this vector is applied to the player each update, so to jump you apply a single up value and on each frame
    // that value will be used to move the player up, and gravity will reduce it
    // when grounded the y component will be set to zero
    public Vector3 universalMovementVector = new Vector3(0, 0, 0);
    public Vector3 platformRotationVector = new Vector3(0, 0, 0);

    public bool InputAccepted = true;
    public int groundPoundTimer = 0;
    public int groundPoundTimeLimit = 60; // frames


    public int fallDamageTimer = 60;
    public int fallDamageTime = 60;
    public int injurySquishTimer = 100;
    public int injurySquishTime = 100;

    public int hangStateTimer = 0; // upon entering the hang state all input is ignored for a little bit
    public int hangStateTime = 60;
    public bool allowedToEdgeGrab = true; // this is set false when we don't want to edge grab again until grounded
    public int climbStateTimer = 0;
    public int climbStateTime = 60;


    public int poleLeaveTimer = 0;
    public int PoleLeaveTime = 5;
    public bool allowedToGrabPole = true; // set to false if player just lets go, and set to true upon being grounded
    public float poleClimbRate = 0.1f;
    public float poleDescendRate = -0.5f;
    public float poleRotateSpeed = 1f;
    public Vector2 poleInitialJumpVelocity = new Vector2(0.3f, 0.3f);

    public int injuryKnockbackTimer = 0;
    public int injuryKnockbackTime = 60;
    public float explosionKnockBackVelocity = 0.4f; // keep this below the high jump velocity
    public float explosionKnockBackDecay = 0.97f;


    // these values are reset every update call
    bool xzModified = false;
    bool yModified = false;
    Vector3 XZ;
    Vector3 Y;
    float transX;
    float transXM;
    float transY;
    float transYUp;
    bool wasGrounded = false;
    bool slideApplied = false;

    //float floorNormalY = 1.0f;
    Vector3 floorNormal = new Vector3(0, 1, 0);

    float actualMovementFactor = 0.0f;
    Vector3 theForward;

    // Start is called before the first frame update
    void Start()
    {
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        Application.targetFrameRate = 30;
        layerMask = ~layerMask;
        cameraT = Camera.main.transform;

        playerStats = transform.GetComponent<characterStats>();


        regularFloorStartArc = regularFloorStart * arcRatio;
        regularFloorSemiSlipperyFloorArc = regularFloorSemiSlipperyFloor * arcRatio;
        semiSlipperyFloorSlipperyFloorArc = SemiSlipperyFloorSlipperyFloor * arcRatio;
        SlipperyFloorWallArc = SlipperyFloorWall * arcRatio;
        wallCeilingArc = wallCeiling * arcRatio;
        ceilingArc = ceiling * arcRatio;


        // a regular floor is defined as any surface with normal.y between   1.000  and  0.707
        // a slippery floor is defined as any surface with normal.y between  0.707  and  0.174
        // a wall is defined as any surface with normal.y between            0.174  and -0.174
        // a ceiling is defined as any surface with normal.y between        -0.174  and -1.000

        floorUpperLimitNormalY = 1.0f; // floor is <= 1.0
        floorLowerLimitNormalY = Mathf.Sin((90 - regularFloorSemiSlipperyFloor) * arcRatio); // floor is >= 0.866

        semiSlipperyFloorUpperLimitNormalY = floorLowerLimitNormalY; // semi slippery is < 0.866f
        semiSlipperyFloorLowerLimitNormalY = Mathf.Sin((90 - SemiSlipperyFloorSlipperyFloor) * arcRatio); // semi slippery is >= 0.5f

        slipperyFloorUpperLimitNormalY = semiSlipperyFloorLowerLimitNormalY; // slippery is < 0.5
        slipperyFloorLowerLimitNormalY = Mathf.Sin((90 - SlipperyFloorWall) * arcRatio); // slippery is > 0.174

        wallUpperLimitNormalY = slipperyFloorLowerLimitNormalY; // wall is <= 0.174
        wallLowerLimitNormalY = -wallUpperLimitNormalY; // wall is >= -0.174

        ceilingUpperLimitNormalY = Mathf.Sin((90 - wallCeiling) * arcRatio); // ceiling is < -0.174

        slopeDownwardCheckDistance = runSpeed * Mathf.Tan(SlipperyFloorWallArc); // the distance downward to check for the slope the player is running down on

        maximumPlayerHorizontalVelocity = Mathf.Max(runSpeed, initialLongJumpVelocity.x);
        // not too sure but given a slope will move us back by a maxium of the current walk speed even if walk with
        // the slope, if we set that value to run speed the the fastest horizontal movement is now at 2 * runSpeed
        // when the player runs down a 60 degree slope, but this value is only used to get a vertical raycast
        // which is a fail safe value for when we run UP a slope, thus since we are moving down when exploiting the
        // slope max horizontal speed we are actually moving down and don't need to worry about this
        //maximumPlayerHorizontalVelocity = Mathf.Max(runSpeed * 2, initialLongJumpVelocity.x);

        horizontalRaycastOffset = -maxFallVelocity * Mathf.Tan((90 - SlipperyFloorWall) * arcRatio) + 0.3f; // should add a little extra like 0.01f
        verticalRaycastOffset = maximumPlayerHorizontalVelocity * Mathf.Tan(SlipperyFloorWall * arcRatio) + 0.0f; // should add a little extra like 0.01f
        verticalCeilingRaycastOffset = maximumPlayerHorizontalVelocity * Mathf.Tan((180 - wallCeiling) * arcRatio) + 0.0f; // should add a little extra like 0.01f

        // the raycasts start at these offsets but must also extend beyond by the amount the player can move

        anim = gameObject.GetComponentInChildren<Animator>();
        anim.SetTrigger("StationaryState");
        anim.SetFloat("speed", 0.0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CollectAndConfigureInputs(); // inputs are collected
        ExecuteState(); // execute actions based of current state of player

        // DO NOT USE DELTA TIME, only use motion as frame by frame Application.targetFrameRate = 30

        // reset these variables for each update

        wasGrounded = isGrounded;
        isGrounded = false;
        slideApplied = false;
        touchingWall = false;
        xzModified = false;
        yModified = false;

        XZ = new Vector3(universalMovementVector.x, 0, universalMovementVector.z);
        Y = new Vector3(0, universalMovementVector.y, 0);
        
        transX = forwardThickness + (horizontalRaycastOffset - forwardThickness) + 0.01f;
        transXM = XZ.magnitude + transX;
        transY = -(Y.y - downwardThickness) + (verticalRaycastOffset - downwardThickness) + 0.1f; // 0.1f check for bigger slope values
        transYUp = verticalCeilingRaycastOffset + Y.y + 0.01f; // 0.1f check for bigger slope values
        
        ExecuteNoDirectionRaycasts();
        ExecuteMovementForwardRaycasts();
        ExecuteMovementUpwardRaycasts();
        ExecuteMovementDownwardRaycasts();

        // only if not modified will this function move the player in that coordinate
        MoveCharacter(universalMovementVector + platformRotationVector, xzModified, yModified);
        platformRotationVector = Vector3.zero;

        if (isGrounded)
        {
            allowedToEdgeGrab = true;
            allowedToGrabPole = true;
        }
        // do not combine these, other conditions might be added to allow edge collision
        if (allowedToEdgeGrab)
        {
            EdgeCollision();
        }

        if(characterState != CharacterState.FALLING && characterState != CharacterState.JUMPING_LONG)
        {
            //Debug.Log("reset");
            fallDamageTimer = fallDamageTime;
        }

        //Debug.DrawRay(transform.position + new Vector3(0, forwardHeightTop, 0), transform.forward * 10, Color.red);
    }

    void CollectAndConfigureInputs()
    {
        // Speed Up and Stroke Forward

        if (!InputAccepted)
        {
            // act like no inputs being made
            input = new Vector2(0, 0);
            inputRaw = new Vector2(0, 0);

            // buttons
            jump = 0;
            specialJump = 0;
            groundPound = 0;
            speedUp = 0;
            strokeForward = 0;
        }
        else
        {
            // inputs are allowed
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            inputRaw = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = input.normalized;

            specialJump = Input.GetAxisRaw("Special Jump"); // special jump is a pressed button not a clicked button
            speedUp = Input.GetAxisRaw("Speed Up");



            // buttons
            if (Input.GetAxisRaw("Jump") == 0)
            {
                // we are currently not holding down the jump button
                jumpEnable = true;
            }
            if (jumpEnable && Input.GetAxisRaw("Jump") > 0)
            {
                jump = Input.GetAxisRaw("Jump");
                jumpEnable = false;
            }
            else
            {
                jump = 0;
            }

            if (Input.GetAxisRaw("Ground Pound") == 0)
            {
                // we are currently not holding down the ground pound button
                groundPoundEnable = true;
            }
            if (groundPoundEnable && Input.GetAxisRaw("Ground Pound") > 0)
            {
                groundPound = Input.GetAxisRaw("Ground Pound");
                groundPoundEnable = false;
            }
            else
            {
                groundPound = 0;
            }

            if (Input.GetAxisRaw("Stroke Forward") == 0)
            {
                // we are currently not holding down the stroke forward button
                strokeForwardEnable = true;
            }
            if (strokeForwardEnable && Input.GetAxisRaw("Stroke Forward") > 0)
            {
                strokeForward = Input.GetAxisRaw("Stroke Forward");
                strokeForwardEnable = false;
            }
            else
            {
                strokeForward = 0;
            }

        }
    }

    void ExecuteState()
    {
        if (characterState == CharacterState.STATIONARY)
        {
            StationaryState();
        }
        else if (characterState == CharacterState.RUNNING)
        {
            RunningState();
        }
        else if (characterState == CharacterState.JUMPING_LOW)
        {
            LowJumpingState();
        }
        else if (characterState == CharacterState.JUMPING_LONG)
        {
            LongJumpingState();
        }
        else if (characterState == CharacterState.JUMPING_HIGH)
        {
            HighJumpingState();
        }
        else if (characterState == CharacterState.FALLING)
        {
            FallingState();
        }
        else if (characterState == CharacterState.FALLING_NO_CONTROL)
        {
            FallingNoControlState();
        }
        else if (characterState == CharacterState.SLIPPING_WITH_CONTROL)
        {
            SlippingWithControlState();
        }
        else if (characterState == CharacterState.SLIPPING_NO_CONTROL)
        {
            SlippingNoControlState();
        }
        else if (characterState == CharacterState.GROUND_POUND)
        {
            GroundPoundState();
        }
        else if (characterState == CharacterState.SWIMMING)
        {
            SwimmingState();
        }
        else if (characterState == CharacterState.EDGE_GRAB_HANG)
        {
            HangingState();
        }
        else if (characterState == CharacterState.EDGE_GRAB_CLIMB)
        {
            ClimbEdgeState();
        }
        else if (characterState == CharacterState.POLE_GRABBING)
        {
            PoleGrabbingState();
        }
        else if (characterState == CharacterState.INJURY_KNOCKBACK)
        {
            InjuryKnockbackState();
        }
        else if (characterState == CharacterState.INJURY_SQUISH)
        {
            InjurySquishedState();
        }
    }

    void ExecuteNoDirectionRaycasts()
    {
        Vector3 XZNormalizedForward = XZ.normalized;
        
        if (XZNormalizedForward == Vector3.zero)
        {
            XZNormalizedForward = transform.forward;
        }

        Vector3 XZNormalizedRight = Quaternion.Euler(0, 90, 0) * XZNormalizedForward;

        if (!wasGrounded || XZ == Vector3.zero)
        {
            RaycastHit noDirection;

            if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightBottom, 0) + XZNormalizedForward * (horizontalRaycastOffset - forwardThickness), -XZNormalizedForward, out noDirection, transX, layerMask))
            {
                WallCollisionMoveBack(noDirection);
            }
            if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightBottom, 0) - XZNormalizedRight * (horizontalRaycastOffset - forwardThickness), XZNormalizedRight, out noDirection, transX, layerMask))
            {
                WallCollisionMoveBack(noDirection);
            }
            if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightBottom, 0) + XZNormalizedRight * (horizontalRaycastOffset - forwardThickness), -XZNormalizedRight, out noDirection, transX, layerMask))
            {
                WallCollisionMoveBack(noDirection);
            }
        }
    }

    void ExecuteMovementForwardRaycasts()
    {

        Vector3 XZNormalizedForward = XZ.normalized;
        if (XZNormalizedForward == Vector3.zero)
        {
            XZNormalizedForward = transform.forward;
        }

        RaycastHit hitForwardBottom;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightBottom, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), XZNormalizedForward, out hitForwardBottom, transXM, layerMask))
        {
            WallCollisionSlide(hitForwardBottom);
        }

        RaycastHit hitForwardTop;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), XZNormalizedForward, out hitForwardTop, transXM, layerMask))
        {
            WallCollisionSlide(hitForwardTop);
        }

        RaycastHit hitForwardMiddle1;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightMiddle1, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), XZNormalizedForward, out hitForwardMiddle1, transXM, layerMask))
        {
            WallCollisionSlide(hitForwardMiddle1);
        }

        RaycastHit hitForwardMiddle2;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightMiddle2, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), XZNormalizedForward, out hitForwardMiddle2, transXM, layerMask))
        {
            WallCollisionSlide(hitForwardMiddle2);
        }

        RaycastHit hitForwardMiddle3;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightMiddle3, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), XZNormalizedForward, out hitForwardMiddle3, transXM, layerMask))
        {
            WallCollisionSlide(hitForwardMiddle3);
        }
    }

    void ExecuteMovementUpwardRaycasts()
    {
        RaycastHit hitUpward;
        if (Physics.Raycast(transform.position + new Vector3(0, upwardThickness - verticalCeilingRaycastOffset, 0), transform.up, out hitUpward, transYUp, layerMask))
        {
            CeilingCollision(hitUpward); // double check if we want to know if ymodified
        }
    }

    void ExecuteMovementDownwardRaycasts()
    {

        RaycastHit hitDownward;
        float distanceDownward;

        if (characterState == CharacterState.STATIONARY || characterState == CharacterState.RUNNING || characterState == CharacterState.SLIPPING_WITH_CONTROL || characterState == CharacterState.SLIPPING_NO_CONTROL) {
            distanceDownward = slopeDownwardCheckDistance + verticalRaycastOffset + 0.01f;
        }
        else
        {
            distanceDownward = transY;
        }

        if (Physics.Raycast(transform.position + new Vector3(0, (verticalRaycastOffset - downwardThickness) + 0.01f, 0), -transform.up, out hitDownward, distanceDownward, layerMask))
        {
            FloorCollision(hitDownward); // double check if we want to know if ymodified
        }
        else
        {
            isGrounded = false;
        }
        if (!isGrounded)
        {
            if (characterState != CharacterState.EDGE_GRAB_HANG && characterState != CharacterState.EDGE_GRAB_CLIMB)
            {
                transform.parent = null;
            }
        }
    }
    
    void WallCollisionMoveBack(RaycastHit hit)
    {
        if (hit.normal.y >= wallLowerLimitNormalY && hit.normal.y <= wallUpperLimitNormalY)
        {
            Vector3 direction = (transform.position - hit.point);
            direction.y = 0;
            direction.Normalize();
            Vector3 translator = hit.point + direction * (forwardThickness + 0.011f);

            transform.position = new Vector3(translator.x, transform.position.y, translator.z);
            touchingWall = true;
        }
        else
        {
            // a surface was found but it does not meet the definition of a wall
        }
    }

    void WallCollisionSlide(RaycastHit hit)
    {
        if (hit.normal.y >= wallLowerLimitNormalY && hit.normal.y <= wallUpperLimitNormalY)
        {
            Vector2 forward = new Vector2(transform.forward.x, transform.forward.z);
            Vector2 normal = new Vector2(hit.normal.x, hit.normal.z);
            float angle = Vector2.Angle(forward, normal);
            RaycastHit pinch;
            if (/*floorNormalY < floorLowerLimitNormalY ||*/ slideApplied || Physics.Raycast(hit.point, hit.normal, out pinch, forwardThickness * 2 + 0.01f, layerMask) || angle > 135)
            {
                // we hit a corner pinch
                Vector3 direction = (transform.position - hit.point);
                direction.y = 0;
                direction.Normalize();
                Vector3 translator = hit.point + direction * (forwardThickness + 0.011f);

                transform.position = new Vector3(translator.x, transform.position.y, translator.z);
                universalMovementVector.x = 0;
                universalMovementVector.z = 0;
                
                xzModified = true;
                if (!isGrounded && currentSpeed > walkSpeed)
                {
                    universalMovementVector.y = Mathf.Min(universalMovementVector.y, 0.0f);
                }
                currentSpeed = 0.0f;
                touchingWall = true;
            }
            else
            {
                slideApplied = true;
                // player is allowed to slide
                Vector3 hitNormalxz = new Vector3(hit.normal.x, 0, hit.normal.z);
                hitNormalxz.Normalize();

                Vector3 translator = hit.point + hitNormalxz * (forwardThickness + 0.02f);

                transform.position = new Vector3(translator.x, transform.position.y, translator.z);
                xzModified = true;
                if (!isGrounded && currentSpeed > walkSpeed)
                {
                    universalMovementVector.y = Mathf.Min(universalMovementVector.y, 0.0f);
                }
            }
        }
        else
        {
            // a surface was found but it does not meet the definition of a wall
        }
    }
    
    void CeilingCollision(RaycastHit hit)
    {
        if (hit.normal.y < ceilingUpperLimitNormalY)
        {
            // a ceiling has been hit
            float translator = hit.point.y - upwardThickness - 0.02f;
            transform.position = new Vector3(transform.position.x, translator, transform.position.z);
            yModified = true;
            universalMovementVector.y = 0;
            universalMovementVector.x = hit.normal.x * 0.1f;
            universalMovementVector.z = hit.normal.z * 0.1f;
        }
    }

    void FloorCollision(RaycastHit hit)
    {
        //Debug.Log("state before " + characterState);
        if (hit.normal.y > slipperyFloorLowerLimitNormalY) // the boundary between wall and slippery floor
        {
            // we have found a floor below we must truncate to it
            if(characterState == CharacterState.JUMPING_HIGH || characterState == CharacterState.JUMPING_LONG || characterState == CharacterState.JUMPING_LOW)
            {
                characterState = CharacterState.RUNNING;
                //Debug.Log("floor to run");
                anim.SetTrigger("RunningState");
                universalMovementVector.y = 0;
            }
            float translator = hit.point.y + downwardThickness;
            transform.position = new Vector3(transform.position.x, translator, transform.position.z);
            yModified = true;
            isGrounded = true;
            if (hit.transform.tag == "rotatingPlatform")
            {
                transform.parent = hit.transform;
            }
            else if (hit.transform.tag == "switch" && characterState == CharacterState.GROUND_POUND)
            {
                hit.collider.gameObject.GetComponent<buttonCollapse>().Collapse();
            }
            else if (hit.transform.tag == "movingPlatform")
            {
                transform.parent = hit.transform;
            }
            else if (hit.transform.tag == "noFriction")
            {
                noFriction = true;
            }

            //floorNormalY = hit.normal.y;
            floorNormal = hit.normal;
            Vector3 hitNormalXZ = new Vector3(hit.normal.x, 0, hit.normal.z);
            RaycastHit ray;
            bool useRegFloor = false;
            if (Physics.Raycast(transform.position, hitNormalXZ, out ray, 1.0f, layerMask))
            {
                useRegFloor = true;
            }

            if (floorNormal.y >= semiSlipperyFloorUpperLimitNormalY || useRegFloor)
            {
                // regular floor, no resistance
                if(characterState == CharacterState.SLIPPING_WITH_CONTROL || characterState == CharacterState.SLIPPING_NO_CONTROL)
                {
                    characterState = CharacterState.STATIONARY;
                    anim.SetTrigger("StationaryState");
                }
            }
            else if (floorNormal.y < semiSlipperyFloorUpperLimitNormalY && floorNormal.y > semiSlipperyFloorLowerLimitNormalY)
            {
                if (characterState == CharacterState.SLIPPING_WITH_CONTROL || characterState == CharacterState.SLIPPING_NO_CONTROL)
                {
                    characterState = CharacterState.STATIONARY;
                    anim.SetTrigger("StationaryState");
                }

                float reductionFactor;
                // within the range of a resistive floor
                reductionFactor = (floorNormal.y - semiSlipperyFloorLowerLimitNormalY) / (semiSlipperyFloorUpperLimitNormalY - semiSlipperyFloorLowerLimitNormalY);
                // we need to limit the player's movement with respect to the slope the player is on
                // we first limit the x and z by the general slope so he doesn't sky rocket up
                // then we must deduct the uni vector by the slopes xz in respect to the slopes y
                // the amount we deduct by is proportional to the xz of the slope, the y of the slope
                // and how much the player is trying to walk up the slope

                // the reduction factor takes care of the sky rocket effect it but is doesn't factor how hard a steep
                // slope is to climb
                Vector2 slopeXZ = new Vector2(hit.normal.x, hit.normal.z);
                Vector2 slopeXZNormalized = slopeXZ.normalized;
                Vector2 uniXZ = new Vector2(XZ.x, XZ.z);
                float uniXZMag = uniXZ.magnitude;
                Vector2 slopeXZMag = slopeXZNormalized * uniXZMag;

                float dot = Vector2.Dot(uniXZ.normalized, slopeXZ.normalized);
                if (dot <= 0)
                {
                    dot *= -1;
                    float factor = 1 - reductionFactor;
                    Vector2 deduction = slopeXZMag * dot * factor;
                    universalMovementVector += new Vector3(deduction.x, 0, deduction.y);
                }
                else
                {
                    // we are not moving against the slope do nothing
                }
            }
            else
            {
                // we are on a slippery floor but we can't apply the motion because the universal movement vector has
                // used by the wall raycasts and thus slipping can cause us to clip between the walls we slip into
                // by having us slip with a different uni vector than what was used for the walls
                // we should save the hit.normal, and apply it in the slipping state then we can raycast for walls

                if (Vector2.Dot(new Vector2(transform.forward.x, transform.forward.z), new Vector2(floorNormal.x, floorNormal.z)) <= 0)
                {
                    // player was trying to move up the slope
                    float targetRotation = Mathf.Atan2(-floorNormal.x, -floorNormal.z) * Mathf.Rad2Deg;// + cameraT.eulerAngles.y;
                    transform.eulerAngles = Vector3.up * targetRotation;
                    if (characterState != CharacterState.SLIPPING_NO_CONTROL)
                    {
                        characterState = CharacterState.SLIPPING_NO_CONTROL;
                        anim.SetTrigger("SlippingNoControl");
                        //anim.SetFloat("angle", (90 - Mathf.Asin(floorNormal.y) * arcRatio)/80);
                    }
                    Debug.Log(floorNormal.y);
                    anim.SetFloat("angle", (90 - Mathf.Asin(floorNormal.y) / arcRatio) / 80);
                }
                else
                {
                    // player was trying to move down the slope
                    float targetRotation = Mathf.Atan2(floorNormal.x, floorNormal.z) * Mathf.Rad2Deg;// + cameraT.eulerAngles.y;
                    transform.eulerAngles = Vector3.up * targetRotation;
                    if (characterState != CharacterState.SLIPPING_WITH_CONTROL)
                    {
                        characterState = CharacterState.SLIPPING_WITH_CONTROL;
                        anim.SetTrigger("SlippingWithControl");
                    }
                }
            }
        }
        else
        {
            isGrounded = false;
        }
        //Debug.Log("state after " + characterState);
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

    void EdgeCollision()
    {
        if (universalMovementVector.y <= 0 && !isGrounded)
        {
            if (input.magnitude > 0.5f)
            {
                // we are falling and trying to move forward, edge grabbing is allowed
                RaycastHit hitEdgeForward;
                RaycastHit hitEdgeDownward;

                if (Physics.Raycast(transform.position + transform.forward * (1 + forwardThickness) + new Vector3(0, forwardHeightTop - maxFallVelocity, 0), -transform.up, out hitEdgeDownward, -maxFallVelocity, layerMask))
                {
                    if (hitEdgeDownward.normal.y >= 1 - 0.09f)
                    {
                        if (hitEdgeDownward.transform.tag != "notEdge")
                        {
                            if (Physics.Raycast(new Vector3(transform.position.x, hitEdgeDownward.point.y - 0.1f, transform.position.z), transform.forward, out hitEdgeForward, forwardThickness + 0.5f, layerMask))
                            {
                                // we struck a wall that meets the first definition of an edge
                                if (hitEdgeForward.normal.y >= -0.09f && hitEdgeForward.normal.y <= 0.09f)
                                {
                                    // we struck an edge and a wall that works
                                    characterState = CharacterState.EDGE_GRAB_HANG;
                                    anim.SetTrigger("EdgeGrabHang");
                                    anim.SetFloat("speed", 0.5f);
                                    universalMovementVector = Vector3.zero;
                                    float distanceBack = forwardThickness;// + 0.55f;
                                    float posYOffset = 0.0f;
                                    // need to truncate the player to the wall in a defined space to fit the animation
                                    float posX = hitEdgeForward.point.x + hitEdgeForward.normal.x * distanceBack;
                                    float posZ = hitEdgeForward.point.z + hitEdgeForward.normal.z * distanceBack;
                                    float posY = hitEdgeDownward.point.y - forwardHeightTop + posYOffset;
                                    transform.position = new Vector3(posX, posY, posZ);

                                    hangStateTimer = hangStateTime;
                                    allowedToEdgeGrab = false;
                                    transform.forward = new Vector3(-hitEdgeForward.normal.x, 0, -hitEdgeForward.normal.z);
                                    if (hitEdgeDownward.transform.tag == "rotatingPlatform" || hitEdgeDownward.transform.tag == "movingPlatform")
                                    {
                                        transform.parent = hitEdgeDownward.transform;
                                    }
                                    // now we need to create states for edge grabbing and where to put the character when climbing up
                                    // also if player lets go of edge then the player needs to disable edge grab until grounded again
                                    // in edge grab state we should be able to pull up or let go, maybe even shift to the side if more edge
                                    // in that direction exists
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void StationaryState()
    {
        if (noFriction)
        {
            universalMovementVector = (universalMovementVector * slipFactor) / (slipFactor + 1);
            noFriction = false;
        }
        else
        {
            universalMovementVector = Vector3.zero;
            currentSpeed = 0.0f;
        }

        if (!isGrounded)
        {
            // we are in the stationary state but the floor is not present, (got pushed off the edge?)
            // we swap the state and skip the input
            characterState = CharacterState.FALLING;
            anim.SetTrigger("Falling");
        }
        else
        {
            if (input != Vector2.zero)
            {
                // we have input for running
                // must exit the stationary state
                characterState = CharacterState.RUNNING;
                //Debug.Log("stationary to run");
                anim.SetTrigger("RunningState");
                // notice we can swap into and then out of running state if horizontal and jump
                // input, this means the character will jump rather than start running from
                // stationary pose, which means the character can't do a long jump from stationary
            }
            // now check if a jump has been called
            // notice how we go to a jump state rather than running state if both running and jumping
            if (jump > 0 && isGrounded)
            {
                // later come up with a keyboard click tester (don't let holding down button work as jump input)
                // now determine what kind of jump
                if (specialJump > 0)
                {
                    // since we are in the stationary state we will do a high jump
                    HighJump();
                }
                else
                {
                    RegularJump();
                }
            }
            anim.SetFloat("speed", 0.0f);
        }
    }

    void RunningState()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("RunningState")) // might have to do this for every state
        {
            //Debug.Log("fucker");
            anim.SetTrigger("RunningState");
        }
        // running covers any horizontal movement, so check speed to do a long jump
        if (!isGrounded)
        {
            // we are in the running state but no ground is present, we swap the state
            // and skip the input
            characterState = CharacterState.FALLING;
            anim.SetTrigger("Falling");
        }
        else
        {

            if (input != Vector2.zero)
            {
                // all this is to rotate the character in the desired running direction
                float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
                transform.eulerAngles = Vector3.up * targetRotation;// Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
            }
            else
            {
                // no input so we move back to stationary
                characterState = CharacterState.STATIONARY;
                anim.SetTrigger("StationaryState");
            }

            // determine the speed at which to move the player
            float targetSpeed = ((running || speedUp > 0) ? runSpeed : walkSpeed) * input.magnitude * floorNormal.y;

            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, runningSpeedSmoothTime);
            if (noFriction)
            {
                universalMovementVector = (universalMovementVector * slipFactor + transform.forward * currentSpeed) / (slipFactor + 1);
                universalMovementVector.y = 0;
                noFriction = false;
            }
            else
            {
                universalMovementVector = transform.forward * currentSpeed;
            }

            if (jump > 0)
            {
                // later come up with a keyboard click tester (don't let holding down button work as jump input)
                // now determine what kind of jump
                if (specialJump > 0)
                {
                    if ((running || speedUp > 0) && !touchingWall)
                    {
                        // since we are in the running state we will do a high jump
                        LongJump();
                    }
                    else
                    {
                        HighJump(); // later we should restrict this to the stationary state
                    }
                }
                else
                {
                    RegularJump();
                }
            }
            //Debug.Log("hello" + (currentSpeed / runSpeed) * (1 / Mathf.Sqrt(1 - floorNormalY * floorNormalY)));
            //Debug.Log("hello " + (currentSpeed / runSpeed) / floorNormalY);
            //anim.SetFloat("speed", input.magnitude); // divide by reductionFactor
            anim.SetFloat("speed", (currentSpeed / runSpeed) / floorNormal.y); // divide by reductionFactor
        }
    }

    void LongJumpingState()
    {
        // we dont enter falling state with long jump, because falling state lets us change the projectory
        // also since in long jump we dont want to change the character's rotation
        universalMovementVector.y += gravityIncrement;
        universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);
        if(universalMovementVector.y <= 0.0f)
        {
            fallingWithInput = false;
            characterState = CharacterState.FALLING;
        }
    }

    void HighJumpingState()
    {
        if (input.magnitude > 0)
        {
            float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        }
        float targetSpeed = ((running || speedUp > 0) ? runSpeed : walkSpeed) * input.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, JumpingSpeedSmoothTime);

        universalMovementVector.x = transform.forward.x * currentSpeed;
        universalMovementVector.z = transform.forward.z * currentSpeed;


        universalMovementVector.y += gravityIncrement;
        universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

        if (universalMovementVector.y <= 0.0f)
        {
            characterState = CharacterState.FALLING;
            anim.SetTrigger("Falling");
        }

        if (groundPound > 0)
        {
            characterState = CharacterState.GROUND_POUND;
            anim.SetTrigger("GroundPound");
            universalMovementVector = Vector3.zero;
        }
    }

    void LowJumpingState()
    {
        // we are using this as we enter the low jump from a pole
        if (poleLeaveTimer > 0)
        {
            poleLeaveTimer--;
        }

        if (input.magnitude > 0)
        {
            float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        }

        float targetSpeed = ((running || speedUp > 0) ? runSpeed : walkSpeed) * input.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, JumpingSpeedSmoothTime);

        universalMovementVector.x = transform.forward.x * currentSpeed;
        universalMovementVector.z = transform.forward.z * currentSpeed;


        universalMovementVector.y += gravityIncrement;
        universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

        if (universalMovementVector.y <= 0.0f)
        {
            characterState = CharacterState.FALLING;
            anim.SetTrigger("Falling");
        }

        if (groundPound > 0)
        {
            characterState = CharacterState.GROUND_POUND;
            anim.SetTrigger("GroundPound");
            universalMovementVector = Vector3.zero;
        }
    }

    void FallingState()
    {
        if (isGrounded)
        {
            fallingWithInput = true;
            if (fallDamageTimer <= 0)
            {
                // the player must take fall damage
                Debug.Log("squished");
                characterState = CharacterState.INJURY_SQUISH;
                anim.SetTrigger("InjurySquishUpright");
                universalMovementVector = Vector3.zero;
            }
            else
            {
                characterState = CharacterState.RUNNING; // see if stationary is more appropriate
                anim.SetTrigger("RunningState");
            }
            fallDamageTimer = fallDamageTime;
        }
        else
        {
            fallDamageTimer--;
            if (fallingWithInput)
            {
                if (input.magnitude > 0)
                {
                    float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
                    transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
                }

                float targetSpeed = ((running || speedUp > 0) ? runSpeed : walkSpeed) * input.magnitude;
                currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, JumpingSpeedSmoothTime);

                universalMovementVector.x = transform.forward.x * currentSpeed;
                universalMovementVector.z = transform.forward.z * currentSpeed;
            }

            universalMovementVector.y += gravityIncrement;
            universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

            if (groundPound > 0)
            {
                characterState = CharacterState.GROUND_POUND;
                anim.SetTrigger("GroundPound");
                universalMovementVector = Vector3.zero;
            }
        }
    }

    void SlippingWithControlState()
    {
        // this is a dead state we have no input only letting the slope push us down with the raycasts
        if (!isGrounded)
        {
            characterState = CharacterState.FALLING;
            anim.SetTrigger("Falling");
        }
        else
        {
            Vector3 a = Vector3.zero;
            if (input.magnitude == 0)
            {
                theForward = Vector3.SmoothDamp(theForward, Vector3.zero, ref a, 1.0f);
            }
            else
            {
                float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
                theForward = Quaternion.AngleAxis(targetRotation, Vector3.up) * Vector3.forward;
                theForward.Normalize();
            }

            float targetSpeed = ((running || speedUp > 0) ? runSpeed : walkSpeed) * input.magnitude;
            
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, 1.0f);
            universalMovementVector = theForward * currentSpeed;
            universalMovementVector.y = 0;
            
            // we are on a slippery floor but we can't apply the motion because the universal movement vector has
            // used by the wall raycasts and thus slipping can cause us to clip between the walls we slip into
            // by having us slip with a different uni vector than what was used for the walls
            // we should save the hit.normal, and apply it in the slipping state then we can raycast for walls

            float slopeSpeed = (1 - floorNormal.y) * runSpeed;

            Vector2 desiredSlip = new Vector2(floorNormal.x, floorNormal.z).normalized * slopeSpeed;

            //actualMovementFactor; // this should be 0.9 if player is fighting slope, ~0.5 if dot 0 of slope, 0 if player moving with slope
            float newDot = Vector2.Dot(new Vector2(universalMovementVector.x, universalMovementVector.z).normalized, new Vector2(floorNormal.x, floorNormal.z).normalized);

            if (newDot < 0)
            {
                // range is 0.3 to 0.5
                actualMovementFactor = 0.3f + ((0.5f - 0.3f) * (1 + newDot));
            }
            else
            {
                // range is 0.5 to 1.0
                actualMovementFactor = 0.5f + ((1.0f - 0.5f) * (newDot * currentSpeed/runSpeed));
            }

            Vector2 actualMovement = Vector2.Lerp(desiredSlip, new Vector2(universalMovementVector.x, universalMovementVector.z), actualMovementFactor);

            universalMovementVector.x = actualMovement.x;
            universalMovementVector.z = actualMovement.y;
        }
    }

    void SlippingNoControlState()
    {
        // right now the player has the same control as the slipping with control state, change this later
        // only if we slipped while facing into the floor
        if (!isGrounded)
        {
            characterState = CharacterState.FALLING;
            anim.SetTrigger("Falling");
            Debug.Log("falling");
        }
        else
        {
            Vector3 a = Vector3.zero;
            if (input.magnitude == 0)
            {
                theForward = Vector3.SmoothDamp(theForward, Vector3.zero, ref a, 1.0f);
            }
            else
            {
                float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
                theForward = Quaternion.AngleAxis(targetRotation, Vector3.up) * Vector3.forward;
                theForward.Normalize();
            }

            float targetSpeed = ((running || speedUp > 0) ? runSpeed : walkSpeed) * input.magnitude;

            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, 1.0f);
            universalMovementVector = theForward * currentSpeed;
            universalMovementVector.y = 0;

            // we are on a slippery floor but we can't apply the motion because the universal movement vector has
            // used by the wall raycasts and thus slipping can cause us to clip between the walls we slip into
            // by having us slip with a different uni vector than what was used for the walls
            // we should save the hit.normal, and apply it in the slipping state then we can raycast for walls

            float slopeSpeed = (1 - floorNormal.y) * runSpeed;

            Vector2 desiredSlip = new Vector2(floorNormal.x, floorNormal.z).normalized * slopeSpeed;

            //actualMovementFactor; // this should be 0.9 if player is fighting slope, ~0.5 if dot 0 of slope, 0 if player moving with slope
            float newDot = Vector2.Dot(new Vector2(universalMovementVector.x, universalMovementVector.z).normalized, new Vector2(floorNormal.x, floorNormal.z).normalized);

            if (newDot < 0)
            {
                // range is 0.3 to 0.5
                actualMovementFactor = 0.3f + ((0.5f - 0.3f) * (1 + newDot));
            }
            else
            {
                // range is 0.5 to 1.0
                actualMovementFactor = 0.5f + ((1.0f - 0.5f) * (newDot * currentSpeed / runSpeed));
            }

            Vector2 actualMovement = Vector2.Lerp(desiredSlip, new Vector2(universalMovementVector.x, universalMovementVector.z), actualMovementFactor);

            universalMovementVector.x = actualMovement.x;
            universalMovementVector.z = actualMovement.y;
        }
    }

    void GroundPoundState()
    {
        // in the ground pound state we halt all motion and start the ground pound animation
        // a timer is started which will cover the length of the animation plus a little delay
        // then the rest of the code is allowed to process and the character falls at maximum
        // velocity. once ground is detected we revert back to stationary
        if (groundPoundTimer >= groundPoundTimeLimit)
        {
            // times up proceed with falling
            if (isGrounded)
            {
                characterState = CharacterState.STATIONARY;
                anim.SetTrigger("StationaryState");
                groundPoundTimer = 0;
            }
            else
            {
                // we are falling during ground pound
                universalMovementVector.y = maxFallVelocity;
            }
        }
        else
        {
            groundPoundTimer++;
        }
    }

    void HangingState()
    {
        // in this state the character hangs from the edge we check for input to indicate moving
        // to the side or to climb up or to let go
        // left and right no longer factor in the camera they move in respect to the edge
        // must check if more edge is avaialable to move over to

        if (hangStateTimer <= 0)
        {
            // input now allowed
            if (input != Vector2.zero)
            {
                // input being made
                Vector2 tempInput = input;
                Vector3 direction = transform.position - cameraT.position;
                if(Vector2.Dot(new Vector2(transform.forward.x, transform.forward.z), new Vector2(direction.x, direction.z)) < 0)
                {
                    // we need to use reverse input for this task
                    tempInput *= -1;
                }


                if (tempInput.y >= 0.95f)
                {
                    // climb up
                    // must check if a wide enough floor is available to climb up to (use forward ray cast and only if
                    // no response we can climb up)
                    Debug.DrawRay(transform.position + new Vector3(0, forwardHeightTop + 0.5f, 0), transform.forward * 2.0f, Color.red);
                    RaycastHit allowedToClimb;
                    if (!Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop + 0.5f, 0), transform.forward, out allowedToClimb, 2.0f, layerMask))
                    {
                        characterState = CharacterState.EDGE_GRAB_CLIMB;
                        anim.SetTrigger("EdgeGrabClimb");
                        GameObject.Find("Main Camera").GetComponent<cameraController>().useSmooth(climbStateTime);
                        float TX = transform.forward.x * (0.65f + 1.0f);
                        float TZ = transform.forward.z * (0.65f + 1.0f);
                        float TY = forwardHeightTop - 0.01f;
                        transform.Translate(new Vector3(TX, TY, TZ), Space.World);
                        climbStateTimer = climbStateTime;
                    }
                }
                else if (tempInput.x >= 0.95f)
                {
                    // move right
                    // first check if more edge is available
                    RaycastHit moveRight;
                    if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop - 0.1f, 0) + transform.right * 1f, transform.forward, out moveRight, 0.7f, layerMask))
                    {
                        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop + 0.1f, 0) + transform.right * 1f + transform.forward * 1f, -transform.up, out moveRight, 0.2f, layerMask))
                        {
                            // more edge
                            transform.Translate(transform.right * 0.1f, Space.World);
                        }
                    }
                }
                else if (tempInput.x <= -0.95f)
                {
                    // move left
                    RaycastHit moveLeft;
                    if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop - 0.1f, 0) - transform.right * 1f, transform.forward, out moveLeft, 0.7f, layerMask))
                    {
                        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop + 0.1f, 0) - transform.right * 1f + transform.forward * 1f, -transform.up, out moveLeft, 0.2f, layerMask))
                        {
                            // more edge
                            transform.Translate(-transform.right * 0.1f, Space.World);
                        }
                    }
                }
                else if (tempInput.y <= -0.95f)
                {
                    // let go
                    characterState = CharacterState.FALLING;
                    anim.SetTrigger("Falling");
                }
            }
        }
        else
        {
            hangStateTimer--;
        }
    }

    void ClimbEdgeState()
    {
        // no input allowed only a timer after which we return to stationary
        if (climbStateTimer <= 0)
        {
            characterState = CharacterState.STATIONARY;
            anim.SetTrigger("StationaryState");
        }
        else
        {
            climbStateTimer--;
        }
    }

    void SwimmingState()
    {
        if (input != Vector2.zero)
        {
            // all this is to rotate the character in the desired running direction
            float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, swimmingTurnSmoothTime);
        }

        // determine the speed at which to move the player
        float targetSpeed = ((isGrounded) ? swimmingCoastGroundedSpeed : swimmingCoastSpeed) * input.magnitude;
        if (currentSpeed > swimmingCoastSpeed)
        {
            // we should instead reduce the current speed if it greater than the swim speed
            // proportional to how much bigger the speed is (bigger speed, faster we slow down simulating resistance)
            currentSpeed *= swimSpeedDecay;
        }
        if (isGrounded)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, swimmingSpeedSmoothTime / 6);
        }
        else
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, swimmingSpeedSmoothTime);
        }

        universalMovementVector.x = transform.forward.x * currentSpeed;
        universalMovementVector.z = transform.forward.z * currentSpeed;


        if (!isGrounded)
        {
            universalMovementVector.y += swimmingGravityIncrement;
            universalMovementVector.y = Mathf.Max(universalMovementVector.y, swimmingMaxFallSpeed);
        }

        if (!isGrounded && transform.position.y + 2.5f > swimBodyHeight)
        {
            // we are not grounded and too high above the surface
            universalMovementVector.y = -(transform.position.y + 2.5f - swimBodyHeight);
        }

        if (jump > 0)
        {
            // player wants to scroke upwards
            SwimmingStrokeUpward();
        }
        else if (strokeForward > 0)
        {
            // player wants to stroke forward
            SwimmingStrokeForward();
        }
        else if (specialJump > 0)
        {
            universalMovementVector.y += swimmingGravitySinkIncrement;
        }




        if (isGrounded)
        {
            anim.SetFloat("speed", currentSpeed / runSpeed);
        }
        else
        {
            anim.SetFloat("speed", 0.0f);
        }
    }

    void PoleGrabbingState()
    {
        // in this state we can move up and down and rotate about the center point with the 
        // thickness of the pole included, also be able to jump. thus thick or thin poles can be treated
        // must check if more pole allowed and make sure all poles are not touching any walls
        // also allow the character to let go of the pole by becoming grounded
        // treat collision like water, let the pole trigger the state

        // the player also need to be able to just let go of the pole in the air
        // thus we need a bool to say no more pole grabbing until grounded
        // also a timer to allow the player to exit the pole trigger from the jump
        // if no more pole can be dropped down to then check for truncation (current truncation only works if stationary or running)
        // and let go of pole, else then just let go

        // use the same set up for long jump when jumping off pole and use the long jump state
        if (input != Vector2.zero)
        {
            // input being made
            if (inputRaw.y >= 0.5f)
            {
                anim.SetFloat("speed", 1.0f);
                // climb up
                // must check if more pole is available
                RaycastHit allowedToClimb;
                if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop + poleClimbRate, 0) - transform.forward * 0.5f, transform.forward, out allowedToClimb, 0.7f))
                {
                    if (allowedToClimb.transform.tag == "pole")
                    {
                        // there is more pole to climb
                        transform.Translate(new Vector3(0, poleClimbRate, 0));
                    }
                }
                else
                {
                    // no more pole to climb
                }
            }
            else if (inputRaw.y <= -0.5f)
            {
                anim.SetFloat("speed", 0f);
                // climb down
                // first check if more pole is available
                RaycastHit allowedToSlide;
                if (Physics.Raycast(transform.position + new Vector3(0, poleDescendRate, 0) - transform.forward * 0.5f, transform.forward, out allowedToSlide, 0.7f))
                {
                    if (allowedToSlide.transform.tag == "pole")
                    {
                        // there is more pole to slide
                        transform.Translate(new Vector3(0, poleDescendRate, 0));
                    }
                }
                else
                {
                    // there is no more pole to slide down check for ground
                    // if ground then truncate and let go, else just let go
                    RaycastHit ground;
                    if (Physics.Raycast(transform.position - transform.forward * 0.5f, -transform.up, out ground, -poleDescendRate, layerMask))
                    {
                        // ground detected
                        transform.position = new Vector3(transform.position.x, ground.point.y, transform.position.z);
                        characterState = CharacterState.STATIONARY;
                        anim.SetTrigger("StationaryState");
                    }
                    else
                    {
                        // just let go
                        allowedToGrabPole = false;
                        characterState = CharacterState.FALLING;
                        anim.SetTrigger("Falling");
                    }
                }
            }

            if (inputRaw.x >= 0.5f)
            {
                // rotate about the pole counterclockwise from the top
                transform.RotateAround(transform.position, new Vector3(0, 1, 0), -poleRotateSpeed);
            }
            else if (inputRaw.x <= -0.5f)
            {
                // rotate about the pole clockwise from the top
                transform.RotateAround(transform.position, new Vector3(0, 1, 0), poleRotateSpeed);
            }
        }
        else
        {
            anim.SetFloat("speed", 0f);
        }

        if (jump > 0)
        {
            // player wants to jump
            // rotate the player 180 then we jump off using the same code as a long jump
            transform.RotateAround(transform.position, new Vector3(0, 1, 0), 180f);
            universalMovementVector = transform.forward * poleInitialJumpVelocity.x + new Vector3(0, poleInitialJumpVelocity.y, 0);
            characterState = CharacterState.JUMPING_LOW;
            anim.SetTrigger("JumpingLow");
            currentSpeed = initialLongJumpVelocity.x;
            poleLeaveTimer = PoleLeaveTime;
        }
        else if (specialJump > 0)
        {
            // just let go of pole
            characterState = CharacterState.FALLING;
            anim.SetTrigger("Falling");
            allowedToGrabPole = false;
        }
        // separately we now check for jump
    }

    void InjuryKnockbackState()
    {
        // we have no control in this state, only obey gravity, and when timer runs out check for grounded
        // if so then go to stationary, else go to falling no control
        universalMovementVector.y += gravityIncrement;
        universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

        universalMovementVector.x *= explosionKnockBackDecay;
        universalMovementVector.z *= explosionKnockBackDecay;

        if (injuryKnockbackTimer < injuryKnockbackTime)
        {
            injuryKnockbackTimer++;
        }
        else
        {
            // we are done being injured
            if (isGrounded)
            {
                characterState = CharacterState.STATIONARY;
                anim.SetTrigger("StationaryState");
            }
            else
            {
                characterState = CharacterState.FALLING_NO_CONTROL;
                anim.SetTrigger("FallingNoControl");
            }
        }
    }

    void InjurySquishedState()
    {
        // we have no control in this state, only obey gravity, and when timer runs out check for grounded
        // if so then go to stationary, else go to falling no control
        universalMovementVector.y += gravityIncrement;
        universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

        if (injurySquishTimer <= 0)
        {
            injurySquishTimer = injurySquishTime;
            // we are done being squished
            if (isGrounded)
            {
                characterState = CharacterState.STATIONARY;
                anim.SetTrigger("StationaryState");
            }
            else
            {
                characterState = CharacterState.FALLING_NO_CONTROL;
                anim.SetTrigger("FallingNoControl");
            }
        }
        else
        {
            injurySquishTimer--;
        }
    }

    void FallingNoControlState()
    {
        if (isGrounded)
        {
            characterState = CharacterState.STATIONARY;
            anim.SetTrigger("StationaryState");
        }
        else
        {
            universalMovementVector.y += gravityIncrement;
            universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);
        }
    }

    void SwimmingStrokeUpward()
    {
        universalMovementVector.y = swimmingStrokeUpwardInitialSpeed;
        anim.SetTrigger("SwimmingStrokeUpward");
    }

    void SwimmingStrokeForward()
    {
        currentSpeed = swimmingStrokeForwardInitialSpeed.x;
        universalMovementVector.y = swimmingStrokeForwardInitialSpeed.y;
        if (isGrounded)
        {
            universalMovementVector.y += 0.04f;
        }
        anim.SetTrigger("SwimmingStrokeForward");
    }

    void LongJump()
    {
        universalMovementVector = transform.forward * initialLongJumpVelocity.x + new Vector3(0, initialLongJumpVelocity.y, 0);
        characterState = CharacterState.JUMPING_LONG;
        anim.SetTrigger("JumpingLong");
        currentSpeed = initialLongJumpVelocity.x;
    }

    void HighJump()
    {
        // keep the x and z as we want to continue to move (but they might be zero anyway since our character has to be stationary)
        universalMovementVector.y = initialHighJumpVelocity;
        characterState = CharacterState.JUMPING_HIGH;
        anim.SetTrigger("JumpingHigh");
    }

    void RegularJump()
    {
        // keep the x and z as we want to continue to move
        universalMovementVector.y = initialJumpVelocity;
        characterState = CharacterState.JUMPING_LOW;
        anim.SetTrigger("JumpingLow");
    }

    public enum CharacterState
    {
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
        SLIPPING_WITH_CONTROL,
        SLIPPING_NO_CONTROL,
        GROUND_POUND,
        SWIMMING,
        EDGE_GRAB_HANG,
        EDGE_GRAB_CLIMB,
        POLE_GRABBING,
        INJURY_KNOCKBACK,
        INJURY_SQUISH
        // a function is called to set this value and must clean up a few values before forcing a no input senario

    }

    public void AcceptInput(bool accepted)
    {
        // this function is called when we need to deactivate inputs,
        // must clean up a few things before setting the state to no inputs
        InputAccepted = accepted;
    }

    public void PoleDetected(Vector3 polePosition, float poleThickness, float poleHeight)
    {
        // pole thickness used if we want thick poles (logs) to climb up
        // we are only concerned with the pole x and z
        // the player can only enter the pole climb state if we are not grounded, but we do not care about the y direction
        // we cancel the universal vector and set the player to the pole with the original y we had before trigger
        if (characterState != CharacterState.POLE_GRABBING && allowedToGrabPole && !isGrounded && poleLeaveTimer <= 0)
        {
            // we are not yet grabbing the pole and allowed to
            characterState = CharacterState.POLE_GRABBING;
            anim.SetTrigger("PoleGrabbing");
            float y = Mathf.Min(polePosition.y + poleHeight - forwardHeightTop, transform.position.y);
            transform.position = new Vector3(polePosition.x, y, polePosition.z);
            //transform.position = new Vector3(polePosition.x, transform.position.y, polePosition.z);
            universalMovementVector = Vector3.zero;
            currentSpeed = 0;
            // have the mesh moved back in animation to account for the pole occupying the player's x and z
        }
    }

    public void SwitchIntoSwimState(float height)
    {
        if (characterState != CharacterState.SWIMMING /*&& characterState != CharacterState.SWIMMING_STROKE_FORWARD && characterState != CharacterState.SWIMMING_STROKE_UP*/)
        {
            swimBodyHeight = height;
            characterState = CharacterState.SWIMMING;
            anim.SetTrigger("Swimming");
            //currentSpeed = 0;
            // we should instead reduce the current speed if it greater than the swim speed
            // proportional to how much bigger the speed is (bigger speed, faster we slow down simulating resistance)
        }
    }

    public void SwitchOutOfSwimState()
    {
        if (characterState == CharacterState.SWIMMING /*|| characterState == CharacterState.SWIMMING_STROKE_FORWARD || characterState == CharacterState.SWIMMING_STROKE_UP*/)
        {
            characterState = CharacterState.RUNNING;
            //Debug.Log("swim to run");
            anim.SetTrigger("RunningState");
        }
    }

    public void InjureCharacter(AttackData data/*int damage, Vector3 point, bool minor*/)
    {
        // this function is called upon triggering an event that harms the player
        if (characterState != CharacterState.INJURY_KNOCKBACK)
        {
            characterState = CharacterState.INJURY_KNOCKBACK;

            //AttackData data = (AttackData)


            if (data.minor)
            {
                anim.SetTrigger("InjuryKnockbackMinor");
            }
            else
            {
                anim.SetTrigger("InjuryKnockbackMajor");
            }

            Vector3 direction = (transform.position - data.point).normalized;
            //transform.forward = direction;
            universalMovementVector = direction * explosionKnockBackVelocity;
            injuryKnockbackTimer = 0;
            if (playerStats.UpdatePlayerHealth_IsDead(data.damage))
            {
                // player is dead call for a scene transition to exit back to a default scene
                Debug.Log("player is Dead");
                InputAccepted = false;
            }
        }
    }

    public bool IsStationary()
    {
        if(characterState == CharacterState.STATIONARY)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool isGroundPounding()
    {
        if(characterState == CharacterState.GROUND_POUND)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}