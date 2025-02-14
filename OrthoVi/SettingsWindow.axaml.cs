using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Avalonia.Platform.Storage;
using System.Net;
using Avalonia.Media.Imaging;
using static OrthoVi.MainWindow;

namespace OrthoVi;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        var draggableArea = this.FindControl<Border>("DraggableArea");
        draggableArea.PointerPressed += DraggableArea_PointerPressed;
    }

    private void DraggableArea_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Begin dragging the window only if clicked within the draggable area
            BeginMoveDrag(e);
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

    private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
    {
        var box2 = MessageBoxManager
                   .GetMessageBoxStandard("Important!", "You are about to delete your account!" + " Are you sure you want to continue?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Warning);
        var result = await box2.ShowWindowAsync();

        if (result == ButtonResult.Yes)
        {
            TrayWindow.DeleteUser(SessionManager.LoggedInUser.Username, this);
        }
    }


    private async void ProfilePictureButton_Click(object sender, RoutedEventArgs e)
    {
        // Validate sender and get the top-level control.
        if (!(sender is Button sourceButton))
        {
            throw new ArgumentNullException(nameof(sourceButton), "The sender is not a Button.");
        }

        var topLevel = TopLevel.GetTopLevel(sourceButton);
        if (topLevel == null)
            throw new InvalidOperationException("The button is not attached to a top-level control.");

        // Set up file picker options.
        var options = new FilePickerOpenOptions
        {
            Title = "Select Patient Image",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        };

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        if (files == null || files.Count == 0)
            throw new OperationCanceledException("No file selected.");

        // Get the file path.
        string filePath = files[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(filePath))
            throw new InvalidOperationException("Unable to retrieve a valid file path.");

        // Read the image file as bytes.
        byte[] imageBytes;
        try
        {
            imageBytes = await Task.Run(() => File.ReadAllBytes(filePath));
        }
        catch (Exception ex)
        {
            throw new IOException("Error reading the image file.", ex);
        }

        // Update the database with the new profile picture.
        var dbManager = new DatabaseManager();
        dbManager.UpdateProfilePicture(SessionManager.LoggedInUser.Username, imageBytes);

        // Update the in-memory user with the new image.
        SessionManager.LoggedInUser.DoctorInformation.ProfilePicture = Convert.ToBase64String(imageBytes);

        // Refresh the UI.
        SetProfileImage();
    }


    public void SetProfileImage()
    {
        // Check if there's a valid profile picture string.
        var base64Image = SessionManager.LoggedInUser.DoctorInformation.ProfilePicture;
        if (string.IsNullOrWhiteSpace(base64Image))
        {
            // Optionally, set a default or placeholder image.
            ProfilePicture.Source = null;
            return;
        }

        try
        {
            // Convert the Base64 string back to a byte array.
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            // Create a memory stream from the byte array.
            using (var stream = new MemoryStream(imageBytes))
            {
                // Create a Bitmap from the stream.
                var bitmap = new Bitmap(stream);
                ProfilePicture.Source = bitmap;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error converting the image: " + ex.Message);
            // Handle conversion error (set a default image or leave the control blank).
            ProfilePicture.Source = null;
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetProfileImage();
    }

}