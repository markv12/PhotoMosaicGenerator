using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MosaicManager : MonoBehaviour
{
    public Texture2D textureToRecreate;
    public TextureCollection textureCollection;
    public void Run()
    {
        TextureChunkData[] chunksToPullFrom = textureCollection.GetTextureChunkData();

        TextureChunkData[] chunksToRecreate = TextureUtility.GetChunkDataForTexture(textureToRecreate);
        
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        TextureChunkData[] recreatedChunks = chunksToRecreate.AsParallel().Select((chunkToRecreate, index) =>
                      GetBestChunkForChunk(chunkToRecreate, chunksToPullFrom)).ToArray();

        stopwatch.Stop();
        Debug.Log(stopwatch.ElapsedMilliseconds);

        Texture2D finalTexture = TextureUtility.AssembleChunksIntoTexture(recreatedChunks, textureToRecreate.width);
        TextureUtility.WriteTexToPNG(finalTexture, "Result");
    }

    private TextureChunkData GetBestChunkForChunk(TextureChunkData mainChunk, TextureChunkData[] chunksToPullFrom)
    {
        int bestChunkIndex = -1;
        int lowestDifference = int.MaxValue;

        for (int i = 0; i < chunksToPullFrom.Length; i++)
        {
            TextureChunkData candidateChunk = chunksToPullFrom[i];
            int difference = mainChunk.DiffWithChunk(candidateChunk);
            if(difference < lowestDifference)
            {
                lowestDifference = difference;
                bestChunkIndex = i;
            }
        }
        return chunksToPullFrom[bestChunkIndex];
    }
}
