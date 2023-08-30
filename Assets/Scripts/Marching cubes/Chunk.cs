using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Chunk
{
    private int chunkWidth; //x
    private int chunkHeight; //y
    private int chunkDepth; //z
    public Point[] chunkPoints;

    public Chunk(int x, int y, int z) //width, height, depth
    {
        chunkWidth = x;
        chunkHeight = y;
        chunkDepth = z;
        chunkPoints = new Point[chunkWidth * chunkHeight * chunkDepth];
    }
    public void AddToChunk(Point point, int index)
    {
        chunkPoints[index] = point;
    }

    public Point this[int i]
    {
        get { return chunkPoints[i]; }
    }
}
