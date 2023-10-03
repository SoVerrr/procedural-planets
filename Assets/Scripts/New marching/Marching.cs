using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

struct Marching : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> heightMap;
    [ReadOnly] public NativeArray<int> triangulations;
    [ReadOnly] public NativeArray<int2> edges;
    [ReadOnly] public NativeArray<int3> corners;
    [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
    [NativeDisableParallelForRestriction] public NativeArray<int> triangles;
    [NativeDisableParallelForRestriction] public NativeArray<float> cube;
    [NativeDisableParallelForRestriction] public NativeArray<int> triangCounter;
    private int GetCubeConfig(NativeArray<float> cube) //Get configuration of the cube for the triangulation table
    {
        int configIdx = 0;

        for(int i = 0; i < 8; i++) //iterate through corners of the cube
        {
            if(cube[i] < Values.Instance.SurfaceLevel)
            {
                configIdx |= 1 << i; //OR gate inserts 1 at the index of a point higher than surface level by bitshifting
            }
        }

        return configIdx;

    }

    private int3 UnflattenIndex(int i) //Convert 1d index to a 3d index
    {
        int3 idx = new int3();

        idx.z = i % Values.Instance.PlanetSize.z;
        idx.y = (i / Values.Instance.PlanetSize.z) % Values.Instance.PlanetSize.y;
        idx.x = (i / (Values.Instance.PlanetSize.y * Values.Instance.PlanetSize.z)) % Values.Instance.PlanetSize.x;

        return idx;
    }
    private int FlattenIndex(int3 position) //Convert 3d index to 1d index
    {
        if ((position.z * Values.Instance.PlanetSize.x * Values.Instance.PlanetSize.y) + (position.y * Values.Instance.PlanetSize.x) + position.x > 27000)
            Debug.Log($"{(position.z * Values.Instance.PlanetSize.x * Values.Instance.PlanetSize.y) + (position.y * Values.Instance.PlanetSize.x) + position.x} || {position}");
        return (position.z * Values.Instance.PlanetSize.x * Values.Instance.PlanetSize.y) + (position.y * Values.Instance.PlanetSize.x) + position.x;
    }


    public void Execute(int i)
    {
        

        int3 position = UnflattenIndex(i); //Assuming density of 1 for now
        if (position.x < Values.Instance.PlanetSize.x - 1 && position.y < Values.Instance.PlanetSize.y - 1 && position.z < Values.Instance.PlanetSize.y - 1)
        {


            #region corners
            //Setting the values of the corners of the cube
            cube[0] = heightMap[FlattenIndex(new int3(position.x + corners[0].x, position.y + corners[0].y, position.z + corners[0].z))];
            cube[1] = heightMap[FlattenIndex(new int3(position.x + corners[1].x, position.y + corners[1].y, position.z + corners[1].z))];
            cube[2] = heightMap[FlattenIndex(new int3(position.x + corners[2].x, position.y + corners[2].y, position.z + corners[2].z))];
            cube[3] = heightMap[FlattenIndex(new int3(position.x + corners[3].x, position.y + corners[3].y, position.z + corners[3].z))];
            cube[4] = heightMap[FlattenIndex(new int3(position.x + corners[4].x, position.y + corners[4].y, position.z + corners[4].z))];
            cube[5] = heightMap[FlattenIndex(new int3(position.x + corners[5].x, position.y + corners[5].y, position.z + corners[5].z))];
            cube[6] = heightMap[FlattenIndex(new int3(position.x + corners[6].x, position.y + corners[6].y, position.z + corners[6].z))];
            cube[7] = heightMap[FlattenIndex(new int3(position.x + corners[7].x, position.y + corners[7].y, position.z + corners[7].z))];
            #endregion
            int cubeConfig = GetCubeConfig(cube);
            if (cubeConfig == 0 || cubeConfig == 255) //Returns if cube config is 0 or 255 because the edge cases are just an empty or full cube so it wont have any triangles anyway
                return;

            for (int t = 0; t < 15; t++)
            {
                if (triangulations[cubeConfig + t] == -1) //If an indice index is equal to -1 it means that all the triangles for the case have been added
                    break;
                int edgeIndex = triangulations[cubeConfig + t];

                float3 vectorA = new float3(position.x + corners[edges[edgeIndex].x].x, position.y + corners[edges[edgeIndex].x].y, position.z + corners[edges[edgeIndex].x].z);
                float3 vectorB = new float3(position.x + corners[edges[edgeIndex].y].x, position.y + corners[edges[edgeIndex].y].y, position.z + corners[edges[edgeIndex].y].z);

                float3 edge = (vectorA + vectorB) / 2; //TODO: interpolate the corners to get smooth edges

                //Iterate through vertices to check if the vertice was already added and if it wasnt add it at the end
                int verticeIndex = -1; //If the vetice was already added save its index to add it to triangles

                for (int p = 0; p < triangles.Length; p++)
                {
                    if (vertices[p].Equals(edge)) //If vertice already exists set verticeIndex to its index
                    {
                        verticeIndex = p;
                    }
                    else if (vertices[p].Equals(null)) //If it reaches end of vertices without detecting a duplicate add a new one and add it to triangles
                    {
                        vertices[p] = edge;
                        triangles[triangCounter[0]] = p;
                        triangCounter[0]++;
                        break;
                    }

                    if (p == triangCounter[0]) //If it reaches end of triangles add a duplicate vertex and its index to triangles
                    {
                        triangles[triangCounter[0]] = verticeIndex;
                        triangCounter[0]++;
                    }
                }
            }
        }
    }
}