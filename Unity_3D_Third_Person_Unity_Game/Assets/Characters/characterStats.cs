using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterStats : MonoBehaviour
{
    public GameObject canvas;
    public GameObject masterObject;

    public int health;
    public int maxHealth;
    public healthBarScript healthBarScript;

    public int diamondAbsoluteValue;
    public int diamondDisplayedValue;
    public const int diamondValueGoal = 50;
    public const int caveDoorOpenObjectiveID = 1;
    public diamondValueScript diamondValueScript;

    public int frameCounter = 0;
    public int frameCount = 16; // some updates we want slower maybe update every 2,4, etc frames

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        healthBarScript = canvas.transform.Find("HealthBar").GetComponent<healthBarScript>();

        diamondAbsoluteValue = 0;
        diamondDisplayedValue = 0;
        diamondValueScript = canvas.transform.Find("DiamondImage").Find("Text").GetComponent<diamondValueScript>();
    }

    // Update is called once per frame
    void Update()
    {
        frameCounter++;
        if(frameCounter >= frameCount)
        {
            frameCounter = 0;
        }

        if (frameCounter % 2 == 0)
        {
            if (diamondDisplayedValue < diamondAbsoluteValue)
            {
                diamondDisplayedValue++;
                diamondValueScript.UpdateValue(diamondDisplayedValue);
            }
        }
    }

    public void IncrementDiamondValueCount(int value)
    {
        // what we really want is to show the value going up
        // so each while the actual value is instantanously updated, the display should only update
        // by one per frame

        diamondAbsoluteValue += value;
        CheckForObjectiveCompleted();
        //diamondValueScript.UpdateValue(diamonds);
    }

    public void CheckForObjectiveCompleted()
    {
        if(diamondAbsoluteValue >= diamondValueGoal)
        {
            masterObject.GetComponent<masterScript>().ObjectiveCompleted(caveDoorOpenObjectiveID);
        }
    }

    public bool UpdatePlayerHealth_IsDead(int injury)
    {
        health -= injury;
        health = Mathf.Max(health, 0);
        healthBarScript.UpdateHealth(health);
        if(health == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
