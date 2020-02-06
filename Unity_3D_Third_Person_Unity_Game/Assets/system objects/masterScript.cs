using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class masterScript : MonoBehaviour
{
	// this script will handle universal game interactions
	// it will keep track of in game scores, health, progress, etc.

	public GameObject player;
    public CutSceneManager cutSceneManager;
    public bool[] objectiveCompletedList;
    public bool caveDoorOpened = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cutSceneManager = GetComponent<CutSceneManager>();
        objectiveCompletedList = new bool[3];
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetKeyDown(KeyCode.I)){
			InjurePlayer(1);
		}
    }

	public void InjurePlayer(int damage){
        //player.GetComponent<UpdatedCharacterControllerScript>().InjureCharacter(damage, player.transform.position - new Vector3(0, 1, 0), false);
        player.SendMessage("InjureCharacter", new AttackData(damage, transform.position - new Vector3(0, 1, 0), false));
    }

    public void ObjectiveCompleted(int objectiveID)
    {
        objectiveCompletedList[objectiveID] = true;
        if (!caveDoorOpened)
        {
            CheckForCaveDoorOpenConditions();
        }
    }

    public void CheckForCaveDoorOpenConditions()
    {
        bool open = true;
        for(int i = 0; i < objectiveCompletedList.Length; i++)
        {
            if(objectiveCompletedList[i] == false)
            {
                open = false;
            }
        }
        if (open)
        {
            cutSceneManager.StartOpenCaveDoorCutScene();
            caveDoorOpened = true;
        }
    }
}
