using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager: SingletonMonoBehaviour<MapManager>
{
    //Tile matrix that represents the map
    [HideInInspector] public Tile[,] tileMap;
    //Tiles that are objectives (should be sorted)
    [HideInInspector] public List<Tile> eventTiles;

    //List of colors changes in tiles to help visualing the algorithm
    [HideInInspector] public List<Color> visualizeColors = new List<Color>();
    [HideInInspector] public List<Tile> visualizeTiles = new List<Tile>();

    //Reference to character in scene
    public GameObject character;

    public void StartPathFinding() {
        changePosition(eventTiles[0]);

        AStar algo = new AStar();
        List<Tile> shortestPath = algo.aStar(tileMap, eventTiles[0], eventTiles[1]);

        foreach (Tile tile in shortestPath)
            Debug.Log(tile);

        for (int i = 0; i < visualizeColors.Count; i++)
            StartCoroutine(Visualize(visualizeColors[i], visualizeTiles[i], i + 1));

        if (character != null)
            StartCoroutine(FollowPath(shortestPath));
    }

    IEnumerator FollowPath(List<Tile> path)
    {   
        float timeInterval = 0.5f;
        yield return new WaitForSeconds(9.0f); 
        foreach (Tile tile in path)
        {
            yield return new WaitForSeconds(timeInterval);
            changePosition(tile);
        }
    }

    IEnumerator Visualize(Color color, Tile tile, int index)
    {
        float timeInterval = 0.5f;
        yield return new WaitForSeconds(index * timeInterval); 
        changeColor(color, tile);
    }

    void changeColor(Color color, Tile tile)
    {
        MeshRenderer r = tile.tile3DRef.GetComponentInChildren<MeshRenderer>();
        r.material.SetColor("_BaseColor", color);
        if (tile.type == TileType.Water)
            r.material.SetColor("_DeepColor", color);
    }

    void changePosition(Tile tile)
    {
        Vector3 tilePosition = tile.tile3DRef.transform.position;
        character.transform.position = new Vector3(tilePosition.x, character.transform.position.y, tilePosition.z);
    }
}