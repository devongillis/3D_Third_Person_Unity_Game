using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockTwo : MonoBehaviour
{

    public float speed = 0.001f;
    public float rotationSpeed = 4.0f;
    public Vector3 averageHeading;
    public Vector3 averagePosition;
    public float neighbourDistance = 3.0f;

    // Use this for initialization
    void Start()
    {
        speed = Random.Range(0.5f, 1f);
    }

    // Update is called once per frame
    void Update()
    {

        if (Random.Range(0, 5) < 1)
        {
            ApplyRules();
        }

        transform.Translate(0, 0, Time.deltaTime * speed);
    }

    void ApplyRules()
    {
        GameObject[] gos;
        gos = GlobalFlockTwo.allFish;

        Vector3 vCenter = GlobalFlockTwo.tankPosition;
        Vector3 vAvoid = Vector3.zero;
        float gSpeed = 0.1f;

        Vector3 goalPos = GlobalFlockTwo.goalPos;

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

                    FlockTwo anotherFlock = go.GetComponent<FlockTwo>();
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
