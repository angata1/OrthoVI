using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using static OrthoVi.MainWindow;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Avalonia.Platform.Storage;
using System.Net;

namespace OrthoVi;

public partial class CreateNewPatientWindow : Window
{
    public CreateNewPatientWindow()
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
        this.Hide();
    }

    public async void CreatePatientButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Retrieve user input
            string patientFirstName = PatientFirstNameTextBox.Text.Trim();
            string patientMiddleName = PatientMiddleNameTextBox.Text.Trim(); // Unused, include if necessary
            string patientLastName = PatientLastNameTextBox.Text.Trim();
            int patientAge = int.Parse(PatientAgeTextBox.Text.Trim());
            string patientGender = PatientFemaleToggle.IsChecked == true ? "Female" : "Male";

            // Validate inputs (optional but recommended)
            if (string.IsNullOrEmpty(patientFirstName) || string.IsNullOrEmpty(patientLastName))
            {
                var box1 = MessageBoxManager
               .GetMessageBoxStandard("Input Error", "First and Last Name are required.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);

                var result1 = await box1.ShowWindowAsync();
                return;
            }

            DatabaseManager dbManager = new DatabaseManager();

            if (SessionManager.LoggedInUser != null)
            {
                // Create and add a new client
                ClientInformation newClient = new ClientInformation
                {
                    ClientFirstName = patientFirstName,
                    ClientMiddleName = patientMiddleName,
                    ClientLastName = patientLastName,
                    Gender = patientGender,
                    ClientAge = patientAge,
                    Image = images
                };

                SessionManager.LoggedInUser.DoctorInformation.Clients.Add(newClient);
                dbManager.UpdateDatabase(SessionManager.LoggedInUser.Username, SessionManager.LoggedInUser);


                // Notify the user
                var box2 = MessageBoxManager
                .GetMessageBoxStandard("Creation Successful", "Patient was created!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);

                var result2 = await box2.ShowWindowAsync();
            }

            
           
        }
        catch (Exception ex)
        {
            // Handle errors gracefully
            var box3 = MessageBoxManager
               .GetMessageBoxStandard("Error", $"An error occurred: {ex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);

            var result3 = await box3.ShowWindowAsync();

        }
    }

    public List<Image> images { get; set; }

    public async Task<Image> AddImageAsync(Button sourceButton)
    {
        // Get the top-level control (typically the Window) from the button.
        var topLevel = TopLevel.GetTopLevel(sourceButton);
        if (topLevel == null)
            throw new InvalidOperationException("Unable to determine top-level window.");

        // Set up file picker options.
        var filePickerOptions = new FilePickerOpenOptions
        {
            Title = "Select Patient Image",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageJpg, FilePickerFileTypes.ImagePng }
        };

        // Open the file picker dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(filePickerOptions);
        if (files == null || files.Count == 0)
            throw new OperationCanceledException("No file was selected.");

        // Get the first selected file's local path.
        string filePath = files[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(filePath))
            throw new InvalidOperationException("Could not retrieve a valid file path.");

        // Read the file into a byte array.
        byte[] imageBytes = await Task.Run(() => File.ReadAllBytes(filePath));

        // Use the x:Name of the button as the image name.
        string imageName = sourceButton.Name;

        // Create a new Image object (using your provided definition).
        var newImage = new Image
        {
            // ImageId is assumed to be set by the database or ORM later.
            ImageName = imageName,
            ImageContent = imageBytes,
            Annotation = new List<ImageAnnotation>()
        };

        // Add the new image to the public images list.
        images.Add(newImage);

        // (Optional) If later you assign the client's image list and then want to empty this list,
        // you can do so after this method returns.
        return newImage;
    }

    private async void OnAddImageButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            // Cast sender to Button.
            var button = sender as Button;
            if (button == null)
                return;

            Image addedImage = await AddImageAsync(button);
            // Further processing can be done with addedImage.
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., show a message box).
        }
    }

}