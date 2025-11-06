using Microsoft.Data.SqlClient;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using By_ProV2.Reports;

namespace By_ProV2
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private bool indicatorVisible = true;
        private int tickCounter = 0;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                UpdateConnectionStatus();
                UpdateUserStatus();

                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MainWindow başlatılırken hata oluştu: {ex.Message}\n\n{ex.InnerException?.Message}", 
                                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Yeşil noktanın yanıp sönme efekti
            indicatorVisible = !indicatorVisible;
            ConnectionIndicator.Visibility = indicatorVisible ? Visibility.Visible : Visibility.Hidden;

            // Her 5 saniyede bir veritabanı bağlantısını test et
            tickCounter++;
            if (tickCounter >= 5)
            {
                tickCounter = 0;
                UpdateConnectionStatus();
            }
            
            // Always update user status
            UpdateUserStatus();
        }

        private void UpdateConnectionStatus()
        {
            if (TestDatabaseConnection())
            {
                DurumText.Text = "Durum: Bağlantı Hazır";
                DurumText.Foreground = new SolidColorBrush(Colors.Gray);
                ConnectionIndicator.Fill = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                DurumText.Text = "Durum: Bağlantı başarısız";
                DurumText.Foreground = new SolidColorBrush(Colors.Red);
                ConnectionIndicator.Fill = new SolidColorBrush(Colors.Red);
            }
        }

        private void UpdateUserStatus()
        {
            if (App.AuthService?.CurrentUser != null)
            {
                var currentUser = App.AuthService.CurrentUser;
                UserStatusText.Text = $"Kullanıcı: {currentUser.FullName ?? currentUser.Username} ({currentUser.Role})";
            }
            else
            {
                UserStatusText.Text = "Kullanıcı: Giriş yapılmadı";
            }
        }

        private bool TestDatabaseConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["db"].ConnectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void BtnDosya_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
                // Spacing yok, onun yerine butonlara Margin veriyoruz
            };

            var btnParametreler = new Button
            {
                Content = "⚙️ Parametreler",
                FontSize = 24,
                Width = 200,
                Height = 150,
                Margin = new Thickness(20, 0, 20, 0) // Butonlar arasında yatay boşluk
            };
            btnParametreler.Click += BtnParametreler_Click;

            var btnKullanicilar = new Button
            {
                Content = "👥 Kullanıcılar",
                FontSize = 24,
                Width = 200,
                Height = 150,
                Margin = new Thickness(20, 0, 20, 0)
            };
            btnKullanicilar.Click += BtnKullanicilar_Click;

            panel.Children.Add(btnParametreler);
            panel.Children.Add(btnKullanicilar);

            ContentArea.Children.Add(panel);
        }

        private void BtnParametreler_Click(object sender, RoutedEventArgs e)
        {
            ParametrelerWindow pencere = new ParametrelerWindow();
            pencere.ShowDialog(); // modal olarak aç
        }

        private void BtnKullanicilar_Click(object sender, RoutedEventArgs e)
        {
            if (App.AuthService?.CurrentUser?.Role != "Admin")
            {
                MessageBox.Show("Bu işlemi yapmaya yetkiniz yok!", 
                                "Yetki Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Open the User Management Window as a modal dialog
            var userManagementWindow = new UserManagementWindow();
            userManagementWindow.ShowDialog();
        }

        private void BtnCari_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();

            var panel = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            // Ortak buton stilleri
            double btnWidth = 200;
            double btnHeight = 150;
            Thickness btnMargin = new Thickness(20);

            var btnCariKayit = new Button
            {
                Content = "📝 Cari Kayıt",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnCariKayit.Click += BtnCariKayit_Click;

            var btnCariListesi = new Button
            {
                Content = "📋 Cari Listesi",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnCariListesi.Click += BtnCariListesi_Click;

            var btnCariHareket = new Button
            {
                Content = "📈 Cari Hareket",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnCariHareket.Click += BtnCariHareket_Click;

            var btnRaporlar = new Button
            {
                Content = "📊 Raporlar",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnRaporlar.Click += BtnRaporlar_Click;

            panel.Children.Add(btnCariKayit);
            panel.Children.Add(btnCariListesi);
            panel.Children.Add(btnCariHareket);
            panel.Children.Add(btnRaporlar);

            ContentArea.Children.Add(panel);
        }

        private void BtnCariKayit_Click(object sender, RoutedEventArgs e)
        {
            CariKayitWindow pencere = new CariKayitWindow();
            pencere.ShowDialog(); // modal olarak aç
        }

        private void BtnCariListesi_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();
            ContentArea.Children.Add(new TextBlock
            {
                Text = "Cari Listesi ekranı buraya gelecek.",
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        private void BtnCariHareket_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();
            ContentArea.Children.Add(new TextBlock
            {
                Text = "Cari Hareket ekranı buraya gelecek.",
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        private void BtnRaporlar_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();
            ContentArea.Children.Add(new TextBlock
            {
                Text = "Cari Raporlar ekranı buraya gelecek.",
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }


        private void BtnStok_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();

            var panel = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            double btnWidth = 200;
            double btnHeight = 150;
            Thickness btnMargin = new Thickness(20);

            var btnStokKayit = new Button
            {
                Content = "📦 Stok Kayıt",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnStokKayit.Click += BtnStokKayit_Click;

            var btnStokHareket = new Button
            {
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin,
                Content = "🔄 Stok Hareket",
                FontSize = 24,
            };
            btnStokHareket.Click += BtnStokHareket_Click;

            var btnStokRapor = new Button
            {
                Content = "📊 Raporlar",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnStokRapor.Click += BtnStokRapor_Click;

            panel.Children.Add(btnStokKayit);
            panel.Children.Add(btnStokHareket);
            panel.Children.Add(btnStokRapor);

            ContentArea.Children.Add(panel);
        }
        private void BtnStokKayit_Click(object sender, RoutedEventArgs e)
        {
            StokKayitWindow pencere = new StokKayitWindow();
            pencere.ShowDialog(); // modal olarak aç
        }
        private void BtnStokHareket_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();
            ContentArea.Children.Add(new TextBlock
            {
                Text = "Stok Hareket Listesi ekranı buraya gelecek.",
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }
        private void BtnStokRapor_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();
            ContentArea.Children.Add(new TextBlock
            {
                Text = "Raporlar ekranı buraya gelecek.",
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }
        private void BtnYem_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();

            var panel = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            double btnWidth = 200;
            double btnHeight = 150;
            Thickness btnMargin = new Thickness(20);

            var btnYemAlisSatis = new Button
            {
                Content = "🛒 Alış / Satış",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnYemAlisSatis.Click += BtnYemAlisSatis_Click;

            var btnEskiKayit = new Button
            {
                Content = "📝 Eski Kayıt",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnEskiKayit.Click += BtnEskiKayit_Click;

            var btnYemFiyatListeleri = new Button
            {
                Content = "💲 Fiyat Listeleri",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnYemFiyatListeleri.Click += BtnYemFiyatListeleri_Click;

            var btnYemRaporlar = new Button
            {
                Content = "📊 Raporlar",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnYemRaporlar.Click += BtnYemRaporlar_Click;

            panel.Children.Add(btnYemAlisSatis);
            panel.Children.Add(btnEskiKayit);
            panel.Children.Add(btnYemFiyatListeleri);
            panel.Children.Add(btnYemRaporlar);

            ContentArea.Children.Add(panel);
        }
        private void BtnYemAlisSatis_Click(object sender, RoutedEventArgs e)
        {
            SiparisFormu pencere = new SiparisFormu();
            pencere.ShowDialog(); // modal olarak aç
        }

        private void BtnEskiKayit_Click(object sender, RoutedEventArgs e)
        {
            BelgeSorgulama pencere = new BelgeSorgulama();
            pencere.ShowDialog(); // modal olarak aç
        }

        private void BtnYemFiyatListeleri_Click(object sender, RoutedEventArgs e)
        {
            FiyatGuncellemeApp pencere = new FiyatGuncellemeApp();
            pencere.ShowDialog(); // modal olarak aç
        }

        private void BtnYemRaporlar_Click(object sender, RoutedEventArgs e)
        {
            RaporSayfasi pencere = new RaporSayfasi();
            pencere.ShowDialog(); // modal olarak aç
        }

        private void BtnSut_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();

            var panel = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            double btnWidth = 220;
            double btnHeight = 150;
            Thickness btnMargin = new Thickness(20);

            var btnSutAlim = new Button
            {
                Content = "🥛 Süt Alım Formu",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };

            var sutAlimText = new TextBlock
            {
                Text = "🥛 Süt Alım Formu",
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(5)
            };
            btnSutAlim.Content = sutAlimText;
            btnSutAlim.Click += BtnSutAlim_Click;

            var btnSutDepoSevk = new Button
            {
                Content = "🚚 Depodan Sevk",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnSutDepoSevk.Click += BtnSutDepoSevk_Click;

            var btnSutDirekSevk = new Button
            {
                Content = "🔄 Direkt Sevk",
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            btnSutDirekSevk.Click += BtnSutDirekSevk_Click;

            var btnSutRapor = new Button
            {
               FontSize = 24,
               Width = btnWidth,
               Height = btnHeight,
               Margin = btnMargin
             };
            
             // Create a TextBlock with wrapping enabled
             var gunlukSutAlimText = new TextBlock
             {
                Text = "📊 Günlük Süt Alım Formu",
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(5)
             };
             btnSutRapor.Content = gunlukSutAlimText;
             btnSutRapor.Click += BtnSutRapor_Click; // Keep the original behavior

            // Add a new button for Sut Alim Sorgulama
            var btnSutSorgulama = new Button
            {
                FontSize = 24,
                Width = btnWidth,
                Height = btnHeight,
                Margin = btnMargin
            };
            
            // Create a TextBlock with wrapping enabled for Sut Alim Sorgulama
            var sutSorgulamaText = new TextBlock
            {
                Text = "🔍 Süt Alım Sorgulama",
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(5)
            };
            btnSutSorgulama.Content = sutSorgulamaText;
            btnSutSorgulama.Click += BtnSutAlimSorgulama_Click; // This will open the new search window

            panel.Children.Add(btnSutAlim);
            panel.Children.Add(btnSutDepoSevk);
            panel.Children.Add(btnSutDirekSevk);
            panel.Children.Add(btnSutRapor);
            panel.Children.Add(btnSutSorgulama); // Add the new button

            ContentArea.Children.Add(panel);
        }

        private void BtnSutRapor_Click(object sender, RoutedEventArgs e)
        {
            GunlukSutAlimPreview pencere = new GunlukSutAlimPreview();
            pencere.ShowDialog();
        }

        private void BtnSutAlim_Click(object sender, RoutedEventArgs e)
        {
            SutAlimFormu pencere = new SutAlimFormu();
            pencere.ShowDialog();
        }
        
        private void BtnSutAlimSorgulama_Click(object sender, RoutedEventArgs e)
        {
            SutAlimSorgulama pencere = new SutAlimSorgulama();
            pencere.ShowDialog();
        }
        
        private void BtnSutDepoSevk_Click(object sender, RoutedEventArgs e)
        {
            SutDepoSevkFormu pencere = new SutDepoSevkFormu();
            pencere.ShowDialog();
        }
        private void BtnSutDirekSevk_Click(object sender, RoutedEventArgs e)
        {
            SutDirekSevkFormu pencere = new SutDirekSevkFormu();
            pencere.ShowDialog();
        }
        private void BtnRapor_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Children.Clear();
            ContentArea.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "📊 Raporlar",
                FontSize = 28,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }

        private void BtnAuditTrail_Click(object sender, RoutedEventArgs e)
        {
            // Only allow admin users to see audit trail
            if (App.AuthService?.CurrentUser?.Role != "Admin")
            {
                MessageBox.Show("Bu işlemi yapmaya yetkiniz yok!", 
                                "Yetki Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var auditTrailWindow = new AuditTrailWindow();
            auditTrailWindow.ShowDialog();
        }

        private void BtnCikis_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Uygulamadan çıkmak istediğinizden emin misiniz?", "Çıkış", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Logout the current user before shutting down
                App.AuthService?.Logout();
                Application.Current.Shutdown();
            }
        }
    }
}