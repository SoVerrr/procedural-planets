using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Marching : MonoBehaviour
{
    

    private static int CubeConfig(float[] cube) // Get the index for the triangulation table
    {
        int config = 0;

        for(int i = 0; i < 8; i++)
        {
            if(cube[i] < Values.Instance.SurfaceLevel)
            {
                config |= 1 << i;
            }
        }

        return config;
    }

    private static Vector3 CalculateVertexPosition(Vector3 vertexA, Vector3 vertexB, ref float[,,] heightMap)
    {
        //Get values of the vertices
        float valueA = heightMap[(int)vertexA.x, (int)vertexA.y, (int)vertexA.z];
        float valueB = heightMap[(int)vertexB.x, (int)vertexB.y, (int)vertexB.z];

        float t = (0 - valueA) / (valueB - valueA);
        Vector3 vertexPosition = Vector3.Lerp(vertexA, vertexB, t);

        return vertexPosition;
    }

    private static void MarchCube(float[] cube, Vector3 position, ref List<Vector3> vertices, ref List<int> triangles, ref float[,,] heightMap)
    {
        int config = CubeConfig(cube);

        for (int i = 0; i < 15; i++) // Iterate through the triangulations from config
        {
            int edgeIndex = Values.Instance.Triangulations[config, i]; // Get the index of the edge from triangulations table
            if (edgeIndex == -1) //Break the loop if edge index equals to -1 because it means that there will be no more indices in this triangulation
                break;
            
            
            Vector3 vertexB = position + Values.Instance.Corners[Values.Instance.Edges[edgeIndex].y];
            Vector3 vertexA = position + Values.Instance.Corners[Values.Instance.Edges[edgeIndex].x];

            Vector3 edge = CalculateVertexPosition(vertexA, vertexB, ref heightMap); //TODO: interpolate the value to get smooth edges

            if (vertices.Contains(edge)) // If vertice already exists only add the index of it to triangles to achieve less vertices and smoother edges
            {
                int index = vertices.IndexOf(edge);
                triangles.Add(index);
            }
            else // Otherwise add vertice to the list and its index to the triangles list
            {
                vertices.Add(edge);
                triangles.Add(vertices.Count - 1);
            }
        }

    }

    public static void MarchingCubes(ref List<Vector3> vertices, ref List<int> triangles, ref float[,,] heightMap)
    {
        //Iterate thorugh the heightmap
        for(int x = 0; x < Values.Instance.PlanetSize.x - 1; x++)
        {
            for(int y = 0; y < Values.Instance.PlanetSize.y - 1; y++)
            {
                for (int z = 0; z < Values.Instance.PlanetSize.z - 1; z++)
                {
                    
                    #region cubeCorners
                    //Set the values of cube corners by taking the heightMap value at the sum of original position and the corner values
                    float[] cube = new float[8];
                    cube[0] = heightMap[(int)Values.Instance.Corners[0].x + x, (int)Values.Instance.Corners[0].y + y, (int)Values.Instance.Corners[0].z + z];
                    cube[1] = heightMap[(int)Values.Instance.Corners[1].x + x, (int)Values.Instance.Corners[1].y + y, (int)Values.Instance.Corners[1].z + z];
                    cube[2] = heightMap[(int)Values.Instance.Corners[2].x + x, (int)Values.Instance.Corners[2].y + y, (int)Values.Instance.Corners[2].z + z];
                    cube[3] = heightMap[(int)Values.Instance.Corners[3].x + x, (int)Values.Instance.Corners[3].y + y, (int)Values.Instance.Corners[3].z + z];
                    cube[4] = heightMap[(int)Values.Instance.Corners[4].x + x, (int)Values.Instance.Corners[4].y + y, (int)Values.Instance.Corners[4].z + z];
                    cube[5] = heightMap[(int)Values.Instance.Corners[5].x + x, (int)Values.Instance.Corners[5].y + y, (int)Values.Instance.Corners[5].z + z];
                    cube[6] = heightMap[(int)Values.Instance.Corners[6].x + x, (int)Values.Instance.Corners[6].y + y, (int)Values.Instance.Corners[6].z + z];
                    cube[7] = heightMap[(int)Values.Instance.Corners[7].x + x, (int)Values.Instance.Corners[7].y + y, (int)Values.Instance.Corners[7].z + z];
                    #endregion
                    MarchCube(cube, new Vector3(x, y, z), ref vertices, ref triangles, ref heightMap);
                }
            }
        }
    }
}
