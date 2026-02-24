using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent navMeshAgent;

    [SerializeField]
    private PathFinder pathFinder;

    [SerializeField]
    private float speed = 5f;

    [SerializeField]private bool isMoving = false;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void GoToDestination(Vector3 destination)
    {
        if (!DungeonGenerator2.Instance.dijkstraPathFind)
        {
            navMeshAgent.SetDestination(destination);
        }
        else
        {
            if (!isMoving)
            {
                StartCoroutine(FollowPathCoroutine(pathFinder.CalculatePath(transform.position, destination)));
            }
            
        }
        
    }
    IEnumerator FollowPathCoroutine(List<Vector3> path)
    {
        if (path == null || path.Count == 0)
        {
            yield break;
        }
        isMoving = true;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 target = path[i]+Vector3.up;

            while (Vector3.Distance(transform.position, target) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
                yield return null;
            }
 
        }
        isMoving = false;
    }
}
