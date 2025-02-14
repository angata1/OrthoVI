using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using static OrthoVi.MainWindow;

namespace OrthoVi;

public partial class TrayWindow : Window
{
    public static TrayWindow Instance;
    public TrayWindow()
    {
        InitializeComponent();
        Instance = this;
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
        
    }

    public  static void DeleteUser(string username, SettingsWindow settingsWindow)
    {
        settingsWindow.Close();
        var dbManager = new DatabaseManager();

        try
        {
            dbManager.DeleteDatabase(SessionManager.LoggedInUser.Username);
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
        catch (System.Exception)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Error", "Could not delete account!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
        }
        
    }
}