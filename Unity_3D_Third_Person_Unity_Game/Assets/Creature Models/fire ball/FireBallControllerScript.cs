using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallControllerScript : MonoBehaviour
{
    private int includeAllButPoles = LayerMaskCollection.includeAllButPoles;
    private int onlyPlayer = LayerMaskCollection.onlyPlayer;

    public float speed = 0.1f; // if the random is 10 then we will move 10 units in total in 0.1f increments
    public float maxAngleChange = 180.0f;
    public Vector2 distanceAllowed = new Vector2(0.0f, 20.0f);
    public Vector2 idleTime = new Vector2(0, 200);

    public State state = State.IDLE;

    private float arcRatio = (2 * Mathf.PI / 360);
    public float SlipperyFloorWall = 80; // anything below is slippery, above is wall
    private float floorWallNormalY;
    private float verticalRaycastOffset;
    private float distanceDownward;

    private Vector3 rotationVector;
    private bool useMovementVector = true;
    private bool useY = true;

    public int stateTimer; // each state will set this value

    // Start is called before the first frame update
    void Start()
    {
        floorWallNormalY = Mathf.Sin((90 - SlipperyFloorWall) * arcRatio); // slippery is > 0.174
        verticalRaycastOffset = speed * Mathf.Tan(SlipperyFloorWall * arcRatio);
        distanceDownward = verticalRaycastOffset * 2 + 0.01f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {


        if(state == State.WALK)
        {
            WalkState();
        }
        else if(state == State.IDLE)
        {
            IdleState();
        }
        else if(state == State.TARGET)
        {
            TargetState();
        }

        useMovementVector = true;
        useY = true;

        PerformRaycasts();

        if (useMovementVector && state != State.IDLE)
        {
            transform.Translate(transform.forward * speed, Space.World);
        }
        if (useY)
        {
            transform.Translate(new Vector3(0, -0.01f, 0), Space.World);
        }
        transform.Rotate(rotationVector);

    }

    void PerformRaycasts()
    {
        RaycastHit forward;
        if (Physics.Raycast(transform.position + new Vector3(0, verticalRaycastOffset + 0.01f, 0), transform.forward, out forward, speed, includeAllButPoles))
        {
            // we hit a surface confirm it is a wall
            if (forward.normal.y >= -floorWallNormalY && forward.normal.y <= floorWallNormalY)
            {
                // a wall has been found
                useMovementVector = false;
            }
        }

        bool allowedToMoveForward = true;

        RaycastHit downwardTruncate;
        if (Physics.Raycast(transform.position + new Vector3(0, verticalRaycastOffset + 0.01f, 0), -transform.up, out downwardTruncate, distanceDownward, includeAllButPoles))
        {
            if (downwardTruncate.normal.y <= floorWallNormalY)
            {
                // no floor found we must fall
                useMovementVector = false;
                allowedToMoveForward = false;
            }
            else
            {
                transform.position = new Vector3(transform.position.x, downwardTruncate.point.y, transform.position.z); // truncate us to the floor
                useY = false;
            }
        }
        else
        {
            // no floor found we must fall
            useMovementVector = false;
            allowedToMoveForward = false;
        }

        if (allowedToMoveForward)
        {
            RaycastHit downwardAllowedToMoveForward_Forward;
            if (Physics.Raycast(transform.position + new Vector3(0, verticalRaycastOffset + 0.01f, 0) + transform.forward * (speed + 1.1f), -transform.up, out downwardAllowedToMoveForward_Forward, distanceDownward, includeAllButPoles))
            {
                if (downwardAllowedToMoveForward_Forward.normal.y <= floorWallNormalY)
                {
                    // no floor found ahead
                    useMovementVector = false;
                    stateTimer--;
                }
                else
                {
                    // we can proceed
                }
            }
            else
            {
                // no floor found ahead
                useMovementVector = false;
                stateTimer--;
            }

            
            Debug.DrawRay(transform.position + new Vector3(0, verticalRaycastOffset + 0.01f, 0) - transform.right * (speed + 1f), -transform.up * 10, Color.red);
            Debug.DrawRay(transform.position + new Vector3(0, verticalRaycastOffset + 0.01f, 0) + transform.right * (speed + 1f), -transform.up * 10, Color.red);


            RaycastHit downwardAllowedToMoveForward_Left;
            if (Physics.Raycast(transform.position + new Vector3(0, verticalRaycastOffset + 0.01f, 0) - transform.right * (speed + 1f), -transform.up, out downwardAllowedToMoveForward_Left, distanceDownward, includeAllButPoles))
            {
                if (downwardAllowedToMoveForward_Left.normal.y <= floorWallNormalY)
                {
                    // no floor found ahead
                    useMovementVector = false;
                    stateTimer--;
                }
                else
                {
                    // we can proceed
                }
            }
            else
            {
                // nothing found below
                useMovementVector = false;
                stateTimer--;
            }

            RaycastHit downwardAllowedToMoveForward_Right;
            if (Physics.Raycast(transform.position + new Vector3(0, verticalRaycastOffset + 0.01f, 0) + transform.right * (speed + 1f), -transform.up, out downwardAllowedToMoveForward_Right, distanceDownward, includeAllButPoles))
            {
                if (downwardAllowedToMoveForward_Right.normal.y <= floorWallNormalY)
                {
                    // no floor found ahead
                    useMovementVector = false;
                    stateTimer--;
                }
                else
                {
                    // we can proceed
                }
            }
            else
            {
                // nothing found below
                useMovementVector = false;
                stateTimer--;
            }
        }
    }

    void WalkState()
    {
        if(stateTimer <= 0)
        {
            // time to change state
            state = State.IDLE;
            stateTimer = Random.Range((int)idleTime.x, (int)idleTime.y);
            rotationVector = Vector3.zero;
        }
        else
        {
            stateTimer--;
        }
    }

    void IdleState()
    {
        if (stateTimer <= 0)
        {
            SwitchToWalkState();
        }
        else
        {
            stateTimer--;
        }
    }

    void SwitchToWalkState()
    {
        state = State.WALK;
        float distance = Random.Range(distanceAllowed.x, distanceAllowed.y);
        float angle = Random.Range(-maxAngleChange, maxAngleChange);
        stateTimer = (int)(distance / speed); // the amount of time we will spend doing this motion
        if (stateTimer == 0)
        {
            stateTimer = 1;
        }
        rotationVector = new Vector3(0, angle / stateTimer, 0);
    }

    void TargetState()
    {
        // for now we just run in the direction we saw the player and after reaching that
        // spot we do a rotating while running state and then back to walk around
    }

    public enum State
    {
        IDLE,
        WALK,
        TARGET
    }
}
