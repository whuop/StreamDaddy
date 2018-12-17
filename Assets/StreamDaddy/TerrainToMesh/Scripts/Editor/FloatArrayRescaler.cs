using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloatArrayRescaler
{
    public static float[,] RescaleArray(float[,] sourceArray, float[,] destinationArray)
    {
        //  Amount of samples in x and y for the source array.
        int srcSamplesX = sourceArray.GetLength(0);
        int srcSamplesY = sourceArray.GetLength(1);

        //  Amount of samples in x and y for the destination array.
        int dstSamplesX = destinationArray.GetLength(0);
        int dstSamplesY = destinationArray.GetLength(1);

        float ratioX =  (float)srcSamplesX / (float)dstSamplesX;
        float ratioY =  (float)srcSamplesY / (float)dstSamplesY;

        Debug.LogError("Size Src X/Y: " + srcSamplesX + "/" + srcSamplesY);
        Debug.LogError("Size Dst X/Y: " + dstSamplesX + "/" + dstSamplesX);
        Debug.LogError("Ratio X/Y: " + ratioX + "/" + ratioY);

        float[,] scaledArray = new float[dstSamplesX, dstSamplesY];

        for(int x = 0; x <= dstSamplesX; x++)
        {
            for(int y = 0; y <= dstSamplesY; y++)
            {
                float samplePosX = (float)x * ratioX;
                float samplePosY = (float)y * ratioY;

                float normalizedPosX = samplePosX / (float)srcSamplesX;
                float normalizedPosY = samplePosY / (float)srcSamplesY;

                Debug.LogError("Pos X/Y: " + samplePosX + "/" + samplePosY);
                Debug.LogError("Normalized Pos X/Y: " + normalizedPosX + "/" + normalizedPosY);

                int prevSampleX = Mathf.FloorToInt(samplePosX);
                int nextSampleX = Mathf.CeilToInt(samplePosX);

                int prevSampleY = Mathf.FloorToInt(samplePosY);
                int nextSampleY = Mathf.CeilToInt(samplePosY);

                Debug.LogError("Prev/Next X: " + prevSampleX + "/" + nextSampleX);
                Debug.LogError("Prev/Next Y: " + prevSampleY + "/" + nextSampleY);
            }
        }


        return scaledArray;
    }
}
