using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterStats : MonoBehaviour
{
    public int health;
    public int maxHealth;
    public GameObject canvas;
    public healthBarScript healthBar;
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        healthBar = canvas.transform.Find("HealthBar").GetComponent<healthBarScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool UpdatePlayerHealth_IsDead(int injury)
    {
        health -= injury;
        health = Mathf.Max(health, 0);
        healthBar.UpdateHealth(health);
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
