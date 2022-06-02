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
    EventFighting,
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
    public float eventFightTimeFactor = 1.0f;

    //AStar measures
    private AStar aStar = new AStar();
    [HideInInspector] public float pathCost = 0.0f; //can be modified by follower
    private float totalCost = 0.0f; //sum of all path costs

    //Genetic variables
    private GeneticAlgorithm genetic;
    private List<(float, List<Character>)> geneticResults;
    private (float, List<Character>) currentEventFight;
    private float geneticCost = 0.0f;

    //Update control variables
    private MapState currentState = MapState.None;
    private int currentEventIndex = -1;
    private float timeSinceLastUpdate;

    public void StartPathFinding() {
        genetic = new GeneticAlgorithm(eventTiles.Count);
        // genetic.startGenetic();

        geneticResults = genetic.getResults();
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
        Debug.Log("AStar Total Cost: " + totalCost.ToString());
        Debug.Log("Genetic Total Cost: " + geneticCost.ToString());
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

    private void characterLookAtEvent()
    {
        if (currentEventIndex < eventTiles.Count - 1)
        {
            Transform objective = eventTiles[currentEventIndex + 1].tile3DRef.transform;
            if (Vector3.Distance(objective.position, character.transform.position) > 1.0)
            {
                Vector3 targetPosition = new Vector3( objective.position.x, character.transform.position.y, objective.position.z) ;
                character.transform.LookAt( targetPosition ) ;
            }
        }
    }

    private void Update() 
    {
        characterLookAtEvent();

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
                timeSinceLastUpdate = 0.0f;
                currentEventFight = geneticResults[currentEventIndex];
                currentState = MapState.EventFighting;
            }
        }
        else if (currentState == MapState.EventFighting)
        {
            (float timeCost, List<Character> characters) = currentEventFight;
            float factor = eventFightTimeFactor * timeCost;
            int updateTimes = calculateUpdateTimes(factor);
            if (updateTimes >= 1 || geneticResults == null)
            {
                Debug.Log("Event Fight Cost: " + timeCost);
                geneticCost += timeCost;
                foreach(Character character in characters)
                    Debug.Log(character);
                goToNextEvent();
            }
        }
    }
}