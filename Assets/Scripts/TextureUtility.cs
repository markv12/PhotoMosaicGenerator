using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class TextureUtility
{
    private const int CHUNK_SIZE = 64;

    public static TextureChunkData[] GetChunkDataForTexture(Texture2D t)
    {
        List<TextureChunkData> result = new List<TextureChunkData>();
        GetChunkDataForTexture(t, result);
        return result.ToArray();
    }
    public static void GetChunkDataForTexture(Texture2D t, List<TextureChunkData> result)
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
                result.Add(new TextureChunkData()
                {
                    colors = GetPieceOfImage(colors, xOffset, yOffset, t.width)
                });
            }
        }
    }

    private static Color32[] GetPieceOfImage(Color32[] colors, int xOffset, int yOffset, int width)
    {
        Color32[] result = new Color32[CHUNK_SIZE * CHUNK_SIZE];
        for (int i = 0; i < CHUNK_SIZE; i++)
        {
            for (int j = 0; j < CHUNK_SIZE; j++)
            {
                result[i + j * CHUNK_SIZE] = colors[(xOffset + i) + (yOffset + (j * width))];
            }
        }
        return result;
    }

    public static Texture2D AssembleChunksIntoTexture(TextureChunkData[] recreatedChunks, int origImageWidth)
    {
        int chunkCount = recreatedChunks.Length;
        int columns = origImageWidth / CHUNK_SIZE;
        int rows = chunkCount / columns;
        Color32[] colors = new Color32[columns * CHUNK_SIZE * rows * CHUNK_SIZE];

        for (int i = 0; i < recreatedChunks.Length; i++)
        {
            int x = i / columns;
            int y = i % columns;
            int xOffset = x * CHUNK_SIZE;
            int yOffset = y * CHUNK_SIZE * origImageWidth;
            WriteToColorArray(colors, recreatedChunks[i].colors, xOffset, yOffset, origImageWidth);
        }

        Texture2D result = new Texture2D(columns * CHUNK_SIZE, rows * CHUNK_SIZE);
        result.SetPixels32(colors);
        return result;
    }

    private static void WriteToColorArray(Color32[] destination, Color32[] source, int xOffset, int yOffset, int width)
    {
        for (int i = 0; i < CHUNK_SIZE; i++)
        {
            for (int j = 0; j < CHUNK_SIZE; j++)
            {
                destination[(xOffset + i) + (yOffset + (j * width))] = source[i + j * CHUNK_SIZE];
            }
        }
    }

    public static void WriteTexToPNG(Texture2D tex, string fileName)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "\\" + fileName + ".png", bytes);
    }
}
