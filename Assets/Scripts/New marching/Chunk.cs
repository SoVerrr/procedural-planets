using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    private Vector3Int chunkSize;
    public ChunkData cData;

    public Chunk(float[,,] heightMap, Vector3Int chunkID, GameObject parent)
    {
        chunkSize = Values.Instance.ChunkSize;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Marching.MarchingCubes(ref vertices, ref triangles, ref heightMap, chunkSize.x, chunkSize.y, chunkSize.z, chunkID);
        triangles.Reverse();
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            triangles = triangles.ToArray()
        };
        mesh.RecalculateNormals();

        GameObject meshObject = new GameObject($"Chunk {chunkID}");
        meshObject.AddComponent<MeshFilter>();
        meshObject.AddComponent<MeshRenderer>();
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Planet");
        meshObject.transform.parent = parent.transform;
        meshObject.AddComponent<MeshCollider>();

        ChunkData data = meshObject.AddComponent<ChunkData>();
        data.chunkID = chunkID;
        cData = data;

    }
}
