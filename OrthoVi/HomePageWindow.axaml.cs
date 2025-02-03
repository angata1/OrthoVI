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
        CreateRecentPatientsButtons();
    }

    private string SetPatientInformation_Name(int i)
    {
        string patientFullName = "";

        if (SessionManager.LoggedInUser != null && SessionManager.LoggedInUser.DoctorInformation.Clients.Count > 0)
        {

            var client = SessionManager.LoggedInUser.DoctorInformation.Clients[i];

            if (client != null)
            {
                return patientFullName = $"{client.ClientFirstName} {client.ClientLastName}";

            }
            else
            {
                return patientFullName = "Client information not available";
            }
        }
        else
        {
            return patientFullName = "Client not found";
        }
    }

    private void CreateRecentPatientsButtons()
    {
        int j;
        if (SessionManager.LoggedInUser != null && SessionManager.LoggedInUser.DoctorInformation.Clients.Count > 0)
        {
            switch (SessionManager.LoggedInUser.DoctorInformation.Clients.Count)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    j = SessionManager.LoggedInUser.DoctorInformation.Clients.Count; 
                    break;

                case 4:
                default:
                    j = 4;
                    break;
            }
        }
        else { j = 0; }

        for (int i = 0; i < j; i++) 
        {
            // Create StackPanel
            var stackPanel = new StackPanel
            {
                Spacing = 5
            };

            // Create Button
            var button = new Button
            {
                Width = 116,
                Height = 108
            };

            button.Classes.Add("ImageButtonAnimation");

            // Attach Click Event
            button.Click += ViewPatientButton_Click;

            // Create Image
            var image = new Avalonia.Controls.Image
            {
               
                Stretch = Avalonia.Media.Stretch.Uniform
            };

            // Set Image as Button Content
            button.Content = image;

            // Create TextBlock
            var textBlock = new TextBlock
            {
                Text = SetPatientInformation_Name(i),
                FontSize = 20
            };

            // Add Button and TextBlock to StackPanel
            stackPanel.Children.Add(button);
            stackPanel.Children.Add(textBlock);

            // Add StackPanel to Parent Container (example: a Grid or another Panel)
            MainStackPanel.Children.Add(stackPanel);

        }
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