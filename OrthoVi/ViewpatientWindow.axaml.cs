using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using static OrthoVi.MainWindow;

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
        this.Hide();
    }
    private void CephStepsWindow_Click(object sender, RoutedEventArgs e)
    {
        CephStepsWindow cph = new CephStepsWindow();
        cph.Show();
        this.Hide();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetPatientInfo();
    }

    private async void DeletePatientButton_Click(object sender, RoutedEventArgs e)
    {
        var dbManager = new DatabaseManager();
        int clientIndex = int.Parse(ClientIndexTextBlock.Text);
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
            int clientIndex =int.Parse(ClientIndexTextBlock.Text);
            string patientName = patientListWindow.SetPatientInformation_Name(clientIndex);
            string patientAgeGender = patientListWindow.SetPatientInformation_AgeGender(clientIndex);
            PatientNameTB.Text= patientName;
            PatientAgeGenderTB.Text= patientAgeGender;
        }
    }
}
