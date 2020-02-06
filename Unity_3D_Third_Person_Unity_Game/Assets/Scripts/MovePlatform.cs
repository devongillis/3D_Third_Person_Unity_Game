using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlatform : MonoBehaviour
{
    public Vector3 translate = new Vector3(0, 0, 0);
    public float speed = 0.0f;
    bool move = false;
    bool playerOnTop = false;
    public float maxDistance = 40;
    public Vector3 initialPosition;
    public bool movingUp = true;
    // Start is called before the first frame update
    // for platforms that move up and down you want it
    void Start()
    {
        initialPosition = transform.position;
        translate.Normalize();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (move)
        {
            if (movingUp)
            {
                transform.Translate(translate * speed);
                if (Vector3.Distance(initialPosition, transform.position) >= maxDistance)
                {
                    movingUp = false;
                }
            }
            else
            {
                if (Vector3.Distance(initialPosition, transform.position) <= speed)
                {
                    movingUp = true;
                    transform.position = initialPosition;
                    if (!playerOnTop)
                    {
                        move = false;
                    }
                }
                else
                {
                    transform.Translate(-translate * speed);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerOnTop = true;
            move = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerOnTop = false;
        }
    }

}
