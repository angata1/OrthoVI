using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoloDotNet;
using YoloDotNet.Enums;
using YoloDotNet.Extensions;
using YoloDotNet.Models;



namespace OrthoVi
{
    internal class CephalometricDetectionClasses
    {
        internal void Predict(int clientID)
        {
           
            using var yolo = new Yolo(new YoloOptions
            {
                OnnxModel = @"./YOLO_cephalometric_landmarks.onnx",
                ModelType = ModelType.ObjectDetection,
                Cuda = false,
                GpuId = 0,
                PrimeGpu = false,
            });


            
            byte[] imageData = MainWindow.SessionManager.LoggedInUser.DoctorInformation.Clients[0].Images[0].ImageContent;

            // Decode the byte array into an SKBitmap
            using var skData = SKData.CreateCopy(imageData);
            using var skBitmap = SKBitmap.Decode(skData);

            // Create an SKImage from the SKBitmap
            using var image = SKImage.FromBitmap(skBitmap);

            var results = yolo.RunObjectDetection(image, confidence: 0.15, iou: 0.8);

            results = RemoveDuplicates(results);


            using var resultImage = image.Draw(results);


            pictureBox2.Image = ConvertToBitmap(resultImage);

            string outputPath = Path.Combine(Environment.CurrentDirectory, "output.jpg");
            resultImage.Save(outputPath, SKEncodedImageFormat.Jpeg, 80);
            
            
        }

        // Helper method to convert SKImage to Bitmap
        private Bitmap ConvertToBitmap(SKImage skImage)
        {
            using var data = skImage.Encode();
            using var ms = new MemoryStream(data.ToArray());
            return new Bitmap(ms);
        }


        public static List<ObjectDetection> RemoveDuplicates(List<ObjectDetection> Results)
        {

            var resultsDict = new Dictionary<string, ObjectDetection>();

            foreach (var result in Results)
            {
                if (resultsDict.TryGetValue(result.Label.Name, out var existingResult))
                {
                    if (existingResult.Confidence < result.Confidence)
                    {
                        resultsDict[result.Label.Name] = result;
                    }
                }
                else
                {
                    resultsDict[result.Label.Name] = result;
                }
            }

            Results = resultsDict.Values.ToList();
            return Results;
        }
    }
}
