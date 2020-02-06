using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class diamondCollect : MonoBehaviour
{
    // Start is called before the first frame update
    public int value;
    public AudioClip audioClip;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Player")
        {
            other.GetComponent<characterStats>().IncrementDiamondValueCount(value);
            other.GetComponent<characterPlayAudio>().PlayObjectSoundEffect(audioClip);
            gameObject.SetActive(false);
        }
    }
}
