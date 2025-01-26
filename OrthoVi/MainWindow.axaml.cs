using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Text;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;

namespace OrthoVi
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            var draggableArea = this.FindControl<Border>("DraggableArea");
            draggableArea.PointerPressed += DraggableArea_PointerPressed;
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

        private void DraggableArea_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                // Begin dragging the window only if clicked within the draggable area
                BeginMoveDrag(e);
            }
        }
        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            var dbManager = new DatabaseManager();
            string logInUsername = UsernameLogInTextBox.Text;
            string logInPassword = PasswordLogInTextBox.Text;
            if (logInUsername != null && logInPassword != null)
            {
                var user = dbManager.ReadDatabase(logInUsername, logInPassword);
                if (user != null)
                {
                    // Show the message box dialog
                     var box = MessageBoxManager
                    .GetMessageBoxStandard("Log In Successful", "You successfully logged in your account!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);

                     var result = await box.ShowWindowAsync();

                     HomePageWindow homeWindow = new HomePageWindow();
                     homeWindow.Show();
                     this.Hide();
                }
                else
                {
                    var box = MessageBoxManager
                    .GetMessageBoxStandard("Error", "The username or password is wrong", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);

                    var result = await box.ShowWindowAsync();
                }
               
            }
            else
            {
                var box = MessageBoxManager
                .GetMessageBoxStandard("Error", "Required fields are empty. Please enter all details to continue.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);

                var result = await box.ShowWindowAsync();
            }
           
        }
        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            var dbManager = new DatabaseManager();

            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Text;
            string doctorFirstName = FistNameTextBox.Text;
            string doctorLastName = LastNameTextBox.Text;
            byte[] profilePicture = Encoding.UTF8.GetBytes("ProfilePicturePlaceholder");

            if (username != null && password != null && doctorFirstName != null && doctorLastName != null)
            {
                dbManager.CreateDatabase(username, password, doctorFirstName, doctorLastName, profilePicture);

                // Show the message box dialog
                var box = MessageBoxManager
                .GetMessageBoxStandard("Creation Successful", "Account was created!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);

                 var result = await box.ShowWindowAsync(); 
            }
            else
            {
                var box = MessageBoxManager
                .GetMessageBoxStandard("Error", "Required fields are empty. Please enter all details to continue.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);

                var result = await box.ShowWindowAsync();
            }


        }



    }
}