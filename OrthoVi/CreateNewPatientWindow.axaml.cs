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

    private async void CreatePatientButton_Click(object sender, RoutedEventArgs e)
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

            // Create the new ClientInformation object
            ClientInformation newClient = new ClientInformation
            {
                ClientFirstName = patientFirstName,
                ClientLastName = patientLastName,
                Gender = patientGender,
                ClientAge = patientAge
            };

            // Call the database manager to add the new client
            DatabaseManager dbManager = new DatabaseManager();
            await Task.Run(() =>
            {
                dbManager.AddNewClient(SessionManager.LoggedInUser.Username, newClient);
            });

            // Notify the user
            var box2 = MessageBoxManager
                .GetMessageBoxStandard("Creation Successful", "Patient was created!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);

            var result2 = await box2.ShowWindowAsync();
        }
        catch (Exception ex)
        {
            // Handle errors gracefully
            var box3 = MessageBoxManager
               .GetMessageBoxStandard("Error", $"An error occurred: {ex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);

            var result3 = await box3.ShowWindowAsync();

        }
    }

}