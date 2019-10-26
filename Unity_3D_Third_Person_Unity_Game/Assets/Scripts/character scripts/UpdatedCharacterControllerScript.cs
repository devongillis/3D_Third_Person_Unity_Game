using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatedCharacterControllerScript : MonoBehaviour
{
    public float maximumPlayerHorizontalVelocity;

    public float verticalRaycastOffset;
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
    public float forwardThickness = 0.3f; // this need to be have the diameter of the total player


    // all of these values are the angle of the normal from the vertical plane,clockwise
    public float regularFloorStart = 0;
    public float regularFloorSlipperyFloor = 45; // anything below is normal, above is slippery
    public float SlipperyFloorWall = 80; // anything below is slippery, above is wall
    public float wallCeiling = 100; // anything below is wall, above is ceiling
    public float ceiling = 180;

    public float regularFloorStartArc;
    public float regularFloorSlipperyFloorArc;
    public float SlipperyFloorWallArc;
    public float wallCeilingArc;
    public float ceilingArc;

    public float arcRatio = (2 * Mathf.PI / 360);



    // a wall is defined as any surface with normal.y between -0.174 and 0.174
    public float wallUpperLimitNormalY;// = 0.174f; // <=
    public float wallLowerLimitNormalY;// = -0.174f; // >=
    // a regular floor is defined as any surface with normal.y between 0.707 and 1.0
    public float floorUpperLimitNormalY;// = 1.0f; // <=
    public float floorLowerLimitNormalY;// = 0.707f; // >=
    // a slippery floor is defined as any surface with normal.y between 0.5 and 0.707
    public float slipperyFloorUpperLimitNormalY;// = 0.707f; // <
    public float slipperyFloorLowerLimitNormalY;// = 0.5f; // >=
    // a ceiling is defined as any surface with normal.y between -1.0 and -0.5
    public float ceilingUpperLimitNormalY;// = -0.5f; // <=
    // -1.0f is the minimum value for normal.y so we don't need to use this value for ceilings


    
    public float slopeDownwardCheckDistance = 0.0f; // set in start, this is for checking if the player should be truncated to the floor by the maximum slope value
    //public float slopeWallMinimumForwardSpeed = 0.0f; // set in start

    // Bit shift the index of the layer (8) to get a bit mask
    public int layerMask = ~(1 << 8);
    // This would cast rays only against colliders in layer 8.

    private Animator anim;
    public float animMoveSpeed = 0.0f;
    public float animDeltaSpeed = 0.01f;
    public float animIdleSpeed = 0.0f;
    public float animWalkSpeed = 0.6f;
    public float animRunSpeed = 1.0f;

    public CharacterState characterState = CharacterState.STATIONARY;

    public float walkSpeed = 0.15f;
    public float runSpeed = 0.3f;

    // swimming physics
    public float swimmingCoastSpeed = 0.05f;
    public float swimmingCoastGroundedSpeed = 0.07f;
    public Vector2 swimmingStrokeForwardInitialSpeed = new Vector2(0.15f, 0.1f);
    public float swimmingStrokeUpwardInitialSpeed = 0.20f;
    public float swimmingGravityIncrement = -0.01f;
    public float swimmingMaxFallSpeed = -0.1f;
    public float swimmingJumpSpeed = 0.1f;
    public float swimmingTurnSmoothTime = 1.0f;
    public float swimmingSpeedSmoothTime = 3.0f;
    public float swimBodyHeight; // the top of the swimming medium in world quardinates

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
    //float wallTouchSpeed = 0.01f;
    //bool touchingWall = false;


    public float initialJumpVelocity = 0.35f;
    public float initialHighJumpVelocity = 0.45f;
    public Vector2 initialLongJumpVelocity = new Vector2(0.6f, 0.25f); // length and height




    // input variables
    Vector2 input;
    Vector2 inputRaw;
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
    public Vector3 platformRotationVector = new Vector3(0, 0, 0);
    //public float slopeSlipSpeed = 0.1f;
    public bool InputAccepted = true;
    public int groundPoundTimer = 0;
    public int groundPoundTimeLimit = 60; // frames


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

    // Start is called before the first frame update
    void Start()
    {
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        Application.targetFrameRate = 30;
        layerMask = ~layerMask;
        cameraT = Camera.main.transform;




        regularFloorStartArc = regularFloorStart * arcRatio;
        regularFloorSlipperyFloorArc = regularFloorSlipperyFloor * arcRatio;
        SlipperyFloorWallArc = SlipperyFloorWall * arcRatio;
        wallCeilingArc = wallCeiling * arcRatio;
        ceilingArc = ceiling * arcRatio;


        // a regular floor is defined as any surface with normal.y between   1.000  and  0.707
        // a slippery floor is defined as any surface with normal.y between  0.707  and  0.174
        // a wall is defined as any surface with normal.y between            0.174  and -0.174
        // a ceiling is defined as any surface with normal.y between        -0.174  and -1.000

        floorUpperLimitNormalY = 1.0f; // floor is <= 1.0
        floorLowerLimitNormalY = Mathf.Sin((90 - regularFloorSlipperyFloor) * arcRatio); // floor is >= 0.707
        
        slipperyFloorUpperLimitNormalY = floorLowerLimitNormalY; // slippery is < 0.707
        slipperyFloorLowerLimitNormalY = Mathf.Sin((90 - SlipperyFloorWall) * arcRatio); // slippery is > 0.174

        wallUpperLimitNormalY = Mathf.Sin((90 - SlipperyFloorWall) * arcRatio); // wall is <= 0.174
        wallLowerLimitNormalY = -wallUpperLimitNormalY; // wall is >= -0.174

        ceilingUpperLimitNormalY = Mathf.Sin((90 - wallCeiling) * arcRatio); // ceiling is < -0.174

        slopeDownwardCheckDistance = runSpeed * Mathf.Tan(SlipperyFloorWallArc); // the distance downward to check for the slope the player is running down on
        
        maximumPlayerHorizontalVelocity = Mathf.Max(runSpeed, initialLongJumpVelocity.x);
        horizontalRaycastOffset = -maxFallVelocity * Mathf.Tan((90 - SlipperyFloorWall) * arcRatio) + 0.3f; // should add a little extra like 0.01f
        verticalRaycastOffset = maximumPlayerHorizontalVelocity * Mathf.Tan(SlipperyFloorWall * arcRatio) + 0.0f; // should add a little extra like 0.01f
        // the raycasts start at these offsets but must also extend beyond by the amount the player can move
        //Debug.Log(verticalRaycastOffset + "offset");

        anim = gameObject.GetComponentInChildren<Animator>();
        anim.SetFloat("speed", animIdleSpeed);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // first we check for "no input status"
        // then collect the inputs from the player
        // enter the function call from current character state
        // and then change the state respsectively, followed by
        // acting on the input values
        if (!InputAccepted)
        {
            // act like no inputs being made
            input = new Vector2(0, 0);
            inputRaw = new Vector2(0, 0);
            jump = 0;
            specialJump = 0;
            groundPound = 0;
        }
        else
        {
            // inputs are allowed
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            inputRaw = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = input.normalized;
            jump = Input.GetAxisRaw("Jump");
            specialJump = Input.GetAxisRaw("Special Jump");
            groundPound = Input.GetAxisRaw("Ground Pound");
        }
        // inputs are collected

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

        }
        else if (characterState == CharacterState.SLIPPING_NO_CONTROL)
        {
            SlippingState();
        }
        else if (characterState == CharacterState.GROUND_POUND)
        {
            GroundPoundState();
        }
        else if (characterState == CharacterState.SWIMMING)
        {
            SwimmingState();
        }
        else if(characterState == CharacterState.EDGE_GRAB_HANG)
        {
            HangingState();
        }
        else if(characterState == CharacterState.EDGE_GRAB_CLIMB)
        {
            ClimbEdgeState();
        }
        else if(characterState == CharacterState.POLE_GRABBING)
        {
            PoleGrabbingState();
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
        isGrounded = false;

        // we are moving vertically but not horizontally thus we have no direction for wall collisions
        // must use the transform.forward, and do a vector in all four directions
        float transX = forwardThickness + (horizontalRaycastOffset - forwardThickness) + 0.01f;

        RaycastHit noDirection;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightBottom, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), transform.forward, out noDirection, transX, layerMask))
        {
            xzModified = WallCollsion(noDirection);
        }
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightBottom, 0) + transform.forward * (horizontalRaycastOffset - forwardThickness), -transform.forward, out noDirection, transX, layerMask))
        {
            xzModified = WallCollsion(noDirection);
        }
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightBottom, 0) - transform.right * (horizontalRaycastOffset - forwardThickness), transform.right, out noDirection, transX, layerMask))
        {
            xzModified = WallCollsion(noDirection);
        }
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightBottom, 0) + transform.right * (horizontalRaycastOffset - forwardThickness), -transform.right, out noDirection, transX, layerMask))
        {
            xzModified = WallCollsion(noDirection);
        }



        float transXM = XZ.magnitude + transX;

        // remember the distance is based off the universalMovementVector
        // we want the player's thickness included in the raycast
        RaycastHit hitForwardTop;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), XZ, out hitForwardTop, transXM, layerMask))
        {
            xzModified = WallCollsion(hitForwardTop);
        }

        RaycastHit hitForwardMiddle1;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightMiddle1, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), XZ, out hitForwardMiddle1, transXM, layerMask))
        {
            xzModified = WallCollsion(hitForwardMiddle1);
        }

        RaycastHit hitForwardMiddle2;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightMiddle2, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), XZ, out hitForwardMiddle2, transXM, layerMask))
        {
            xzModified = WallCollsion(hitForwardMiddle2);
        }


        RaycastHit hitForwardMiddle3;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightMiddle3, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), XZ, out hitForwardMiddle3, transXM, layerMask))
        {
            xzModified = WallCollsion(hitForwardMiddle3);
        }

        RaycastHit hitForwardBottom;
        if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightBottom, 0) - transform.forward * (horizontalRaycastOffset - forwardThickness), XZ, out hitForwardBottom, transXM, layerMask))
        {
            xzModified = WallCollsion(hitForwardBottom);
        }

        // a tiny 0.01f offset to ensure the raycast doesn't just miss the floor on next frame after moving character
        // to the floor
        RaycastHit hitDownward;
        isGrounded = false;
        
        if (characterState == CharacterState.SLIPPING_NO_CONTROL)
        {
            characterState = CharacterState.STATIONARY;
        }
        // we are not moving up (but what if we are slightly moving up and moving forward really fast then we might clip into floor horizontally
        float transY = -(Y.y - downwardThickness) + (verticalRaycastOffset - downwardThickness) + 0.01f;
        if (Physics.Raycast(transform.position + new Vector3(0, verticalRaycastOffset - downwardThickness, 0), -transform.up, out hitDownward, transY, layerMask))
        {
            yModified = FloorCollision(hitDownward, false, ref XZ, ref xzModified); // double check if we want to know if ymodified
        }
        else
        {
            isGrounded = false;
        }
        

        // now we test for the truncated ground movement
        // just check if we are running or stationary, if so then test the truncate
        // we need this as it starts from a different height
        if (characterState == CharacterState.STATIONARY || characterState == CharacterState.RUNNING)
        {
            // check for the ground
            RaycastHit truncate;
            if (Physics.Raycast(transform.position + new Vector3(0, 0.01f, 0), -transform.up, out truncate, slopeDownwardCheckDistance + 0.01f, layerMask))
            {
                yModified = FloorCollision(truncate, true, ref XZ, ref xzModified);
            }
            else
            {
                isGrounded = false;
            }
        }
        if (!isGrounded)
        {
            if(characterState != CharacterState.EDGE_GRAB_HANG && characterState != CharacterState.EDGE_GRAB_CLIMB)
            {
                transform.parent = null;
            }
        }

        // only if not modified will this function move the player in that coordinate
        MoveCharacter(universalMovementVector + platformRotationVector, xzModified, yModified);
        platformRotationVector = Vector3.zero;

        if (isGrounded)
        {
            allowedToEdgeGrab = true;
            allowedToGrabPole = true;
        }
        // do not combine these
        if (allowedToEdgeGrab)
        {
            EdgeCollision();
        }


        //Debug.DrawRay(transform.position + new Vector3(0, forwardHeightTop, 0), transform.forward * 10, Color.red);
    }

    bool WallCollsion(RaycastHit hit)
    {
        bool xzModified = false;
        if (hit.normal.y >= wallLowerLimitNormalY && hit.normal.y <= wallUpperLimitNormalY)
        {
            Vector3 translator = hit.point + hit.normal * (forwardThickness + 0.01f);
            transform.position = new Vector3(translator.x, transform.position.y, translator.z);
            xzModified = true;
            if (!isGrounded && currentSpeed > walkSpeed)
            {
                universalMovementVector.y = Mathf.Min(universalMovementVector.y, 0.0f);
            }
            currentSpeed = 0.0f;
            //touchingWall = true;
        }
        else
        {
            // a surface was found but it does not meet the definition of a wall
        }
        return xzModified;
    }

    bool FloorCollision(RaycastHit hit, bool truncate, ref Vector3 xz, ref bool xzModified)
    {
        bool yModified = false;
        if (hit.normal.y >= slipperyFloorLowerLimitNormalY)
        {
            // we have found a floor below we must truncate to it
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
            
            // now we need to move the character back by a value proportional to the slope
            if (!truncate)
            {
                // instead use this code for forcing the player to slip
                /*
                Vector3 moveBack = Mathf.Sin(90 * arcRatio - Mathf.Asin(hit.normal.y)) * xz * 0.5f;
                Debug.Log(moveBack + " " + xz);
                xz -= moveBack;
                transform.Translate(xz, Space.World);
                xzModified = true;
                */
                //Vector3 moveBack = Mathf.Sin(90 * arcRatio - Mathf.Asin(hit.normal.y)) * xz * 0.5f;
                //Debug.Log(moveBack + " " + xz);

                /*
                xz *= Mathf.Sqrt(Mathf.Sqrt(hit.normal.y));
                transform.Translate(xz, Space.World);
                xzModified = true;
                */
                //Debug.Log(hit.normal.y + " fuck");
                /*
                if(hit.normal.y < slipperyFloorUpperLimitNormalY)
                {
                    // slippery
                    characterState = CharacterState.SLIPPING_NO_CONTROL;
                    universalMovementVector.x = 0;
                    universalMovementVector.z = 0;
                    //universalMovementVector.y -= 0.01f;
                    // we should also enter the slip state
                    xz = hit.normal;
                    xz.y = 0;
                    xz *= 0.2f;
                    transform.Translate(xz, Space.World);
                    xzModified = true;
                }
                else
                {
                    
                }
                */
            }
            
            if (hit.normal.y < slipperyFloorUpperLimitNormalY)
            {
                // slippery slope
            }
            else if (hit.normal.y >= slipperyFloorUpperLimitNormalY)
            {
                // regular floor
            }
        }
        else
        {
            isGrounded = false;
        }
        return yModified;
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
            // we are falling, edge grabbing is allowed
            RaycastHit hitEdgeForward;
            RaycastHit hitEdgeDownward;

            if (Physics.Raycast(transform.position + transform.forward * 1f + new Vector3(0, forwardHeightTop - maxFallVelocity, 0), -transform.up, out hitEdgeDownward, -maxFallVelocity, layerMask))
            {
                if (hitEdgeDownward.normal.y >= 1 - 0.09f)
                {
                    if (hitEdgeDownward.transform.tag != "notEdge")
                    {
                        if (Physics.Raycast(new Vector3(transform.position.x, hitEdgeDownward.point.y - 0.1f, transform.position.z), transform.forward, out hitEdgeForward, 0.6f, layerMask))
                        {

                            // we struck a wall that meets the first definition of an edge
                            if (hitEdgeForward.normal.y >= -0.09f && hitEdgeForward.normal.y <= 0.09f)
                            {

                                // we struck an edge and a wall that works
                                Debug.Log("edge found");
                                characterState = CharacterState.EDGE_GRAB_HANG;
                                universalMovementVector = Vector3.zero;
                                float distanceBack = 0.65f;
                                float posYOffset = 0.0f;
                                // need to truncate the player to the wall in a defined space to fit the animation
                                float posX = hitEdgeForward.point.x + hitEdgeForward.normal.x * distanceBack;
                                float posZ = hitEdgeForward.point.z + hitEdgeForward.normal.z * distanceBack;
                                float posY = hitEdgeDownward.point.y - forwardHeightTop + posYOffset;
                                transform.position = new Vector3(posX, posY, posZ);
                                anim.SetFloat("speed", 0);
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

    void StationaryState()
    {
        universalMovementVector = Vector3.zero;
        currentSpeed = 0.0f;

        if (!isGrounded)
        {
            // we are in the stationary state but the floor is not present, (got pushed off the edge?)
            // we swap the state and skip the input
            characterState = CharacterState.FALLING;
        }
        else
        {
            if (input != Vector2.zero)
            {
                // we have input for running
                // must exit the stationary state
                characterState = CharacterState.RUNNING;
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
            // if no input was registered we remain in the stationary state, and animation will reflect that

            if (animMoveSpeed <= animIdleSpeed)
            {
                animMoveSpeed = animIdleSpeed;
            }
            else
            {
                animMoveSpeed -= animDeltaSpeed;
            }

            anim.SetFloat("speed", animMoveSpeed);
        }
    }

    void RunningState()
    {
        // running covers any horizontal movement, so check speed to do a long jump
        if (!isGrounded)
        {
            // we are in the running state but no ground is present, we swap the state
            // and skip the input
            characterState = CharacterState.FALLING;
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
            }

            // determine the speed at which to move the player
            float targetSpeed = ((running || Input.GetKey(KeyCode.E)) ? runSpeed : walkSpeed) * input.magnitude;
            
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, runningSpeedSmoothTime);
            universalMovementVector = transform.forward * currentSpeed;

            if (jump > 0)
            {
                // later come up with a keyboard click tester (don't let holding down button work as jump input)
                // now determine what kind of jump
                if (specialJump > 0)
                {
                    if (running || Input.GetKey(KeyCode.E))
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

            if (running || Input.GetKey(KeyCode.E))
            {
                if (animMoveSpeed < animRunSpeed)
                {
                    animMoveSpeed += animDeltaSpeed * 5;
                }
                else
                {
                    animMoveSpeed -= animDeltaSpeed * 5;
                }
                anim.SetFloat("speed", animMoveSpeed);
            }
            else
            {
                if (animMoveSpeed < animWalkSpeed)
                {
                    animMoveSpeed += animDeltaSpeed;
                }
                else
                {
                    animMoveSpeed -= animDeltaSpeed;
                }
                anim.SetFloat("speed", animWalkSpeed); // double check this i think it should be animmovespeed
            }
        }
    }

    void LongJumpingState()
    {
        // we dont enter falling state with long jump, because falling state lets us change the projectory
        // also since in long jump we dont want to change the character's rotation
        universalMovementVector.y += gravityIncrement;
        universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

        if (isGrounded)
        {
            characterState = CharacterState.RUNNING;
        }
    }

    void HighJumpingState()
    {
        if (input.magnitude > 0)
        {
            float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        }
        float targetSpeed = ((running || Input.GetKey(KeyCode.E)) ? runSpeed : walkSpeed) * input.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, JumpingSpeedSmoothTime);

        universalMovementVector.x = transform.forward.x * currentSpeed;
        universalMovementVector.z = transform.forward.z * currentSpeed;


        universalMovementVector.y += gravityIncrement;
        universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

        if (universalMovementVector.y <= 0.0f)
        {
            characterState = CharacterState.FALLING;
        }

        if (groundPound > 0)
        {
            characterState = CharacterState.GROUND_POUND;
            universalMovementVector.y = 0;
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

        float targetSpeed = ((running || Input.GetKey(KeyCode.E)) ? runSpeed : walkSpeed) * input.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, JumpingSpeedSmoothTime);

        universalMovementVector.x = transform.forward.x * currentSpeed;
        universalMovementVector.z = transform.forward.z * currentSpeed;


        universalMovementVector.y += gravityIncrement;
        universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

        if (universalMovementVector.y <= 0.0f)
        {
            characterState = CharacterState.FALLING;
        }

        if (groundPound > 0)
        {
            characterState = CharacterState.GROUND_POUND;
            universalMovementVector.y = 0;
        }
    }

    void FallingState()
    {
        if (isGrounded)
        {
            characterState = CharacterState.RUNNING; // see if stationary is more appropriate
        }
        else
        {
            if (input.magnitude > 0)
            {
                float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
            }

            float targetSpeed = ((running || Input.GetKey(KeyCode.E)) ? runSpeed : walkSpeed) * input.magnitude;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, JumpingSpeedSmoothTime);

            universalMovementVector.x = transform.forward.x * currentSpeed;
            universalMovementVector.z = transform.forward.z * currentSpeed;


            universalMovementVector.y += gravityIncrement;
            universalMovementVector.y = Mathf.Max(universalMovementVector.y, maxFallVelocity);

            if (groundPound > 0)
            {
                characterState = CharacterState.GROUND_POUND;
                universalMovementVector.y = 0;
            }
        }
    }

    void SlippingState()
    {
        // slipping state will put the character back and down the slope by the moveback vector
        // it will not accept inputs and will set the state back to stationary, if still on slope,
        // the collision function will send us back to slipping for a 0 net change
        // we want all initial movement zeroed out so don't "add" to but rather "set" the universal vector
        // to avoid issues with collider not allowing movement due to y component forcing character into mesh (maybe)
        // apply the x and z first and then the y
        // intially from another state the character moves and the OnControllerColliderHit function is called within after moving
        // this should then determine a vector to move back and down by and set the state to slipping, on the first slipping call
        // we move by the moveback vector and thus call the collision function again it will decide wether to swap the state
        if (!isGrounded)
        {
            //characterState = CharacterState.FALLING;
        }
        //characterState = CharacterState.STATIONARY;
        // the slope has been determined by the raycast and it will have saved it in a vector
        // we move by this vector, might want to adjust speed by the angle of the slope (60 = 0 and 90 = max)
        //universalMovementVector = slopeVector * slopeSlipSpeed;

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
        //Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop + 0.1f, 0) + transform.right * 1f + transform.forward * 1f, -transform.up, out moveRight, 0.2f, layerMask);
        //Debug.DrawRay(transform.position + new Vector3(0, forwardHeightTop + 0.1f, 0) + transform.right * 1f + transform.forward * 1f, -transform.up * 0.2f, Color.red);

        if (hangStateTimer <= 0)
        {
            // input now allowed
            if (input != Vector2.zero)
            {
                // input being made
                if (input.y >= 0.95f)
                {
                    // climb up
                    // must check if a wide enough floor is available to climb up to (use forward ray cast and only if
                    // no response we can climb up)
                    Debug.DrawRay(transform.position + new Vector3(0, forwardHeightTop + 0.5f, 0), transform.forward * 2.0f, Color.red);
                    RaycastHit allowedToClimb;
                    if (!Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop + 0.5f, 0), transform.forward, out allowedToClimb, 2.0f, layerMask))
                    {
                        characterState = CharacterState.EDGE_GRAB_CLIMB;
                        float TX = transform.forward.x * (0.65f + 1.0f);
                        float TZ = transform.forward.z * (0.65f + 1.0f);
                        float TY = forwardHeightTop - 0.01f;
                        transform.Translate(new Vector3(TX, TY, TZ), Space.World);
                        climbStateTimer = climbStateTime;
                    }
                }
                else if (input.x >= 0.95f)
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
                else if (input.x <= -0.95f)
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
                else if (input.y <= -0.95f)
                {
                    // let go
                    characterState = CharacterState.FALLING;
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
        if(climbStateTimer <= 0)
        {
            characterState = CharacterState.STATIONARY;
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

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, swimmingSpeedSmoothTime);
        
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
        else if (Input.GetKey(KeyCode.E))
        {
            // player wants to stroke forward
            SwimmingStrokeForward();
        }





        if (input.y > 0)
        {
            if (animMoveSpeed < animWalkSpeed)
            {
                animMoveSpeed += animDeltaSpeed;
            }
            else
            {
                animMoveSpeed -= animDeltaSpeed;
            }

        }
        else
        {
            if (animMoveSpeed <= animIdleSpeed)
            {
                animMoveSpeed = animIdleSpeed;
            }
            else
            {
                animMoveSpeed -= animDeltaSpeed;
            }
        }
        anim.SetFloat("speed", animMoveSpeed);
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
                // climb up
                // must check if more pole is available
                RaycastHit allowedToClimb;
                if (Physics.Raycast(transform.position + new Vector3(0, forwardHeightTop + poleClimbRate, 0) - transform.forward * 0.5f, transform.forward, out allowedToClimb, 0.7f))
                {
                    if(allowedToClimb.transform.tag == "pole")
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
                    }
                    else
                    {
                        // just let go
                        allowedToGrabPole = false;
                        characterState = CharacterState.FALLING;
                    }
                }
            }

            if (inputRaw.x >= 0.5f)
            {
                // rotate about the pole counterclockwise from the top
                transform.RotateAround(transform.position, new Vector3(0, 1, 0), -poleRotateSpeed);
            }
            else if(inputRaw.x <= -0.5f)
            {
                // rotate about the pole clockwise from the top
                transform.RotateAround(transform.position, new Vector3(0, 1, 0), poleRotateSpeed);
            }
        }

        if(jump > 0)
        {
            // player wants to jump
            // rotate the player 180 then we jump off using the same code as a long jump
            transform.RotateAround(transform.position, new Vector3(0, 1, 0), 180f);
            universalMovementVector = transform.forward * poleInitialJumpVelocity.x + new Vector3(0, poleInitialJumpVelocity.y, 0);
            characterState = CharacterState.JUMPING_LOW;
            currentSpeed = initialLongJumpVelocity.x;
            poleLeaveTimer = PoleLeaveTime;
        }
        else if(specialJump > 0)
        {
            // just let go of pole
            characterState = CharacterState.FALLING;
            allowedToGrabPole = false;
        }
        // separately we now check for jump
    }


    void SwimmingStrokeUpward()
    {
        universalMovementVector.y = swimmingStrokeUpwardInitialSpeed;
    }
    
    void SwimmingStrokeForward()
    {
        currentSpeed = swimmingStrokeForwardInitialSpeed.x;
        universalMovementVector.y = swimmingStrokeForwardInitialSpeed.y;
    }

    void LongJump()
    {
        universalMovementVector = transform.forward * initialLongJumpVelocity.x + new Vector3(0, initialLongJumpVelocity.y, 0);
        characterState = CharacterState.JUMPING_LONG;
        currentSpeed = initialLongJumpVelocity.x;
    }

    void HighJump()
    {
        // keep the x and z as we want to continue to move (but they might be zero anyway since our character has to be stationary)
        universalMovementVector.y = initialHighJumpVelocity;
        characterState = CharacterState.JUMPING_HIGH;
    }

    void RegularJump()
    {
        // keep the x and z as we want to continue to move
        universalMovementVector.y = initialJumpVelocity;
        characterState = CharacterState.JUMPING_LOW;
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
        SLIPPING_NO_CONTROL,
        GROUND_POUND,
        SWIMMING,
        SWIMMING_STROKE_UP,
        SWIMMING_STROKE_FORWARD,
        EDGE_GRAB_HANG,
        EDGE_GRAB_CLIMB,
        POLE_GRABBING
        // a function is called to set this value and must clean up a few values before forcing a no input senario

    }

    public void SwitchToNoInput()
    {
        // this function is called when we need to deactivate inputs,
        // must clean up a few things before setting the state to no inputs
    }

    public void PoleDetected(Vector3 polePosition, float poleThickness)
    {
        // pole thickness used if we want thick poles (logs) to climb up
        // we are only concerned with the pole x and z
        // the player can only enter the pole climb state if we are not grounded, but we do not care about the y direction
        // we cancel the universal vector and set the player to the pole with the original y we had before trigger
        if (characterState != CharacterState.POLE_GRABBING && allowedToGrabPole && !isGrounded && poleLeaveTimer <= 0) {
            // we are not yet grabbing the pole and allowed to
            characterState = CharacterState.POLE_GRABBING;
            transform.position = new Vector3(polePosition.x, transform.position.y, polePosition.z);
            universalMovementVector = Vector3.zero;
            currentSpeed = 0;
            // have the mesh moved back in animation to account for the pole occupying the player's x and z
        }
    }

    public void SwitchIntoSwimState(float height)
    {
        if (characterState != CharacterState.SWIMMING && characterState != CharacterState.SWIMMING_STROKE_FORWARD && characterState != CharacterState.SWIMMING_STROKE_UP)
        {
            swimBodyHeight = height;
            Debug.Log("player is swimming");
            characterState = CharacterState.SWIMMING;
            currentSpeed = 0;
        }
    }

    public void SwitchOutOfSwimState()
    {
        if (characterState == CharacterState.SWIMMING || characterState == CharacterState.SWIMMING_STROKE_FORWARD || characterState == CharacterState.SWIMMING_STROKE_UP)
        {
            characterState = CharacterState.RUNNING;
        }
    }

}
