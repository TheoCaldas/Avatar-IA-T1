using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager: SingletonMonoBehaviour<MapManager>
{
    public Tile[,] tileMap;
    public List<Tile> eventTiles;

    public (int, int) currentPosition = new (0, 0);

    private void Start() {
        AStar algo = new AStar();
        algo.aStar(new Tile('A', 0, 0), new Tile('A', 0, 0));
    }

    private void Update() {
        
    }
}