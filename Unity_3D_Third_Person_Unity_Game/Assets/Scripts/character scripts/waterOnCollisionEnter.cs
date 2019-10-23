using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterOnCollisionEnter : MonoBehaviour
{
    public GameObject player;
    NewCharacterControllerScript script;
    public AudioClip OnEnterWater;
    public GameObject camera;

    // Start is called before the first frame update
    void Start()
    {
        script = player.GetComponent<NewCharacterControllerScript>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            camera.GetComponent<cameraAudioManager>().switchBackgroundMusic(OnEnterWater);
        }
    }

    void OnTriggerStay(Collider other)
    {
        //Debug.Log("collision");
        if (other.tag == "Player")
        {
            if (other.transform.position.y + 2.5f < transform.position.y)
            {
                // we are atleast 2.5f deep
                script.SwitchIntoSwimState(transform.position.y);
            }
            else
            {
                // we are in the water but not deep enough
                script.SwitchOutOfSwimState();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            script.SwitchOutOfSwimState();
        }
    }
}