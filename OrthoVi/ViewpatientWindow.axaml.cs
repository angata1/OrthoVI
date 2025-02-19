using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using static OrthoVi.TrayWindow;
using Avalonia.Media.Imaging;
using System.IO;
using Avalonia.VisualTree;
using System.Linq;
using System.Collections.Generic;

namespace OrthoVi;

public partial class ViewpatientWindow : Window
{
    public static ViewpatientWindow? vpw { get; set; }
    public static int CliendIndex;

    public ViewpatientWindow()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

        // Assign the current instance to the static property
        vpw = this;

        // Set up draggable area
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
        homeWindow.InvalidateMeasure();
        homeWindow.InvalidateVisual();
        homeWindow.Show();
        this.Close();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = new SettingsWindow();
        settingsWindow.Show();
        this.Close();
    }

    private void CephStepsWindow_Click(object sender, RoutedEventArgs e)
    {
        CephStepsWindow cph = new CephStepsWindow();
        cph.Show();
        this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetPatientInfo();
        SetProfileImage();
        AssignImagesToElements();
    }

    private async void DeletePatientButton_Click(object sender, RoutedEventArgs e)
    {
        var dbManager = new DatabaseManager();
        int clientIndex = ViewpatientWindow.CliendIndex;
        dbManager.ReadDatabase(SessionManager.LoggedInUser.Username, SessionManager.LoggedInUser.Password);
        var clientToDelete = SessionManager.LoggedInUser.DoctorInformation.Clients[clientIndex];

        if (clientToDelete != null)
        {
            dbManager.DeleteClient(SessionManager.LoggedInUser.Username, SessionManager.LoggedInUser.Password, clientToDelete.ClientInformationId);
            
            var box = MessageBoxManager
                    .GetMessageBoxStandard("Deletion Successful", "You successfully deleted the patient!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);

            var result = await box.ShowWindowAsync();
        }
        else
        {
            var box = MessageBoxManager
                   .GetMessageBoxStandard("Error", "Patient not found", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);

            var result = await box.ShowWindowAsync();
        }
    }

    private void SetPatientInfo()
    {
        if (SessionManager.LoggedInUser?.DoctorInformation != null)
        {
            PatientListWindow patientListWindow = new PatientListWindow();
            int clientIndex =ViewpatientWindow.CliendIndex;
            string patientName = patientListWindow.SetPatientInformation_Name(clientIndex);
            string patientAgeGender = patientListWindow.SetPatientInformation_AgeGender(clientIndex);
            PatientNameTB.Text= patientName;
            PatientAgeGenderTB.Text= patientAgeGender;
        }
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

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button clickedButton)
        {
            // Access the button's x:Name property.
            string buttonName = clickedButton.Name;

            // Use the buttonName as needed.
            Console.WriteLine($"Button clicked: {buttonName}");
            // Add any other logic that depends on the button's name here.
        }
    }

    private void AssignImagesToElements()
    {
        var patientImagesPanel = this.FindControl<StackPanel>("PatientImagesSP");
        if (patientImagesPanel == null)
        {
            // If the panel is not found, exit the method.
            return;
        }

        // Get only the Image elements that are descendants of the found StackPanel.
        var imageControls = GetAllDescendants(patientImagesPanel)
                            .OfType<Avalonia.Controls.Image>()
                            .Where(img => !string.IsNullOrEmpty(img.GetValue<string>(Control.NameProperty)));

        int clientIndex = ViewpatientWindow.CliendIndex;
        Bitmap bitmap = null;
        var clientImages = SessionManager.LoggedInUser.DoctorInformation.Clients[clientIndex].Images;

        foreach (var image in imageControls)
        {
            string name = image.GetValue<string>(Control.NameProperty);
            if (!string.IsNullOrEmpty(name))
            {
                var clientImage = clientImages.FirstOrDefault(img => img.ImageName == name);
                if (clientImage == null)
                {
                    image.Source = null;
                }
                else
                {
                    using (var stream = new MemoryStream(clientImage.ImageContent))
                    {
                        image.Source = new Bitmap(stream);
                    }
                }
            }
        }
    }

    // Helper method to recursively get all visual children
    private static IEnumerable<Visual> GetAllDescendants(Visual parent)
    {
        foreach (var child in parent.GetVisualChildren())
        {
            yield return child;
            foreach (var descendant in GetAllDescendants(child))
            {
                yield return descendant;
            }
        }
    }

}
