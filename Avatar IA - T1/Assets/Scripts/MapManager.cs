using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager: SingletonMonoBehaviour<MapManager>
{
    //Tile matrix that represents the map
    public Tile[,] tileMap;
    //Tiles that are objectives (should be sorted)
    public List<Tile> eventTiles;

    //Reference to character position
    public (int, int) currentPosition = new (0, 0);

    //List of colors changes in tiles to help visualing the algorithm
    public List<Color> visualizeColors = new List<Color>();
    public List<Tile> visualizeTiles = new List<Tile>();

    public void StartPathFinding() {
        AStar algo = new AStar();
        // Dictionary<Tile, Tile> predecessors = algo.aStar(tileMap, eventTiles[0]);
        List<Tile> shortestPath = algo.aStar(tileMap, eventTiles[0], eventTiles[1]);
        foreach (Tile tile in shortestPath)
            Debug.Log(tile);

        for (int i = 0; i < visualizeColors.Count; i++)
            StartCoroutine(Visualize(visualizeColors[i], visualizeTiles[i], i + 1));
    }

    IEnumerator Visualize(Color color, Tile tile, int index)
    {
        float timeInterval = 1.0f;
        yield return new WaitForSeconds(index * timeInterval); 
        changeColor(color, tile);
    }

    void changeColor(Color color, Tile tile)
    {
        MeshRenderer r = tile.tile3DRef.GetComponentInChildren<MeshRenderer>();
        r.material.SetColor("_BaseColor", color);
    }
}