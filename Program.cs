using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SCTMS.Data;
using SCTMS.Forms;
using SCTMS.Services;

namespace SCTMS
{
    internal static class Program
    {
        public static ServiceProvider? ServiceProvider { get; private set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            try
            {
                // Build configuration
                var configuration = BuildConfiguration();
                
                // Setup dependency injection
                ServiceProvider = ConfigureServices(configuration);

                // Test database connection before starting
                var dbHelper = ServiceProvider.GetRequiredService<DatabaseHelper>();
                if (!dbHelper.TestConnectionAsync().Result)
                {
                    MessageBox.Show("Unable to connect to the database. Please check your connection string and ensure SQL Server is running.",
                        "Database Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Start main application form
                var mainForm = ServiceProvider.GetRequiredService<MainForm>();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while starting the application:\n\n{ex.Message}",
                    "Application Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ServiceProvider?.Dispose();
            }
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private static ServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            // Add configuration
            services.AddSingleton(configuration);

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Add database services
            services.AddSingleton<DatabaseHelper>();
            services.AddSingleton<UserRepository>();
            services.AddSingleton<TrainingAssignmentRepository>();

            // Add business services
            services.AddSingleton<WindowsAuthService>();
            services.AddSingleton<NotificationService>();
            services.AddSingleton<TrainingService>();
            services.AddSingleton<ReportingService>();
            services.AddSingleton<ComplianceService>();

            // Add forms
            services.AddTransient<MainForm>();
            services.AddTransient<LoginForm>();
            services.AddTransient<UserManagementForm>();
            services.AddTransient<TrainingManagementForm>();
            services.AddTransient<ComplianceDashboardForm>();
            services.AddTransient<ReportsForm>();
            services.AddTransient<NotificationForm>();

            return services.BuildServiceProvider();
        }
    }
} 