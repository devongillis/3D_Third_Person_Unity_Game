using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterPlayAudio : MonoBehaviour
{
    // there are two audio sources the player uses, one for player sound effects that the player makes
    // the other is for objects to play their sound effects at the player, this is done for objects that
    // are very plentyful in the game such as collectables


    public AudioSource playerSoundEffects;
    public AudioSource objectSoundEffects;

    // Start is called before the first frame update
    void Start()
    {
        playerSoundEffects = GetComponents<AudioSource>()[0];
        objectSoundEffects = GetComponents<AudioSource>()[1];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayPlayerSoundEffect(AudioClip clip)
    {
        // this is called upon entering a world and an audio source has not started yet
        playerSoundEffects.Stop();
        playerSoundEffects.clip = clip;
        playerSoundEffects.Play();
    }

    public void PlayObjectSoundEffect(AudioClip clip)
    {
        // this is called upon entering a world and an audio source has not started yet
        objectSoundEffects.Stop();
        objectSoundEffects.clip = clip;
        objectSoundEffects.Play();
    }

}
