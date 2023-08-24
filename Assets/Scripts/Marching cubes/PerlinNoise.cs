using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public static float Perlin3D(Vector3 pos)
    {
        pos = (Vector3.Normalize(pos) + Vector3.one) / 2;
        float AB = Mathf.PerlinNoise(pos.x, pos.y);
        float BC = Mathf.PerlinNoise(pos.y, pos.z);
        float AC = Mathf.PerlinNoise(pos.x, pos.z);

        float BA = Mathf.PerlinNoise(pos.x, pos.y);
        float CB = Mathf.PerlinNoise(pos.y, pos.z);
        float CA = Mathf.PerlinNoise(pos.x, pos.z);

        float ABC = AB + BC + AC + BA + CB + CA;
        return ABC / 6;
    }
}
