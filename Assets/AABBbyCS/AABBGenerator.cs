using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public struct AABBData
{
    public AABBData(uint minX, uint minY, uint maxX, uint maxY)
    {
        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;
    }
    public uint minX;
    public uint minY;
    public uint maxX;
    public uint maxY;
}

public class AABBGenerator : MonoBehaviour
{
    public Texture2D inputTexture;
    public RenderTexture outputRT;
    [Header("AABB计算结果")]
    public RenderTexture aabbRT;
    public ComputeShader shader;
    [Header("最大区域数")]
    public int maxRegionCount = 32;

    [Header("预览")]
    public RawImage outputRawImage;

    // Start is called before the first frame update
    void Start()
    {
        outputRT = new RenderTexture(inputTexture.width, inputTexture.height, 24);
        outputRT.enableRandomWrite = true;
        outputRawImage.texture = outputRT;

        var kernelId = shader.FindKernel("CSMain");
        shader.SetTexture(kernelId, "inputTexture", inputTexture);
        shader.SetInt( "inputTextureSizeX", inputTexture.width);
        shader.SetInt( "inputTextureSizeY", inputTexture.width);
        shader.SetTexture(kernelId, "Result", outputRT);

        //最大区域数
        var AABBMaxCount = maxRegionCount;
        ComputeBuffer buffer = new ComputeBuffer(AABBMaxCount, 4*4);

        var defaultAABB = new AABBData((uint)inputTexture.width, (uint)inputTexture.width, 0, 0);
        var aabbDatas = new AABBData[AABBMaxCount];
        for (int i = 0; i < aabbDatas.Length; i++)
        {
            aabbDatas[i] = new AABBData((uint)inputTexture.width, (uint)inputTexture.height, 0, 0);
        }
        buffer.SetData(aabbDatas);
        shader.SetBuffer(kernelId, "AABBs", buffer);
        
        shader.Dispatch(kernelId, inputTexture.width, inputTexture.height, 1);
        
        // 计数器
        // uint[] countBufferData = new uint[1] { 0 };
        // var countBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.IndirectArguments);
        // ComputeBuffer.CopyCount(buffer, countBuffer, 0);
        // countBuffer.GetData(countBufferData);
        // //buffer中的元素数量即为：countBufferData[0]
        
        aabbRT = new RenderTexture(AABBMaxCount, 1, 24);
        aabbRT.enableRandomWrite = true;
        // outputRawImage.texture = outputRT;

        var copyKernelId = shader.FindKernel("CSCopyAABBsToRWTex");
        shader.SetTexture(copyKernelId, "AABBsTex", aabbRT);
        shader.SetBuffer(copyKernelId, "AABBs", buffer);
        //copy RWStructuredBuffer<uint> AABBs To AABBsTex so we can read back later
        shader.Dispatch(copyKernelId, AABBMaxCount, 1, 1);
        
        //read back
        var aabbTex = RenderTexture2Texture2D(aabbRT);
        foreach (var aabb in aabbTex.GetPixels(0))
        {
            Debug.Log($"AABB is min = ({aabb.r},{aabb.g}), max = {aabb.b},{aabb.a}");

        }

        buffer.Release();
        aabbTex.filterMode = FilterMode.Point;
        outputRawImage.texture = aabbTex;
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
