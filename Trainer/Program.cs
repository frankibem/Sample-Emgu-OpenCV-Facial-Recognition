using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Trainer
{
    public class Program
    {
        private static string cascadePath = @"haarcascade_frontalface_default.xml";

        static void ExitOnError()
        {
            Console.Error.WriteLine("Usage: prepare <inDir> <outDir>"); // e.g. prepare c:\users\me\pictures c:\users\me\pictures\detected
            Console.Error.WriteLine("Usage: train <inDir> <outPath>");  // e.g. train c:\users\me\pictures\labeled c:\users\me\pictures\train.yaml
            Console.Error.WriteLine("Usage: stitch <inDir> <outPath>"); // e.g. stitch c:\users\me\pictures\labeled c:\users\me\pictures\stitched.jpg
            Environment.Exit(-1);
        }

        static void Main(string[] args)
        {
            if(args.Length != 3)
                ExitOnError();

            switch(args[0])
            {
                case "prepare":
                    PrepareTrainingData(args[1], args[2]);
                    break;
                case "train":
                    TrainRecognizer(args[1], args[2]);
                    break;
                case "stitch":
                    Stitch(args[1], args[2]);
                    break;
                default:
                    ExitOnError();
                    break;
            }
        }

        // Extracts faces in the given input directory and saves them to the given output directory
        static void PrepareTrainingData(string inputDir, string outputDir)
        {
            int imageNumber = 0;
            CascadeClassifier detector = new CascadeClassifier(cascadePath);

            foreach(var fileName in Directory.EnumerateFiles(inputDir))
            {
                // Load the image and detect faces in it
                Mat img = CvInvoke.Imread(fileName, Emgu.CV.CvEnum.LoadImageType.Grayscale);
                var faces = detector.DetectMultiScale(img);

                // Resize and save the faces to the output directory
                foreach(var faceRect in faces)
                {
                    var image = new Mat(img, faceRect).ToImage<Bgr, byte>();
                    image = image.Resize(100, 100, Emgu.CV.CvEnum.Inter.Area);

                    string path = Path.Combine(outputDir, imageNumber.ToString() + ".jpg");
                    Console.WriteLine($"Writing {path}");

                    image.Save(path);
                    imageNumber++;
                }
            }

            Console.WriteLine($"\nDetected {imageNumber} faces.");
        }

        // Trains a recognizer using the labeled images in inputDir and saves the result
        // to outputPath
        static void TrainRecognizer(string inputDir, string outputPath)
        {
            var imageFiles = Directory.EnumerateFiles(inputDir).ToList();
            var images = new Image<Gray, byte>[imageFiles.Count];
            var labels = new int[imageFiles.Count];

            // Load each file and it's label
            Console.WriteLine("Loading data...");
            int i = 0;
            foreach(var imageFile in imageFiles)
            {
                var label = Path.GetFileNameWithoutExtension(imageFile).Split('_')[0];
                labels[i] = int.Parse(label);
                images[i] = CvInvoke.Imread(imageFile, Emgu.CV.CvEnum.LoadImageType.Grayscale).ToImage<Gray, byte>();

                i++;
            }
            Console.WriteLine("Loading complete.");

            // Train the recognizer and save the result
            Console.WriteLine("Training...");
            LBPHFaceRecognizer recognizer = new LBPHFaceRecognizer();
            recognizer.Train(images, labels);
            Console.WriteLine("Training done. Saving results...");
            recognizer.Save(outputPath);
        }

        // Stitch the processed images into a grid (6 columns) for easy visualization
        // Assumes that all input files are 100 x 100 pixels
        static void Stitch(string inDir, string outPath)
        {
            var files = Directory.EnumerateFiles(inDir).ToList();

            List<Image<Gray, byte>> rows = new List<Image<Gray, byte>>();
            Image<Gray, byte> cur = null;

            // Stitch images horizontally into rows
            Console.WriteLine($"Stitching {files.Count} images...");
            for(int i = 0; i < files.Count; i++)
            {
                var image = new Image<Gray, byte>(files[i]);
                if(cur == null)
                    cur = image;
                else
                    cur = cur.ConcateHorizontal(image);   // Stitch horizontally

                if((i + 1) % 6 == 0)
                {
                    rows.Add(cur);
                    cur = null;
                }
            }
            if(cur != null)
                rows.Add(cur);

            // Stitch rows vertically into grid
            Console.WriteLine($"Stitching {rows.Count} rows into a grid");
            if(rows.Count == 0)
                return;

            cur = rows[0];
            for(int i = 1; i < rows.Count; i++)
            {
                cur = cur.ConcateVertical(rows[i]);
            }

            Console.WriteLine("Stitching done");
            Console.WriteLine($"Writing to {outPath}");
            cur.Save(outPath);
            Console.WriteLine("Done.");
        }
    }
}