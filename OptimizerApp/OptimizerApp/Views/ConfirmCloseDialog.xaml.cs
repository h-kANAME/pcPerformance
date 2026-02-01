using System.Windows;
using OptimizerApp.Models;

namespace OptimizerApp.Views;

public partial class ConfirmCloseDialog : Window
{
    public CloseDecision Decision { get; private set; } = CloseDecision.Skip;

    public ConfirmCloseDialog(string title, string message)
    {
        InitializeComponent();
        Title = title;
        MessageText.Text = message;
    }

    public static CloseDecision ShowDialog(Window owner, string title, string message)
    {
        var dialog = new ConfirmCloseDialog(title, message)
        {
            Owner = owner
        };
        dialog.ShowDialog();
        return dialog.Decision;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Decision = CloseDecision.Close;
        Close();
    }

    private void Skip_Click(object sender, RoutedEventArgs e)
    {
        Decision = CloseDecision.Skip;
        Close();
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        Decision = CloseDecision.Stop;
        Close();
    }
}
