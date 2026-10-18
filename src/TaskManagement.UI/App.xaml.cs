using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Threading;
using TaskManagement.Data;
using TaskManagement.Data.Services;
using TaskManagement.UI.Services;
using TaskManagement.UI.ViewModels;

namespace TaskManagement.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        _services = ConfigureServices();
        _services.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton(new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseTaskManagementDatabase(ConnectionStrings.FromEnvironment())
            .Options);
        services.AddScoped<TaskManagementDbContext>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IEmployeeService, EmployeeService>();

        services.AddSingleton<ScopedServiceRunner>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
