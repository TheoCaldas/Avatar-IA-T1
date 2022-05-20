using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager: SingletonMonoBehaviour<MapManager>
{
    public Tile[,] tileMap;
    public List<Tile> eventTiles;

    public (int, int) currentPosition = new (0, 0);

    public void StartPathFinding() {
        AStar algo = new AStar();
        // Dictionary<Tile, Tile> predecessors = algo.aStar(tileMap, eventTiles[0]);
        List<Tile> shortestPath = algo.aStar(tileMap, eventTiles[0], eventTiles[1]);
        foreach (Tile tile in shortestPath)
            Debug.Log(tile);
    }
}