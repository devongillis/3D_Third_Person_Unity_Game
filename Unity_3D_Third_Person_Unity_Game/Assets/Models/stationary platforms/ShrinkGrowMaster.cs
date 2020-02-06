using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkGrowMaster : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] platforms;

    public int resetTimer = 60;
    public int resetTime = 60;
    public bool resetCountDown = false;

    void Start()
    {
        platforms[0] = transform.GetChild(0).gameObject;
        platforms[0].GetComponent<ShrinkGrowPlatform>().ID = 0;
        for (int i = 1; i < platforms.Length; i++)
        {
            platforms[i] = transform.GetChild(i).gameObject;
            platforms[i].GetComponent<ShrinkGrowPlatform>().ID = i;
            platforms[i].GetComponent<MeshCollider>().enabled = false;
            platforms[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (resetCountDown)
        {
            if (resetTimer <= 0)
            {
                // reset positions
                resetCountDown = false;
                platforms[0].GetComponent<ShrinkGrowPlatform>().StartGrowth();
                for(int i = 1; i < platforms.Length; i++)
                {
                    platforms[i].GetComponent<ShrinkGrowPlatform>().StartShrink();
                }
            }
            else
            {
                resetTimer--;
            }
        }
    }

    public void PlayerEnterPlatform(int ID)
    {
        resetCountDown = false;
        // we need to shrink the previous platform and grow the next one
        if(ID < platforms.Length - 1)
        {
            platforms[ID + 1].GetComponent<ShrinkGrowPlatform>().StartGrowth();
        }
        if (ID > 0)
        {
            platforms[ID - 1].GetComponent<ShrinkGrowPlatform>().StartShrink();
        }
    }

    public void PlayerExitPlatform()
    {
        // called by each platform upon player leaving it
        // it starts a count down to which the entire platform layout
        // resets unless another platform is collided with
        resetTimer = resetTime;
        resetCountDown = true;
    }
}
