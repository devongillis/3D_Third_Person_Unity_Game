using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraAudioManager : MonoBehaviour
{
    // this script is responsible for playing audio files at the camera
    // it accepts calls to change its default background music (through transition)
    // or play an additional sound effect regarding the player
    // conceptually only one audio source

    public AudioSource[] background;
    public int sourcePlaying = 0; // points to which source is dominate for background
    public float fadeTime = 2.0f; 
    float timeGoneBy = 0.0f;
    public bool transition = false;
    public AudioClip defaultSource;

    // Start is called before the first frame update
    void Start()
    {
        background = GetComponents<AudioSource>();
        StartBackgroundMusic(defaultSource);
    }

    // Update is called once per frame
    void Update()
    {
        if (transition)
        {
            // we are transitioning from one source to another
            TransitionBackground();
        }
    }

    public void StartBackgroundMusic(AudioClip clip)
    {
        // this is called upon entering a world and an audio source has not started yet
        background[0].clip = clip;
        background[0].volume = 0.0f;
        background[0].Play();
        //background[0].volume = 0.0f;
        sourcePlaying = 0;
        transition = true;
        //Debug.Log("entered" + transition);
    }

    public void switchBackgroundMusic(AudioClip clip)
    {
        if (background[sourcePlaying].clip != clip)
        {
            // new clip request
            background[1 - sourcePlaying].Stop();
            background[1 - sourcePlaying].clip = clip;
            background[1 - sourcePlaying].volume = 0.0f;
            background[sourcePlaying].volume = 0.5f;
            background[1 - sourcePlaying].Play();
            sourcePlaying = 1 - sourcePlaying;
            // now transition the other source
            transition = true;
        }
    }
    /*
    void TransitionBackground()
    {
        //Debug.Log("transitioning" + background[sourcePlaying].volume);
        // the source that should be increasing in volume is the pointer
        volumeIncrement += (1.0f / (fadeTime * 2)) * Time.deltaTime;
        background[sourcePlaying].volume += volumeIncrement; // audio volume ranges from 0.0 to 1.0
        background[1 - sourcePlaying].volume -= volumeIncrement;
        if (background[sourcePlaying].volume >= 0.5f)
        {
            background[sourcePlaying].volume = 1.0f;
            background[1 - sourcePlaying].volume = 0.0f;
            // transition complete
            transition = false;
        }
    }
    */
    void TransitionBackground()
    {
        // if fade time = 6 seconds then 6/3 = 2 thus every 2 seconds the volume is 1/2 (1 - 0.1), and the
        // volume is gone after becoming half 3 times
        timeGoneBy += Time.deltaTime;
        background[1- sourcePlaying].volume = 1 / Mathf.Pow(10, timeGoneBy/(fadeTime/3));
        background[sourcePlaying].volume = 0.001f / background[1 - sourcePlaying].volume;
        
        if (background[sourcePlaying].volume >= 0.99f)
        {
            background[1 - sourcePlaying].volume = 0.0f;
            background[sourcePlaying].volume = 1.0f;
            transition = false;
            timeGoneBy = 0.0f;
        }
    }
}
