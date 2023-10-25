using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
public class PlanetMap : MonoBehaviour
{

    public static float[,,] planetMap;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    NativeArray<float> pointVal;
    [SerializeField] GameObject meshObject;
    List<Chunk> chunks = new List<Chunk>();
    /*private void Update()
    {
        GeneratePlanetMap(Values.Instance.PlanetSize, Values.Instance.Radius, Values.Instance.Density, ref planetMap);
    }*/

    void Start()
    {

        GameObject Chunks = new GameObject("Chunks");
        planetMap = new float[Values.Instance.PlanetSize.x, Values.Instance.PlanetSize.y, Values.Instance.PlanetSize.z];

        GeneratePlanetMap(Values.Instance.PlanetSize, Values.Instance.Radius, Values.Instance.Density, ref planetMap);

        for (int x = 0; x < Values.Instance.PlanetSize.x / Values.Instance.ChunkSize.x; x++)
        {
            for (int y = 0; y < Values.Instance.PlanetSize.y / Values.Instance.ChunkSize.y; y++)
            {
                for (int z = 0; z < Values.Instance.PlanetSize.z / Values.Instance.ChunkSize.z; z++)
                {
                    chunks.Add(new Chunk(planetMap, new Vector3Int(x, y, z), Chunks));
                }
            }
        }
        CreateOcean();
        /*Chunk chunk0 = new Chunk(planetMap, new Vector3Int(0,0,0));*/

    }

    private void CreateOcean() //Generate a blue sphere primitive with radius of the planet in the centre point
    {
        GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ocean.transform.localScale = new Vector3(Values.Instance.Radius, Values.Instance.Radius, Values.Instance.Radius) * 2;
        ocean.transform.position = new Vector3(Values.Instance.PlanetSize.x / 2, Values.Instance.PlanetSize.y / 2, Values.Instance.PlanetSize.z / 2);
        ocean.GetComponent<Renderer>().material.color = Color.blue;

    }

