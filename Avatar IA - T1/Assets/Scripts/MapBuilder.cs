using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    Tile[,] tileMap;
    private const string fileName = "Assets/Resources/text.txt";
    public GameObject baseTile;
    public List<Material> tileMaterials;
    public float tilesDistance = 10;
    void Start()
    {
        tileMap = textToTileMap(fileName);
        renderTileMap(tileMap);
    }

    private void renderTileMap(Tile[,] tileMap)
    {
        //get x and y dimensions
        int m = tileMap.GetLength(0);
        int n = tileMap.GetLength(1);

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                //copy base tile and reflect it to tileMap[i,j]
                GameObject newTile = Instantiate(baseTile);
                newTile.SetActive(true);
                newTile.transform.SetParent(transform);
                newTile.transform.position = new Vector3(i * tilesDistance, 0, j * tilesDistance);
                newTile.GetComponent<Renderer>().material = tileTypeToMaterial(tileMap[i,j].type);
                newTile.name = tileMap[i,j].ToString();

                tileMap[i,j].tile3DRef = newTile;
            }
        }  
    }

    private Tile[,] textToTileMap(string fileName)
    {
        //read all lines of .txt
        string[] lines = System.IO.File.ReadAllLines(fileName);
        int nLines = lines.Length;
        int charPerLine = lines[0].Length;

        //new tile map matrix
        Tile[,] tileMap = new Tile[nLines,charPerLine];

        for (int i = 0; i < nLines; i++) //each line
        {
            for (int j = 0; j < charPerLine; j++) //each char in line
            {
                tileMap[i,j] = new Tile(lines[i][j], i, j); //create new tile
            }
        }   
        return tileMap;
    }

    private Material tileTypeToMaterial(TileType type)
    {
        switch (type)
        {
            case TileType.Plain:
                return tileMaterials[0];
            case TileType.Rocky:
                return tileMaterials[1];
            case TileType.Florest:
                return tileMaterials[2];
            case TileType.Water:
                return tileMaterials[3];
            case TileType.Mountain:
                return tileMaterials[4];
            case TileType.Event:
                return tileMaterials[5];
            default:
                return tileMaterials[0];
        }
    }
}