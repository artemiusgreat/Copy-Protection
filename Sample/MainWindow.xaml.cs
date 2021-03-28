using CopyProtection;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Sample
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      try
      {
        Protector.CheckDrive();
      }
      catch (Exception e)
      {
        ShowError(e.Message);
      }
    }

    /// <summary>
    /// Show popup with error message
    /// </summary>
    /// <param name="message"></param>
    private void ShowError(string message)
    {
      var popup = new Window
      {
        Title = "Error",
        Content = new Border
        {
          Padding = new Thickness(15, 15, 15, 15),
          Child = new TextBlock
          {
            Text = message
          }
        }
      };

      popup.Show();
    }
  }
}
