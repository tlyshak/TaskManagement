using System.Windows;
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
}