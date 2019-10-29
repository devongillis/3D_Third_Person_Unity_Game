using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class diamondSpawner : MonoBehaviour
{
    // Reference to the Prefab. Drag a Prefab into this field in the Inspector.
    public GameObject diamond;
    public Vector3[] positionOffsets;

    public bool useCircleFormation = false;
    public int number = 1;
    public float circleRadius = 2.0f;
    // This script will simply instantiate the Prefab when the game starts.
    void Start()
    {
        if (useCircleFormation && number >= 1)
        {
            Vector3 initial = transform.forward * circleRadius;
            float angle = 360.0f / number;

            for (int i = 0; i < number; i++)
            {
                Vector3 displacement = Quaternion.Euler(0, angle * i, 0) * initial;
                Instantiate(diamond, transform.position + displacement, Quaternion.identity);
            }
        }
        else
        {
            for (int i = 0; i < positionOffsets.Length; i++)
            {
                Instantiate(diamond, transform.position + positionOffsets[i], Quaternion.identity);
            }
        }
        gameObject.SetActive(false);
    }
}
