using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using static OrthoVi.MainWindow;

namespace OrthoVi;

public partial class PatientListWindow : Window
{
    public PatientListWindow()
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

    private void ViewPatientButton_Click(object sender, RoutedEventArgs e)
    {
        ViewpatientWindow vpw = new ViewpatientWindow();
        vpw.Show();
        this.Hide();
    }


    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CreateButton();
    }

    public int clientNumber;

    private void CreateButton()
    {
        if (SessionManager.LoggedInUser != null && SessionManager.LoggedInUser.DoctorInformation.Clients.Count > 0)
        {
            for (int i = 0; i < SessionManager.LoggedInUser.DoctorInformation.Clients.Count; i++)
            {
                clientNumber = i;
                // Create Button
                var button = new Button
                {
                    Width = 740,
                    Background = Brushes.Transparent,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                button.Click += ViewPatientButton_Click;

                // Create main StackPanel (horizontal layout)
                var mainStackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 50
                };

                // Create Border containing Image
                var imageBorder = new Border
                {
                    Width = 81,
                    Height = 81,
                    Background = new SolidColorBrush(Color.Parse("#FF00B4D8"))
                };

                var image = new Avalonia.Controls.Image
                {
                    Stretch = Avalonia.Media.Stretch.Uniform,
                    Width = 65
                };

                imageBorder.Child = image;

                // Create Vertical StackPanel for TextBlocks
                var textStackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                var patientFullNameTB1 = new TextBlock
                {
                    Name = "PatientFullNameTB1",
                    Text = SetPatientInformation_Name(i),
                    FontSize = 25,
                    FontWeight = FontWeight.Bold
                };

                var patientAgeAndGenderTB1 = new TextBlock
                {
                    Name = "PatientAgeAndGenderTB1",
                    Text = SetPatientInformation_AgeGender(i),
                    FontSize = 25
                };

                PatientsListStackPanel.Children.Add(button);

                // Add TextBlocks to textStackPanel
                textStackPanel.Children.Add(patientFullNameTB1);
                textStackPanel.Children.Add(patientAgeAndGenderTB1);

                // Add Border and textStackPanel to mainStackPanel
                mainStackPanel.Children.Add(imageBorder);
                mainStackPanel.Children.Add(textStackPanel);

                // Set StackPanel as Button's content
                button.Content = mainStackPanel;
            }
        }
        
    }

    
    public string SetPatientInformation_Name(int i)
    {   
        string patientFullName= "";

        if (SessionManager.LoggedInUser != null && SessionManager.LoggedInUser.DoctorInformation.Clients.Count > 0)
        {

            var client = SessionManager.LoggedInUser.DoctorInformation.Clients[i];
            
            if (client != null)
            {
                return patientFullName = $"{client.ClientFirstName} {client.ClientMiddleName} {client.ClientLastName}";
               
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

    public string SetPatientInformation_AgeGender(int i)
    {
        string patientFullName = "";

        if (SessionManager.LoggedInUser != null && SessionManager.LoggedInUser.DoctorInformation.Clients.Count > 0)
        {

            var client = SessionManager.LoggedInUser.DoctorInformation.Clients[i];

            if (client != null)
            {
                return patientFullName = $"{client.ClientAge} - {client.Gender}";

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

}
