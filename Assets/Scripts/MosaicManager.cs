using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class MosaicManager : MonoBehaviour
{
    public Button runButton;
    public Text statusText;

    public Texture2D textureToRecreate;
    public TextureCollection textureCollection;


    private void Awake()
    {
        runButton.onClick.AddListener(delegate { StartCoroutine(RunRountine()); });
    }


    private IEnumerator RunRountine()
    {
        runButton.gameObject.SetActive(false);
        statusText.text = "Running";
        yield return null;
        Run();
        runButton.gameObject.SetActive(true);
    }

    public void Run()
    {

        NativeArray<Color32> pixelsToPullFrom = textureCollection.GetTextureChunkData();

        NativeArray<Color32> mainImagePixels = TextureUtility.GetChunkDataForTexture(textureToRecreate);

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        int mainChunkCount = mainImagePixels.Length / TextureUtility.CHUNK_PIXEL_COUNT;
        NativeArray<int> bestIndexOutput = new NativeArray<int>(mainChunkCount, Allocator.Persistent);

        GetBestChunkJob job = new GetBestChunkJob
        {
            mainImagePixels = mainImagePixels,
            pixelsToPullFrom = pixelsToPullFrom,
            bestIndexOutput = bestIndexOutput
        };
        JobHandle jobHandle = job.Schedule(mainChunkCount, 1);
        jobHandle.Complete();

        stopwatch.Stop();
        Debug.Log(stopwatch.ElapsedMilliseconds);
        statusText.text = "Finished: " + stopwatch.ElapsedMilliseconds + "ms";

        Texture2D finalTexture = TextureUtility.AssembleChunksIntoTexture(bestIndexOutput, pixelsToPullFrom, textureToRecreate.width);
        TextureUtility.WriteTexToPNG(finalTexture, "Result");
        pixelsToPullFrom.Dispose();
        mainImagePixels.Dispose();
        bestIndexOutput.Dispose();

    }
}

[BurstCompile(CompileSynchronously = true)]
public struct GetBestChunkJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<Color32> mainImagePixels;

    [ReadOnly]
    public NativeArray<Color32> pixelsToPullFrom;

    [WriteOnly]
    public NativeArray<int> bestIndexOutput;

    public void Execute(int index)
    {
        int mainImageChunkIndex = index * TextureUtility.CHUNK_PIXEL_COUNT;

        int lowestDifference = int.MaxValue;

        int chunkCount = pixelsToPullFrom.Length / TextureUtility.CHUNK_PIXEL_COUNT;
        for (int i = 0; i < chunkCount; i++)
        {
            int pixelToPullFromStartIndex = i * TextureUtility.CHUNK_PIXEL_COUNT;
            int difference = 0;
            for (int j = 0; j < TextureUtility.CHUNK_PIXEL_COUNT; j++)
            {
                Color32 color1 = mainImagePixels[mainImageChunkIndex + j];
                Color32 color2 = pixelsToPullFrom[pixelToPullFromStartIndex + j];
                
                ////BW Method
                //int bw1 = color1.r * 30 + color1.g * 59 + color1.b * 11;
                //int bw2 = color2.r * 30 + color2.g * 59 + color2.b * 11;
                //int bwDiff = bw1 - bw2;

                //bwDiff = (bwDiff + (bwDiff >> 31)) ^ (bwDiff >> 31); //Absolute Value

                //difference += bwDiff;

                //Color Method
                int diffr = color1.r - color2.r;
                int diffg = color1.g - color2.g;
                int diffb = color1.b - color2.b;
                diffr = (diffr + (diffr >> 31)) ^ (diffr >> 31);
                diffg = (diffg + (diffg >> 31)) ^ (diffg >> 31);
                diffb = (diffb + (diffb >> 31)) ^ (diffb >> 31);
                difference += diffr + diffg + diffb;
            }
            if (difference < lowestDifference)
            {
                lowestDifference = difference;
                bestIndexOutput[index] = i;
            }
        }
    }
}
