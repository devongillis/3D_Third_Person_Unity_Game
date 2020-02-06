using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkGrowPlatform : MonoBehaviour
{
    // Start is called before the first frame update
    public int transitionTimer = 30;
    public int transitionTime = 30;
    public State state = State.SMALL;

    public float largeScale;
    public float smallScale;

    public GameObject forwardTarget;
    public GameObject backwardTarget;
    public GameObject startTarget;

    public int selfRemoveTimer = 30;
    public int selfRemoveTime = 30;

    public int selfRespawnTimer = 30;
    public int selfRespawnTime = 30;

    public int ID;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(state == State.GROWING)
        {
            Grow();
        }
        else if (state == State.SHRINKING)
        {
            Shrink();
        }
    }

    public void StartGrowth()
    {
        if (state != State.LARGE)
        {
            state = State.GROWING;
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<MeshCollider>().enabled = true;
            transitionTimer = transitionTime;
        }
    }

    public void StartShrink()
    {
        if (state != State.SMALL)
        {
            state = State.SHRINKING;
            transitionTimer = transitionTime;
        }
    }

    public void Grow()
    {
        // this function when called will grow the platform to the correct size
        if (transitionTimer <= 0)
        {
            state = State.LARGE;
        }
        else
        {
            transform.localScale = Mathf.Lerp(largeScale, smallScale, (float)transitionTimer/transitionTime) * new Vector3(1, 1, 1);
            transitionTimer--;
        }
    }

    public void Shrink()
    {
        if (transitionTimer <= 0)
        {
            state = State.SMALL;
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<MeshCollider>().enabled = false;
        }
        else
        {
            transform.localScale = Mathf.Lerp(smallScale, largeScale, (float)transitionTimer / transitionTime) * new Vector3(1, 1, 1);
            transitionTimer--;
        }
    }

    public enum State
    {
        GROWING,
        SHRINKING,
        LARGE,
        SMALL
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("entered");
        if (other.tag == "Player")
        {
            transform.parent.GetComponent<ShrinkGrowMaster>().PlayerEnterPlatform(ID);
        }
    }

    void OnTriggerExit(Collider other)
    {
        //Debug.Log("exit");
        if (other.tag == "Player")
        {
            transform.parent.GetComponent<ShrinkGrowMaster>().PlayerExitPlatform();
        }
    }
}
