using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Linq;
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
        this.Close();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = new SettingsWindow();
        settingsWindow.Show();
        this.Close();
    }


    
    private void ViewPatientButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int clientIndex)
        { 
            ViewpatientWindow.CliendIndex = clientIndex;
            ViewpatientWindow.vpw = new ViewpatientWindow();
            ViewpatientWindow.vpw.Show();
            ViewpatientWindow.CliendIndex = clientIndex;
            this.Close();
        }
    }


    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CreateButton();
        SetProfileImage();
    }


    private void CreateButton()
    {
        if (SessionManager.LoggedInUser != null &&
            SessionManager.LoggedInUser.DoctorInformation.Clients.Count > 0)
        {
            for (int i = 0; i < SessionManager.LoggedInUser.DoctorInformation.Clients.Count; i++)
            {
                // Capture the current index
                int clientIndex = i;

                // Create Button and store the index in its Tag property.
                var button = new Button
                {
                    Width = 740,
                    Background = Brushes.Transparent,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Tag = clientIndex
                };

                button.Click += ViewPatientButton_Click;

                // Create and set up the rest of your UI elements (StackPanels, Borders, etc.)
                var mainStackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 50
                };

                var imageBorder = new Border
                {
                    Width = 81,
                    Height = 81,
                    Background = new SolidColorBrush(Color.Parse("#FF00B4D8"))
                };

                Bitmap bitmap = null;
                var clientImages = SessionManager.LoggedInUser.DoctorInformation.Clients[clientIndex].Images;

                if (clientImages != null && clientImages.Any())
                {
                    // Find the image with the name "FrontalPhoto"
                    var clientImage = clientImages.FirstOrDefault(img => img.ImageName == "FrontalPhoto");
                    if (clientImage != null)
                    {
                        // Load the client's image from the stored image bytes.
                        using (var stream = new MemoryStream(clientImage.ImageContent))
                        {
                            bitmap = new Bitmap(stream);
                        }
                    }
                }


                // Create the Avalonia Image control.
                var imageControl = new Avalonia.Controls.Image
                {
                    Source = bitmap,
                    Stretch = Avalonia.Media.Stretch.Uniform,
                    Width = 65,
                };

                imageBorder.Child = imageControl;

                var textStackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                var patientFullNameTB1 = new TextBlock
                {
                    Name = "PatientFullNameTB1",
                    Text = SetPatientInformation_Name(clientIndex),
                    FontSize = 25,
                    FontWeight = FontWeight.Bold
                };

                var patientAgeAndGenderTB1 = new TextBlock
                {
                    Name = "PatientAgeAndGenderTB1",
                    Text = SetPatientInformation_AgeGender(clientIndex),
                    FontSize = 25
                };

                textStackPanel.Children.Add(patientFullNameTB1);
                textStackPanel.Children.Add(patientAgeAndGenderTB1);

                mainStackPanel.Children.Add(imageBorder);
                mainStackPanel.Children.Add(textStackPanel);

                button.Content = mainStackPanel;
                PatientsListStackPanel.Children.Add(button);
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

}

