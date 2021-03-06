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
    [HideInInspector] public float totalCost = 0.0f; //sum of all path costs

    //Genetic variables
    private GeneticAlgorithm genetic;
    private List<(float, List<Character>)> geneticResults;
    private (float, List<Character>) currentEventFight;
    [HideInInspector] public float geneticCost = 0.0f;
    [HideInInspector] public float currentFightCost = 0.0f;
    [HideInInspector] public int[] charactersEnergy = {0, 0, 0, 0, 0, 0, 0};

    //Update control variables
    private MapState currentState = MapState.None;
    [HideInInspector] public int currentEventIndex = -1;
    private float timeSinceLastUpdate;

    public void StartPathFinding() {
        genetic = new GeneticAlgorithm(eventTiles.Count);
        // genetic.startGenetic();

        geneticResults = genetic.getResults();
        follower.changeObjectPosition(eventTiles[0], character.transform);
        // objective.GetComponent<ParticleSystem>().Play();

        goToNextEvent();
        // findAllPaths();
    }

    private void goToNextEvent()
    {
        objective.GetComponent<ParticleSystem>().Play();
        currentState = MapState.Ready;
        currentEventIndex++;
        visualizer.reset();
        follower.reset();
        pathCost = 0.0f;
        currentFightCost = 0.0f;
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

    public void objectLookAtEvent(Transform obj)
    {
        if (currentEventIndex < eventTiles.Count - 1)
        {
            Transform objective = eventTiles[currentEventIndex + 1].tile3DRef.transform;
            if (Vector3.Distance(objective.position, obj.position) > 1.0)
            {
                Vector3 targetPosition = new Vector3( objective.position.x, obj.position.y, objective.position.z) ;
                obj.LookAt( targetPosition ) ;
            }
        }
    }

    private void disableCharacter(int index)
    {
       (character.transform.GetChild(0)).GetChild(index).gameObject.SetActive(false);
    }

    private void scaleCharacter(int index, float factor)
    {
        Transform t = (character.transform.GetChild(0)).GetChild(index);
        t.localScale = factor * new Vector3(2, 2, 2);
        t.localPosition = factor * new Vector3(0, 3 + index, 0);
    }

    private void resetScale(int index)
    {
        Transform t = (character.transform.GetChild(0)).GetChild(index);
        t.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        if (t.name == "Appa")
            t.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }

    private void Update() 
    {
        // objectLookAtEvent(character.transform);

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
                follower.rotateCharacterTowardsNext(character.transform.GetChild(0));
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
                objective.GetComponent<ParticleSystem>().Stop();
            }
        }
        else if (currentState == MapState.EventFighting)
        {
            (float timeCost, List<Character> characters) = currentEventFight;
            float factor = eventFightTimeFactor * timeCost;
            currentFightCost = timeCost;
            foreach(Character character in characters)
            {
                int index = (int) character;
                scaleCharacter(index, timeSinceLastUpdate / factor);
            }

            int updateTimes = calculateUpdateTimes(factor);
            if (updateTimes >= 1 || geneticResults == null)
            {
                Debug.Log("Event Fight Cost: " + timeCost);
                geneticCost += timeCost;
                foreach(Character character in characters)
                {
                    
                    int index = (int) character;
                    charactersEnergy[index]++;
                    resetScale(index);
                    if (charactersEnergy[index] >= 8)
                        disableCharacter(index);
                }
                goToNextEvent();
            }
        }
    }
}