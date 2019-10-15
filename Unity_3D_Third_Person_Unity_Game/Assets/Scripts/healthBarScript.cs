using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthBarScript : MonoBehaviour
{

	public Sprite[] spriteList;

    // Start is called before the first frame update
    void Start()
    {
		this.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = spriteList[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void UpdateHealth(int health){
		this.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = spriteList[8 - health];
	}


}
