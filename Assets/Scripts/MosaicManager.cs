using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MosaicManager : MonoBehaviour
{
    public Texture2D textureToRecreate;
    public TextureCollection textureCollection;
    public ComputeShader computeShader;

    private int kernelHandle;
    private ComputeBuffer color1Buffer;
    private ComputeBuffer color2Buffer;
    private ComputeBuffer diffBuffer;

    private const int BUFFER_LENGTH = TextureUtility.CHUNK_SIZE * TextureUtility.CHUNK_SIZE;

    public void Run()
    {
        kernelHandle = computeShader.FindKernel("CSMain");
        color1Buffer = new ComputeBuffer(BUFFER_LENGTH, sizeof(float) * 4, ComputeBufferType.Default);
        color2Buffer = new ComputeBuffer(BUFFER_LENGTH, sizeof(float) * 4, ComputeBufferType.Default);
        diffBuffer = new ComputeBuffer(BUFFER_LENGTH, sizeof(float), ComputeBufferType.Default);

        TextureChunkData[] chunksToPullFrom = textureCollection.GetTextureChunkData();

        TextureChunkData[] chunksToRecreate = TextureUtility.GetChunkDataForTexture(textureToRecreate);
        
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        TextureChunkData[] recreatedChunks = chunksToRecreate.Select((chunkToRecreate, index) =>
                      GetBestChunkForChunk(chunkToRecreate, chunksToPullFrom)).ToArray();

        stopwatch.Stop();
        Debug.Log(stopwatch.ElapsedMilliseconds);

        Texture2D finalTexture = TextureUtility.AssembleChunksIntoTexture(recreatedChunks, textureToRecreate.width);
        TextureUtility.WriteTexToPNG(finalTexture, "Result");
    }

    private TextureChunkData GetBestChunkForChunk(TextureChunkData mainChunk, TextureChunkData[] chunksToPullFrom)
    {
        int bestChunkIndex = -1;
        float lowestDifference = float.MaxValue;
        color1Buffer.SetData(mainChunk.colors);

        for (int i = 0; i < chunksToPullFrom.Length; i++)
        {
            TextureChunkData candidateChunk = chunksToPullFrom[i];
            
            color2Buffer.SetData(candidateChunk.colors);

            diffBuffer.SetData(new float[mainChunk.colors.Length]);

            computeShader.SetBuffer(kernelHandle, "color1Buffer", color1Buffer);
            computeShader.SetBuffer(kernelHandle, "color2Buffer", color2Buffer);
            computeShader.SetBuffer(kernelHandle, "sumBuffer", diffBuffer);

            computeShader.Dispatch(0, BUFFER_LENGTH / 8, BUFFER_LENGTH/8, 1);
            float[] resultContainer = new float[mainChunk.colors.Length];
            diffBuffer.GetData(resultContainer);

            float difference = 0;
            for (int j = 0; j < resultContainer.Length; j++)
            {
                difference += resultContainer[i];
            }
            
            if(difference < lowestDifference)
            {
                lowestDifference = difference;
                bestChunkIndex = i;
            }
        }
        return chunksToPullFrom[bestChunkIndex];
    }
}
