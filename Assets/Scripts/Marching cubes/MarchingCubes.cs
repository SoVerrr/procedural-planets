using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
public class MarchingCubes : MonoBehaviour
{
    [SerializeField] private CubeGridJob cubeGrid;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] GameObject chunkParent;
    JobHandle job;
    //List<Vector3> vertices = new List<Vector3>();
    NativeArray<int> triangles;
    NativeArray<Vector3> vertices;
    NativeArray<int> nativeTriangulations;
    NativeArray<Vector3Int> nativeCorners;
    NativeArray<Vector2Int> nativeEdges;
    GameObject meshObject;
    public static Vector3Int[] corners = new Vector3Int[8]
    {
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(1, 1, 0)
    };
    public static Vector2Int[] edges = new Vector2Int[12]
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 2),
        new Vector2Int(2, 3),
        new Vector2Int(3, 0),
        new Vector2Int(4, 5),
        new Vector2Int(5, 6),
        new Vector2Int(6, 7),
        new Vector2Int(7, 4),
        new Vector2Int(0, 4),
        new Vector2Int(1, 5),
        new Vector2Int(2, 6),
        new Vector2Int(3, 7)
    };
    private static int[,] triangulations = new int[256, 15]
    {
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  8,  3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  1,  9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  8,  3,  9,  8,  1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  2,  10,-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  8,  3,  1,  2,  10,-1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9,  2,  10, 0,  2,  9, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 2,  8,  3,  2,  10, 8,  10, 9,  8, -1, -1, -1, -1, -1, -1 },
        { 3,  11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  11, 2,  8,  11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  9,  0,  2,  3,  11,-1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  11, 2,  1,  9,  11, 9,  8,  11,-1, -1, -1, -1, -1, -1 },
        { 3,  10, 1,  11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  10, 1,  0,  8,  10, 8,  11, 10,-1, -1, -1, -1, -1, -1 },
        { 3,  9,  0,  3,  11, 9,  11, 10, 9, -1, -1, -1, -1, -1, -1 },
        { 9,  8,  10, 10, 8,  11,-1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4,  7,  8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4,  3,  0,  7,  3,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  1,  9,  8,  4,  7, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4,  1,  9,  4,  7,  1,  7,  3,  1, -1, -1, -1, -1, -1, -1 },
        { 1,  2,  10, 8,  4,  7, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3,  4,  7,  3,  0,  4,  1,  2,  10,-1, -1, -1, -1, -1, -1 },
        { 9,  2,  10, 9,  0,  2,  8,  4,  7, -1, -1, -1, -1, -1, -1 },
        { 2,  10, 9,  2,  9,  7,  2,  7,  3,  7,  9,  4, -1, -1, -1 },
        { 8,  4,  7,  3,  11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 11, 4,  7,  11, 2,  4,  2,  0,  4, -1, -1, -1, -1, -1, -1 },
        { 9,  0,  1,  8,  4,  7,  2,  3,  11,-1, -1, -1, -1, -1, -1 },
        { 4,  7,  11, 9,  4,  11, 9,  11, 2,  9,  2,  1, -1, -1, -1 },
        { 3,  10, 1,  3,  11, 10, 7,  8,  4, -1, -1, -1, -1, -1, -1 },
        { 1,  11, 10, 1,  4,  11, 1,  0,  4,  7,  11, 4, -1, -1, -1 },
        { 4,  7,  8,  9,  0,  11, 9,  11, 10, 11, 0,  3, -1, -1, -1 },
        { 4,  7,  11, 4,  11, 9,  9,  11, 10,-1, -1, -1, -1, -1, -1 },
        { 9,  5,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9,  5,  4,  0,  8,  3, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  5,  4,  1,  5,  0, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 8,  5,  4,  8,  3,  5,  3,  1,  5, -1, -1, -1, -1, -1, -1 },
        { 1,  2,  10, 9,  5,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3,  0,  8,  1,  2,  10, 4,  9,  5, -1, -1, -1, -1, -1, -1 },
        { 5,  2,  10, 5,  4,  2,  4,  0,  2, -1, -1, -1, -1, -1, -1 },
        { 2,  10, 5,  3,  2,  5,  3,  5,  4,  3,  4,  8, -1, -1, -1 },
        { 9,  5,  4,  2,  3,  11,-1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  11, 2,  0,  8,  11, 4,  9,  5, -1, -1, -1, -1, -1, -1 },
        { 0,  5,  4,  0,  1,  5,  2,  3,  11,-1, -1, -1, -1, -1, -1 },
        { 2,  1,  5,  2,  5,  8,  2,  8,  11, 4,  8,  5, -1, -1, -1 },
        { 10, 3,  11, 10, 1,  3,  9,  5,  4, -1, -1, -1, -1, -1, -1 },
        { 4,  9,  5,  0,  8,  1,  8,  10, 1,  8,  11, 10,-1, -1, -1 },
        { 5,  4,  0,  5,  0,  11, 5,  11, 10, 11, 0,  3, -1, -1, -1 },
        { 5,  4,  8,  5,  8,  10, 10, 8,  11,-1, -1, -1, -1, -1, -1 },
        { 9,  7,  8,  5,  7,  9, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9,  3,  0,  9,  5,  3,  5,  7,  3, -1, -1, -1, -1, -1, -1 },
        { 0,  7,  8,  0,  1,  7,  1,  5,  7, -1, -1, -1, -1, -1, -1 },
        { 1,  5,  3,  3,  5,  7, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9,  7,  8,  9,  5,  7,  10, 1,  2, -1, -1, -1, -1, -1, -1 },
        { 10, 1,  2,  9,  5,  0,  5,  3,  0,  5,  7,  3, -1, -1, -1 },
        { 8,  0,  2,  8,  2,  5,  8,  5,  7,  10, 5,  2, -1, -1, -1 },
        { 2,  10, 5,  2,  5,  3,  3,  5,  7, -1, -1, -1, -1, -1, -1 },
        { 7,  9,  5,  7,  8,  9,  3,  11, 2, -1, -1, -1, -1, -1, -1 },
        { 9,  5,  7,  9,  7,  2,  9,  2,  0,  2,  7,  11,-1, -1, -1 },
        { 2,  3,  11, 0,  1,  8,  1,  7,  8,  1,  5,  7, -1, -1, -1 },
        { 11, 2,  1,  11, 1,  7,  7,  1,  5, -1, -1, -1, -1, -1, -1 },
        { 9,  5,  8,  8,  5,  7,  10, 1,  3,  10, 3,  11,-1, -1, -1 },
        { 5,  7,  0,  5,  0,  9,  7,  11, 0,  1,  0,  10, 11, 10, 0 },
        { 11, 10, 0,  11, 0,  3,  10, 5,  0,  8,  0,  7,  5,  7,  0 },
        { 11, 10, 5,  7,  11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 6,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  8,  3,  5,  10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9,  0,  1,  5,  10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  8,  3,  1,  9,  8,  5,  10, 6, -1, -1, -1, -1, -1, -1 },
        { 1,  6,  5,  2,  6,  1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  6,  5,  1,  2,  6,  3,  0,  8, -1, -1, -1, -1, -1, -1 },
        { 9,  6,  5,  9,  0,  6,  0,  2,  6, -1, -1, -1, -1, -1, -1 },
        { 5,  9,  8,  5,  8,  2,  5,  2,  6,  3,  2,  8, -1, -1, -1 },
        { 2,  3,  11, 10, 6,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 11, 0,  8,  11, 2,  0,  10, 6,  5, -1, -1, -1, -1, -1, -1 },
        { 0,  1,  9,  2,  3,  11, 5,  10, 6, -1, -1, -1, -1, -1, -1 },
        { 5,  10, 6,  1,  9,  2,  9,  11, 2,  9,  8,  11,-1, -1, -1 },
        { 6,  3,  11, 6,  5,  3,  5,  1,  3, -1, -1, -1, -1, -1, -1 },
        { 0,  8,  11, 0,  11, 5,  0,  5,  1,  5,  11, 6, -1, -1, -1 },
        { 3,  11, 6,  0,  3,  6,  0,  6,  5,  0,  5,  9, -1, -1, -1 },
        { 6,  5,  9,  6,  9,  11, 11, 9,  8, -1, -1, -1, -1, -1, -1 },
        { 5,  10, 6,  4,  7,  8, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4,  3,  0,  4,  7,  3,  6,  5,  10,-1, -1, -1, -1, -1, -1 },
        { 1,  9,  0,  5,  10, 6,  8,  4,  7, -1, -1, -1, -1, -1, -1 },
        { 10, 6,  5,  1,  9,  7,  1,  7,  3,  7,  9,  4, -1, -1, -1 },
        { 6,  1,  2,  6,  5,  1,  4,  7,  8, -1, -1, -1, -1, -1, -1 },
        { 1,  2,  5,  5,  2,  6,  3,  0,  4,  3,  4,  7, -1, -1, -1 },
        { 8,  4,  7,  9,  0,  5,  0,  6,  5,  0,  2,  6, -1, -1, -1 },
        { 7,  3,  9,  7,  9,  4,  3,  2,  9,  5,  9,  6,  2,  6,  9 },
        { 3,  11, 2,  7,  8,  4,  10, 6,  5, -1, -1, -1, -1, -1, -1 },
        { 5,  10, 6,  4,  7,  2,  4,  2,  0,  2,  7,  11,-1, -1, -1 },
        { 0,  1,  9,  4,  7,  8,  2,  3,  11, 5,  10, 6, -1, -1, -1 },
        { 9,  2,  1,  9,  11, 2,  9,  4,  11, 7,  11, 4,  5,  10, 6 },
        { 8,  4,  7,  3,  11, 5,  3,  5,  1,  5,  11, 6, -1, -1, -1 },
        { 5,  1,  11, 5,  11, 6,  1,  0,  11, 7,  11, 4,  0,  4,  11},
        { 0,  5,  9,  0,  6,  5,  0,  3,  6,  11, 6,  3,  8,  4,  7 },
        { 6,  5,  9,  6,  9,  11, 4,  7,  9,  7,  11, 9, -1, -1, -1 },
        { 10, 4,  9,  6,  4,  10,-1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4,  10, 6,  4,  9,  10, 0,  8,  3, -1, -1, -1, -1, -1, -1 },
        { 10, 0,  1,  10, 6,  0,  6,  4,  0, -1, -1, -1, -1, -1, -1 },
        { 8,  3,  1,  8,  1,  6,  8,  6,  4,  6,  1,  10,-1, -1, -1 },
        { 1,  4,  9,  1,  2,  4,  2,  6,  4, -1, -1, -1, -1, -1, -1 },
        { 3,  0,  8,  1,  2,  9,  2,  4,  9,  2,  6,  4, -1, -1, -1 },
        { 0,  2,  4,  4,  2,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 8,  3,  2,  8,  2,  4,  4,  2,  6, -1, -1, -1, -1, -1, -1 },
        { 10, 4,  9,  10, 6,  4,  11, 2,  3, -1, -1, -1, -1, -1, -1 },
        { 0,  8,  2,  2,  8,  11, 4,  9,  10, 4,  10, 6, -1, -1, -1 },
        { 3,  11, 2,  0,  1,  6,  0,  6,  4,  6,  1,  10,-1, -1, -1 },
        { 6,  4,  1,  6,  1,  10, 4,  8,  1,  2,  1,  11, 8,  11, 1 },
        { 9,  6,  4,  9,  3,  6,  9,  1,  3,  11, 6,  3, -1, -1, -1 },
        { 8,  11, 1,  8,  1,  0,  11, 6,  1,  9,  1,  4,  6,  4,  1 },
        { 3,  11, 6,  3,  6,  0,  0,  6,  4, -1, -1, -1, -1, -1, -1 },
        { 6,  4,  8,  11, 6,  8, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 7,  10, 6,  7,  8,  10, 8,  9,  10,-1, -1, -1, -1, -1, -1 },
        { 0,  7,  3,  0,  10, 7,  0,  9,  10, 6,  7,  10,-1, -1, -1 },
        { 10, 6,  7,  1,  10, 7,  1,  7,  8,  1,  8,  0, -1, -1, -1 },
        { 10, 6,  7,  10, 7,  1,  1,  7,  3, -1, -1, -1, -1, -1, -1 },
        { 1,  2,  6,  1,  6,  8,  1,  8,  9,  8,  6,  7, -1, -1, -1 },
        { 2,  6,  9,  2,  9,  1,  6,  7,  9,  0,  9,  3,  7,  3,  9 },
        { 7,  8,  0,  7,  0,  6,  6,  0,  2, -1, -1, -1, -1, -1, -1 },
        { 7,  3,  2,  6,  7,  2, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 2,  3,  11, 10, 6,  8,  10, 8,  9,  8,  6,  7, -1, -1, -1 },
        { 2,  0,  7,  2,  7,  11, 0,  9,  7,  6,  7,  10, 9,  10, 7 },
        { 1,  8,  0,  1,  7,  8,  1,  10, 7,  6,  7,  10, 2,  3,  11},
        { 11, 2,  1,  11, 1,  7,  10, 6,  1,  6,  7,  1, -1, -1, -1 },
        { 8,  9,  6,  8,  6,  7,  9,  1,  6,  11, 6,  3,  1,  3,  6 },
        { 0,  9,  1,  11, 6,  7, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 7,  8,  0,  7,  0,  6,  3,  11, 0,  11, 6,  0, -1, -1, -1 },
        { 7,  11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 7,  6,  11,-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3,  0,  8,  11, 7,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  1,  9,  11, 7,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 8,  1,  9,  8,  3,  1,  11, 7,  6, -1, -1, -1, -1, -1, -1 },
        { 10, 1,  2,  6,  11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  2,  10, 3,  0,  8,  6,  11, 7, -1, -1, -1, -1, -1, -1 },
        { 2,  9,  0,  2,  10, 9,  6,  11, 7, -1, -1, -1, -1, -1, -1 },
        { 6,  11, 7,  2,  10, 3,  10, 8,  3,  10, 9,  8, -1, -1, -1 },
        { 7,  2,  3,  6,  2,  7, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 7,  0,  8,  7,  6,  0,  6,  2,  0, -1, -1, -1, -1, -1, -1 },
        { 2,  7,  6,  2,  3,  7,  0,  1,  9, -1, -1, -1, -1, -1, -1 },
        { 1,  6,  2,  1,  8,  6,  1,  9,  8,  8,  7,  6, -1, -1, -1 },
        { 10, 7,  6,  10, 1,  7,  1,  3,  7, -1, -1, -1, -1, -1, -1 },
        { 10, 7,  6,  1,  7,  10, 1,  8,  7,  1,  0,  8, -1, -1, -1 },
        { 0,  3,  7,  0,  7,  10, 0,  10, 9,  6,  10, 7, -1, -1, -1 },
        { 7,  6,  10, 7,  10, 8,  8,  10, 9, -1, -1, -1, -1, -1, -1 },
        { 6,  8,  4,  11, 8,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3,  6,  11, 3,  0,  6,  0,  4,  6, -1, -1, -1, -1, -1, -1 },
        { 8,  6,  11, 8,  4,  6,  9,  0,  1, -1, -1, -1, -1, -1, -1 },
        { 9,  4,  6,  9,  6,  3,  9,  3,  1,  11, 3,  6, -1, -1, -1 },
        { 6,  8,  4,  6,  11, 8,  2,  10, 1, -1, -1, -1, -1, -1, -1 },
        { 1,  2,  10, 3,  0,  11, 0,  6,  11, 0,  4,  6, -1, -1, -1 },
        { 4,  11, 8,  4,  6,  11, 0,  2,  9,  2,  10, 9, -1, -1, -1 },
        { 10, 9,  3,  10, 3,  2,  9,  4,  3,  11, 3,  6,  4,  6,  3 },
        { 8,  2,  3,  8,  4,  2,  4,  6,  2, -1, -1, -1, -1, -1, -1 },
        { 0,  4,  2,  4,  6,  2, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  9,  0,  2,  3,  4,  2,  4,  6,  4,  3,  8, -1, -1, -1 },
        { 1,  9,  4,  1,  4,  2,  2,  4,  6, -1, -1, -1, -1, -1, -1 },
        { 8,  1,  3,  8,  6,  1,  8,  4,  6,  6,  10, 1, -1, -1, -1 },
        { 10, 1,  0,  10, 0,  6,  6,  0,  4, -1, -1, -1, -1, -1, -1 },
        { 4,  6,  3,  4,  3,  8,  6,  10, 3,  0,  3,  9,  10, 9,  3 },
        { 10, 9,  4,  6,  10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4,  9,  5,  7,  6,  11,-1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  8,  3,  4,  9,  5,  11, 7,  6, -1, -1, -1, -1, -1, -1 },
        { 5,  0,  1,  5,  4,  0,  7,  6,  11,-1, -1, -1, -1, -1, -1 },
        { 11, 7,  6,  8,  3,  4,  3,  5,  4,  3,  1,  5, -1, -1, -1 },
        { 9,  5,  4,  10, 1,  2,  7,  6,  11,-1, -1, -1, -1, -1, -1 },
        { 6,  11, 7,  1,  2,  10, 0,  8,  3,  4,  9,  5, -1, -1, -1 },
        { 7,  6,  11, 5,  4,  10, 4,  2,  10, 4,  0,  2, -1, -1, -1 },
        { 3,  4,  8,  3,  5,  4,  3,  2,  5,  10, 5,  2,  11, 7,  6 },
        { 7,  2,  3,  7,  6,  2,  5,  4,  9, -1, -1, -1, -1, -1, -1 },
        { 9,  5,  4,  0,  8,  6,  0,  6,  2,  6,  8,  7, -1, -1, -1 },
        { 3,  6,  2,  3,  7,  6,  1,  5,  0,  5,  4,  0, -1, -1, -1 },
        { 6,  2,  8,  6,  8,  7,  2,  1,  8,  4,  8,  5,  1,  5,  8 },
        { 9,  5,  4,  10, 1,  6,  1,  7,  6,  1,  3,  7, -1, -1, -1 },
        { 1,  6,  10, 1,  7,  6,  1,  0,  7,  8,  7,  0,  9,  5,  4 },
        { 4,  0,  10, 4,  10, 5,  0,  3,  10, 6,  10, 7,  3,  7,  10},
        { 7,  6,  10, 7,  10, 8,  5,  4,  10, 4,  8,  10,-1, -1, -1 },
        { 6,  9,  5,  6,  11, 9,  11, 8,  9, -1, -1, -1, -1, -1, -1 },
        { 3,  6,  11, 0,  6,  3,  0,  5,  6,  0,  9,  5, -1, -1, -1 },
        { 0,  11, 8,  0,  5,  11, 0,  1,  5,  5,  6,  11,-1, -1, -1 },
        { 6,  11, 3,  6,  3,  5,  5,  3,  1, -1, -1, -1, -1, -1, -1 },
        { 1,  2,  10, 9,  5,  11, 9,  11, 8,  11, 5,  6, -1, -1, -1 },
        { 0,  11, 3,  0,  6,  11, 0,  9,  6,  5,  6,  9,  1,  2,  10},
        { 11, 8,  5,  11, 5,  6,  8,  0,  5,  10, 5,  2,  0,  2,  5 },
        { 6,  11, 3,  6,  3,  5,  2,  10, 3,  10, 5,  3, -1, -1, -1 },
        { 5,  8,  9,  5,  2,  8,  5,  6,  2,  3,  8,  2, -1, -1, -1 },
        { 9,  5,  6,  9,  6,  0,  0,  6,  2, -1, -1, -1, -1, -1, -1 },
        { 1,  5,  8,  1,  8,  0,  5,  6,  8,  3,  8,  2,  6,  2,  8 },
        { 1,  5,  6,  2,  1,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  3,  6,  1,  6,  10, 3,  8,  6,  5,  6,  9,  8,  9,  6 },
        { 10, 1,  0,  10, 0,  6,  9,  5,  0,  5,  6,  0, -1, -1, -1 },
        { 0,  3,  8,  5,  6,  10,-1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 10, 5,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 11, 5,  10, 7,  5,  11,-1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 11, 5,  10, 11, 7,  5,  8,  3,  0, -1, -1, -1, -1, -1, -1 },
        { 5,  11, 7,  5,  10, 11, 1,  9,  0, -1, -1, -1, -1, -1, -1 },
        { 10, 7,  5,  10, 11, 7,  9,  8,  1,  8,  3,  1, -1, -1, -1 },
        { 11, 1,  2,  11, 7,  1,  7,  5,  1, -1, -1, -1, -1, -1, -1 },
        { 0,  8,  3,  1,  2,  7,  1,  7,  5,  7,  2,  11,-1, -1, -1 },
        { 9,  7,  5,  9,  2,  7,  9,  0,  2,  2,  11, 7, -1, -1, -1 },
        { 7,  5,  2,  7,  2,  11, 5,  9,  2,  3,  2,  8,  9,  8,  2 },
        { 2,  5,  10, 2,  3,  5,  3,  7,  5, -1, -1, -1, -1, -1, -1 },
        { 8,  2,  0,  8,  5,  2,  8,  7,  5,  10, 2,  5, -1, -1, -1 },
        { 9,  0,  1,  5,  10, 3,  5,  3,  7,  3,  10, 2, -1, -1, -1 },
        { 9,  8,  2,  9,  2,  1,  8,  7,  2,  10, 2,  5,  7,  5,  2 },
        { 1,  3,  5,  3,  7,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  8,  7,  0,  7,  1,  1,  7,  5, -1, -1, -1, -1, -1, -1 },
        { 9,  0,  3,  9,  3,  5,  5,  3,  7, -1, -1, -1, -1, -1, -1 },
        { 9,  8,  7,  5,  9,  7, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 5,  8,  4,  5,  10, 8,  10, 11, 8, -1, -1, -1, -1, -1, -1 },
        { 5,  0,  4,  5,  11, 0,  5,  10, 11, 11, 3,  0, -1, -1, -1 },
        { 0,  1,  9,  8,  4,  10, 8,  10, 11, 10, 4,  5, -1, -1, -1 },
        { 10, 11, 4,  10, 4,  5,  11, 3,  4,  9,  4,  1,  3,  1,  4 },
        { 2,  5,  1,  2,  8,  5,  2,  11, 8,  4,  5,  8, -1, -1, -1 },
        { 0,  4,  11, 0,  11, 3,  4,  5,  11, 2,  11, 1,  5,  1,  11},
        { 0,  2,  5,  0,  5,  9,  2,  11, 5,  4,  5,  8,  11, 8,  5 },
        { 9,  4,  5,  2,  11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 2,  5,  10, 3,  5,  2,  3,  4,  5,  3,  8,  4, -1, -1, -1 },
        { 5,  10, 2,  5,  2,  4,  4,  2,  0, -1, -1, -1, -1, -1, -1 },
        { 3,  10, 2,  3,  5,  10, 3,  8,  5,  4,  5,  8,  0,  1,  9 },
        { 5,  10, 2,  5,  2,  4,  1,  9,  2,  9,  4,  2, -1, -1, -1 },
        { 8,  4,  5,  8,  5,  3,  3,  5,  1, -1, -1, -1, -1, -1, -1 },
        { 0,  4,  5,  1,  0,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 8,  4,  5,  8,  5,  3,  9,  0,  5,  0,  3,  5, -1, -1, -1 },
        { 9,  4,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4,  11, 7,  4,  9,  11, 9,  10, 11,-1, -1, -1, -1, -1, -1 },
        { 0,  8,  3,  4,  9,  7,  9,  11, 7,  9,  10, 11,-1, -1, -1 },
        { 1,  10, 11, 1,  11, 4,  1,  4,  0,  7,  4,  11,-1, -1, -1 },
        { 3,  1,  4,  3,  4,  8,  1,  10, 4,  7,  4,  11, 10, 11, 4 },
        { 4,  11, 7,  9,  11, 4,  9,  2,  11, 9,  1,  2, -1, -1, -1 },
        { 9,  7,  4,  9,  11, 7,  9,  1,  11, 2,  11, 1,  0,  8,  3 },
        { 11, 7,  4,  11, 4,  2,  2,  4,  0, -1, -1, -1, -1, -1, -1 },
        { 11, 7,  4,  11, 4,  2,  8,  3,  4,  3,  2,  4, -1, -1, -1 },
        { 2,  9,  10, 2,  7,  9,  2,  3,  7,  7,  4,  9, -1, -1, -1 },
        { 9,  10, 7,  9,  7,  4,  10, 2,  7,  8,  7,  0,  2,  0,  7 },
        { 3,  7,  10, 3,  10, 2,  7,  4,  10, 1,  10, 0,  4,  0,  10},
        { 1,  10, 2,  8,  7,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4,  9,  1,  4,  1,  7,  7,  1,  3, -1, -1, -1, -1, -1, -1 },
        { 4,  9,  1,  4,  1,  7,  0,  8,  1,  8,  7,  1, -1, -1, -1 },
        { 4,  0,  3,  7,  4,  3, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 4,  8,  7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 9,  10, 8,  10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3,  0,  9,  3,  9,  11, 11, 9,  10,-1, -1, -1, -1, -1, -1 },
        { 0,  1,  10, 0,  10, 8,  8,  10, 11,-1, -1, -1, -1, -1, -1 },
        { 3,  1,  10, 11, 3,  10,-1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  2,  11, 1,  11, 9,  9,  11, 8, -1, -1, -1, -1, -1, -1 },
        { 3,  0,  9,  3,  9,  11, 1,  2,  9,  2,  11, 9, -1, -1, -1 },
        { 0,  2,  11, 8,  0,  11,-1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 3,  2,  11,-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 2,  3,  8,  2,  8,  10, 10, 8,  9, -1, -1, -1, -1, -1, -1 },
        { 9,  10, 2,  0,  9,  2, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 2,  3,  8,  2,  8,  10, 0,  1,  8,  1,  10, 8, -1, -1, -1 },
        { 1,  10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 1,  3,  8,  9,  1,  8, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  9,  1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        { 0,  3,  8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
    }; //credit https://www.youtube.com/watch?v=KvwVYJY_IZ4
    private int[] GetTriangulation(Point cube) //Takes the triangulation config index and returns from the triangulation array
    {
        int config_idx = GetTriangulationIndex(cube);
        int[] triangulation = new int[15];


        for (int i = 0; i < 15; i++)
        {
            triangulation[i] = triangulations[config_idx, i];
        }

        return triangulation;
    }
    public int GetTriangulationIndex(Point cube) //If a corner is "active" it adds 2^i to the binary config index which corresponds to the index in the triangulation array
    {
        int config_idx = 0b00000000;

        Vector3Int cube_idx = cubeGrid.PositionToIndex(cube.pointPosition);
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3Int corner_idx = cube_idx + corners[i];
            if (cubeGrid.AccessPointIndex(corner_idx.x, corner_idx.y, corner_idx.z).pointOn)
            {
                config_idx += (int)Mathf.Pow(2, i);
            }
            corner_idx -= corners[i];
        }

        return config_idx;
    }
    public Vector3 GetEdgeVector(Vector3 pointPoisition, Vector2Int edgeIndex) //calculating the Vector3 in the middle of the edge of 2 given cube corners
    {
        Vector3 edge;
        Vector3 cornerA = new Vector3(corners[edgeIndex.x].x * cubeGrid.edgeLength, corners[edgeIndex.x].y * cubeGrid.edgeLength, corners[edgeIndex.x].z * cubeGrid.edgeLength);
        Vector3 cornerB = new Vector3(corners[edgeIndex.y].x * cubeGrid.edgeLength, corners[edgeIndex.y].y * cubeGrid.edgeLength, corners[edgeIndex.y].z * cubeGrid.edgeLength);
        edge = ((pointPoisition + cornerA) + (pointPoisition + cornerB)) / 2;
        return edge;
    }

    /*public void MarchCubes()
    {
        List<int> triangulations = new List<int>();
        Vector3Int gridSize = cubeGrid.GetGridSizes();
        int triang = 0;
        for (int x = 0; x < gridSize.x - 1; x++) //Iterating through the cube without going on outermost faces
        {
            for (int y = 0; y < gridSize.y - 1; y++)
            {
                for (int z = 0; z < gridSize.z - 1; z++)
                {
                    int[] triangulation = GetTriangulation(cubeGrid.AccessPointIndex(x, y, z)); //Calculating triangulations
                    foreach (var item in triangulation)
                    {
                        if (item != -1) //If the triangulation is not -1 then the edge vertex with the index from triangulations goes into vertices array and is added to triangles 
                        {
                            vertices.Add(GetEdgeVector(cubeGrid.AccessPointIndex(x, y, z).pointPosition, edges[item]));
                            triangulations.Add(triang);
                            triang++;
                        }
                    }
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //incrase the triangle limit from uint16 to uint32
        vertices.Reverse();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangulations.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }*/
    [BurstCompile]
    public struct MarchChunks : IJobParallelFor
    {
        public int chunkSizeX;
        public int chunkSizeY;
        public int chunkSizeZ;
        public int chunkAmountX;
        public int chunkAmountY;
        public int chunkAmountZ;
        public int chunkID;
        public NativeArray<int> triangulations;
        public NativeArray<Vector3Int> corners;
        public NativeArray<Vector2Int> edges;
        public NativeArray<int> verticeCounter;
        public NativeArray<int> triangleCounter;
        [NativeDisableParallelForRestriction] public NativeArray<int> triangles;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> vertices;
        [NativeDisableParallelForRestriction] public NativeArray<Point> chunkPoints;

        public float edgeLength;
        private Vector3 CalculateStartPosition(int i)
        {
            Vector3 startPosition = new Vector3();

            startPosition.x = (i % chunkAmountX) * edgeLength * chunkSizeX;
            startPosition.y = (Mathf.Floor((i / Mathf.Pow(chunkAmountY, 2))) % chunkAmountY) * edgeLength * chunkSizeZ;
            startPosition.z = (Mathf.Floor((i / chunkAmountZ)) % chunkAmountZ) * edgeLength * chunkSizeZ;
            return startPosition;
        }
        public Vector3Int PositionToIndex(Vector3 position)
        {
            Vector3 startPosition = CalculateStartPosition(chunkID);
            int zIndex = (int)Mathf.Round((position.z / edgeLength) - startPosition.z);
            int xIndex = (int)Mathf.Round((position.x / edgeLength) - startPosition.x);
            int yIndex = (int)Mathf.Round((position.y / edgeLength) - startPosition.y);

            
            return new Vector3Int(xIndex, yIndex, zIndex);
        }
        public Point AccessPointIndex(int x, int y, int z)
        {
            int i = (x * chunkSizeX * chunkSizeY + y * chunkSizeZ + z);
            return chunkPoints[i];
        }
        
        private NativeArray<int> GetTriangulation(Point cube) //Takes the triangulation config index and returns from the triangulation array
        {
            int config_idx = GetTriangulationIndex(cube) * 15;
            NativeArray<int> triangulation = new NativeArray<int>(15, Allocator.Temp);


            for (int i = 0; i < 15; i++)
            {
                triangulation[i] = triangulations[config_idx + i];
            }

            return triangulation;
        }

        public int GetTriangulationIndex(Point cube) //If a corner is "active" it adds 2^i to the binary config index which corresponds to the index in the triangulation array
        {
            int config_idx = 0b00000000;

            Vector3Int cube_idx = PositionToIndex(cube.pointPosition);
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3Int corner_idx = cube_idx + corners[i];
                if (AccessPointIndex(corner_idx.x, corner_idx.y, corner_idx.z).pointOn)
                {
                    config_idx += (int)Mathf.Pow(2, i);
                }
                corner_idx -= corners[i];
            }

            return config_idx;
        }

        /*public MarchChunks(NativeArray<Point> chunkPoints, int chunkSizeX, int chunkSizeY, int chunkSizeZ, NativeArray<int> triangles, NativeArray<Vector3> vertices, float edgeLength,
            int chunkAmountX, int chunkAmountZ, int chunkAmountY, int chunkID, NativeArray<int> triangulations, NativeArray<Vector3Int> corners, NativeArray<Vector2Int> edges,
            NativeArray<int> verticeCounter, NativeArray<int> triangleCounter)
        {
            this.chunkPoints = chunkPoints;
            this.chunkSizeX = chunkSizeX;
            this.chunkSizeY = chunkSizeY;
            this.chunkSizeZ = chunkSizeZ;
            this.triangles = triangles;
            this.vertices = vertices;
            this.edgeLength = edgeLength;
            this.chunkAmountX = chunkAmountX;
            this.chunkAmountZ = chunkAmountZ;
            this.chunkAmountY = chunkAmountY;
            this.chunkID = chunkID;
            this.triangulations = triangulations;
            this.corners = corners;
            this.edges = edges;
            this.verticeCounter = verticeCounter;
            this.triangleCounter = triangleCounter;
        }*/

        public Vector3 GetEdgeVector(Vector3 pointPoisition, Vector2Int edgeIndex) //calculating the Vector3 in the middle of the edge of 2 given cube corners
        {
            Vector3 edge;
            Vector3 cornerA = new Vector3(corners[edgeIndex.x].x * edgeLength, corners[edgeIndex.x].y * edgeLength, corners[edgeIndex.x].z * edgeLength);
            Vector3 cornerB = new Vector3(corners[edgeIndex.y].x * edgeLength, corners[edgeIndex.y].y * edgeLength, corners[edgeIndex.y].z * edgeLength);
            edge = ((pointPoisition + cornerA) + (pointPoisition + cornerB)) / 2;
            return edge;
        }

        public void Execute(int i)
        {
            Point point = chunkPoints[i];
            Vector3Int pointIndex = PositionToIndex(point.pointPosition);
            if (pointIndex.x < chunkSizeX - 1 && pointIndex.y < chunkSizeY - 1 && pointIndex.z < chunkSizeZ - 1) //check if corners wouldnt go out of chunk bounds
            {
                NativeArray<int> triangulation = GetTriangulation(point);
                for(int j = 0; j < 15; j++)
                {
                    if(triangulation[j] != -1) //if triangulation value is not -1 then add the edge to vertices
                    {
                        vertices[triangleCounter[0] + j] = GetEdgeVector(point.pointPosition, edges[triangulation[j]]);
                        triangles[triangleCounter[0] + j] = triangleCounter[0] + j;
                    }
                    else //adding predefined values to arrays to "trim" them after the job is done
                    {
                        triangles[triangleCounter[0] + j] = -1;
                        verticeCounter[0]++;
                    }
                }
                triangleCounter[0] += 15;
                triangulation.Dispose();
            }
        }
    }


    private void Start()
    {
        nativeTriangulations = new NativeArray<int>(256 * 16, Allocator.Persistent);
        nativeCorners = new NativeArray<Vector3Int>(corners.Length, Allocator.Persistent);
        nativeEdges = new NativeArray<Vector2Int>(edges.Length, Allocator.Persistent);

        NativeArray<Vector3Int>.Copy(corners, nativeCorners);
        NativeArray<Vector2Int>.Copy(edges, nativeEdges);

        int[] flattenedTriangulations = new int[256 * 16];
        for(int x = 0; x < 256; x++) //loop to flatten the 2d triangulation array
        {
            for(int y = 0; y < 15; y++)
            {
                flattenedTriangulations[x * 15 + y] = triangulations[x, y];
            }
        }
        NativeArray<int>.Copy(flattenedTriangulations, nativeTriangulations);
        NativeArray<int> triangCount = new NativeArray<int>(1, Allocator.Persistent);
        NativeArray<int> verticeCount = new NativeArray<int>(1, Allocator.Persistent);
        for (int i = 0; i < cubeGrid.ChunkAmount; i++)
        {
            triangles = new NativeArray<int>(15 * cubeGrid.ChunkSizeX * cubeGrid.ChunkSizeY * cubeGrid.ChunkSizeZ, Allocator.Persistent);
            vertices = new NativeArray<Vector3>(15 * cubeGrid.ChunkSizeX * cubeGrid.ChunkSizeY * cubeGrid.ChunkSizeZ, Allocator.Persistent);
            NativeArray<Point> chunkPoints = new NativeArray<Point>(cubeGrid.ChunkSizeX * cubeGrid.ChunkSizeY * cubeGrid.ChunkSizeZ, Allocator.Persistent);
            NativeArray<Point>.Copy(cubeGrid.GetChunk[i].chunkPoints, chunkPoints);
            /* MarchChunks march = new MarchChunks(chunkPoints, cubeGrid.ChunkSizeX, cubeGrid.ChunkSizeY, cubeGrid.ChunkSizeZ, triangles, vertices, cubeGrid.edgeLength,
                 cubeGrid.ChunkAmountX, cubeGrid.ChunkAmountY, cubeGrid.ChunkAmountZ, i, nativeTriangulations, nativeCorners, nativeEdges, verticeCount, triangCount);*/

            triangCount[0] = 0;
            verticeCount[0] = 0;

            var march = new MarchChunks()
            {
                chunkPoints = chunkPoints,
                chunkSizeX = cubeGrid.ChunkSizeX,
                chunkSizeY = cubeGrid.ChunkSizeY,
                chunkSizeZ = cubeGrid.ChunkSizeZ,
                triangles = triangles,
                vertices = vertices,
                edgeLength = cubeGrid.edgeLength,
                chunkAmountX = cubeGrid.ChunkAmountX,
                chunkAmountY = cubeGrid.ChunkAmountY,
                chunkAmountZ = cubeGrid.ChunkAmountZ,
                chunkID = i,
                triangulations = nativeTriangulations,
                corners = nativeCorners,
                edges = nativeEdges,
                verticeCounter = verticeCount,
                triangleCounter = triangCount
            };

            job = march.Schedule((cubeGrid.ChunkSizeX * cubeGrid.ChunkSizeY * cubeGrid.ChunkSizeZ) - 2, 6400);


            job.Complete();
            chunkPoints.Dispose();

            List<Vector3> verticesList = new List<Vector3>();
            List<int> triangleList = new List<int>();
            for (int j = 0; j < 15 * cubeGrid.ChunkSizeX * cubeGrid.ChunkSizeY * cubeGrid.ChunkSizeZ; j++)
            {
                if (triangles[j] != -1)
                {
                    verticesList.Add(vertices[j]);
                }
            }

            if (verticesList.Count > 0)
            {

                for (int j = 0; j < verticesList.Count; j++)
                    triangleList.Add(j);

                verticesList.Reverse();
                Mesh mesh = new()
                {
                    vertices = verticesList.ToArray(),
                    triangles = triangleList.ToArray()
                };
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //incrase the triangle limit from uint16 to uint32

                /*mesh.triangles = triangleList.ToArray();
                mesh.vertices = verticesList.ToArray();*/
                mesh.RecalculateNormals();


                meshObject = new GameObject($"Chunk{i}");
                meshObject.AddComponent<MeshFilter>();
                meshObject.AddComponent<MeshRenderer>();
                meshObject.GetComponent<MeshFilter>().mesh = mesh;

                meshObject.transform.parent = chunkParent.transform;
                meshObject = null;
            }
            vertices.Dispose();
            triangles.Dispose();
            triangleList.Clear();
            verticesList.Clear();

        }

    }
    private void Update()
    {
    }
}

