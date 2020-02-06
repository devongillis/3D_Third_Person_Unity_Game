using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class propellerInjurePlayer : MonoBehaviour
{
    // Start is called before the first frame update
    public RotatePlatform script;
    public float fastSpeed = 6.0f;
    public float slowSpeed = 0.5f;

    public PropellerState state = PropellerState.FAST;

    public float fastStateTimer = 180;
    public float fastStateTime = 180;

    public float slowStateTimer = 180;
    public float slowStateTime = 180;

    public int transitionTimer = 180;
    public int transitionTime = 180;

    void Start()
    {
        script = GetComponent<RotatePlatform>();
    }

    void FixedUpdate()
    {
        if(state == PropellerState.FAST)
        {
            FastState();
        }
        else if(state == PropellerState.SLOW_DOWN)
        {
            SlowDownState();
        }
        else if (state == PropellerState.SLOW)
        {
            SlowState();
        }
        else if (state == PropellerState.SPEED_UP)
        {
            SpeedUpState();
        }
    }

    // because the player has a trigger capsule this function will be called regardless whether this mesh collider is set as a trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && script.rotationSpeed >= (fastSpeed - slowSpeed) / 2 + slowSpeed)
        {
            Vector3 trans = other.transform.position - transform.position;
            Vector3 forward = transform.forward;
            trans.y = 0;
            forward.y = 0;
            if (Vector3.Dot(trans, forward) >= 0)
            {
                Vector3 pos = other.transform.position - transform.forward;
                pos.y = other.transform.position.y;
                other.SendMessage("InjureCharacter", new AttackData(1, pos, false));
            }
            else
            {
                Vector3 pos = other.transform.position + transform.forward;
                pos.y = other.transform.position.y;
                other.SendMessage("InjureCharacter", new AttackData(1, pos, false));
            }
        }
    }


    void SpeedUpState()
    {
        if (transitionTimer <= 0)
        {
            fastStateTimer = fastStateTime;
            state = PropellerState.FAST;
        }
        else
        {
            script.rotationSpeed = Mathf.Lerp(fastSpeed, slowSpeed, (float)transitionTimer / transitionTime);
            transitionTimer--;
        }
    }

    void SlowDownState()
    {
        if (transitionTimer <= 0)
        {
            slowStateTimer = slowStateTime;
            state = PropellerState.SLOW;
        }
        else
        {
            script.rotationSpeed = Mathf.Lerp(slowSpeed, fastSpeed, (float)transitionTimer / transitionTime);
            transitionTimer--;
        }
    }

    void FastState()
    {
        if(fastStateTimer <= 0)
        {
            transitionTimer = transitionTime;
            state = PropellerState.SLOW_DOWN;
        }
        else
        {
            fastStateTimer--;
        }
    }

    void SlowState()
    {
        if(slowStateTimer <= 0)
        {
            transitionTimer = transitionTime;
            state = PropellerState.SPEED_UP;
        }
        else
        {
            slowStateTimer--;
        }
    }

    public enum PropellerState
    {
        FAST,
        SLOW_DOWN,
        SLOW,
        SPEED_UP
    }
}
