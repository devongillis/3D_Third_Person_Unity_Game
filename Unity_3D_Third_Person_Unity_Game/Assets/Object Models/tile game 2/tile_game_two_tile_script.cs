using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tile_game_two_tile_script : MonoBehaviour
{
    // Start is called before the first frame update
    bool resetCalled = false;
    bool flipRequested = false;
    bool triggerDisabled = false; // set true during and after flip, only set to false if tile is reset by master
    int ID; // the ID (1-16) of the tile

    int angle = 180; // the current angle the tile is flipped by
    int increment = 0; // keeping track of how many angles we rotated by in a given flip call
    int speed = 10;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (flipRequested)
        {
            angle += speed;
            if(angle >= 360)
            {
                angle = 0;
            }
            increment += speed;
            transform.localRotation = Quaternion.Euler(angle, 0, 0);
            
            if(increment >= 180)
            {
                flipRequested = false;
                transform.GetComponentInParent<tile_two_game_master>().CheckIfPairMatched();
                transform.GetComponentInParent<tile_two_game_master>().tileIsCurrentlyFlipping = false;
                increment = 0;
                if (resetCalled)
                {
                    triggerDisabled = false;
                    resetCalled = false;
                }
            }
        }
    }

    public void SetID(int id)
    {
        ID = id;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggerDisabled && other.tag == "Player")
        {
            if (other.GetComponent<UpdatedCharacterControllerScript>().isGroundPounding())
            {
                // player has selected this tile to flip, we call the master to inform the flip and
                // only flip if given permission, this is done so that two tiles don't flip at the same time
                if (transform.GetComponentInParent<tile_two_game_master>().AllowedToFlip(ID))
                {
                    // we are allowed to flip, deactivate the trigger and flip
                    triggerDisabled = true;
                    flipRequested = true;
                }
            }
        }
    }

    public void ResetTile()
    {
        StartCoroutine(ExecuteAfterTime(0.5f));
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        resetCalled = true;
        flipRequested = true;
        triggerDisabled = true; // technically trigger should already be disabled
    }

    public void DisableTrigger()
    {
        triggerDisabled = true;
    }
}
