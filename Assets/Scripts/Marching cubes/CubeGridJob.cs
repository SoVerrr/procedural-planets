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
    [SerializeField] private float edgeLength;
    [SerializeField] private bool drawGrid;
    [SerializeField] private bool populateGrid;

    [SerializeField] private float debugCubeSize;
    public NativeArray<Point> gridPoints;
    private JobHandle job;
    [BurstCompile]
    public struct CubeGridJobs : IJobParallelFor
    {
        [ReadOnly]private float edgeLen;
        private int gridSizeX, gridSizeY, gridSizeZ;
        NativeArray<Point> gridPoints1;
        

        public CubeGridJobs(int gridX, int gridY, int gridZ, float edge, NativeArray<Point> arr)
        {
            gridSizeX = gridX;
            gridSizeY = gridY;
            gridSizeZ = gridZ;
            edgeLen = edge;
            gridPoints1 = arr;
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
            gridPoints1[i] = new Point(new Vector3(indexes.x * edgeLen, indexes.y * edgeLen, indexes.z * edgeLen), i < 2);

        }
    }
    private void Awake()
    {
        gridPoints = new NativeArray<Point>(gridSizeX * gridSizeY * gridSizeZ, Allocator.Persistent);

    }
    private void Start()
    {
        CubeGridJobs cubejob = new CubeGridJobs(gridSizeX, gridSizeY, gridSizeZ, edgeLength, gridPoints);
        job = cubejob.Schedule(gridSizeY * gridSizeX * gridSizeZ, 6400);
        
        job.Complete();
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
    private void OnDrawGizmos()
    {
        if (drawGrid && gridPoints != null)
        {
            foreach (Point point in gridPoints)
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
    private void OnDestroy()
    {
        gridPoints.Dispose();
    }
}
