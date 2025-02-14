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
using static OrthoVi.MainWindow;



namespace OrthoVi
{
    internal class CephalometricDetectionClasses
    {
        internal void Predict(int clientID)
        {
            // Step 1: Validate image content
            byte[] imageData = MainWindow.SessionManager.LoggedInUser.DoctorInformation.Clients[ViewpatientWindow.CliendIndex].Images[0].ImageContent;
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

            // Step 2: Initialize YOLO safely
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

                // Encode the result image to a byte array
                using var imageEncoded = resultImage.Encode(SKEncodedImageFormat.Png, quality: 100); // Encode as PNG
                using var memoryStream = new MemoryStream();
                imageEncoded.SaveTo(memoryStream); // Save encoded image to memory stream
                byte[] updatedImageData = memoryStream.ToArray(); // Convert to byte array

                
                    SessionManager.LoggedInUser.DoctorInformation.Clients[clientID].Images[0].ImageContent = updatedImageData;
                
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error during YOLO prediction.", ex);
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
