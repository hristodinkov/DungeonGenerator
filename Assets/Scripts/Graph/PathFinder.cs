using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    private Vector3 startNode;
    private Vector3 endNode;

    public List<Vector3> path = new List<Vector3>();
    HashSet<Vector3> discovered = new HashSet<Vector3>();

    private Vector3 GetClosestNodeToPosition(Vector3 position)
    {
        Vector3 closestNode = Vector3.zero;
        float closestDistance = Mathf.Infinity;

        foreach (var node in DijkstraPathFinder.Instance.graph.GetNodes())
        {
            float dist = (node - position).magnitude;
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestNode = node;
            }
        }
        //Find the closest node to the position

        return closestNode;
    }

    public List<Vector3> CalculatePath(Vector3 from, Vector3 to)
    {
        Vector3 playerPosition = from;

        startNode = GetClosestNodeToPosition(playerPosition);
        endNode = GetClosestNodeToPosition(to); 

        path = Dijkstra(startNode, endNode);

        return path;
    }

    public List<Vector3> Dijkstra(Vector3 start, Vector3 end)
    {
        //Use this "discovered" list to see the nodes in the visual debugging used on OnDrawGizmos()
        discovered.Clear();
        Vector3 startNode = start;
        List<(Vector3 node, float priority)> queue = new List<(Vector3 node, float priority)>();
        Dictionary<Vector3, float> cost = new Dictionary<Vector3, float>();
        Dictionary<Vector3, Vector3> parentToChild = new Dictionary<Vector3, Vector3>();
        queue.Add((startNode, 0));
        cost.Add(startNode, 0);
        while (queue.Count > 0)
        {
            queue = queue.OrderByDescending(node => node.priority).ToList();
            startNode = queue[queue.Count - 1].node;
            queue.RemoveAt(queue.Count - 1);
            if (startNode == end)
            {
                return ReconstructPath(parentToChild, start, end);
            }
            foreach (var node in DijkstraPathFinder.Instance.graph.GetNeighbors(startNode))
            {
                float newCost = cost[startNode] + Cost(node, startNode);
                if (!cost.ContainsKey(node) || newCost < cost[node])
                {
                    cost[node] = newCost;
                    parentToChild[node] = startNode;
                    queue.Add((node, newCost));
                }
            }
        }
        /* */
        return new List<Vector3>();
    }

    public float Cost(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(from, to);
    }
    List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> parentMap, Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 currentNode = end;

        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = parentMap[currentNode];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startNode, .3f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endNode, .3f);

        if (discovered != null)
        {
            foreach (var node in discovered)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(node, .3f);
            }
        }

        if (path != null)
        {
            foreach (var node in path)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(node, .3f);
            }
        }
    }
}
