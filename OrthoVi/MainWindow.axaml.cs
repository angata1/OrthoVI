using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace OrthoVi
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
           InitializeComponent();
           
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }


    }
}