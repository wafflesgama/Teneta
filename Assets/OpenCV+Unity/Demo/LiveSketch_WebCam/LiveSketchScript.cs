namespace OpenCvSharp.Demo
{
    using UnityEngine;
    using OpenCvSharp;
    using System.Threading.Tasks;

    public class LiveSketchScript : WebCamera
    {
        public Texture2D generatedTex;
        public Texture2D externalTex;
        public bool bw;
        public bool upscale;
        public int blurSize = 5;
        public float thres = 70f;
        public float maxVal = 255f;
        public int mode = 0;
        public int resize = 1;
        public double canny1 = 10;
        public double canny2 = 70;

        Mat refImage;
        Mat img;
        Mat img2;


        protected override void Awake()
        {
            base.Awake();
            this.forceFrontalCamera = true;
            generatedTex = new Texture2D(2, 2);
            externalTex = new Texture2D(2, 2);
        }


        private async void AddRef()
        {
            await Task.Delay(2000);
            refImage = img2;
        }

        // Our sketch generation function
        protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
        {

            img = Unity.TextureToMat(input, TextureParameters);
            img2 = new Mat();




            Cv2.Resize(img, img2, new Size(resize, resize), 0, 0, InterpolationFlags.Nearest);


            //Convert image to grayscale
            //Mat imgGray = new Mat();
            //Cv2.CvtColor(img2, img2, ColorConversionCodes.BGR2GRAY);



            //Mat imgMed = new Mat();
            Cv2.MedianBlur(img2, img2, blurSize);

            if (Input.GetKey(KeyCode.Space))
            {
                AddRef();
                return true;
            }


            if (refImage == null) return true;

            // Clean up image using Gaussian Blur
            //Mat imgGrayBlur = new Mat();
            //Cv2.GaussianBlur(imgGray, imgGrayBlur, new Size(blurSize, blurSize), 0);


            Mat diff = new Mat();
            Cv2.Absdiff(refImage, img2, diff);



            //Mat after = new Mat();
            //Cv2.CvtColor(diff, after, ColorConversionCodes.BGR2GRAY);

            //Cv2.MorphologyEx(imgGray, morpho, MorphTypes.TopHat, imgGray);


            ////Do an invert binarize the image
            Mat mask = new Mat();


            ////Cv2.Subtract
            if (mode == -1)
                mask = diff;
            else if (mode == 0)
                Cv2.Threshold(diff, mask, thres, maxVal, ThresholdTypes.Binary);
            else if (mode == 1)
                Cv2.Threshold(diff, mask, thres, maxVal, ThresholdTypes.Triangle);
            else if (mode == 2)
                Cv2.Threshold(diff, mask, thres, maxVal, ThresholdTypes.BinaryInv);
            else if (mode == 3)
                Cv2.Threshold(diff, mask, thres, maxVal, ThresholdTypes.Trunc);
            else if (mode == 4)
                Cv2.Threshold(diff, mask, thres, maxVal, ThresholdTypes.Triangle);
            else
                Cv2.Threshold(diff, mask, thres, maxVal, ThresholdTypes.Tozero);


            //Mat maskConverted = new Mat();
            if (bw)
                Cv2.CvtColor(mask, mask, ColorConversionCodes.BGR2GRAY);
            //else
            //    maskConverted = mask;

            //Destroy(generatedTex);
            //generatedTex = Unity.MatToTexture(maskConverted, output);
            //if (upscale)
            //{
            //    Mat upScaled = new Mat();
            //    Cv2.Resize(mask, upScaled, new Size(1280, 720), 0, 0, InterpolationFlags.Nearest);
            //    //Destroy(generatedTex);
            //    output = Unity.MatToTexture(upScaled, output);
            //}
            //else
            //{
            //    //Destroy(generatedTex);
            //}
            //output = Unity.MatToTexture(mask, output);
            //var rawTest = output.GetRawTextureData();
            //var rawImageCompressed = CLZF.Compress(rawTest);
            //var rawImageEncodedCompressed = CLZF.Compress(output.EncodeToJPG(5));
            //var rawDataInit=mask.ToBytes();
            var rawDataMy = Unity.MatToByteData(mask);
            var raw1BitDataMy = Unity.MatTo1BitData(mask);
            var rawColorsMy = Unity.MatToColors(mask);


            var rawDataMyComp = CLZF.Compress(raw1BitDataMy);


            Color32[] cColors = new Color32[raw1BitDataMy.Length];
            for (int i = 0; i < rawDataMy.Length; i++)
                cColors[i] = new Color32(raw1BitDataMy[i], raw1BitDataMy[i], raw1BitDataMy[i], 255);


            Mat imported = Unity.PixelsToMat(cColors, mask.Width, mask.Height, false, false, 0);

            //var rawDataInitComp = CLZF.Compress(rawDataInit);
            //var rawImageCompressed  = CLZF.Compress(rawTest);
            //var rawImageEncodedCompressed  = CLZF.Compress(output.EncodeToJPG(5));
            //Destroy(generatedTex);
            //}

            if (upscale)
            {
                //Mat upScaled = new Mat();
                Cv2.Resize(imported, imported, new Size(1280, 720), 0, 0, InterpolationFlags.Nearest);
                //Destroy(generatedTex);
                //output = Unity.MatToTexture(upScaled, output);
            }
            

                output = Unity.MatToTexture(imported, output);
            //output = Unity.MatToTexture(mask, output);

            return true;
        }


        protected override void ExternalProcess()
        {
            if (upscale)
            {
                img = Unity.TextureToMat(externalTex);
                img2 = new Mat();
                Cv2.Resize(img, img2, new Size(resize, resize), 0, 0, InterpolationFlags.Nearest);
                surfaceImage.texture = Unity.MatToTexture(img2);
            }
            else
            {
                surfaceImage.texture = externalTex;
            }

        }
    }


}