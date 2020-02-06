using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punched : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerHasPunched()
    {
        // this script is applied to all objects that interact with the player's punch attack
        Debug.Log("punched");
    }
}
