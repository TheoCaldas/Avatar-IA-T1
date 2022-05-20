using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager: SingletonMonoBehaviour<MapManager>
{
    //Tile matrix that represents the map
    [HideInInspector] public Tile[,] tileMap;
    //Tiles that are objectives (should be sorted when set)
    [HideInInspector] public List<Tile> eventTiles;

    //List of colors changes in tiles to help visualing the algorithm
    [HideInInspector] public List<Color> visualizeColors = new List<Color>();
    [HideInInspector] public List<Tile> visualizeTiles = new List<Tile>();

    private float pathCost = 0.0f;

    //Reference to character in scene
    public GameObject character;
    //Time factors
    public float followPathTimeFactor = 0.5f; //TO DO: Change time interval to updates per frame
    public float visualizerTimeFactor = 0.5f;

    public void StartPathFinding() {
        changePosition(eventTiles[0]);
        StartCoroutine(FindAllPaths());
    }

    IEnumerator FindAllPaths()
    {
        AStar algh = new AStar();
        for (int i = 0; i < eventTiles.Count - 1; i++)
        {
            float temp = Time.realtimeSinceStartup;
            List<Tile> shortestPath = algh.aStar(tileMap, eventTiles[i], eventTiles[i + 1]);
            Debug.Log("Did find path from event " + eventTiles[i].eventID.ToString() + 
            " to event " + eventTiles[i + 1].eventID.ToString() + "! Took: " + (Time.realtimeSinceStartup - temp).ToString("f6") + " seconds");

            yield return VisualizeThenFollow(shortestPath);
        }
    }

    IEnumerator VisualizeThenFollow(List<Tile> path)
    {
        yield return Visualize();
        if (character != null)
            yield return FollowPath(path);
    }

    IEnumerator FollowPath(List<Tile> path)
    {   
        //TO DO: Make reflect tile cost
        pathCost = 0.0f;
        foreach (Tile tile in path)
        {
            yield return new WaitForSeconds(followPathTimeFactor);
            changePosition(tile);
            pathCost += tile.timeCost;
        }
        Debug.Log("Path Cost: " + pathCost.ToString());
    }

    IEnumerator Visualize()
    {
        for (int i = 0; i < visualizeColors.Count; i++)
        {
            yield return new WaitForSeconds(visualizerTimeFactor); 
            visualizeTiles[i].changeColor(visualizeColors[i]);
        }

        //clean visualiztion
        yield return new WaitForSeconds(visualizerTimeFactor);
        foreach (Tile tile in visualizeTiles)
            tile.revertColor();

        visualizeColors.Clear();
        visualizeTiles.Clear();
    }

    void changePosition(Tile tile)
    {
        Vector3 tilePosition = tile.tile3DRef.transform.position;
        character.transform.position = new Vector3(tilePosition.x, character.transform.position.y, tilePosition.z);
    }
}