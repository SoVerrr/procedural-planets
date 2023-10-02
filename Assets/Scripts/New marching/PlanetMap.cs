using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
public class PlanetMap : MonoBehaviour
{

    float[,,] planetMap;

    private void Update()
    {
        GeneratePlanetMap(Values.Instance.PlanetSize, Values.Instance.Radius, Values.Instance.Density, ref planetMap);
    }

    void Start()
    {
        planetMap = new float[Values.Instance.PlanetSize.x, Values.Instance.PlanetSize.y, Values.Instance.PlanetSize.z];

        GeneratePlanetMap(Values.Instance.PlanetSize, Values.Instance.Radius, Values.Instance.Density, ref planetMap);
    }

    public static float Perlin3D(Vector3 pos, float scale) //Random Perlin3D noise function, will be changed later on
    {
        pos = (Vector3.Normalize(pos) + Vector3.one) / 2;
        pos.x *= scale;
        pos.y *= scale;
        pos.z *= scale;
        float AB = Mathf.PerlinNoise(pos.x, pos.y);
        float BC = Mathf.PerlinNoise(pos.y, pos.z);
        float AC = Mathf.PerlinNoise(pos.x, pos.z);

        float BA = Mathf.PerlinNoise(pos.x, pos.y);
        float CB = Mathf.PerlinNoise(pos.y, pos.z);
        float CA = Mathf.PerlinNoise(pos.x, pos.z);

        float ABC = AB + BC + AC + BA + CB + CA;
        return ABC / 6;
    }
    private void GeneratePlanetMap(Vector3Int planetSize, int radius, int density, ref float[,,] map)
    {
        JobHandle job;
        int arraySize = planetSize.x * planetSize.y * planetSize.z * (int)Mathf.Pow(density, 3); //The size of the array based on planet size and density of points
        NativeArray<float> pointVal = new NativeArray<float>(arraySize, Allocator.Persistent);

        ProcessPlanetPoints processing = new ProcessPlanetPoints //Assign values to the processing job
        {
            planetSize = new int3(planetSize.x, planetSize.y, planetSize.z),
            radius = radius,
            density = density,
            centrePoint = new float3(planetSize.x / 2, planetSize.y / 2, planetSize.z / 2),
            pointValues = pointVal
        };
        job = processing.Schedule(arraySize, 6400);

        job.Complete();
        
        for(int i = 0; i < arraySize; i++) //Copy native array to 3d planetMap array
        {
            int z = i % planetSize.z;
            int y = (i / planetSize.z) % planetSize.y;
            int x = (i / (planetSize.y * planetSize.z)) % planetSize.x;
            planetMap[x, y, z] = pointVal[i];
        }

        pointVal.Dispose(); //Dispose of the native array to avoid memory leaks

    }
    struct ProcessPlanetPoints : IJobParallelFor //generate a 3d array of floats for a sphere with applied noise
    {
        [ReadOnly] public int3 planetSize;
        [ReadOnly] public int radius;
        [ReadOnly] public int density;
        [ReadOnly] public float3 centrePoint;
        [WriteOnly] public NativeArray<float> pointValues;

        private float3 IndiceToPos(int i) //Convert 1d index to a 3d position by calculating 3d indices and then dividing them by density
        {
            float3 pos = new float3();

            pos.z = i % planetSize.z;
            pos.y = (i / planetSize.z) % planetSize.y;
            pos.x = (i / (planetSize.y * planetSize.z)) % planetSize.x;

            return pos / density;
        }

        public void Execute(int i)
        {
            float3 position = IndiceToPos(i); //Calculate points position in the world based on its index
            float distFromCentre = Vector3.Distance(centrePoint, position); //Calculate position's distance from centre
            pointValues[i] = (distFromCentre - radius) + PlanetMap.Perlin3D(position, 1); //Assign value to the point
        }
    }

    private void OnDrawGizmos()
    {
        if (planetMap == null)
            return;


        for(int x = 0; x < Values.Instance.PlanetSize.x; x++)
        {
            for(int y = 0; y < Values.Instance.PlanetSize.y; y++)
            {
                for(int z = 0; z < Values.Instance.PlanetSize.z; z++)
                {
                    if (planetMap[x, y, z] < Values.Instance.SurfaceLevel)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(new Vector3(x, y, z), new Vector3(0.1f, 0.1f, 0.1f));
                    }
                    else
                    {
                        
                    }
                }
            }
        }
    }
}


