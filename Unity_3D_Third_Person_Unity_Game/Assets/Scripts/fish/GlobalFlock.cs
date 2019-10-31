using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalFlock : MonoBehaviour
{
    public GameObject fishPrefab;
    public Vector3 goalPos;
    public Vector3 tankDimensions;

    public int numberOfFish = 10;
    public GameObject[] allFish;
    //public GameObject goal;

    // Use this for initialization
    void Start()
    {
        allFish = new GameObject[numberOfFish];
        for (int i = 0; i < numberOfFish; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-tankDimensions.x + transform.position.x, tankDimensions.x + transform.position.x),
                                    Random.Range(-tankDimensions.y + transform.position.y, tankDimensions.y + transform.position.y),
                                    Random.Range(-tankDimensions.z + transform.position.z, tankDimensions.z + transform.position.z));
            allFish[i] = Instantiate(fishPrefab, pos, Quaternion.identity);
            allFish[i].GetComponent<Flock>().AssignTheTank(gameObject);
        }
        goalPos = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0, 10000) < 50)
        {
            goalPos = new Vector3(Random.Range(-tankDimensions.x + transform.position.x, tankDimensions.x + transform.position.x),
                                    Random.Range(-tankDimensions.y + transform.position.y, tankDimensions.y + transform.position.y),
                                    Random.Range(-tankDimensions.z + transform.position.z, tankDimensions.z + transform.position.z));
        }
        //goal.transform.position = goalPos;
    }
}
