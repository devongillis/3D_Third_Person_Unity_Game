using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toxicGas : MonoBehaviour
{
    // Start is called before the first frame update

	public Vector2 scrollSpeed = new Vector2(0.02f, 0.02f);
	Renderer gasRenderer;

    void Start()
    {
        gasRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        gasRenderer.material.mainTextureOffset = scrollSpeed * Time.time;
    }
}
