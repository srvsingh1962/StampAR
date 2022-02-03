using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Demo;

public class Stampscript : MonoBehaviour
{
    public GameObject canvas;
    public RawImage preview;
    UnityEngine.Rect capRect;
    Texture2D capTexture;
    Mat bgraMat, binMat;
    byte[,] colors = { { 255,255,255}, { 18, 0, 230 }, { 0, 152, 243 }, { 0, 241, 255 },
                        { 31, 195, 143 }, { 68, 153, 0 }, { 150, 158, 0 }, { 233, 160, 0 },
                        { 183, 104, 0 }, { 136, 32, 29 }, { 131, 7, 146 }, { 127, 0, 228 },
                        { 79, 0, 229 }, { 0, 0, 0 }, };
    int colorNo = 0;
    public GameObject original;
    List<GameObject> stampList = new List<GameObject>();

    void Start()
    {
        int w = Screen.width;
        int h = Screen.height;
        capRect = new UnityEngine.Rect(0, 0, w, h);
        capTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        preview.material.mainTexture = capTexture;        
    }

    public void PutObject()
    {
        Camera cam = Camera.main;
        Vector3 v1 = cam.ViewportToWorldPoint(new Vector3(0, 0, 0.6f));
        Vector3 v2 = cam.ViewportToWorldPoint(new Vector3(1, 1, 0.6f));
        Vector3 v3 = cam.ViewportToWorldPoint(new Vector3(0, 1, 0.6f));
        float w = Vector3.Distance(v2, v3);
        float h = Vector3.Distance(v1, v3);
        GameObject stamp = GameObject.Instantiate(original);
        stamp.transform.parent = cam.transform;
        stamp.transform.localPosition = new Vector3(0, 0, 0.6f);
        stamp.transform.localRotation = Quaternion.identity;
        stamp.transform.localScale = new Vector3(w, h, 1);
        Texture2D stampTexture = new Texture2D(capTexture.width, capTexture.height);
        SetColor(stampTexture);
        stamp.GetComponent<Renderer>().material.mainTexture = stampTexture;
        stamp.transform.parent = null;
        stampList.Add(stamp);
        if(stampList.Count==10)
        {
            DestroyImmediate(stampList[0].GetComponent<Renderer>().material.mainTexture);
            DestroyImmediate(stampList[0]);
            stampList.RemoveAt(0);
        }
        preview.enabled = false;
    }

    IEnumerator ImageProcessing()
    {
        canvas.SetActive(false);
        if (bgraMat != null) { bgraMat.Release(); }
        if (binMat != null) { binMat.Release(); }
        yield return new WaitForEndOfFrame();
        CreateImages();
        SetColor(capTexture);
        canvas.SetActive(true);
        preview.enabled = true;
    }

    public void ChangeColor()
    {
        colorNo++;
        colorNo %= colors.Length / 3;
        SetColor(capTexture);
    }

    void SetColor(Texture2D texture)
    {
        if(bgraMat == null || binMat == null) { return; }
        unsafe
        {
            byte* bgraPtr = bgraMat.DataPointer;
            byte* binPtr = binMat.DataPointer;
            int pixelCount = binMat.Width * binMat.Height;
            for(int i=0;i<pixelCount;i++)
            {
                int bgrapos = i * 4;
                if(binPtr[i]==255)
                {
                    bgraPtr[bgrapos + 3] = 0;
                }
                else
                {
                    bgraPtr[bgrapos] = colors[colorNo, 0];  //B
                    bgraPtr[bgrapos + 1] = colors[colorNo, 1];  //G
                    bgraPtr[bgrapos + 2] = colors[colorNo, 2]; //R
                    bgraPtr[bgrapos + 3] = 255;
                }
            }
        }
        OpenCvSharp.Unity.MatToTexture(bgraMat, texture);
    }

    void CreateImages()
    {
        capTexture.ReadPixels(capRect, 0, 0);
        capTexture.Apply();
        bgraMat = OpenCvSharp.Unity.TextureToMat(capTexture);
        binMat = bgraMat.CvtColor(ColorConversionCodes.BGR2GRAY);
        binMat = binMat.Threshold(100, 255, ThresholdTypes.Otsu);
        bgraMat = binMat.CvtColor(ColorConversionCodes.GRAY2BGRA);
    }

    public void StartCV()
    {
        StartCoroutine(ImageProcessing());
    }
}
