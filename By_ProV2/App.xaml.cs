using System.Globalization;
using System.Threading;
using System.Windows;
using PdfSharp.Fonts;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System.Diagnostics; // PDF'yi oluşturduktan sonra açmak için (Process.Start)
using System.IO;           // Dosya işlemleri
using System.Linq;         // Toplam ve liste işlemleri
using System.Collections.Generic; // Liste ile çalışken
using By_ProV2.Helpers;
using By_ProV2.Services;

namespace By_ProV2
{
    public partial class App : Application
    {
        public static AuthenticationService AuthService { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Kültür ayarları
            CultureInfo culture = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Set shutdown mode to explicit so the app doesn't close when windows close
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Veritabanı başlatma - Initialize database first to ensure Users table exists
            DatabaseInitializer.InitializeDatabase();

            // Handle daily inventory logic on startup
            try
            {
                var envanterService = new SutEnvanteriService();
                envanterService.HandleDayChange();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Envanter güncellenirken bir hata oluştu: {ex.Message}", "Envanter Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Initialize authentication service
            AuthService = new AuthenticationService();
            
            // Show login window first if no users exist
            if (!AuthService.HasUsers())
            {
                // Show first-time setup for admin user
                var setupWindow = new FirstTimeSetupWindow();
                if (setupWindow.ShowDialog() != true)
                {
                    Shutdown(); // Close app if setup cancelled
                    return;
                }
            }
            
            // Show regular login window
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() != true)
            {
                Shutdown(); // Close app if login cancelled
                return;
            }

            // Font resolver ayarı
            GlobalFontSettings.FontResolver = CustomFontResolver.Instance;

            // Show the main window after successful authentication
            try
            {
                var mainWindow = new MainWindow();
                this.MainWindow = mainWindow;  // Set this as the main window of the application
                mainWindow.Show();
                
                // Now change shutdown mode to close when main window closes
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ana pencere açılırken hata oluştu: {ex.Message}\n\n{ex.InnerException?.Message}", 
                                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // Call the base OnStartup after setting up the main window
            base.OnStartup(e);
        }
    }

}
