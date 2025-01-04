using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV.Reg;
using System.Runtime.InteropServices;

namespace Labyrinth
{
    public partial class FrmPrompt : Form
    {
        private Bitmap _image;
        int _size;
        public FrmPrompt(Bitmap image,int size)
        {
            InitializeComponent();

            // string path2=Path.Combine(Application.StartupPath,"images");
            //if (System.IO.Directory.Exists(path2))
            //{
            //    System.IO.Directory.Delete(path2, true);
            //}
            //Directory.CreateDirectory(path2);

            //path = Path.Combine(path2, DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp");
            //image.Save(path, ImageFormat.Bmp);
            //Test(path);
            //_image=image;
            //_size=size;
            //Test();
            _image=image;
            pictureBox1.Image=image;
        }

        private void FrmPrompt_FormClosing(object sender, FormClosingEventArgs e)
        {
            _image?.Dispose();
            this.pictureBox1.Image?.Dispose();
            this.Dispose();
        }
        int ksize = 15;
        int ksize2 = 2;
        private void Test()
        {
            if (_size == 0)
            {
                ksize = 15;
            }
            else if(_size == 1)
            {
                ksize = 22;
                ksize2 = 4;
            }
            else
            {
                ksize = 20;
                ksize2 = 3;
            }
            using var img = _image.ToImage<Bgr, byte>();

            using var grayImg = img.Convert<Gray, byte>();
            // 进行膨胀腐蚀操作
            using var kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(ksize, ksize), new Point(-1, -1));
            // 使用CLAHE增强对比度
            CvInvoke.CLAHE(grayImg, 2, new Size(4, 4), grayImg);

            using var inputGrayOut = new Image<Gray, byte>(grayImg.Size);
            // 计算OTSU阈值
            var threshold = CvInvoke.Threshold(grayImg, inputGrayOut, 0, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);
            // 二值化图像
            using var binaryImage = inputGrayOut.ThresholdBinary(new Gray(threshold), new Gray(255));

            // 裁剪图像
            using var dilated2 = new Mat();
            CvInvoke.Dilate(binaryImage, dilated2, kernel, new Point(-1, -1), ksize2, BorderType.Default, new MCvScalar());
            using Mat hierarchy3 = new Mat();
            using VectorOfVectorOfPoint contours3 = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(dilated2, contours3, hierarchy3, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            using var binaryImage2 = binaryImage.Copy(CvInvoke.BoundingRectangle(contours3[0]));
            //CvInvoke.DrawContours(img, contours3, 0, new Bgr(0, 0, 255).MCvScalar, 1);


            // 查找轮廓，并绘制在全黑图像上
            using VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            using Mat hierarchy = new Mat();
            CvInvoke.FindContours(binaryImage2, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            // 绘制在全黑图像上
            using var draw = new Image<Gray, byte>(binaryImage2.Size);
            double maxArea2 = 0;
            int maxAreaIndex2 = 0;
            for (int i = 0; i < contours.Size; i++)
            {
                double area = CvInvoke.ContourArea(contours[i]);
                if (area > maxArea2)
                {
                    maxArea2 = area;
                    maxAreaIndex2 = i;
                }
            }
            CvInvoke.DrawContours(draw, contours, maxAreaIndex2, new Bgr(255, 255, 255).MCvScalar, 1);
            Trace.WriteLine($"边框1：{contours.Size}");


            // 膨胀
            using var dilated = new Mat();
            CvInvoke.Dilate(draw, dilated, kernel, new Point(-1, -1), ksize2, BorderType.Default, new MCvScalar());
            // 腐蚀
            using var eroded = new Mat();
            CvInvoke.Erode(dilated, eroded, kernel, new Point(-1, -1), ksize2, BorderType.Default, new MCvScalar());

            // 膨胀腐蚀相减
            using var diff = new Mat();
            CvInvoke.AbsDiff(dilated, eroded, diff);

            // 在差异图diff中查找轮廓，并在原图上绘制轮廓
            using VectorOfVectorOfPoint contours2 = new VectorOfVectorOfPoint();
            using var hierarchy2 = new Mat();
            //CvInvoke.CvtColor(diff, diff, ColorConversion.Bgr2Gray);
            CvInvoke.FindContours(diff, contours2, hierarchy2, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            double maxArea = 0;
            int maxAreaIndex = 0;
            for (int i = 0; i < contours2.Size; i++)
            {
                double area = CvInvoke.ContourArea(contours2[i]);
                if (area > maxArea)
                {
                    maxArea = area;
                    maxAreaIndex = i;
                }
            }

            CvInvoke.DrawContours(img, contours2, maxAreaIndex, new Bgr(0, 0, 255).MCvScalar, 1);
            Trace.WriteLine($"边框2：{contours2.Size}");

            this.pictureBox1.Image = img.ToBitmap();
        }
    }
}
