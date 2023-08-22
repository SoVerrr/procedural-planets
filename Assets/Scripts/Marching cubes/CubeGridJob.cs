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
    private NativeArray<Vector3> gridPoints;
    private JobHandle job;
    [BurstCompile]
    public struct CubeGridJobs : IJobParallelFor
    {
        [ReadOnly]private float edgeLen;
        private int gridSizeX, gridSizeY, gridSizeZ;
        NativeArray<Vector3> gridPoints1;
        

        public CubeGridJobs(int gridX, int gridY, int gridZ, float edge, NativeArray<Vector3> arr)
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
            gridPoints1[i] = new Vector3(indexes.x * edgeLen, indexes.y * edgeLen, indexes.z * edgeLen);

        }
    }
    private void Start()
    {
        gridPoints = new NativeArray<Vector3>(gridSizeX * gridSizeY * gridSizeZ, Allocator.Persistent);
        CubeGridJobs cubejob = new CubeGridJobs(gridSizeX, gridSizeY, gridSizeZ, edgeLength, gridPoints);
        job = cubejob.Schedule(gridSizeY * gridSizeX * gridSizeZ, 6400);
        
        job.Complete();
    }

    public Vector3 AccessPoint(float x, float y, float z)
    {
        int zIndex = (int)Mathf.Round(z / edgeLength);
        int xIndex = (int)Mathf.Round(x / edgeLength);
        int yIndex = (int)Mathf.Round(y / edgeLength);
        int i = (xIndex * gridSizeY * gridSizeZ + yIndex * gridSizeZ + zIndex);

        return gridPoints[i];
    }

    private void OnDrawGizmos()
    {
        if (drawGrid && gridPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 point in gridPoints)
            {
                Gizmos.DrawCube(AccessPoint(point.x, point.y, point.z), new Vector3(debugCubeSize, debugCubeSize, debugCubeSize));
            }
        }
    }
    private void OnDestroy()
    {
        gridPoints.Dispose();
    }
}
