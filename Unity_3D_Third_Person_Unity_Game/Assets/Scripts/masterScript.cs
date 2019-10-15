using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class masterScript : MonoBehaviour
{
	// this script will handle universal game interactions
	// it will keep track of in game scores, health, progress, etc.

	public int playerHealth = 8;
	public int maxPlayerHeath = 8;
	public int minPlayerHealth = 0;

	public GameObject healthBar;
	public healthBarScript healthBarScript;

    // Start is called before the first frame update
    void Start()
    {
		healthBarScript = healthBar.GetComponent<healthBarScript>();
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetKeyDown(KeyCode.I)){
			InjurePlayer(1);
		}
    }

	public void InjurePlayer(int damage){
		playerHealth -= damage;
		if(playerHealth <= minPlayerHealth){
			playerHealth = minPlayerHealth;
			KillPlayer();
		}
		healthBarScript.UpdateHealth(playerHealth);

	}

	public void KillPlayer(){
		// this function will kill the player it can be called by any game object
		// such as lava, deadzones, enemies, etc.
		// it is also called by the health monitor if players health drops to zero

	}
}
