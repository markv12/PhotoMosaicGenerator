using UnityEngine;

public class TextureChunkData //64x64
{
    public Color32[] colors;

    public void WriteToPNG(string fileName)
    {
        Texture2D tex = new Texture2D(64, 64);
        tex.SetPixels32(colors);
        TextureUtility.WriteTexToPNG(tex, fileName);
    }
}