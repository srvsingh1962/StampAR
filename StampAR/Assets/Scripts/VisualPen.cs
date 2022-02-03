using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;

public class VisualPen : WebCamera
{
    private FaceProcessorLive<WebCamTexture> processor;

    protected override void Awake()
    {
        base.Awake();
        base.forceFrontalCamera = true; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly
        

        processor = new FaceProcessorLive<WebCamTexture>();
        

    }


    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        processor.ProcessTexture(input, TextureParameters);
        return true;
    }
}
