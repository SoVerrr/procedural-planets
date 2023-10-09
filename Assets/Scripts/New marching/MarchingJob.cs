using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

struct MarchingJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> heightMap;
    [ReadOnly] public NativeArray<int> triangulations;
    [ReadOnly] public NativeArray<int2> edges;
    [ReadOnly] public NativeArray<int3> corners;
    [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
    [NativeDisableParallelForRestriction] public NativeArray<int> triangles;
    [NativeDisableParallelForRestriction] public NativeArray<int> triangCounter;
    [NativeDisableParallelForRestriction] public NativeArray<int> vertCounter;
    private int GetCubeConfig(int3 position) //Get configuration of the cube for the triangulation table
    {
        int configIdx = 0;

        for(int i = 0; i < 8; i++) //iterate through corners of the cube
        {
            float corner = heightMap[FlattenIndex(new int3(position.x + corners[i].x, position.y + corners[i].y, position.z + corners[i].z))];
            if (corner < Values.Instance.SurfaceLevel)
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
        return (position.z * Values.Instance.PlanetSize.x * Values.Instance.PlanetSize.y) + (position.y * Values.Instance.PlanetSize.x) + position.x;
    }


    public void Execute(int i)
    {
        

        int3 position = UnflattenIndex(i); //Assuming density of 1 for now
        if (position.x < Values.Instance.PlanetSize.x - 1 && position.y < Values.Instance.PlanetSize.y - 1 && position.z < Values.Instance.PlanetSize.y - 1)
        {
            int cubeConfig = GetCubeConfig(position);
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

                vertices[triangCounter[0]] = new float3(1, 1, 1)/*edge*/;
                triangles[triangCounter[0]] = triangCounter[0];
                //vertCounter[0]++;
                triangCounter[0]++;
                if(!vertices[triangCounter[0]].Equals(new float3(0, 0, 0))){
                    Debug.Log($"i: {i} | position: {position} | edge: {edge} | vert: {vertices[triangCounter[0]]}");
                }
            }
        }
    }
}