using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class masterScript : MonoBehaviour
{
	// this script will handle universal game interactions
	// it will keep track of in game scores, health, progress, etc.

	public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetKeyDown(KeyCode.I)){
			InjurePlayer(1);
		}
    }

	public void InjurePlayer(int damage){
        player.GetComponent<UpdatedCharacterControllerScript>().InjureCharacter(damage);
	}
}
