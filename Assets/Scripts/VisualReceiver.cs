using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;

public class VisualReceiver : MonoBehaviour
{

    public RawImage surface;
    public bool upscale;
    public int resize;
    private Texture2D outputTex;
    private void Awake()
    {
        //outputTex = new Texture2D(2, 2);
    }
    void Start()
    {

    }

    public void UpdateVisual(ref Texture2D input)
    {
        Destroy(outputTex);
        if (upscale)
        {
            Mat mat = new Mat(), mat2 = new Mat();
            mat = OpenCvSharp.Unity.TextureToMat(input);
            Cv2.Resize(mat, mat2, new Size(resize, resize), 0, 0, InterpolationFlags.Nearest);
            outputTex = OpenCvSharp.Unity.MatToTexture(mat2);
            surface.texture = outputTex;
        }
        else
        {
            //outputTex = input;
            surface.texture = input;
        }
        //outputTex = input;
    }
}
