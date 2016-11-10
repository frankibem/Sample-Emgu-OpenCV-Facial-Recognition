using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

using swf = System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using Emgu.CV.Face;
using Emgu.CV;
using Emgu.CV.Structure;

namespace RecognitionDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string cascadePath = @"haarcascade_frontalface_default.xml";
        private string trainedData = @"C:\Users\souls\Pictures\Demo\train.yaml";
        private string labelPath = @"C:\Users\souls\Pictures\Demo\labels.csv";
        private Mat original;

        private CascadeClassifier detector;
        private LBPHFaceRecognizer recognizer;

        // Associates ids to names
        //private Dictionary<int, string> names = new Dictionary<int, string>() { { 0, "Lauren" }, { 1, "Frank" }, { 2, "Michael" }, { 3, "Claire" }, { 4, "Josh" } };
        private Dictionary<int, string> names;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize detector and recognizer
            detector = new CascadeClassifier(cascadePath);
            recognizer = new LBPHFaceRecognizer();
            recognizer.Load(trainedData);

            // Load the labels
            names = new Dictionary<int, string>();
            var labels = System.IO.File.ReadAllLines(labelPath);
            foreach(var line in labels)
            {
                var content = line.Split(',');
                names.Add(int.Parse(content[0]), content[1]);
            }
        }

        private void Select_Image(object sender, RoutedEventArgs e)
        {
            // Show File Picker
            var fileDialog = new swf.OpenFileDialog();
            fileDialog.RestoreDirectory = true;

            var result = fileDialog.ShowDialog();
            if(result != swf.DialogResult.OK)
                return;

            // Display selected image
            original = new Mat(fileDialog.FileName, Emgu.CV.CvEnum.LoadImageType.Color);
            imageBox.Source = ToBitmapSource(original);
        }

        private void Recognize_Face(object sender, RoutedEventArgs e)
        {
            // Convert to grayscale
            var image = original.ToImage<Bgr, byte>();
            var gray = image.Convert<Gray, byte>();

            // Detect face(s)
            var faceRects = detector.DetectMultiScale(gray, 1.4, 4);

            // Recognize faces and label
            var filtered = Filter(faceRects);
            foreach(var faceRect in filtered)
            {
                var scaled = gray.GetSubRect(faceRect).Resize(100, 100, Emgu.CV.CvEnum.Inter.Area);
                var result = recognizer.Predict(scaled);

                // Ignore if above threshold
                if(result.Distance >= 90)
                    continue;

                // Frame using rectangle
                image.Draw(faceRect, new Bgr(System.Drawing.Color.Green), 2);

                // Get label
                string name;
                if(!names.TryGetValue(result.Label, out name))
                    name = "Unknown";

                // Format label and accuracy
                name = string.Format("{0} - {1:f2}", name, result.Distance);

                // Draw text
                int y = Math.Max(0, faceRect.Y - 10);
                image.Draw(name,
                    new System.Drawing.Point(faceRect.X, y),
                    Emgu.CV.CvEnum.FontFace.HersheyTriplex, 0.6,
                    new Bgr(System.Drawing.Color.Yellow));
            }

            imageBox.Source = ToBitmapSource(image);
        }

        // Removes rectangles that are solely contained in another
        private List<System.Drawing.Rectangle> Filter(System.Drawing.Rectangle[] rects)
        {
            var result = new List<System.Drawing.Rectangle>();
            for(int i = 0; i < rects.Length; i++)
            {
                bool insideOther = false;
                for(int j = i + 1; j < rects.Length; j++)
                {
                    if(isInside(rects[i], rects[j]))
                    {
                        insideOther = true;
                        break;
                    }
                }

                if(insideOther)
                    continue;
                else
                    result.Add(rects[i]);
            }

            return result;
        }

        // Returns true if the first rectangle is inside the second and false otherwise
        private bool isInside(System.Drawing.Rectangle first, System.Drawing.Rectangle second)
        {
            return first.X >= second.X &&
                first.Y >= second.Y &&
                ((first.X + first.Width) <= (second.X + second.Width)) &&
                ((first.Y + first.Height) <= (second.Y + second.Height));
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(IImage image)
        {
            using(Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();
                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }
    }
}