using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Plain = 1,
    Rocky = 5,
    Florest = 10,
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

    public GameObject tile3DRef; //TO DO: need to be here?

    public Tile(char caracter, int x, int y)
    {
        xRef = x;
        yRef = y;
        type = charToTileType(caracter);
        timeCost = (type != TileType.Event) ? (int) type : charToEventCost(caracter);
        eventID = (type != TileType.Event) ? -1 : timeCost;
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
                return TileType.Event;
        }
    }

    //TO DO: Match cost with definition table
    private int charToEventCost(char c) //cost equals event number, skips A, R, M, V
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        if (c >= 'B' && c <= 'L')
            return c - 'B' + 10;
        if (c >= 'N' && c <= 'Q')
            return c - 'N' + 21;
        if (c >= 'S' && c <= 'U')
            return c - 'S' + 25;
        if (c >= 'W' && c <= 'Z')
            return c - 'W' + 28;
        return 0;
    }

    public override string ToString()
    {
        return "Tile (" + xRef.ToString() + " , " + yRef.ToString() + ") - " + timeCost.ToString();
    }
}