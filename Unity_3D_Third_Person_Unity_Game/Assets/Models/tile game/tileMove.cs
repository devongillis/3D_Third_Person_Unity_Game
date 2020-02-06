using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tileMove : MonoBehaviour
{
    // Start is called before the first frame update

    int tileNumber;
    int position; // the integer position 0-15 within the grid
    bool moveRequested = false;
    Vector3 newPosition;
    Vector3 oldPosition;
    float factor = 0.0f;
    float increment = 0.05f;

    void Start()
    {
        //oldPosition = transform.localPosition;
    }

    public void AssignValues(int TN, int P)
    {
        tileNumber = TN;
        position = P;
        oldPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (moveRequested)
        {
            factor += increment;
            transform.localPosition = Vector3.Lerp(oldPosition, newPosition, factor);
            if(factor >= 1.0f)
            {
                factor = 0;
                moveRequested = false;
                oldPosition = transform.localPosition;
                transform.parent.GetComponent<tileMaster>().CheckIfSolved();
                transform.parent.GetComponent<tileMaster>().inuse = false;
            }
        }
    }

    public void DisableTile()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).tag != "Player")
            {
                transform.GetChild(i).GetComponents<BoxCollider>()[1].enabled = false;
            }
        }
        
    }
    
    // when calling the move permission function check if return is vector3.zero which means no
    public void RequestMove(int direction)
    {
        Vector3 pos = transform.parent.GetComponent<tileMaster>().MoveTile(tileNumber, position, direction);
        newPosition = new Vector3(pos.x, transform.localPosition.y, pos.z);
        position = (int)pos.y;
        moveRequested = true;
    }
}
