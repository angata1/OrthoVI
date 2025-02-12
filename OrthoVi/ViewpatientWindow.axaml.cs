using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using static OrthoVi.MainWindow;
using Avalonia.Media.Imaging;
using System.IO;

namespace OrthoVi;

public partial class ViewpatientWindow : Window
{
    public ViewpatientWindow()
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
    }

    private async void DeletePatientButton_Click(object sender, RoutedEventArgs e)
    {
        var dbManager = new DatabaseManager();
        int clientIndex = int.Parse(ClientIndexTextBlock.Text);
        SessionManager.LoggedInUser = dbManager.ReadDatabase(SessionManager.LoggedInUser.Username, SessionManager.LoggedInUser.Password);
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
            int clientIndex =int.Parse(ClientIndexTextBlock.Text);
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
}
