using NaughtyAttributes;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEngine.Rendering.DebugUI.Table;

[DefaultExecutionOrder(2)]
public class FloorFillSpawner : MonoBehaviour
{
    List<RectInt> rooms = new List<RectInt>();
    List<RectInt> doors = new List<RectInt>();
    public int[,] tileMap; 
    private int rows;
    private int cols;
    public bool[,] visited;
    public GameObject objectToDisappear;
    public GameObject floor;
    public bool floorPlaced = false;

    public static FloorFillSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Button]
    public void FloorFill()
    {
        rooms = DungeonGenerator2.Instance.GetRooms();
        tileMap = TileMapGenerator.Instance.GetTileMap();
        doors = DungeonGenerator2.Instance.GetDoors();
        InitVisited();
        StartCoroutine(StartFlooring());
        
    }
    ///<summary>
    ///Gets the first room of the graph and starts the floor filling
    ///</summary>
    private IEnumerator StartFlooring()
    {
        Graph<RectInt> graph = DungeonGenerator2.Instance.originalGraph.Clone();

        List<RectInt> rectInts = graph.GetNodes();
        RectInt startRoom = rectInts[0];

        int startC = startRoom.xMin + startRoom.width / 2;
        int startR = startRoom.yMin + startRoom.height / 2;

        yield return StartCoroutine(FloorFillBFS(startR, startC));

        Debug.Log("Floor placed");
        floorPlaced = true;
    }
    ///<summary>
    ///Spawning the floor using a BFS approach
    ///</summary>
    private IEnumerator FloorFillBFS(int startR, int startC)
    {
        Queue<Vector3> queue = new Queue<Vector3>();
        Vector3 startTilePos = new Vector3(startC + 0.5f, 0, startR + 0.5f);

        queue.Enqueue(new Vector3(startC, startR));
        visited[startR, startC] = true;
        
        Instantiate(floor,startTilePos , Quaternion.identity, transform);
        if (DungeonGenerator2.Instance.dijkstraPathFind)
        {
            DijkstraPathFinder.Instance.graph.AddNode(startTilePos);
        }
        int processedPerFrame =50; 
        int processed = 0;

        while (queue.Count > 0)
        {
            Vector3 tile = queue.Dequeue();
            int r = Mathf.RoundToInt(tile.z);
            int c = Mathf.RoundToInt(tile.x);
            Vector3 tileToLookAround = new Vector3(c+0.5f,0,r+0.5f);
            TrySpawnTile(r + 1, c, queue,tileToLookAround); // Up
            TrySpawnTile(r - 1, c, queue,tileToLookAround); // Down
            TrySpawnTile(r, c + 1, queue,tileToLookAround); // Right
            TrySpawnTile(r, c - 1, queue, tileToLookAround); // Left
            if (!IsDoorAdjacent(r,c))
            {
                TrySpawnTile(r + 1, c + 1, queue, tileToLookAround);
                TrySpawnTile(r - 1, c + 1, queue, tileToLookAround);
                TrySpawnTile(r + 1, c - 1, queue, tileToLookAround);
                TrySpawnTile(r - 1, c - 1, queue, tileToLookAround);
            }
            processed++;
            if (processed >= processedPerFrame&& DungeonGenerator2.Instance.CheckExecutionMode()&&!DungeonGenerator2.Instance.skipThisStep)
            {
                processed = 0; // this is equal to yield return
                yield return null;
                
                 //it is like saying to unity after 4*processedPerFrame tiles to take a break to avoid stack overflow
            }
        }
    }

    ///<summary>
    ///Spawning of the tiles
    ///</summary>
    private void TrySpawnTile(int r, int c, Queue<Vector3> queue,Vector3 tileBFS)
    {
        if (r < 0 || r >= rows || c < 0 || c >= cols) return;
        Vector3 tilePos = new Vector3(c + 0.5f, 0, r + 0.5f);
        if (tileMap[r, c] != 0 || visited[r, c])
        {
            if (DungeonGenerator2.Instance.dijkstraPathFind)
            {
                DijkstraPathFinder.Instance.graph.AddEdge(tileBFS, tilePos);
            }
            
            return;
        }
        visited[r, c] = true;

        Instantiate(floor, tilePos, Quaternion.identity, transform);
        queue.Enqueue(new Vector3(c, 0, r));

        if (DungeonGenerator2.Instance.dijkstraPathFind && (r >= 2 && r < rows - 2 && c >= 2 && c < cols - 2))
        {
            DijkstraPathFinder.Instance.graph.AddNode(tilePos);

            DijkstraPathFinder.Instance.graph.AddEdge(tileBFS, tilePos);
        }

    }

    private bool IsDoorAdjacent(int r, int c)
    {
        foreach (var door in doors)
        {
            for (int y = door.yMin; y < door.yMax; y++)
            {
                for (int x = door.xMin; x < door.xMax; x++)
                {
                    if (Mathf.Abs(r - y) <= 2 && Mathf.Abs(c - x) <= 2)
                    {
                        return true; 
                    }
                }
            }
        }
        return false;
    }

    
   


    void InitVisited()
    {
        rows = tileMap.GetLength(0);
        cols = tileMap.GetLength(1);
        visited = new bool[rows, cols];
    }
    
}
