using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneManager : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject caveDoor;
    public GameObject player;
    private UpdatedCharacterControllerScript characterScript;

    public Vector3 caveDoorCameraPosition;
    public Vector3 caveDoorCameraRotation;

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
        mainCamera.GetComponent<cameraController>().enabled = false;
        mainCamera.transform.position = caveDoorCameraPosition;
        mainCamera.transform.forward = Quaternion.Euler(caveDoorCameraRotation) * Vector3.forward;
        caveDoor.GetComponent<openDoor>().OpenDoor();

        yield return new WaitForSeconds(2);

        mainCamera.GetComponent<cameraController>().enabled = true;
        characterScript.AcceptInput(true);
        // can also set active/inactive other objects like the player
    }
}
