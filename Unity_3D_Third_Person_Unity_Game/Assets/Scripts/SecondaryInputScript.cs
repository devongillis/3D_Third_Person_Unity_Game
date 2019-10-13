using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryInputScript : MonoBehaviour
{
	// this script checks for double tap inputs


	public float time = 0.5f;
	float timer = 0.0f;

	public int ticks = 10;
	int ticker = 0;

	character_script_controller script;

	ButtonClick button = ButtonClick.BUTTON_NOT_FIRST_PRESS;
    void Start()
    {
		GameObject player = GameObject.Find("character_astronaut");
		script = player.GetComponent<character_script_controller>();
		//script
    }

    // Update is called once per frame
    void Update()
    {
		if(button == ButtonClick.BUTTON_NOT_FIRST_PRESS){
			// we can check for input
			if(Input.GetAxisRaw("Vertical") > 0.0f){
				button = ButtonClick.BUTTON_IS_PRESSED;
			}
		}
		else if(button == ButtonClick.BUTTON_IS_PRESSED){
			// we can check for no input
			if(Input.GetAxisRaw("Vertical") <= 0.0f){
				button = ButtonClick.FIRST_CLICK;
				//ticker = ticks;
				timer = time;
			}
		}
		else if(button == ButtonClick.FIRST_CLICK){
			// we can check for input
			if(Input.GetAxisRaw("Vertical") > 0.0f){
				button = ButtonClick.SECOND_PRESS;
				//Debug.Log("second click");
			}
			else{
				//ticker--;
				timer -= Time.deltaTime;
				if(timer <= 0.0f){
					button = ButtonClick.BUTTON_NOT_FIRST_PRESS;
					//Debug.Log("too late");
				}
			}
		}
		else{
			if(Input.GetAxisRaw("Vertical") <= 0.0f){
				// button let go
				button = ButtonClick.BUTTON_NOT_FIRST_PRESS;
				Debug.Log("released");
			}
		}

		if(button == ButtonClick.SECOND_PRESS){
			script.running = true;
		}
		else{
			script.running = false;
		}
    }

	enum ButtonClick{
		BUTTON_NOT_FIRST_PRESS, // the default state, if timer goes out or button released in second press
		BUTTON_IS_PRESSED, // button has been pressed for first time
		FIRST_CLICK, // only set when the act of releasing a button is observed
		SECOND_PRESS // only set when in first click, and button is pressed down
	}

}
