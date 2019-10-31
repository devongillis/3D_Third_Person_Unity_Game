using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class pathFinding : MonoBehaviour
{
    public Transform[] points;
    public Transform target, o;
    private NavMeshAgent nav;
    private int destPoint;
    private int nextPoint;

    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!nav.pathPending && nav.remainingDistance < 0.5f)
            GoToNextPoint();
    }

    void GoToNextPoint()
    {
        if (points.Length == 0)
            return;

        Vector3 v1 = o.position - target.position;
        Vector3 v2 = o.position - points[nextPoint].position;
        float length1 = v1.x * v1.x + v1.y * v1.y + v1.z * v1.z;
        float length2 = v2.x * v2.x + v1.y * v1.y + v1.z * v1.z;

        if (length1 <= length2)
            nav.destination = target.position;
        else
        {
            nav.destination = points[nextPoint].position;
            nextPoint = (nextPoint + 1) % points.Length;
        }
    }
}
