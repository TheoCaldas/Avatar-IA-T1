using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    Tile[,] tileMap;
    void Start()
    {
        // byte[] caracteres = System.IO.File.ReadAllBytes("Assets/Resources/text.txt");
        // foreach (byte caracter in caracteres)
        // {
        //     Debug.Log((char) caracter);
        // }

        string[] lines = System.IO.File.ReadAllLines("Assets/Resources/text.txt");
        int nLines = lines.Length;
        int charPerLine = lines[0].Length;

        tileMap = new Tile[nLines,charPerLine];

        for (int i = 0; i < nLines; i++)
        {
            for (int j = 0; j < charPerLine; j++)
            {
                tileMap[i,j] = charToTile(lines[i][j]);
            }
        }   

    }

    // Update is called once per frame
    void Update()
    {
        
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
                return Tile.Moutain;
            default:
                return Tile.Plain;
        }
    }
}
