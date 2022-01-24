namespace OpenCvSharp.Demo
{
    using UnityEngine;
    using OpenCvSharp;
    using System.Threading.Tasks;

    public class LiveSketchScript : WebCamera
    {
        public Texture2D texture2D;

        public int blurSize = 5;
        public float thres = 70f;
        public float maxVal = 255f;
        public int mode = 0;
        public double canny1 = 10;
        public double canny2 = 70;
        public bool modeC;
        public double analFactor;
        int counter = 0;
        Mat refImage;
        Mat img;

        public bool bw;

        public Mat finalMat;

        protected override void Awake()
        {
            base.Awake();
            this.forceFrontalCamera = true;
        }


        private async void AddRef()
        {
            await Task.Delay(3000);
            refImage = img;
        }
        // Our sketch generation function
        protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
        {

            img = Unity.TextureToMat(input, TextureParameters);
            counter++;
            //Convert image to grayscale
            Mat imgGray = new Mat();
            Cv2.CvtColor(img, imgGray, ColorConversionCodes.BGR2GRAY);


            Mat imgMed = new Mat();
            Cv2.MedianBlur(img, imgMed, blurSize);

            if (Input.GetKey(KeyCode.Space))
            {
                AddRef();
                return true;
            }


            //if (Input.GetKey(KeyCode.T))
            //{
            //    anal = imgGray;
            //}

            //if (anal != null)
            //{
            //    //analFactor = Cv2.(anal, imgGray, ShapeMatchModes.I1);
            //}



            if (refImage == null) return true;

            // Clean up image using Gaussian Blur
            //Mat imgGrayBlur = new Mat();
            //Cv2.GaussianBlur(imgGray, imgGrayBlur, new Size(blurSize, blurSize), 0);

            Mat diff = new Mat();
            Cv2.Absdiff(refImage, imgMed, diff);

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


            //////Extract edges
            //Mat cannyEdges = new Mat();
            //Cv2.Canny(mask, cannyEdges, canny1, canny2);
            Mat maskConverted = new Mat();
            if (bw)
                Cv2.CvtColor(mask, maskConverted, ColorConversionCodes.BGR2GRAY);
            else
                maskConverted = mask;


            ////if (modeC)
            ////{
            //Point[][] countourPoints;
            //HierarchyIndex[] hierarchy;
            //Cv2.FindContours(maskConverted, out countourPoints, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            //if (countourPoints.Length > 0)
            //{

            //    Mat fillRec = new Mat(); ;
            //    Cv2.FillConvexPoly(fillRec, countourPoints[0], Scalar.Blue);
            //    // result, passing output texture as parameter allows to re-use it's buffer
            //    // should output texture be null a new texture will be created
            //}


            finalMat = maskConverted;
            output = Unity.MatToTexture(maskConverted, output);
            texture2D = output;
            //}
            return true;
        }
    }
}