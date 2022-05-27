using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    private const string fileName = "Assets/Resources/mapa.txt";
    public GameObject baseTile;
    public List<GameObject> tilePrefabs;
    public float tilesDistance = 10;
    
    [SerializeField]
    private GameObject oceanBlock;

    void Start()
    {
        float temp;

        temp = Time.realtimeSinceStartup;
        (MapManager.Instance.tileMap, MapManager.Instance.eventTiles) = textToTileMap(fileName);
        Debug.Log("Did construct map! Took: " + (Time.realtimeSinceStartup - temp).ToString("f6") + " seconds");

        temp = Time.realtimeSinceStartup;
        renderTileMap(MapManager.Instance.tileMap);
        Debug.Log("Did render map! Took: " + (Time.realtimeSinceStartup - temp).ToString("f6") + " seconds");

        MapManager.Instance.eventTiles.Sort(Tile.compareByEventID);
        MapManager.Instance.StartPathFinding();
    }

    private void renderTileMap(Tile[,] tileMap)
    {
        //get x and y dimensions
        int m = tileMap.GetLength(0);
        int n = tileMap.GetLength(1);
        // Debug.Log("M = " + m.ToString() + ", N  = " + n.ToString());

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                //copy base tile and reflect it to tileMap[i,j]
                GameObject newTile;

                newTile = Instantiate(tileTypeToGameObject(tileMap[i,j].type), transform);
                newTile.SetActive(true);
                newTile.transform.position = new Vector3(i * tilesDistance, 0, j * tilesDistance);
                newTile.name = tileMap[i,j].ToString();
                
                tileMap[i,j].originalMaterial = Instantiate(newTile.GetComponentInChildren<Renderer>().material);
                tileMap[i,j].tile3DRef = newTile;

            }
        }
        oceanBlock.SetActive(true);
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

    private GameObject tileTypeToGameObject(TileType type)
    {
        switch (type)
        {
            case TileType.Plain:
                return tilePrefabs[0];
            case TileType.Rocky:
                return tilePrefabs[1];
            case TileType.Forest:
                return tilePrefabs[2];
            case TileType.Water:
                return tilePrefabs[3];
            case TileType.Mountain:
                return tilePrefabs[4];
            case TileType.Event:
                return tilePrefabs[5];
            default:
                return tilePrefabs[0];
        }
    }
}