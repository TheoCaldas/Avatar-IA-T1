using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    private const string fileName = "Assets/Resources/text.txt";
    public GameObject baseTile;
    public List<Material> tileMaterials;
    public float tilesDistance = 10;
    
    [SerializeField]
    private GameObject plainTile;

    void Start()
    {
        (MapManager.Instance.tileMap, MapManager.Instance.eventTiles) = textToTileMap(fileName);
        renderTileMap(MapManager.Instance.tileMap);
        MapManager.Instance.eventTiles.Sort(Tile.compareByEventID);
        MapManager.Instance.StartPathFinding();
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
                GameObject newTile;
                if (tileMap[i, j].type == TileType.Plain)
                {
                    newTile = Instantiate(plainTile,transform);
                    newTile.SetActive(true);
                    newTile.transform.position = new Vector3(i * tilesDistance, 0, j * tilesDistance);
                    newTile.name = tileMap[i,j].ToString();
                }
                else
                {
                    newTile = Instantiate(baseTile, transform);
                    newTile.SetActive(true);
                    newTile.transform.position = new Vector3(i * tilesDistance, 0, j * tilesDistance);
                    newTile.GetComponent<Renderer>().material = tileTypeToMaterial(tileMap[i,j].type);
                    newTile.name = tileMap[i,j].ToString();
                }
                

                tileMap[i,j].tile3DRef = newTile;
            }
        }  
    }

    //returns both tileMap and a list of its events
    private (Tile[,], List<Tile>) textToTileMap(string fileName)
    {
        //read all lines of .txt
        string[] lines = System.IO.File.ReadAllLines(fileName);
        int nLines = lines.Length;
        int charPerLine = lines[0].Length;

        //new tile map matrix
        Tile[,] tileMap = new Tile[nLines,charPerLine];
        List<Tile> eventTiles = new List<Tile>();

        for (int i = 0; i < nLines; i++) //each line
        {
            for (int j = 0; j < charPerLine; j++) //each char in line
            {
                tileMap[i,j] = new Tile(lines[i][j], j, i); //create new tile
                if (tileMap[i,j].type == TileType.Event)
                    eventTiles.Add(tileMap[i,j]);
            }
        }   
        return (tileMap, eventTiles);
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