using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explosion : MonoBehaviour
{
    // this script will after a trigger will count down to zero
    // and then instantly activate the sphere collider which for any object with
    // health will be injured by, the explosion will linger for a few frames and
    // then the object will die out

    public int count = 10;
    public int counter = 0;
    public bool beginCount = false;

    public int lingerCount = 10;
    public int lingerCounter = 0;
    public bool beginLinger = false;

    // Start is called before the first frame update
    void Start()
    {
        beginCount = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (beginCount)
        {
            counter++;
        }
        if(counter >= count)
        {
            gameObject.GetComponent<SphereCollider>().enabled = true;
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            beginLinger = true;
        }
        if (beginLinger)
        {
            lingerCounter++;
        }
        if(lingerCounter >= lingerCount)
        {
            gameObject.SetActive(false);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("yes");
        //gameObject.SetActive(false);
        if(other.tag == "Player")
        {
            //other.GetComponent<UpdatedCharacterControllerScript>().InjureCharacter(1, transform.position, true);
            other.SendMessage("InjureCharacter", new AttackData(1, transform.position, false));
        }
    }
}
