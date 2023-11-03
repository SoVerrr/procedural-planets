using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTexture : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RenderTexture texture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.RFloat)
        {
            enableRandomWrite = true
        };
        texture.Create();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
