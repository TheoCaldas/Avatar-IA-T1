using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BinaryHeap;

public class AStar
{
    private List<Tile> getNeighbours(Tile tile, int m, int n, Tile[,] tileMap)
    {
        List<Tile> neighbours = new List<Tile>();
        int x = tile.xRef;
        int y = tile.yRef;

        int leftX = x - 1;
        int rightX = x + 1;
        int upY = y - 1;
        int downY = y + 1;

        if (leftX >= 0)
            neighbours.Add(tileMap[leftX, y]);
        if (rightX < m)
            neighbours.Add(tileMap[rightX, y]);
        if (upY >= 0)
            neighbours.Add(tileMap[x, upY]);
        if (downY < n)
            neighbours.Add(tileMap[x, downY]);
        
        return neighbours;
    }

    public void aStar(Tile[,] tileMap, Tile startTile, Tile endTile)
    {
        Debug.Log(startTile);
        Debug.Log(endTile);

        //get x and y dimensions
        int m = tileMap.GetLength(0);
        int n = tileMap.GetLength(1);
        int capacity = m * n;
        int maxDistance = 2000000; //arbitrary large

        Dictionary<Tile, int> distance = new Dictionary<Tile, int>(); //distances estimative from startTile
        Dictionary<Tile, Tile> predecessor = new Dictionary<Tile, Tile>(); //antecessors for each tile
        Dictionary<Tile, bool> hasBeenVisited = new Dictionary<Tile, bool>(); //if tile has been explored
        BinaryHeap<int, Tile> queue = new BinaryHeap<int, Tile>(capacity, -1, maxDistance);
        
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                distance[tileMap[i,j]] = int.MaxValue;
                predecessor[tileMap[i,j]] = null;
                hasBeenVisited[tileMap[i,j]] = false;
            }
        }

        distance[startTile] = 0;
        queue.Enqueue(startTile, distance[startTile]);

        while (queue.Count() > 0)
        {
            Tile tile = queue.Dequeue();
            if (!hasBeenVisited[tile])
            {
                hasBeenVisited[tile] = true;
                List<Tile> neighbours = getNeighbours(tile, m, n, tileMap);

                foreach(Tile neighbour in neighbours)
                {
                    //relax
                    int sum = distance[tile] + neighbour.timeCost;
                    if (distance[neighbour] > sum)
                    {
                        distance[neighbour] = sum;
                        queue.Enqueue(neighbour, distance[neighbour]);
                        predecessor[neighbour] = tile;
                    }
                }
            }
        }

        foreach (KeyValuePair<Tile, int> kvp in distance)
            Debug.Log(kvp);

        foreach (KeyValuePair<Tile, Tile> kvp in predecessor)
            Debug.Log(kvp);
    }
}
