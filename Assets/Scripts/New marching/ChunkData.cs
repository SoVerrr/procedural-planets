using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData : MonoBehaviour
{
    public Vector3Int chunkID;



    public void EditChunk()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Vector3Int chunkSize = Values.Instance.ChunkSize;
        Marching.MarchingCubes(ref vertices, ref triangles, ref PlanetMap.planetMap, chunkSize.x, chunkSize.y, chunkSize.z, chunkID);



        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();

        triangles.Reverse();
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            triangles = triangles.ToArray()
        };
        mesh.RecalculateNormals();
        gameObject.GetComponent<MeshCollider>().sharedMesh= mesh;

        filter.mesh = mesh;
        Debug.Log("Chunk reloaded");
    }

}
