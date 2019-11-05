using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class transitionTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public string newSceneName;
    public bool additive = false; // if you want the original scene still loaded (character is just entering a small room) then set true
    bool transitionCalled = false;
    public GameObject theCamera;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeIntoNewScene()
    {
        // play some type of animation
        // fade out the screen
        // lower music volume
        theCamera.GetComponent<cameraAudioManager>().EndBackgroundMusic();
        // save data if applicable
        // disable inputs
        StartCoroutine(SwitchScene());
    }

    IEnumerator SwitchScene()
    {
        yield return new WaitForSeconds(3);
        if (additive)
        {
            SceneManager.LoadScene(newSceneName, LoadSceneMode.Additive);
        }
        else
        {
            SceneManager.LoadScene(newSceneName, LoadSceneMode.Single);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && !transitionCalled)
        {
            transitionCalled = true;
            FadeIntoNewScene();
        }
    }
}
