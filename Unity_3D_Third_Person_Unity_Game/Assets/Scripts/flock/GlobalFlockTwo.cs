using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalFlockTwo : MonoBehaviour
{

    public GameObject fishPrefab;
    public static Vector3 tankDimensions = new Vector3(5, 5, 5);
    public static Vector3 tankPosition = new Vector3(20, 10, -60);

    static int numFish = 100;
    public static GameObject[] allFish = new GameObject[numFish];

    public static Vector3 goalPos;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numFish; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-tankDimensions.x + tankPosition.x, tankDimensions.x + tankPosition.x),
                                      Random.Range(-tankDimensions.y + tankPosition.y, tankDimensions.y + tankPosition.y),
                                      Random.Range(-tankDimensions.z + tankPosition.z, tankDimensions.z + tankPosition.z));
            allFish[i] = Instantiate(fishPrefab, pos, Quaternion.identity);
        }
        goalPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0, 10000) < 50)
        {
            goalPos = new Vector3(Random.Range(-tankDimensions.x + tankPosition.x, tankDimensions.x + tankPosition.x),
                                  Random.Range(-tankDimensions.y + tankPosition.y, tankDimensions.y + tankPosition.y),
                                  Random.Range(-tankDimensions.z + tankPosition.z, tankDimensions.z + tankPosition.z));
        }
    }
}
