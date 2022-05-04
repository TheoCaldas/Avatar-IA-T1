using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    Tile[,] tileMap;
    private const string fileName = "Assets/Resources/text.txt";
    void Start()
    {
        tileMap = textToTileMap(fileName);
        renderTileMap(tileMap);
    }

    void Update()
    {
        
    }

    private void renderTileMap(Tile[,] tileMap)
    {
        int m = tileMap.GetLength(0);
        int n = tileMap.GetLength(1);

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                string childName = tileToString(tileMap[i,j]);
                GameObject child = transform.Find(childName).gameObject;
                GameObject newTile = Instantiate(child);
                newTile.SetActive(true);
                newTile.transform.SetParent(transform);
                newTile.transform.position = new Vector3(i * 10, 0, j * 10);
            }
        }  
    }

    private Tile[,] textToTileMap(string fileName)
    {
        string[] lines = System.IO.File.ReadAllLines(fileName);
        int nLines = lines.Length;
        int charPerLine = lines[0].Length;

        Tile[,] tileMap = new Tile[nLines,charPerLine];

        for (int i = 0; i < nLines; i++)
        {
            for (int j = 0; j < charPerLine; j++)
            {
                tileMap[i,j] = charToTile(lines[i][j]);
            }
        }   
        return tileMap;
    }

    private Tile charToTile(char c)
    {
        switch (c)
        {
            case '.':
                return Tile.Plain;
            case 'R':
                return Tile.Rocky;
            case 'V':
                return Tile.Florest;
            case 'A':
                return Tile.Water;
            case 'M':
                return Tile.Mountain;
            default:
                return Tile.Plain;
        }
    }

    private string tileToString(Tile tile)
    {
        switch (tile)
        {
            case Tile.Plain:
                return "Plain Tile";
            case Tile.Rocky:
                return "Rocky Tile";
            case Tile.Florest:
                return "Florest Tile";
            case Tile.Water:
                return "Water Tile";
            case Tile.Mountain:
                return "Mountain Tile";
            default:
                return "Plain Tile";
        }
    }
}
