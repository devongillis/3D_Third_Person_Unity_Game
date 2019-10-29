using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class diamondImageScript : MonoBehaviour
{

    public Sprite[] spriteList;
    public int frameIncrement = 10;



    private int frameCount = 0;
    private int spriteRef;
    private UnityEngine.UI.Image spriteRenderer;



    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<UnityEngine.UI.Image>();
        spriteRenderer.sprite = spriteList[0];
    }

    void Update()
    {
        if (frameCount >= frameIncrement)
        {
            frameCount = 0;
            spriteRef++;
            if (spriteRef >= spriteList.Length)
            {
                spriteRef = 0;
            }
            spriteRenderer.sprite = spriteList[spriteRef];
        }
        frameCount++;
    }


}
