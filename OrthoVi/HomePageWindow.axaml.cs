using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.IO;
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
        if (SessionManager.LoggedInUser == null ||
            SessionManager.LoggedInUser.DoctorInformation.Clients.Count == 0)
        {
            return; 
        }

        int totalClients = SessionManager.LoggedInUser.DoctorInformation.Clients.Count;

        int startIndex = Math.Max(0, totalClients - 4);

        for (int i = totalClients - 1; i >= startIndex; i--)
        {
            int clientIndex = i;
            var stackPanel = new StackPanel
            {
                Spacing = 5
            };

            var button = new Button
            {
                Width = 116,
                Height = 108,
                Tag = clientIndex
            };

            button.Classes.Add("ImageButtonAnimation");

            button.Click += ViewPatientButton_Click;

            Bitmap bitmap = null;
            var clientImages = SessionManager.LoggedInUser.DoctorInformation.Clients[clientIndex].Image;

            if (clientImages != null && clientImages.Count > 3 && clientImages[3] != null)
            {
                // Load the client's image from the stored image bytes.
                var clientImage = clientImages[3];
                using (var stream = new MemoryStream(clientImage.ImageContent))
                {
                    bitmap = new Bitmap(stream);
                }
            }

            var image = new Avalonia.Controls.Image
            {
                Source=bitmap,
                Stretch = Avalonia.Media.Stretch.Uniform
            };

            button.Content = image;

            var textBlock = new TextBlock
            {
                Text = SetPatientInformation_Name(i),
                TextAlignment = TextAlignment.Center,
                FontSize = 20
            };

            stackPanel.Children.Add(button);
            stackPanel.Children.Add(textBlock);

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
        if (sender is Button btn && btn.Tag is int clientIndex)
        {
            ViewpatientWindow vpw = new ViewpatientWindow();
            vpw.ClientIndexTextBlock.Text = $"{clientIndex}";
            vpw.Show();
            this.Hide();
        }
    }

}