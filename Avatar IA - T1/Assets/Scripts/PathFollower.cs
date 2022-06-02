using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathFollower
{
    private int currentIndex;
    private List<Tile> currentPath;
    private Tile lastTilePreviousPath;

    private float lastTime;
    public bool followOneStep() //did update following
    {
        lastTime = 0.0f;
        if (currentIndex < currentPath.Count)
        {
            Tile tile = currentPath[currentIndex];
            if (tile.type != TileType.Event)
                MapManager.Instance.pathCost += tile.timeCost;
            currentIndex++;
            return true;
        }
        return false;
    }

    public void rotateCharacterTowardsNext(Transform transform)
    {
        if (currentIndex < currentPath.Count - 1)
        {
            Transform objective = currentPath[currentIndex + 1].tile3DRef.transform;
            Vector3 targetPosition = new Vector3( objective.position.x, transform.position.y, objective.position.z) ;
            transform.LookAt( targetPosition ) ;
            transform.localEulerAngles += new Vector3(0, 90, 0);
            for (int i = 0; i < 7; i++)
               transform.GetChild(i).localEulerAngles = new Vector3(0, 0, 0);
        }
    }

    public void changeObjectPosition(Tile tile, Transform transform)
    {
        transform.position = getTopTilePos(tile, transform);
    }

    private void lerpObjectPosition(Tile start, Tile end, Transform transform, float time)
    {
        Vector3 src = getTopTilePos(start, transform);
        Vector3 dest = getTopTilePos(end, transform);
        transform.position = Vector3.Lerp(src, dest, time);
    }

    public void lerpToNext(Transform transform, float time)
    {
        if (time < lastTime)
            return;
        lastTime = time;
        if (currentIndex >= currentPath.Count)
            return;
        if (currentIndex < 1 && lastTilePreviousPath != null)
            lerpObjectPosition(lastTilePreviousPath, currentPath[currentIndex], transform, time);
        else if (currentIndex >= 1)
            lerpObjectPosition(currentPath[currentIndex - 1], currentPath[currentIndex], transform, time);
    }

    private Vector3 getTopTilePos(Tile tile, Transform transform)
    {
        Vector3 tilePosition = tile.tile3DRef.transform.position;
        Vector3 tileScale = tile.tile3DRef.transform.localScale;
        return new Vector3(tilePosition.x, tilePosition.y + tileScale.y + transform.localScale.y, tilePosition.z);
    }

    public int getCurrentTimeCost() //events -> 1, path ended -> 0
    {
        //TO DO: Normalize costs so that water dont take that long
        if (currentIndex < currentPath.Count)
        {
            Tile tile = currentPath[currentIndex];
            return (tile.type == TileType.Event) ? 1 : tile.timeCost;
        }
        return 0;
    }

    public void setCurrentPath(List<Tile> path)
    {
        currentPath = path;
    }

    public bool hasPath()
    {
        return currentPath != null;
    }

    public void reset()
    {
        currentIndex = 0;
        if (hasPath())
            lastTilePreviousPath = currentPath.Last();
        currentPath = null;
    }
}
