using System.Windows;
using System.Windows.Input;
using TaskManagement.UI.ViewModels;

namespace TaskManagement.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadCommand.ExecuteAsync(null);
    }

    private void OnDatePickerCalendarClosed(object sender, RoutedEventArgs e) => Keyboard.ClearFocus();
}