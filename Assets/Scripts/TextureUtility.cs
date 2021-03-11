using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;

public static class TextureUtility
{
    private const int CHUNK_SIZE = 32;
    public const int CHUNK_PIXEL_COUNT = CHUNK_SIZE * CHUNK_SIZE;
    public static NativeArray<Color32> GetChunkDataForTexture(Texture2D t)
    {
        List<Color32> result = new List<Color32>();
        GetChunkDataForTexture(t, result);
        NativeArray<Color32> nativeResult = new NativeArray<Color32>(result.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        for (int i = 0; i < result.Count; i++)
        {
            nativeResult[i] = result[i];
        }
        return nativeResult;
    }
    public static void GetChunkDataForTexture(Texture2D t, List<Color32> result)
    {
        int columns = t.width / CHUNK_SIZE;
        int rows = t.height / CHUNK_SIZE;

        Color32[] colors = t.GetPixels32();
        for (int i = 0; i < columns; i++)
        {
            int xOffset = i * CHUNK_SIZE;
            for (int j = 0; j < rows; j++)
            {
                int yOffset = j * CHUNK_SIZE * t.width;
                result.AddRange(GetPieceOfImage(colors, xOffset, yOffset, t.width));
            }
        }
    }

    private static Color32[] GetPieceOfImage(Color32[] colors, int xOffset, int yOffset, int width)
    {
        Color32[] result = new Color32[CHUNK_PIXEL_COUNT];
        for (int i = 0; i < CHUNK_SIZE; i++)
        {
            for (int j = 0; j < CHUNK_SIZE; j++)
            {
                result[i + j * CHUNK_SIZE] = colors[(xOffset + i) + (yOffset + (j * width))];
            }
        }
        return result;
    }

    public static Texture2D AssembleChunksIntoTexture(NativeArray<int> bestChunkIndices, NativeArray<Color32> pixelsToPullFrom, int origImageWidth)
    {
        int chunkCount = bestChunkIndices.Length;
        int columns = origImageWidth / CHUNK_SIZE;
        int rows = chunkCount / columns;
        Color32[] colors = new Color32[columns * CHUNK_SIZE * rows * CHUNK_SIZE];

        for (int i = 0; i < bestChunkIndices.Length; i++)
        {
            int chunkOffset = bestChunkIndices[i] * CHUNK_PIXEL_COUNT;
            int x = i / columns;
            int y = i % columns;
            int xOffset = x * CHUNK_SIZE;
            int yOffset = y * CHUNK_SIZE * origImageWidth;
            WriteToColorArray(colors, pixelsToPullFrom, chunkOffset, xOffset, yOffset, origImageWidth);
        }

        Texture2D result = new Texture2D(columns * CHUNK_SIZE, rows * CHUNK_SIZE);
        result.SetPixels32(colors);
        return result;
    }

    private static void WriteToColorArray(Color32[] destination, NativeArray<Color32> pixelsToPullFrom, int chunkOffset, int xOffset, int yOffset, int width)
    {
        for (int i = 0; i < CHUNK_SIZE; i++)
        {
            for (int j = 0; j < CHUNK_SIZE; j++)
            {
                destination[(xOffset + i) + (yOffset + (j * width))] = pixelsToPullFrom[chunkOffset + (i + j * CHUNK_SIZE)];
            }
        }
    }

    public static void WriteTexToPNG(Texture2D tex, string fileName)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "\\" + fileName + ".png", bytes);
    }
}
