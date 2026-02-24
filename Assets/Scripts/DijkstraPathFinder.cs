using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Progress;

[DefaultExecutionOrder(4)]
public class DijkstraPathFinder : MonoBehaviour
{
    public Graph<Vector3> graph;

    public static DijkstraPathFinder Instance { get; private set; }

    private List<Vector3> nodes = new List<Vector3>();
    private List<GameObject> walls = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        graph = new Graph<Vector3>();

        nodes = graph.GetNodes();
        

    }
    public void DeleteWallNodes()
    {
        walls = TileMapGenerator.Instance.walls;

        HashSet<Vector3Int> wallPositions = new HashSet<Vector3Int>();
        foreach (var wall in walls)
        {
            Vector3Int wallTile = new Vector3Int(
                Mathf.FloorToInt(wall.transform.position.x),
                0,
                Mathf.FloorToInt(wall.transform.position.z)
            );
            wallPositions.Add(wallTile);
        }


        HashSet<Vector3Int> doorPositions = new HashSet<Vector3Int>();
        foreach (var door in DungeonGenerator2.Instance.GetDoors())
        {
            for (int x = door.x; x < door.x + door.width; x++)
            {
                for (int y = door.y; y < door.y + door.height; y++)
                {
                    doorPositions.Add(new Vector3Int(x, 0, y));
                }
            }
        }

        List<Vector3> nodesToRemove = new List<Vector3>();
        List<Vector3> currentNodes = new List<Vector3>(graph.GetNodes());

        foreach (var node in currentNodes)
        {
            Vector3Int nodeTile = new Vector3Int(
                Mathf.FloorToInt(node.x),
                0,
                Mathf.FloorToInt(node.z)
            );
            // Remove all nodes that are on wall tiles (even near doors)
            if (wallPositions.Contains(nodeTile))
            {
                nodesToRemove.Add(node);
                DebugExtension.DebugWireSphere(node, Color.red, 1, 10);
            }
        }

        foreach (var node in nodesToRemove)
        {
            foreach (var neighbor in graph.GetNeighbors(node))
            {
                graph.RemoveEdge(node, neighbor);
            }
            graph.RemoveNode(node);
        }
    }

    public IEnumerator ShowGraph()
    {
        List<(Vector3,Vector3)> edges = new List<(Vector3,Vector3)>();
        HashSet<Vector3> visited = new HashSet<Vector3>();
        int processedPerFrame = 50;
        int processed = 0;

        foreach (Vector3 node in graph.GetNodes())
        {
            if (visited.Contains(node)) continue;

            Queue<Vector3> queue = new Queue<Vector3>();
            queue.Enqueue(node);

            while (queue.Count > 0)
            {
                Vector3 current = queue.Dequeue();
                if (visited.Contains(current)) continue;

                visited.Add(current);
                //DebugExtension.DebugWireSphere(current, Color.cyan, 0.2f, 100);
                processed++;

                if (processed >= processedPerFrame)
                {
                    processed = 0;
                    yield return null;
                }

                foreach (var neighbor in graph.GetNeighbors(current))
                {
                    if (!edges.Contains((current,neighbor))&& !edges.Contains((neighbor, current)))
                    {
                        //Debug.DrawLine(current, neighbor, Color.white, 100f);
                        edges.Add((neighbor, current));
                    }

                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
}

}
