using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using MsBox.Avalonia;
using System;
using System.IO;
using static OrthoVi.CreateNewPatientWindow;
using static OrthoVi.TrayWindow;

namespace OrthoVi;

public partial class LandmarksWindow : Window
{
    public LandmarksWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        var draggableArea = this.FindControl<Border>("DraggableArea");
        draggableArea.PointerPressed += DraggableArea_PointerPressed;
        byte[] imageData = FindCephImage();

        // Convert byte[] to Bitmap
        using (MemoryStream ms = new MemoryStream(imageData))
        {
            CephImage.Source = new Bitmap(ms);
        }
    }

    internal static byte[] FindCephImage()
    {
        byte[] image = SessionManager.LoggedInUser.DoctorInformation.Clients[ViewpatientWindow.CliendIndex].Images.Find(x => x.ImageName == "LateralCeph").ImageContent;
        if (image == null || image.Length == 0)
        {
            MessageBoxManager.GetMessageBoxStandard("Error!", "No cephalometric image!");
        }
        return image;
    }

    private void DraggableArea_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Begin dragging the window only if clicked within the draggable area
            BeginMoveDrag(e);

        }
    }



    private void AI_Landmarks_Click(object sender, RoutedEventArgs e)
    {
        if (FindCephImage() == null)
        {
            MessageBoxManager.GetMessageBoxStandard("Error!", "No cephalometric image! Please add a cephalometric image.");
        }
        else
        {
            CephalometricDetectionClasses cephalometricDetection = new CephalometricDetectionClasses();
            CephImage.Source = cephalometricDetection.Predict();
        }
       
    }



    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    private void MinimizeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Minimize the window
        this.WindowState = WindowState.Minimized;
    }
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        HomePageWindow homeWindow = new HomePageWindow();
        homeWindow.Show();
        this.Close();
    }
}
