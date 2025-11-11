using AlgoTrading.Application;
using AlgoTrading.Desktop.ViewModels;
using AlgoTrading.Desktop.ViewModels.Strategy;
using AlgoTrading.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Windows;

namespace AlgoTrading.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/desktop-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            // Build configuration
            var builder = Host.CreateApplicationBuilder();

            // Add configuration
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Add logging
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(Log.Logger);
            });

            // Add Application and Infrastructure layers
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            // Register ViewModels
            builder.Services.AddTransient<StrategyListViewModel>();
            builder.Services.AddTransient<StrategyEditorViewModel>();
            builder.Services.AddTransient<BacktestViewModel>();
            builder.Services.AddTransient<StrategyMonitorViewModel>();

            // Build host
            _host = builder.Build();

            // Set ServiceProvider
            ServiceProvider = _host.Services;

            Log.Information("AlgoTrading Desktop application started");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            MessageBox.Show($"Application startup failed: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            Log.Information("AlgoTrading Desktop application shutting down");

            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            Log.CloseAndFlush();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during shutdown: {ex.Message}", "Shutdown Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        base.OnExit(e);
    }
}

