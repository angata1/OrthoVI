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
using Avalonia.Media.Imaging;

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

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = new SettingsWindow();
        settingsWindow.Show();
        this.Hide();
    }

    private List<ImageData> images = new List<ImageData>();

    public async void CreatePatientButton_Click(object sender, RoutedEventArgs e)
    {
        if (!(images.Count == 12))
        {
            var box2 = MessageBoxManager
                    .GetMessageBoxStandard("Important!", "There are missing images!" + " Are you sure you want to continue?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var result = await box2.ShowWindowAsync();

            if (result == ButtonResult.Yes)
            {
                CreatePatient();
            }
        }
        else
        {
            CreatePatient();
        }
    }

    public async void CreatePatient()
    {
        try
        {
            string patientFirstName = PatientFirstNameTextBox.Text.Trim();
            string patientMiddleName = PatientMiddleNameTextBox.Text.Trim();
            string patientLastName = PatientLastNameTextBox.Text.Trim();
            int patientAge = int.Parse(PatientAgeTextBox.Text.Trim());
            string patientGender = PatientFemaleToggle.IsChecked == true ? "Female" : "Male";

            if (string.IsNullOrEmpty(patientFirstName) || string.IsNullOrEmpty(patientLastName))
            {
                var box1 = MessageBoxManager
                   .GetMessageBoxStandard("Input Error", "First and Last Name are required.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);

                await box1.ShowWindowAsync();
                return;
            }

            DatabaseManager dbManager = new DatabaseManager();

            if (SessionManager.LoggedInUser != null)
            {
                ClientInformation newClient = new ClientInformation
                {
                    ClientFirstName = patientFirstName,
                    ClientMiddleName = patientMiddleName,
                    ClientLastName = patientLastName,
                    Gender = patientGender,
                    ClientAge = patientAge,

                    // Convert List<ImageData> to List<Image>
                    Image = images.ConvertAll(img => new Image
                    {
                        ImageName = img.ImageName,
                        ImageContent = img.ImageContent,
                        Annotation = new List<ImageAnnotation>() // Initialize empty list if needed
                    })
                };

                SessionManager.LoggedInUser.DoctorInformation.Clients.Add(newClient);
                dbManager.UpdateDatabase(SessionManager.LoggedInUser.Username, SessionManager.LoggedInUser);

                var box2 = MessageBoxManager
                    .GetMessageBoxStandard("Creation Successful", "Patient was created!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                await box2.ShowWindowAsync();

                images.Clear(); // Clear the temporary images list

                PatientListWindow plw = new PatientListWindow();
                plw.Show();
                this.Hide();
            }
        }
        catch (Exception ex)
        {
            var box3 = MessageBoxManager
                .GetMessageBoxStandard("Error", $"An error occurred: {ex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box3.ShowWindowAsync();
        }
    }

    public class ImageData
    {
        public int ImageId { get; set; }
        public string ImageName { get; set; }
        public byte[] ImageContent { get; set; }
    }

    public async Task<ImageData> AddImageAsync(Button sourceButton)
    {
        if (sourceButton == null)
            throw new ArgumentNullException(nameof(sourceButton));

        var topLevel = TopLevel.GetTopLevel(sourceButton);
        if (topLevel == null)
            throw new InvalidOperationException("The button is not attached to a top-level control.");

        var options = new FilePickerOpenOptions
        {
            Title = "Select Patient Image",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll}
        };

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        if (files == null || files.Count == 0)
            throw new OperationCanceledException("No file selected.");

        string filePath = files[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(filePath))
            throw new InvalidOperationException("Unable to retrieve a valid file path.");

        byte[] imageBytes;
        try
        {
            imageBytes = await Task.Run(() => File.ReadAllBytes(filePath));
        }
        catch (Exception ex)
        {
            throw new IOException("Error reading the image file.", ex);
        }

        string imageName = sourceButton.Name ?? "UnnamedImage";

        var newImage = new ImageData
        {
            ImageName = imageName,
            ImageContent = imageBytes
        };

        images.Add(newImage); // Now images is accessible and properly referenced

        return newImage;
    }

    public async Task OnAddImageButtonClickAsync(Button sourceButton)
    {
        try
        {
            var newImage = await AddImageAsync(sourceButton);

            Bitmap bitmap;
            using (var ms = new MemoryStream(newImage.ImageContent))
            {
                bitmap = new Bitmap(ms);
            }

            var imageControl = new Avalonia.Controls.Image
            {
                Source = bitmap,
                Width = 50,
                Height = 50,
                Stretch = Avalonia.Media.Stretch.Uniform
            };

            sourceButton.Content = imageControl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async void AddImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            await OnAddImageButtonClickAsync(button);
        }
    }


}