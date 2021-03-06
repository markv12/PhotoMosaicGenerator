using UnityEngine;

public class TextureChunkData //64x64
{
    public Color[] colors;

    public void WriteToPNG(string fileName)
    {
        Texture2D tex = new Texture2D(64, 64);
        tex.SetPixels(colors);
        TextureUtility.WriteTexToPNG(tex, fileName);
    }

    public int DiffWithChunk(TextureChunkData otherChunk)
    {
        int difference = 0;
        for (int i = 0; i < colors.Length; i++)
        {
            difference += DiffBetweenColors(colors[i], otherChunk.colors[i]);
        }

        static int DiffBetweenColors(Color color1, Color color2)
        {
            //int rDiff = color1.r - color2.r;
            //int gDiff = color1.g - color2.g;
            //int bDiff = color1.b - color2.b;
            //rDiff = rDiff < 0 ? -rDiff : rDiff;
            //gDiff = gDiff < 0 ? -gDiff : gDiff;
            //bDiff = bDiff < 0 ? -bDiff : bDiff;

            double bw1 = color1.r * 0.3 + color1.g * 0.59 + color1.b * 0.11;
            double bw2 = color2.r * 0.3 + color2.g * 0.59 + color2.b * 0.11;
            double bwDiff = bw1 - bw2;
            bwDiff = bwDiff < 0 ? -bwDiff : bwDiff;

            return (int)bwDiff;
            //return rDiff + gDiff + bDiff;
        }
        return difference;
    }
}