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
    //Visualizes AStar
    [HideInInspector] public AStarVisualizer visualizer = new AStarVisualizer();
    //Makes character follow path
    [HideInInspector] public PathFollower follower = new PathFollower();

    //Reference to character in scene
    public GameObject character;
    //Reference to objective in scene
    public Transform objective;
    //Time factors
    public float followPathTimeFactor = 0.1f;
    public float visualizerTimeFactor = 0.01f;

    //Algorithm measures
    private AStar aStar = new AStar();
    [HideInInspector] public float pathCost = 0.0f; //can be modified by follower
    private float totalCost = 0.0f; //sum of all path costs

    //Update control variables
    private MapState currentState = MapState.None;
    private int currentEventIndex = -1;
    private float timeSinceLastUpdate;

    public void StartPathFinding() {
        follower.changeObjectPosition(eventTiles[0], character.transform);
        objective.GetComponent<ParticleSystem>().Play();
        goToNextEvent();
        // findAllPaths();
    }

    private void goToNextEvent()
    {
        currentState = MapState.Ready;
        currentEventIndex++;
        visualizer.reset();
        follower.reset();
        pathCost = 0.0f;
        timeSinceLastUpdate = 0.0f;

        if (currentEventIndex + 1 >= eventTiles.Count)
            finish();
        else
            follower.changeObjectPosition(eventTiles[currentEventIndex + 1], objective);
    }

    private void finish()
    {
        currentState = MapState.None;
        Debug.Log("Total Cost: " + totalCost.ToString());
        objective.GetComponent<ParticleSystem>().Stop();
    }

    private void findAllPaths()
    {
        for (int i = 0; i < eventTiles.Count - 1; i++)
        {
            float temp = Time.realtimeSinceStartup;
            List<Tile> shortestPath = aStar.aStar(tileMap, eventTiles[i], eventTiles[i + 1]);
            Debug.Log("Did find path from event " + eventTiles[i].eventID.ToString() + 
            " to event " + eventTiles[i + 1].eventID.ToString() + "! Took: " + (Time.realtimeSinceStartup - temp).ToString("f6") + " seconds");
            pathCost = 0.0f;
            foreach (Tile tile in shortestPath)
                pathCost += (tile.type != TileType.Event) ? tile.timeCost : 0;
            Debug.Log("Path Cost: " + pathCost.ToString());
            totalCost += pathCost;
        }
        Debug.Log("Total Cost: " + totalCost.ToString());
    }

    private List<Tile> findPath(Tile startTile, Tile endTile)
    {
        float temp = Time.realtimeSinceStartup;
        List<Tile> shortestPath = aStar.aStar(tileMap, startTile, endTile);
        Debug.Log("Did find path from event " + startTile.eventID.ToString() + 
            " to event " + endTile.eventID.ToString() + "! Took: " + (Time.realtimeSinceStartup - temp).ToString("f6") + " seconds");
        return shortestPath;
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

    private void Update() 
    {
        if (currentState == MapState.Ready && currentEventIndex < eventTiles.Count - 1)
        {
            currentState = MapState.RunningAStar;
            follower.setCurrentPath(findPath(eventTiles[currentEventIndex], eventTiles[currentEventIndex + 1]));
            currentState = MapState.VisualizingAStar;
        }
        else if (currentState == MapState.VisualizingAStar)
        {
            int updateTimes = calculateUpdateTimes(visualizerTimeFactor);
            bool didChange = true;
            for (int i = 0; i < updateTimes; i++)
                didChange = didChange && visualizer.visualize();

            if (!didChange || visualizerTimeFactor <= 0.0f)
            {
                visualizer.clearVisualization();
                currentState = MapState.FollowingPath;
                timeSinceLastUpdate = 0.0f;
            }
        }
        else if (currentState == MapState.FollowingPath && follower.hasPath())
        {
            //TO DO: Make change cost in the middle of the tile
            float factor = followPathTimeFactor * follower.getCurrentTimeCost();
            int updateTimes = 0;

            if (factor > 0.0f)
            {
                updateTimes = calculateUpdateTimes(factor);
                follower.lerpToNext(character.transform, timeSinceLastUpdate / factor);
            }

            bool didChange = true;
            for (int i = 0; i < updateTimes; i++)
                didChange = didChange && follower.followOneStep();

            if (!didChange || factor <= 0.0f)
            {
                Debug.Log("Path Cost: " + pathCost.ToString());
                totalCost += pathCost;
                goToNextEvent();
            }
        }
    }
}