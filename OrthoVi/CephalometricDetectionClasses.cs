using Avalonia.Media.Imaging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
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
using static OrthoVi.TrayWindow;



namespace OrthoVi
{
    internal class CephalometricDetectionClasses
    {
        internal Bitmap Predict()
        {
            // Step 1: Validate image content
            byte[] imageData = SessionManager.LoggedInUser.DoctorInformation.Clients[ViewpatientWindow.CliendIndex].Images[0].ImageContent;
            if (imageData == null || imageData.Length == 0)
            {
                throw new InvalidOperationException("Image content is null or empty.");
            }

            using var skData = SKData.CreateCopy(imageData);
            using var skBitmap = SKBitmap.Decode(skData);
            if (skBitmap == null)
            {
                throw new InvalidOperationException("Failed to decode image content.");
            }

            // Step 2: Initialize YOLO
            try
            {
                using var yolo = new Yolo(new YoloOptions
                {
                    OnnxModel = @"./YOLO_cephalometric_landmarks.onnx",
                    ModelType = ModelType.ObjectDetection,
                    Cuda = false,
                    GpuId = 0,
                    PrimeGpu = false,
                });

                // Run object detection
                using var image = SKImage.FromBitmap(skBitmap);
                var results = yolo.RunObjectDetection(image, confidence: 0.15, iou: 0.8);
                results = RemoveDuplicates(results);

                // Draw results on the image
                using var resultImage = image.Draw(results);

                
                return ConvertToBitmap(resultImage);
                
            }
           
            catch (Exception ex)
            {
                MessageBoxManager.GetMessageBoxStandard("Error!","Error during landmark prediction!");
                return null;
            }
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
