using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneManager : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject openCaveDoorCustSceneCamera;
    public GameObject caveDoor;
    public GameObject player;
    private UpdatedCharacterControllerScript characterScript;

    // Start is called before the first frame update
    void Start()
    {
        characterScript = player.GetComponent<UpdatedCharacterControllerScript>();
    }

    public void StartOpenCaveDoorCutScene()
    {
        StartCoroutine(OpenCaveDoorCutSceneSequence());
    }


    IEnumerator OpenCaveDoorCutSceneSequence()
    {
        yield return new WaitForSeconds(2); // small delay to let the player know a cut scene is beginning (include sond effect)
        characterScript.AcceptInput(false);
        //openCaveDoorCustSceneCamera.SetActive(true);
        //openCaveDoorCustSceneCamera.GetComponent<cameraController>().enabled = false;
        //openCaveDoorCustSceneCamera.transform.position = 
        //mainCamera.SetActive(false);
        mainCamera.GetComponent<cameraController>().enabled = false;
        mainCamera.transform.position = openCaveDoorCustSceneCamera.transform.position;
        mainCamera.transform.forward = openCaveDoorCustSceneCamera.transform.forward;

        caveDoor.GetComponent<openDoor>().OpenDoor();

        yield return new WaitForSeconds(2);

        //mainCamera.SetActive(true);
        mainCamera.GetComponent<cameraController>().enabled = true;
        //openCaveDoorCustSceneCamera.SetActive(false);
        characterScript.AcceptInput(true);
        // can also set active/inactive other objects like the player
    }
}
