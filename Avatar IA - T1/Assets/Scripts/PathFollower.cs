using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower
{
    private int followPathIndex;
    public List<Tile> currentPath;
    public bool FollowPath() //did update following
    {
        //TO DO: Make reflect tile cost
        if (followPathIndex < currentPath.Count)
        {
            Tile tile = currentPath[followPathIndex];
            changeObjectPosition(tile, MapManager.Instance.character.transform);
            MapManager.Instance.pathCost += tile.timeCost;
            followPathIndex++;
            return true;
        }
        return false;
    }

    public void changeObjectPosition(Tile tile, Transform transform)
    {
        Vector3 tilePosition = tile.tile3DRef.transform.position;
        transform.position = new Vector3(tilePosition.x, transform.position.y, tilePosition.z);
    }

    public void reset()
    {
        followPathIndex = 0;
    }
}
