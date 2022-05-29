using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BinaryHeap;

public class AStar
{
    private List<Tile> convertToList(Dictionary<Tile, Tile> predecessor, Tile startTile, Tile endTile)
    {
        List<Tile> path = new List<Tile>();
        Tile current = endTile;
        while (current != startTile)
        {
            addToVisualizeQueue(current, Color.green);
            path.Insert(0, current);
            current = predecessor[current];
        }

        return path;
    }

    //returns predecessor hashMap for all tiles
    public Dictionary<Tile, Tile> aStar(Tile[,] tileMap, Tile startTile)
    {
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
                List<Tile> neighbours = tile.get4Neighbours(tileMap, m, n);

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

        // foreach (KeyValuePair<Tile, int> kvp in distance)
        //     Debug.Log(kvp);

        // foreach (KeyValuePair<Tile, Tile> kvp in predecessor)
        //     Debug.Log(kvp);

        return predecessor;
    }

    //returns path list from start to end
    public List<Tile> aStar(Tile[,] tileMap, Tile startTile, Tile endTile)
    {
        //get x and y dimensions
        int m = tileMap.GetLength(0);
        int n = tileMap.GetLength(1);
        int capacity = m * n;
        int maxDistance = int.MaxValue; //arbitrary large

        Dictionary<Tile, int> fCosts = new Dictionary<Tile, int>(); //gCost + distance to endTile (heuristic)
        Dictionary<Tile, int> gCosts = new Dictionary<Tile, int>(); //distances estimative from startTile
        Dictionary<Tile, Tile> predecessor = new Dictionary<Tile, Tile>(); //antecessors for each tile
        Dictionary<Tile, bool> hasBeenVisited = new Dictionary<Tile, bool>(); //if tile has been explored
        BinaryHeap<int, Tile> queue = new BinaryHeap<int, Tile>(capacity, -1, maxDistance);
        
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                fCosts[tileMap[i,j]] = int.MaxValue;
                predecessor[tileMap[i,j]] = null;
                hasBeenVisited[tileMap[i,j]] = false;
            }
        }

        fCosts[startTile] = 0;
        gCosts[startTile] = 0;
        queue.Enqueue(startTile, fCosts[startTile]);

        while (queue.Count() > 0)
        {
            Tile tile = queue.Dequeue();
            if (!hasBeenVisited[tile])
            {
                addToVisualizeQueue(tile, Color.red);

                hasBeenVisited[tile] = true;
                List<Tile> neighbours = tile.get4Neighbours(tileMap, m, n);

                foreach(Tile neighbour in neighbours)
                {
                    if (neighbour.type == TileType.Event && neighbour != endTile)
                        continue;

                    int hCost = calculateHCost(endTile, neighbour);
                    int gCost = gCosts[tile] + neighbour.timeCost;

                    if (fCosts[neighbour] > gCost + hCost)
                    {
                        addToVisualizeQueue(neighbour, Color.yellow);

                        fCosts[neighbour] = gCost + hCost;
                        gCosts[neighbour] = gCost;
                        queue.Enqueue(neighbour, fCosts[neighbour]);
                        predecessor[neighbour] = tile;

                        if (neighbour == endTile)
                            return convertToList(predecessor, startTile, endTile);
                    }
                }
            }
        }

        return null;
    }

    void addToVisualizeQueue(Tile tile, Color color)
    {
        MapManager.Instance.visualizer.add(tile, color);
    }

    int calculateHCost(Tile a, Tile b) //using manhattan distance
    {
        int deltaX = Mathf.Abs(a.xRef - b.xRef);
        int deltaY = Mathf.Abs(a.yRef - b.yRef);
        return deltaX + deltaY;
        // return 0;
    }
}
