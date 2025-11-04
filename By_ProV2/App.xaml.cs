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
using System.Collections.Generic; // Liste ile çalışırken
using By_ProV2.Helpers;

namespace By_ProV2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Kültür ayarları
            CultureInfo culture = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Veritabanı başlatma
            DatabaseInitializer.InitializeDatabase();

            // Font resolver ayarı
            GlobalFontSettings.FontResolver = CustomFontResolver.Instance;

            base.OnStartup(e);
        }
    }

}
