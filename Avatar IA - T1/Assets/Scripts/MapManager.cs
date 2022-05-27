using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum MapState
{
    None,
    Ready,
    RunningAStar,
    VisualizingAStar,
    FollowingPath,
}

public class MapManager: SingletonMonoBehaviour<MapManager>
{
    //Tile matrix that represents the map
    [HideInInspector] public Tile[,] tileMap;
    //Tiles that are objectives (should be sorted when set)
    [HideInInspector] public List<Tile> eventTiles;

    //List of colors changes in tiles to help visualing the algorithm
    [HideInInspector] public List<Color> visualizeColors = new List<Color>();
    [HideInInspector] public List<Tile> visualizeTiles = new List<Tile>();

    //Reference to character in scene
    public GameObject character;

    //Reference to objective in scene
    public Transform objective;

    //Time factors
    public float followPathTimeFactor = 0.1f;
    public float visualizerTimeFactor = 0.01f;

    //Algorithm measures
    private AStar aStar = new AStar();
    private float pathCost = 0.0f;
    private float totalCost = 0.0f; //sum of all path costs

    //Update control variables
    private MapState currentState = MapState.None;
    private int currentEventIndex = -1;
    private List<Tile> currentPath;
    private int aStarVisualizationIndex;
    private int followPathIndex;
    private float timeSinceLastUpdate;

    private void goToNextEvent()
    {
        currentState = MapState.Ready;
        currentEventIndex++;
        currentPath = null;
        aStarVisualizationIndex = 0;
        followPathIndex = 0;
        pathCost = 0.0f;
        timeSinceLastUpdate = 0.0f;
        changeObjectPosition(eventTiles[currentEventIndex + 1], objective);
    }

    public void StartPathFinding() {
        changeObjectPosition(eventTiles[0], character.transform);
        objective.GetComponent<ParticleSystem>().Play();
        goToNextEvent();

        // findAllPaths();
    }

    void findAllPaths()
    {
        for (int i = 0; i < eventTiles.Count - 1; i++)
        {
            float temp = Time.realtimeSinceStartup;
            List<Tile> shortestPath = aStar.aStar(tileMap, eventTiles[i], eventTiles[i + 1]);
            Debug.Log("Did find path from event " + eventTiles[i].eventID.ToString() + 
            " to event " + eventTiles[i + 1].eventID.ToString() + "! Took: " + (Time.realtimeSinceStartup - temp).ToString("f6") + " seconds");
            pathCost = 0.0f;
            foreach (Tile tile in shortestPath)
                pathCost += tile.timeCost;
            Debug.Log("Path Cost: " + pathCost.ToString());
            totalCost += pathCost;
        }
        Debug.Log("Total Cost: " + totalCost.ToString());
    }

    private List<Tile> FindPath(Tile startTile, Tile endTile)
    {
        float temp = Time.realtimeSinceStartup;
        List<Tile> shortestPath = aStar.aStar(tileMap, startTile, endTile);
        Debug.Log("Did find path from event " + startTile.eventID.ToString() + 
            " to event " + endTile.eventID.ToString() + "! Took: " + (Time.realtimeSinceStartup - temp).ToString("f6") + " seconds");
        return shortestPath;
    }   

    private bool VisualizeAStar() //did update visualization
    {
        if (aStarVisualizationIndex < visualizeColors.Count)
        {
            Tile tile = visualizeTiles[aStarVisualizationIndex];
            Color color = visualizeColors[aStarVisualizationIndex];
            tile.changeColor(color);
            aStarVisualizationIndex++;
            return true;
        }
        return false;
    }

    private void ClearAStarVisualization()
    {
        foreach (Tile tile in visualizeTiles)
            tile.revertColor();

        visualizeColors.Clear();
        visualizeTiles.Clear();
    }

    private bool FollowPath() //did update following
    {
        //TO DO: Make reflect tile cost
        if (followPathIndex < currentPath.Count)
        {
            Tile tile = currentPath[followPathIndex];
            changeObjectPosition(tile, character.transform);
            pathCost += tile.timeCost;
            followPathIndex++;
            return true;
        }
        return false;
    }

    private int calculateUpdateTimes(float factor)
    {
        if (factor <= 0.0f)
            return 0;
        
        timeSinceLastUpdate += Time.deltaTime;
        int n = 0;
        while (timeSinceLastUpdate > factor)
        {
            timeSinceLastUpdate -= factor;
            n++;
        }
        return n;
    }

    void changeObjectPosition(Tile tile, Transform transform)
    {
        Vector3 tilePosition = tile.tile3DRef.transform.position;
        transform.position = new Vector3(tilePosition.x, transform.position.y, tilePosition.z);
    }

    private void Update() 
    {
        if (currentState == MapState.Ready && currentEventIndex < eventTiles.Count - 1)
        {
            currentState = MapState.RunningAStar;
            currentPath = FindPath(eventTiles[currentEventIndex], eventTiles[currentEventIndex + 1]);
            currentState = MapState.VisualizingAStar;
        }
        else if (currentState == MapState.VisualizingAStar)
        {
            int updateTimes = calculateUpdateTimes(visualizerTimeFactor);
            bool didChange = true;
            for (int i = 0; i < updateTimes; i++)
                didChange = didChange && VisualizeAStar();

            if (!didChange || visualizerTimeFactor <= 0.0f)
            {
                ClearAStarVisualization();
                currentState = MapState.FollowingPath;
                timeSinceLastUpdate = 0.0f;
            }
        }
        else if (currentState == MapState.FollowingPath && currentPath != null)
        {
            int updateTimes = calculateUpdateTimes(followPathTimeFactor);
            bool didChange = true;
            for (int i = 0; i < updateTimes; i++)
                didChange = didChange && FollowPath();

            if (!didChange || followPathTimeFactor <= 0.0f)
            {
                Debug.Log("Path Cost: " + pathCost.ToString());
                totalCost += pathCost;
                goToNextEvent();
            }
        }
    }
}