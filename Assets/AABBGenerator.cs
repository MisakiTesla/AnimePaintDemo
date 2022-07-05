using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AABBGenerator : MonoBehaviour
{
    public Texture2D inputTexture;
    public RenderTexture outputRT;
    public RenderTexture aabbRT;
    public ComputeShader shader;

    public RawImage outputRawImage;
    
    // Start is called before the first frame update
    void Start()
    {
        outputRT = new RenderTexture(inputTexture.width, inputTexture.height, 24);
        outputRT.enableRandomWrite = true;
        outputRawImage.texture = outputRT;

        
        var kernelId = shader.FindKernel("CSMain");
        shader.SetTexture(kernelId, "inputTexture", inputTexture);
        shader.SetTexture(kernelId, "Result", outputRT);

        ComputeBuffer buffer = new ComputeBuffer(4, sizeof(uint));
        buffer.SetData(new List<uint>()
        {
            2048,2048,0,0
        });
        shader.SetBuffer(kernelId, "AABBs", buffer);
        
        shader.Dispatch(kernelId, inputTexture.width, inputTexture.height, 1);
        
        // 计数器
        // uint[] countBufferData = new uint[1] { 0 };
        // var countBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.IndirectArguments);
        // ComputeBuffer.CopyCount(buffer, countBuffer, 0);
        // countBuffer.GetData(countBufferData);
        // //buffer中的元素数量即为：countBufferData[0]
        
        aabbRT = new RenderTexture(1, 1, 24);
        aabbRT.enableRandomWrite = true;
        // outputRawImage.texture = outputRT;

        var copyKernelId = shader.FindKernel("CSCopyAABBsToRWTex");
        shader.SetTexture(copyKernelId, "AABBsTex", aabbRT);
        shader.SetBuffer(copyKernelId, "AABBs", buffer);
        //copy RWStructuredBuffer<uint> AABBs To AABBsTex so we can read back later
        shader.Dispatch(copyKernelId, 1, 1, 1);
        
        //read back
        var aabbTex = RenderTexture2Texture2D(aabbRT);
        var aabb = aabbTex.GetPixel(0, 0);
        Debug.Log($"AABB is min = ({aabb.r},{aabb.g}), max = {aabb.b},{aabb.a}");
        buffer.Release();
    }

    Texture2D RenderTexture2Texture2D(RenderTexture renderTexture)
    {
        int width = renderTexture.width;
        int height = renderTexture.height;
        Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();
        return texture2D;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
