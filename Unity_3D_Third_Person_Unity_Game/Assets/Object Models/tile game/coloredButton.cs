using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coloredButton : MonoBehaviour
{

    public int direction = -1;
    public int counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(counter > 0)
        {
            counter--;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("touched" + direction);
        if (counter <= 0)
        {
            counter = 30;
            if (other.tag == "Player")
            {
                if (other.GetComponent<UpdatedCharacterControllerScript>().isGroundPounding())
                {
                    //Debug.Log("pounded" + direction);
                    transform.parent.GetComponent<tileMove>().RequestMove(direction);
                }
            }
        }
    }
}
