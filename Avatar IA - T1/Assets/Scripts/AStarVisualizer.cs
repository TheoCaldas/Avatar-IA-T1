using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarVisualizer
{
    //List of colors changes in tiles to help visualing the algorithm
    private List<Color> colors = new List<Color>();
    private List<Tile> tiles = new List<Tile>();
    private int currentIndex;

    public bool visualize() //did update visualization
    {
        if (currentIndex < colors.Count)
        {
            Tile tile = tiles[currentIndex];
            Color color = colors[currentIndex];
            tile.changeColor(color);
            currentIndex++;
            return true;
        }
        return false;
    }

    public void clearVisualization()
    {
        foreach (Tile tile in tiles)
            tile.revertColor();

        colors.Clear();
        tiles.Clear();
    }

    public void add(Tile tile, Color color)
    {
        tiles.Add(tile);
        colors.Add(color);
    }

    public void reset()
    {
        currentIndex = 0;
    }
}
