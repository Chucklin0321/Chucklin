using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("迷宮設置")]
    public int width = 20;
    public int height = 20;
    public float cellSize = 1.0f;
    
    [Header("預製體")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject gemPrefab;
    public GameObject exitPrefab;
    
    [Header("生成設置")]
    public int gemCount = 10;
    public float gemSpawnChance = 0.1f;
    
    // 迷宮數據
    private Cell[,] maze;
    private List<Vector2Int> gemPositions = new List<Vector2Int>();
    private Vector2Int exitPosition;
    
    // 迷宮生成方向
    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),  // 上
        new Vector2Int(1, 0),  // 右
        new Vector2Int(0, -1), // 下
        new Vector2Int(-1, 0)  // 左
    };
    
    // 迷宮單元格類
    private class Cell
    {
        public bool visited = false;
        public bool[] walls = new bool[4] { true, true, true, true }; // 上、右、下、左
    }
    
    void Start()
    {
        GenerateMaze();
        PlaceGemsAndExit();
        InstantiateMaze();
    }
    
    // 使用深度優先搜索算法生成迷宮
    void GenerateMaze()
    {
        maze = new Cell[width, height];
        
        // 初始化迷宮
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = new Cell();
            }
        }
        
        // 從隨機位置開始生成
        Vector2Int current = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        
        maze[current.x, current.y].visited = true;
        stack.Push(current);
        
        while (stack.Count > 0)
        {
            current = stack.Pop();
            List<Vector2Int> unvisitedNeighbors = GetUnvisitedNeighbors(current);
            
            if (unvisitedNeighbors.Count > 0)
            {
                stack.Push(current);
                
                // 隨機選擇一個未訪問的鄰居
                Vector2Int next = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                
                // 移除當前單元格和下一個單元格之間的牆
                RemoveWallsBetween(current, next);
                
                maze[next.x, next.y].visited = true;
                stack.Push(next);
            }
        }
    }
    
    // 獲取未訪問的鄰居單元格
    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int neighbor = cell + directions[i];
            
            // 檢查鄰居是否在迷宮範圍內且未被訪問
            if (neighbor.x >= 0 && neighbor.x < width && 
                neighbor.y >= 0 && neighbor.y < height && 
                !maze[neighbor.x, neighbor.y].visited)
            {
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }
    
    // 移除兩個單元格之間的牆
    void RemoveWallsBetween(Vector2Int current, Vector2Int next)
    {
        // 計算方向差異
        Vector2Int diff = next - current;
        
        // 根據方向差異確定要移除的牆
        if (diff.x == 1) // 右
        {
            maze[current.x, current.y].walls[1] = false;
            maze[next.x, next.y].walls[3] = false;
        }
        else if (diff.x == -1) // 左
        {
            maze[current.x, current.y].walls[3] = false;
            maze[next.x, next.y].walls[1] = false;
        }
        else if (diff.y == 1) // 上
        {
            maze[current.x, current.y].walls[0] = false;
            maze[next.x, next.y].walls[2] = false;
        }
        else if (diff.y == -1) // 下
        {
            maze[current.x, current.y].walls[2] = false;
            maze[next.x, next.y].walls[0] = false;
        }
    }
    
    // 放置寶石和出口
    void PlaceGemsAndExit()
    {
        // 清空之前的寶石位置
        gemPositions.Clear();
        
        // 放置寶石
        int gemsPlaced = 0;
        while (gemsPlaced < gemCount)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            
            // 避免在起點放置寶石
            if (x == 0 && y == 0)
                continue;
                
            Vector2Int pos = new Vector2Int(x, y);
            if (!gemPositions.Contains(pos))
            {
                gemPositions.Add(pos);
                gemsPlaced++;
            }
        }
        
        // 放置出口（在迷宮的遠端）
        exitPosition = new Vector2Int(width - 1, height - 1);
    }
    
    // 實例化迷宮
    void InstantiateMaze()
    {
        // 創建地板和牆
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
                
                // 創建地板
                Instantiate(floorPrefab, position, Quaternion.identity, transform);
                
                // 創建牆
                Cell cell = maze[x, y];
                for (int i = 0; i < 4; i++)
                {
                    if (cell.walls[i])
                    {
                        Quaternion rotation = Quaternion.Euler(0, i * 90, 0);
                        Vector3 wallPos = position + rotation * new Vector3(0, 0, cellSize / 2);
                        Instantiate(wallPrefab, wallPos, rotation, transform);
                    }
                }
            }
        }
        
        // 放置寶石
        foreach (Vector2Int gemPos in gemPositions)
        {
            Vector3 position = new Vector3(gemPos.x * cellSize, 0.5f, gemPos.y * cellSize);
            Instantiate(gemPrefab, position, Quaternion.identity, transform);
        }
        
        // 放置出口
        Vector3 exitPos = new Vector3(exitPosition.x * cellSize, 0.1f, exitPosition.y * cellSize);
        Instantiate(exitPrefab, exitPos, Quaternion.identity, transform);
    }
    
    // 獲取迷宮數據（供其他腳本使用）
    public Cell[,] GetMazeData()
    {
        return maze;
    }
    
    public List<Vector2Int> GetGemPositions()
    {
        return gemPositions;
    }
    
    public Vector2Int GetExitPosition()
    {
        return exitPosition;
    }
}