    private void MarchCubes()
    {
        Marching.MarchingCubes(ref vertices, ref triangles, ref planetMap, Values.Instance.PlanetSize.x, Values.Instance.PlanetSize.y, Values.Instance.PlanetSize.z, new Vector3Int(0,0,0));
        triangles.Reverse();
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            triangles = triangles.ToArray()
        };
        mesh.RecalculateNormals();

        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        
    }

    /* private void MarchCubesJob()
     {
         NativeArray<float3> verts = new NativeArray<float3>(Values.Instance.PlanetSize.x * Values.Instance.PlanetSize.y * Values.Instance.PlanetSize.z, Allocator.Persistent);
         NativeArray<int> triangs = new NativeArray<int>(Values.Instance.PlanetSize.x * Values.Instance.PlanetSize.y * Values.Instance.PlanetSize.z, Allocator.Persistent);
         NativeArray<int> triangCounter = new NativeArray<int>(1, Allocator.Persistent);
         NativeArray<int> vertCounter = new NativeArray<int>(1, Allocator.Persistent);
         NativeArray<int> nativeTriangulations = new NativeArray<int>(256 * 16, Allocator.Persistent);
         NativeArray<int3> nativeCorners = new NativeArray<int3>(Values.Instance.Corners.Length, Allocator.Persistent);
         NativeArray<int2> nativeEdges = new NativeArray<int2>(Values.Instance.Edges.Length, Allocator.Persistent);
         NativeArray<float> cube = new NativeArray<float>(8, Allocator.Persistent);
         NativeArray<int3>.Copy(Values.Instance.Corners, nativeCorners);
         NativeArray<int2>.Copy(Values.Instance.Edges, nativeEdges);
         int[] flattenedTriangulations = new int[256 * 16];
         for (int x = 0; x < 256; x++) //loop to flatten the 2d triangulation array
         {
             for (int y = 0; y < 15; y++)
             {
                 flattenedTriangulations[x * 15 + y] = Values.Instance.Triangulations[x, y];
             }
         }
         NativeArray<int>.Copy(flattenedTriangulations, nativeTriangulations);
         MarchingJob marchJob = new MarchingJob()
         {
             triangulations = nativeTriangulations,
             vertices = verts,
             triangles = triangs,
             edges = nativeEdges,
             corners = nativeCorners,
             heightMap = pointVal,
             triangCounter = triangCounter,
             vertCounter = vertCounter
         };

         JobHandle job = marchJob.Schedule(Values.Instance.PlanetSize.x * Values.Instance.PlanetSize.y * Values.Instance.PlanetSize.z, 64);
         job.Complete();
         Debug.Log(triangCounter[0]);
         for(int i = 0; i < triangCounter[0] - 1; i++)
         {
             vertices.Add(verts[i]);
             triangles.Add(triangs[i]);
         }

         Mesh mesh = new()
         {
             vertices = vertices.ToArray(),
             indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
             triangles = triangles.ToArray()
         };

         mesh.RecalculateNormals();

         GameObject meshObject = new GameObject($"Chunk");
         meshObject.AddComponent<MeshFilter>();
         meshObject.AddComponent<MeshRenderer>();
         meshObject.GetComponent<MeshFilter>().mesh = mesh;
     }*/

    public static float Perlin3D(Vector3 pos, float scale, float offsetX, float offsetY, float offsetZ)
    {
        pos = (Vector3.Normalize(pos) + Vector3.one) / 2;
        
        
        float AB = Mathf.PerlinNoise(pos.x, pos.y);
        float BC = Mathf.PerlinNoise(pos.y, pos.z);
        float AC = Mathf.PerlinNoise(pos.x, pos.z);

        float BA = Mathf.PerlinNoise(pos.x, pos.y);
        float CB = Mathf.PerlinNoise(pos.y, pos.z);
        float CA = Mathf.PerlinNoise(pos.x, pos.z);

        float ABC = AB + BC + AC + BA + CB + CA;
        return (ABC / 6);
    }
    //Frequency should be lower than 1 and lacunarity should be greater than 1
    public static float SamplePoint(Vector3 pos)
    {

        float noiseHeight = 0;
        float amplitude = Values.Instance.Amplitude;
        float frequency = 1;

        for(int i = 0; i < Values.Instance.Octaves; i++)
        {
            noiseHeight += noise.snoise(pos / Values.Instance.NoiseScale * frequency) * amplitude;
            amplitude *= Values.Instance.Persistance;
            frequency *= Values.Instance.Lacunarity;
        }
        return noiseHeight;
    }
    private void GeneratePlanetMap(Vector3Int planetSize, int radius, int density, ref float[,,] map)
    {
        JobHandle job;
        int arraySize = planetSize.x * planetSize.y * planetSize.z * (int)Mathf.Pow(density, 3); //The size of the array based on planet size and density of points
        pointVal = new NativeArray<float>(arraySize, Allocator.Persistent);
        
        ProcessPlanetPoints processing = new ProcessPlanetPoints //Assign values to the processing job
        {
            planetSize = new int3(planetSize.x, planetSize.y, planetSize.z),
            radius = radius,
            density = density,
            centrePoint = new float3(planetSize.x / 2, planetSize.y / 2, planetSize.z / 2),
            pointValues = pointVal,
            noiseScale = Values.Instance.NoiseScale
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
    [BurstCompile]
    struct ProcessPlanetPoints : IJobParallelFor //generate a 3d array of floats for a sphere with applied noise
    {
        [ReadOnly] public int3 planetSize;
        [ReadOnly] public int radius;
        [ReadOnly] public int density;
        [ReadOnly] public float3 centrePoint;
        [ReadOnly] public float noiseScale;
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
            float noise = SamplePoint(position);
            pointValues[i] = (distFromCentre - radius) + noise; //Assign value to the point
        }
        
    }
    
    private void OnDrawGizmos()
    {
        if (planetMap == null || Values.Instance.DrawPoints == false)
            return;


        for(int x = 0; x < Values.Instance.PlanetSize.x; x++)
        {
            for(int y = 0; y < Values.Instance.PlanetSize.y; y++)
            {
                for(int z = 0; z < Values.Instance.PlanetSize.z; z++)
                {
                    if(planetMap[x, y, z] < 1 && planetMap[x, y, z] >= 0)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(new Vector3(x, y, z), new Vector3(0.1f, 0.1f, 0.1f));
                    }
                    else if (planetMap[x, y, z] < Values.Instance.SurfaceLevel)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(new Vector3(x, y, z), new Vector3(0.1f, 0.1f, 0.1f));
                    }                   
                }
            }
        }
    }
}


