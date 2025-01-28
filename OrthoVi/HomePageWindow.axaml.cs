using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using static OrthoVi.MainWindow;

namespace OrthoVi;

public partial class HomePageWindow : Window
{
    public HomePageWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        var draggableArea = this.FindControl<Border>("DraggableArea");
        draggableArea.PointerPressed += DraggableArea_PointerPressed;

    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetWelcomingHeader();
    }

    private void SetWelcomingHeader()
    {
        if (SessionManager.LoggedInUser?.DoctorInformation != null)
        {
            string header = $"WELCOME, DR. {SessionManager.LoggedInUser.DoctorInformation.Lastname.ToUpper()}";
            WelcomeHeaderTextBlock.Text = header;
        }
        else
        {
            WelcomeHeaderTextBlock.Text = "WELCOME!";
        }
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

    private void CreateNewPatientButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
       CreateNewPatientWindow cnpw = new CreateNewPatientWindow();
        cnpw.Show();
        this.Hide();
    }
    private void PatientListButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PatientListWindow plw = new PatientListWindow();
        plw.Show();
        this.Hide();
    }
    private void ViewPatientButton_Click(object sender, RoutedEventArgs e)
    {
        ViewpatientWindow vpw = new ViewpatientWindow();
        vpw.Show();
        this.Hide();
    }

}