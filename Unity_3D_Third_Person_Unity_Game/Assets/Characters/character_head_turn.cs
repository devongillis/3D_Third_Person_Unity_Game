using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character_head_turn : MonoBehaviour
{
    private Animator anim;
    public int upper = 100;
    public UpdatedCharacterControllerScript script;
    public int time = 100;
    public int timer = 0;


    void Start()
    {
        anim = gameObject.GetComponentInChildren<Animator>();
        script = gameObject.GetComponent<UpdatedCharacterControllerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!script.IsStationary())
        {
            anim.SetTrigger("head turn interupt");
            timer = 0;
        }
        else
        {
            if (timer <= 0)
            {
                int digit = Random.Range(0, upper);
                if (digit == 0)
                {
                    anim.SetTrigger("head turn left");
                    timer = time;
                }
                else if (digit == 1)
                {
                    anim.SetTrigger("head turn right");
                    timer = time;
                }
            }
            else
            {
                timer--;
            }
        }
    }
}
