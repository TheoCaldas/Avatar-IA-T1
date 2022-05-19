using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BinaryHeap;

public class AStar
{
    public void aStar(Tile startTile, Tile endTile)
    {
        int capacity = 80 * 200;
        int maxDistance = 2000000;
        BinaryHeap<int, int> queue = new BinaryHeap<int, int>(capacity, -1, maxDistance);
        queue.Enqueue(3, 0);
        queue.Enqueue(4, 2);
        queue.Enqueue(5, 1);
        queue.Enqueue(6, 8);
        queue.Enqueue(7, 8);

        while (queue.Count() > 0)
            Debug.Log(queue.Dequeue());
    }
}
