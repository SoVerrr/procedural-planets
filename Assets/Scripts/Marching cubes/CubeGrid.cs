using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGrid : MonoBehaviour
{
    [SerializeField] private int gridSizeX;
    [SerializeField] private int gridSizeY;
    [SerializeField] private int gridSizeZ;
    [SerializeField] private float edgeLength;
    [SerializeField] private bool drawGrid;
    [SerializeField] private bool populateGrid;

    [SerializeField] private float debugCubeSize;
    private Vector3[,,] gridPoints;

    public void PopulateGrid(int gridSizeX, int gridSizeY, int gridSizeZ, float edgeLength)
    {
        gridPoints = new Vector3[gridSizeX, gridSizeY, gridSizeZ];
        Vector3 currentPosition = new Vector3(0, 0, 0);

        for(int x = 0; x < gridSizeX; x++)
        {
            for(int y = 0; y < gridSizeY; y++)
            {
                for(int z = 0; z < gridSizeZ; z++)
                {
                    gridPoints[x, y, z] = currentPosition;
                    currentPosition.z += edgeLength;
                }
                currentPosition.y += edgeLength;
                currentPosition.z = 0;
            }
            currentPosition.x += edgeLength;
            currentPosition.y = 0;
        }
    }
    public void DrawGrid()
    {
        foreach(Vector3 point in gridPoints)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = point;
            cube.transform.localScale = new Vector3(debugCubeSize, debugCubeSize, debugCubeSize);
        }
    }

    private void Start()
    {
    
        if (populateGrid)
        {
            PopulateGrid(gridSizeX, gridSizeY, gridSizeZ, edgeLength);
        }
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

}
