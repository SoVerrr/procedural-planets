using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
struct Marching : IJobParallelFor
{

    [ReadOnly] NativeArray<float> heightMap;
    [WriteOnly] NativeArray<float3> vertices;
    [WriteOnly] NativeArray<int> triangles;
    private int GetCubeConfig(float[] cube) //Get configuration of the cube for the triangulation table
    {
        int configIdx = 0;

        for(int i = 0; i < 8; i++) //iterate through corners of the cube
        {
            if(cube[i] > Values.Instance.SurfaceLevel)
            {
                configIdx |= 1 << i; //OR gate inserts 1 at the index of a point higher than surface level by bitshifting
            }
        }

        return configIdx;

    }

    private float3 UnflattenIndex(int i) //Convert 1d index to a 3d index
    {
        float3 idx = new float3();

        idx.z = i % Values.Instance.PlanetSize.z;
        idx.y = (i / Values.Instance.PlanetSize.z) % Values.Instance.PlanetSize.y;
        idx.x = (i / (Values.Instance.PlanetSize.y * Values.Instance.PlanetSize.z)) % Values.Instance.PlanetSize.x;

        return idx;
    }

    public void Execute(int i)
    {
        float[] cube = new float[8];
        for(int j = 0; j < 8; j++)
        {

        }

        int cubeConfig = GetCubeConfig(cube);
    }
}