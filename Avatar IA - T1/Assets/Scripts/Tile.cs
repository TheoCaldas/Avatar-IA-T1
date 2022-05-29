using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Plain = 1,
    Rocky = 5,
    Forest = 10,
    Water = 15,
    Mountain = 200,
    Event = 0
}

public class Tile
{
    public TileType type;
    public int xRef, yRef;
    public int timeCost;
    public int eventID;
    
    //References to tile object in scene
    public GameObject tile3DRef;
    public List<Material> originalMaterials = new List<Material>();
    public int heightFactor;

    public Tile(char caracter, int x, int y)
    {
        xRef = x;
        yRef = y;
        type = charToTileType(caracter);
        timeCost = (int) type;
        eventID = -1;
        heightFactor = -1;

        if (type == TileType.Event)
        {
            eventID = charToEventID(caracter);
            timeCost = eventID * 10;
        }
    }

    private TileType charToTileType(char c)
    {
        switch (c)
        {
            case '.':
                return TileType.Plain;
            case 'R':
                return TileType.Rocky;
            case 'F':
                return TileType.Forest;
            case 'A':
                return TileType.Water;
            case 'M':
                return TileType.Mountain;
            default:
                return TileType.Event;
        }
    }

    private int charToEventID(char c) //cost equals event number, skips A, F, M, R
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        if (c >= 'B' && c <= 'E')
            return c - 'B' + 10;
        if (c >= 'G' && c <= 'L')
            return c - 'G' + 14;
        if (c >= 'N' && c <= 'Q')
            return c - 'N' + 20;
        if (c >= 'S' && c <= 'Z')
            return c - 'S' + 24;
        return 0;
    }

    public static int compareByEventID(Tile a, Tile b)
    {
        return a.eventID - b.eventID;
    }

    public override string ToString()
    {
        return "Tile (" + xRef.ToString() + " , " + yRef.ToString() + ") - " + timeCost.ToString();
    }

    public void changeColor(Color color)
    {
        MeshRenderer[] renderers = tile3DRef.GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer r in renderers)
        {
            r.material.SetColor("_BaseColor", color);
            r.material.SetColor("_DeepColor", color);
            r.material.SetColor("_Tint", color);
            r.material.SetColor("_DarkerTint", color);
        }
    }

    public void revertColor()
    {
        MeshRenderer[] renderers = tile3DRef.GetComponentsInChildren<MeshRenderer>();
        int index = 0;
        foreach(MeshRenderer r in renderers)
        {
            r.material = originalMaterials[index];
            index++;
        }
    }

    public List<Tile> get4Neighbours(Tile[,] tileMap, int m, int n)
    {
        List<Tile> neighbours = new List<Tile>();
        int x = xRef;
        int y = yRef;

        int leftX = x - 1;
        int rightX = x + 1;
        int upY = y - 1;
        int downY = y + 1;

        if (leftX >= 0)
            neighbours.Add(tileMap[y, leftX]);
        if (rightX < n)
            neighbours.Add(tileMap[y, rightX]);
        if (upY >= 0)
            neighbours.Add(tileMap[upY, x]);
        if (downY < m)
            neighbours.Add(tileMap[downY, x]);
        
        return neighbours;
    }

    public List<Tile> get8Neighbours(Tile[,] tileMap, int m, int n)
    {
        List<Tile> neighbours = new List<Tile>();
        int x = xRef;
        int y = yRef;

        int leftX = x - 1;
        int rightX = x + 1;
        int upY = y - 1;
        int downY = y + 1;

        if (leftX >= 0)
            neighbours.Add(tileMap[y, leftX]);
        if (rightX < n)
            neighbours.Add(tileMap[y, rightX]);
        if (upY >= 0)
            neighbours.Add(tileMap[upY, x]);
        if (downY < m)
            neighbours.Add(tileMap[downY, x]);

        if (leftX >= 0 && upY >= 0)
            neighbours.Add(tileMap[upY, leftX]);
        if (leftX >= 0 && downY < m)
            neighbours.Add(tileMap[downY, leftX]);
        if (rightX < n && upY >= 0)
            neighbours.Add(tileMap[upY, rightX]);
        if (rightX < n && downY < m)
            neighbours.Add(tileMap[downY, rightX]);
        
        return neighbours;
    }
}