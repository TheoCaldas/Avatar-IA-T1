using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower
{
    private int currentIndex;
    public List<Tile> currentPath;
    public bool FollowPath() //did update following
    {
        //TO DO: Make reflect tile cost
        if (currentIndex < currentPath.Count)
        {
            Tile tile = currentPath[currentIndex];
            changeObjectPosition(tile, MapManager.Instance.character.transform);
            MapManager.Instance.pathCost += tile.timeCost;
            currentIndex++;
            return true;
        }
        return false;
    }

    public void changeObjectPosition(Tile tile, Transform transform)
    {
        Vector3 tilePosition = tile.tile3DRef.transform.position;
        Vector3 tileScale = tile.tile3DRef.transform.localScale;
        transform.position = new Vector3(tilePosition.x, tilePosition.y + tileScale.y + transform.localScale.y, tilePosition.z);
    }

    public void reset()
    {
        currentIndex = 0;
    }
}
