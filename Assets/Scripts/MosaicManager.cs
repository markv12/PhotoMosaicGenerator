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
            int difference = 0;
            for (int j = 0; j < mainChunk.colors.Length; j++)
            {
                Color32 color1 = mainChunk.colors[j];
                Color32 color2 = candidateChunk.colors[j];
                int bw1 = color1.r * 30 + color1.g * 59 + color1.b * 11;
                int bw2 = color2.r * 30 + color2.g * 59 + color2.b * 11;
                int bwDiff = bw1 - bw2;

                bwDiff = (bwDiff + (bwDiff >> 31)) ^ (bwDiff >> 31); //Absolute Value
                
                difference += bwDiff;
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
