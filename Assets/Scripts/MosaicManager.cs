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
        color1Buffer = new ComputeBuffer(BUFFER_LENGTH, sizeof(int), ComputeBufferType.Default);
        color2Buffer = new ComputeBuffer(BUFFER_LENGTH, sizeof(int), ComputeBufferType.Default);
        diffBuffer = new ComputeBuffer(BUFFER_LENGTH, sizeof(int), ComputeBufferType.Default);

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
        int[] mainChunkInts = GetIntsForColors(mainChunk.colors);

        color1Buffer.SetData(mainChunkInts);
        computeShader.SetBuffer(kernelHandle, "color1Buffer", color1Buffer);

        for (int i = 0; i < chunksToPullFrom.Length; i++)
        {
            TextureChunkData candidateChunk = chunksToPullFrom[i];

            int[] candidateInts = GetIntsForColors(candidateChunk.colors);
            color2Buffer.SetData(candidateInts);

            diffBuffer.SetData(new float[mainChunk.colors.Length]);

            computeShader.SetBuffer(kernelHandle, "color2Buffer", color2Buffer);
            computeShader.SetBuffer(kernelHandle, "sumBuffer", diffBuffer);

            computeShader.Dispatch(0, BUFFER_LENGTH / 8, BUFFER_LENGTH / 8, 1);
            float[] resultContainer = new float[mainChunk.colors.Length];
            diffBuffer.GetData(resultContainer);

            float difference = 0;
            for (int j = 0; j < resultContainer.Length; j++)
            {
                difference += resultContainer[i];
            }

            if (difference < lowestDifference)
            {
                lowestDifference = difference;
                bestChunkIndex = i;
            }
        }
        return chunksToPullFrom[bestChunkIndex];
    }

    private static int[] GetIntsForColors(Color32[] colors)
    {
        int[] ints = new int[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            Color32 c32 = colors[i];
            int myInt = 0;
            myInt += (int)c32.r;
            myInt += (int)(c32.g << 8);
            myInt += (int)(c32.b << 16);
            ints[i] = myInt;
        }
        return ints;
    }
}
