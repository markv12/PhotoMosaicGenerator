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
        int bigSize = BUFFER_LENGTH * chunksToPullFrom.Length;
        Color[] mainColorBigArray = new Color[bigSize];
        Color[] candidateColorBigArray = new Color[bigSize];
        for (int i = 0; i < chunksToPullFrom.Length; i++)
        {
            TextureChunkData candidateChunk = chunksToPullFrom[i];
            for (int j = 0; j < BUFFER_LENGTH; j++)
            {
                int index = (i * BUFFER_LENGTH) + j;
                mainColorBigArray[index] = mainChunk.colors[j];
                candidateColorBigArray[index] = candidateChunk.colors[j];
            }
        }
        color1Buffer = new ComputeBuffer(bigSize, sizeof(float) * 4, ComputeBufferType.Default);
        color2Buffer = new ComputeBuffer(bigSize, sizeof(float) * 4, ComputeBufferType.Default);
        diffBuffer = new ComputeBuffer(bigSize, sizeof(float), ComputeBufferType.Default);

        color1Buffer.SetData(mainColorBigArray);

        color2Buffer.SetData(candidateColorBigArray);

        diffBuffer.SetData(new float[bigSize]);

        computeShader.SetBuffer(kernelHandle, "color1Buffer", color1Buffer);
        computeShader.SetBuffer(kernelHandle, "color2Buffer", color2Buffer);
        computeShader.SetBuffer(kernelHandle, "sumBuffer", diffBuffer);

        computeShader.Dispatch(0, bigSize / 8, bigSize / 8, 1);
        float[] resultContainer = new float[bigSize];
        diffBuffer.GetData(resultContainer);

        int bestChunkIndex = -1;
        float lowestDifference = float.MaxValue;
        for (int i = 0; i < chunksToPullFrom.Length; i++)
        {
            float difference = 0;
            for (int j = 0; j < BUFFER_LENGTH; j++)
            {
                int index = (i * BUFFER_LENGTH) + j;
                difference += resultContainer[index];
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
