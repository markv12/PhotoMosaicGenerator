using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTextureCollection", menuName = "Texture Collection", order = 0)]
public class TextureCollection : ScriptableObject
{
    public Texture2D[] textures;
    public NativeArray<Color32> GetTextureChunkData()
    {
        List<Color32> result = new List<Color32>(100);
        for (int i = 0; i < textures.Length; i++)
        {
            Texture2D t = textures[i];
            TextureUtility.GetChunkDataForTexture(t, result);
        }
        NativeArray<Color32> nativeResult = new NativeArray<Color32>(result.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        for (int i = 0; i < result.Count; i++)
        {
            nativeResult[i] = result[i];
        }
        return nativeResult;
    }
}