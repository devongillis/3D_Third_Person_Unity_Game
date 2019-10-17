using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterFlowScript : MonoBehaviour
{
	public Vector2 scrollSpeed = new Vector2(0.02f, 0.02f);
	Renderer renderer;

	void Start()
	{
		renderer = GetComponent<Renderer>();
	}

	// Update is called once per frame
	void Update()
	{
		renderer.material.mainTextureOffset = scrollSpeed * Time.time;
	}
}
