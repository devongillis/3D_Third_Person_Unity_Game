using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openDoor : MonoBehaviour
{
    // Start is called before the first frame update
    bool openRequested = false;
    bool doorOpen = false;
    Vector3 newPosition;
    Vector3 oldPosition;
    float factor = 0.0f;
    public float moveByY;
    public float increment = 0.05f;

    void Start()
    {
        oldPosition = transform.localPosition;
        newPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + moveByY, transform.localPosition.z);
        //OpenDoor();
    }

    // Update is called once per frame
    void Update()
    {
        if (openRequested && !doorOpen)
        {
            factor += increment;
            transform.localPosition = Vector3.Lerp(oldPosition, newPosition, factor);
            //transform.Translate(new Vector3(0, -increment, 0));
            if (factor >= 1.0f)
            {
                //factor = 0;
                //openRequested = false;
                doorOpen = true;
            }
        }
    }

    public void OpenDoor()
    {
        if (!openRequested)
        {
            openRequested = true;
        }
    }
}
