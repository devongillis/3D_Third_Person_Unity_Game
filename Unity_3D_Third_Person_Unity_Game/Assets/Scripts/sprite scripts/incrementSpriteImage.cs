using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class incrementSpriteImage : MonoBehaviour
{

    public Sprite[] spriteList;
    public int spriteRef;
    public int frameIncrement = 10;
    public int frameCount = 0;
    public SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRef = 0;
        spriteRenderer.sprite = spriteList[spriteRef];
    }

    // Update is called once per frame
    void Update()
    {
        if(frameCount >= frameIncrement)
        {
            frameCount = 0;
            spriteRef++;
            if(spriteRef >= spriteList.Length)
            {
                spriteRef = 0;
            }
            spriteRenderer.sprite = spriteList[spriteRef];
        }
        frameCount++;
    }

}
