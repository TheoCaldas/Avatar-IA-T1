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
    public Material originalMaterial;

    public Tile(char caracter, int x, int y)
    {
        xRef = x;
        yRef = y;
        type = charToTileType(caracter);
        timeCost = (int) type;
        eventID = -1;

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
        MeshRenderer r = tile3DRef.GetComponentInChildren<MeshRenderer>();
        r.material.SetColor("_BaseColor", color);
        if (type == TileType.Water)
            r.material.SetColor("_DeepColor", color);
    }

    public void revertColor()
    {
        tile3DRef.GetComponentInChildren<MeshRenderer>().material = originalMaterial;
    }
}