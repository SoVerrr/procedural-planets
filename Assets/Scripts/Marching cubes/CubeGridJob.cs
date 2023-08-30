using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;


public class CubeGridJob : MonoBehaviour
{
    [SerializeField] private int gridSizeX;
    [SerializeField] private int gridSizeY;
    [SerializeField] private int gridSizeZ;
    [SerializeField] private int chunkAmount;
    [SerializeField] public float edgeLength;
    [SerializeField] public bool drawGrid;
    [SerializeField] private bool populateGrid;
    [SerializeField] int radius;
    [SerializeField] public float debugCubeSize;
    [SerializeField] public float perlinRange;
    [SerializeField] public float perlinScale = 20.0f;
    [SerializeField] public bool createSphere;
    public NativeArray<Point> points;
    private NativeArray<Chunk> chunks;
    private JobHandle job;
    public Point[] gridPoints;
    private Chunk[] gridChunks;
    private float oldPerlin;
    private Vector3 middlePoint;
    [BurstCompile]
    public struct CubeGridJobs : IJobParallelFor
    {
        [ReadOnly] private float edgeLen;
        [ReadOnly] private float perlinScale;
        [ReadOnly] private float perlinRange;
        [ReadOnly] private float sphereRadius;
        [ReadOnly] private bool createSphere;
        [ReadOnly] Vector3 middlePoint;
        private int gridSizeX, gridSizeY, gridSizeZ;
        NativeArray<Point> gridPoints1;


        public CubeGridJobs(int gridX, int gridY, int gridZ, float edge, NativeArray<Point> arr, float scale, float range, float radius, bool create, Vector3 middlePoint)
        {
            gridSizeX = gridX;
            gridSizeY = gridY;
            gridSizeZ = gridZ;
            edgeLen = edge;
            perlinScale = scale;
            perlinRange = range;
            sphereRadius = radius;
            gridPoints1 = arr;
            createSphere = create;
            this.middlePoint = middlePoint;
        }

        private Vector3Int CalculateIndexes(int i)
        {
            Vector3Int result = Vector3Int.zero;
            result.z = i % gridSizeZ;
            result.y = (i / gridSizeZ) % gridSizeY;
            result.x = (i / (gridSizeY * gridSizeZ) )% gridSizeX;
            return result;
        }

        public void Execute(int i)
        {
            Vector3Int indexes = CalculateIndexes(i);
            Vector3 position = new Vector3(indexes.x * edgeLen, indexes.y * edgeLen, indexes.z * edgeLen);
            bool isInSphere = Vector3.Distance(middlePoint, position) < sphereRadius;
            if (createSphere)
                gridPoints1[i] = new Point(position, (PerlinNoise.Perlin3D(position, perlinScale) > perlinRange) && isInSphere);
            else
                gridPoints1[i] = new Point(position, (PerlinNoise.Perlin3D(position, perlinScale) > perlinRange));
        }
    }

    public struct GenerateChunk : IJob
    {
        [ReadOnly] int chunkSizeX;
        [ReadOnly] int chunkSizeY;
        [ReadOnly] int chunkSizeZ;
        [ReadOnly] float edgeLength;
        [ReadOnly] int chunkID;
        NativeArray<Point> chunkPoints;

        public GenerateChunk(int chunkSizeX, int chunkSizeY, int chunkSizeZ, float edgeLength, int chunkID, NativeArray<Point> chunkPoints)
        {
            this.chunkSizeX = chunkSizeX;
            this.chunkSizeY = chunkSizeY;
            this.chunkSizeZ = chunkSizeZ;
            this.edgeLength = edgeLength;
            this.chunkID = chunkID;
            this.chunkPoints = chunkPoints;
        }

        private Vector3Int CalculateIndexes(int i)
        {
            Vector3Int result = Vector3Int.zero;
            result.z = i % chunkSizeZ;
            result.y = (i / chunkSizeZ) % chunkSizeY;
            result.x = (i / (chunkSizeY * chunkSizeZ)) % chunkSizeX;
            return result;
        }

        public void Execute()
        {

            for (int i = 0; i < chunkSizeX * chunkSizeY * chunkSizeZ; i++)
            {
                Vector3Int indexes = CalculateIndexes(i);
                Vector3 position = new Vector3(indexes.x * edgeLength * chunkID, indexes.y * edgeLength * chunkID, indexes.z * edgeLength * chunkID);
                chunkPoints[i] = new Point(position);
            }


        }
    }





