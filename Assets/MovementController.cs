using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 0;                     //This must be public or SerializeField
    public bool drawSpeed;
    int currentWaypointIndex;


    public void Update()
    {
        if (waypoints.Length > 0)
        { 
            if (!waypoints[currentWaypointIndex] ||  Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < .1f)
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            if (waypoints[currentWaypointIndex])
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, speed * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(waypoints[currentWaypointIndex].position - transform.position);
                Debug.Log("rot: " + transform.rotation);
            }
        }
    }
	
}
