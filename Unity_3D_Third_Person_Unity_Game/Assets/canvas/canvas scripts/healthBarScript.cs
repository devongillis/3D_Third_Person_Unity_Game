using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthBarScript : MonoBehaviour
{
    
	public Sprite[] spriteList;
    public UnityEngine.UI.Image spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<UnityEngine.UI.Image>();
        spriteRenderer.sprite = spriteList[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void UpdateHealth(int health){
        spriteRenderer.sprite = spriteList[8 - health];
	}
    

}