    private void Awake()
    {
        points = new NativeArray<Point>(gridSizeX / chunkAmount * gridSizeY / chunkAmount * gridSizeZ / chunkAmount, Allocator.Persistent);
        //points = new NativeArray<Point>(gridSizeX * gridSizeY * gridSizeZ, Allocator.Persistent);
        gridPoints = new Point[gridSizeX * gridSizeY * gridSizeZ];
        middlePoint = new Vector3(gridSizeX * edgeLength /2, gridSizeY * edgeLength / 2, gridSizeZ * edgeLength / 2);
        gridChunks = new Chunk[chunkAmount];
    }
    private void Start()
    {

        /*CubeGridJobs cubejob = new CubeGridJobs(gridSizeX, gridSizeY, gridSizeZ, edgeLength, points, perlinScale, perlinRange, radius, createSphere, middlePoint);
        job = cubejob.Schedule(gridSizeY * gridSizeX * gridSizeZ, 6400);

        job.Complete();
        points.CopyTo(gridPoints);

        oldPerlin = perlinRange;*/
        for (int i = 1; i <= chunkAmount; i++)
        {
            GenerateChunk chunkJob = new GenerateChunk(gridSizeX / chunkAmount, gridSizeY / chunkAmount, gridSizeZ / chunkAmount, edgeLength, i, points);
            job = chunkJob.Schedule();
            job.Complete();

            gridChunks[i - 1] = new Chunk(gridSizeX / chunkAmount, gridSizeY / chunkAmount, gridSizeZ / chunkAmount);
            points.CopyTo(gridChunks[i - 1].chunkPoints);

        }

    }



    public Vector3Int GetGridSizes()
    {
        return new Vector3Int(gridSizeX, gridSizeY, gridSizeZ);
    }
    public Point AccessPointCoordinates(float x, float y, float z)
    {
        int zIndex = (int)Mathf.Round(z / edgeLength);
        int xIndex = (int)Mathf.Round(x / edgeLength);
        int yIndex = (int)Mathf.Round(y / edgeLength);
        int i = (xIndex * gridSizeY * gridSizeZ + yIndex * gridSizeZ + zIndex);

        return gridPoints[i];
    }
    public Point AccessPointIndex(int x, int y, int z)
    {
        int i = (x * gridSizeY * gridSizeZ + y * gridSizeZ + z);

        return gridPoints[i];
    }
    public Vector3Int PositionToIndex(Vector3 position)
    {
        int zIndex = (int)Mathf.Round(position.z / edgeLength);
        int xIndex = (int)Mathf.Round(position.x / edgeLength);
        int yIndex = (int)Mathf.Round(position.y / edgeLength);
        return new Vector3Int(xIndex, yIndex, zIndex);
    }
    public void SetGridToSphere(int radius)
    {
        Vector3 middlePoint = AccessPointIndex(gridSizeX/2, gridSizeY/2, gridSizeZ/2).pointPosition;
        for(int i = 0; i < gridPoints.Length; i++)
        {
            if(Vector3.Distance(middlePoint, gridPoints[i].pointPosition) < radius)
            {
                float perlin = PerlinNoise.Perlin3D(gridPoints[i].pointPosition, perlinScale);
                Debug.Log(perlin);

                if (perlin > perlinRange)
                {
                    gridPoints[i].pointOn = true;
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (drawGrid && gridPoints != null)
        {
            /*foreach (Point point in gridPoints)
            {
                if (point.pointOn)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawCube(point.pointPosition, new Vector3(debugCubeSize, debugCubeSize, debugCubeSize));
                }
                else
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(point.pointPosition, new Vector3(debugCubeSize, debugCubeSize, debugCubeSize));
                }
            }*/
            foreach(Chunk chunk in gridChunks)
            {
                foreach (Point point in chunk.chunkPoints)
                {
                    if (point.pointOn)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawCube(point.pointPosition, new Vector3(debugCubeSize, debugCubeSize, debugCubeSize));
                    }
                    else
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawCube(point.pointPosition, new Vector3(debugCubeSize, debugCubeSize, debugCubeSize));
                    }
                }
            }
        }
    }
}
