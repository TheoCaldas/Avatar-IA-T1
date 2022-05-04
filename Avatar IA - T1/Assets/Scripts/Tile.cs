using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Plain = 1,
    Rocky = 5,
    Florest = 10,
    Water = 15,
    Mountain = 200
}

public class Tile
{
    public TileType type;
    public int xRef, yRef;
    public GameObject tile3D;
    public int timeCost;
    public Tile(char caracter, int x, int y)
    {
        xRef = x;
        yRef = y;
        type = charToTileType(caracter);
        timeCost = (int) type;
    }

    private TileType charToTileType(char c)
    {
        switch (c)
        {
            case '.':
                return TileType.Plain;
            case 'R':
                return TileType.Rocky;
            case 'V':
                return TileType.Florest;
            case 'A':
                return TileType.Water;
            case 'M':
                return TileType.Mountain;
            default:
                return TileType.Plain;
        }
    }

    public override string ToString()
    {
        return "Tile (" + xRef.ToString() + " , " + yRef.ToString() + ") - " + timeCost.ToString();
    }
}