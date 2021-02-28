using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTextureCollection", menuName = "Texture Collection", order = 0)]
public class TextureCollection : ScriptableObject
{
    public Texture2D[] textures;
    public TextureChunkData[] GetTextureChunkData()
    {
        List<TextureChunkData> result = new List<TextureChunkData>(100);
        for (int i = 0; i < textures.Length; i++)
        {
            Texture2D t = textures[i];
            TextureUtility.GetChunkDataForTexture(t, result);
        }
        return result.ToArray();
    }
}