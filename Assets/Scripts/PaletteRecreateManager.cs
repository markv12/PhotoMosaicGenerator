using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PaletteRecreateManager : MonoBehaviour {
    public Texture2D textureToRecreate;
    public Color32[] palette;

    public void Run() {
        Color32[] pixels = textureToRecreate.GetPixels32();
        Color32[] resultPixels = new Color32[pixels.Length];

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        Color32 closestPixel = new Color32();
        ref Color32 closestPixelRef = ref closestPixel;
        for (int i = 0; i < pixels.Length; i++) {
            Color32 pixel = pixels[i];
            float lowestDifference = float.MaxValue;
            for (int j = 0; j < palette.Length; j++) {
                ref Color32 paletteColor = ref palette[j];
                int diffr = pixel.r - paletteColor.r;
                int diffg = pixel.g - paletteColor.g;
                int diffb = pixel.b - paletteColor.b;
                diffr = (diffr + (diffr >> 31)) ^ (diffr >> 31);
                diffg = (diffg + (diffg >> 31)) ^ (diffg >> 31);
                diffb = (diffb + (diffb >> 31)) ^ (diffb >> 31);
                float difference = diffr + diffg + diffb;
                if(difference < lowestDifference) {
                    closestPixelRef = paletteColor;
                    lowestDifference = difference;
                }
            }
            resultPixels[i] = closestPixelRef;
        }

        stopwatch.Stop();
        Debug.Log(stopwatch.ElapsedMilliseconds);

        Texture2D finalTexture = new Texture2D(textureToRecreate.width, textureToRecreate.height);
        finalTexture.SetPixels32(resultPixels);
        TextureUtility.WriteTexToPNG(finalTexture, "Result");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PaletteRecreateManager))]
public class PaletteRecreateManagerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        PaletteRecreateManager mosaicManager = (PaletteRecreateManager)target;

        if (GUILayout.Button("Run")) {
            mosaicManager.Run();
        }
    }
}
#endif
