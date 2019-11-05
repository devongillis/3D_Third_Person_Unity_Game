using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    
    public bool turning = false;

    public float speed = 0.001f;
    public Vector2 speedRange = new Vector2(0.5f, 1.0f);
    public float rotationSpeed = 4.0f;
    public float neighbourDistance = 3.0f;

    public Vector3 averageHeading;
    public Vector3 averagePosition;
    public Vector3 tankDimensions;
    public Vector3 tankPosition; // the position of the tank in world space

    public GameObject manager;

    // Use this for initialization
    void Start()
    {
        speed = Random.Range(speedRange.x, speedRange.y);
    }

    public void AssignTheTank(GameObject m)
    {
        this.manager = m;
        tankPosition = manager.transform.position;
        tankDimensions = manager.GetComponent<GlobalFlock>().tankDimensions;
    }

    // Update is called once per frame
    void Update()
    {

        if (Mathf.Abs(transform.position.x - tankPosition.x) >= tankDimensions.x)
        {
            turning = true;
        }
        else if (Mathf.Abs(transform.position.y - tankPosition.y) >= tankDimensions.y)
        {
            turning = true;
        }
        else if (Mathf.Abs(transform.position.z - tankPosition.z) >= tankDimensions.z)
        {
            turning = true;
        }
        else
        {
            turning = false;
        }

        if (turning)
        {
            Vector3 direction = tankPosition - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            speed = Random.Range(speedRange.x, speedRange.y);
        }
        else
        {
            if (Random.Range(0, 5) < 1)
            {
                ApplyRules();
            }
        }
        transform.Translate(0, 0, Time.deltaTime * speed);
    }

    void ApplyRules()
    {
        GameObject[] gos;
        gos = manager.GetComponent<GlobalFlock>().allFish;

        Vector3 vCenter = tankPosition;
        Vector3 vAvoid = Vector2.zero;
        float gSpeed = 0.1f;

        Vector3 goalPos = manager.GetComponent<GlobalFlock>().goalPos;

        float dist;

        int groupSize = 0;
        foreach (GameObject go in gos)
        {
            if (go != this.gameObject)
            {
                dist = Vector3.Distance(go.transform.position, this.transform.position);
                if (dist <= neighbourDistance)
                {
                    vCenter += go.transform.position;
                    groupSize++;

                    if (dist < 1.0f)
                    {
                        vAvoid = vAvoid + (this.transform.position - go.transform.position);
                    }

                    Flock anotherFlock = go.GetComponent<Flock>();
                    gSpeed = gSpeed + anotherFlock.speed;
                }
            }
        }

        if (groupSize > 0)
        {
            vCenter = vCenter / groupSize + (goalPos - this.transform.position);
            speed = gSpeed / groupSize;

            Vector3 direction = (vCenter + vAvoid) - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            }
        }
    }
    
}
