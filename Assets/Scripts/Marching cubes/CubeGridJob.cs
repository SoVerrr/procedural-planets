using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class CubeGridJob : MonoBehaviour
{
    [SerializeField] private int gridSizeX;
    [SerializeField] private int gridSizeY;
    [SerializeField] private int gridSizeZ;
    [SerializeField] private float edgeLength;
    [SerializeField] private bool drawGrid;
    [SerializeField] private bool populateGrid;

    [SerializeField] private float debugCubeSize;
    public static Vector3[,,] gridPoints;

    private JobHandle job;
    public struct CubeGridJobs : IJobParallelFor
    {
        private float edgeLen;
        private int gridSizeX, gridSizeY, gridSizeZ;

        

        public CubeGridJobs(int gridX, int gridY, int gridZ, float edge)
        {
            gridSizeX = gridX;
            gridSizeY = gridY;
            gridSizeZ = gridZ;
            edgeLen = edge;
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
            CubeGridJob.gridPoints[indexes.x, indexes.y, indexes.z] = new Vector3(indexes.x * edgeLen, indexes.y * edgeLen, indexes.z * edgeLen);

        }
    }
    private void Start()
    {
        gridPoints = new Vector3[gridSizeX, gridSizeY, gridSizeZ];
        CubeGridJobs cubejob = new CubeGridJobs(gridSizeX, gridSizeY, gridSizeZ, edgeLength);
        job = cubejob.Schedule(gridSizeY * gridSizeX * gridSizeZ, 6400);
        job.Complete();
    }

    private void OnDrawGizmos()
    {
        if (drawGrid && gridPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Vector3 point in gridPoints)
            {
                Gizmos.DrawCube(point, new Vector3(debugCubeSize, debugCubeSize, debugCubeSize));
            }
        }
    }
    private void OnDestroy()
    {
        gridPoints = null;
    }
}
