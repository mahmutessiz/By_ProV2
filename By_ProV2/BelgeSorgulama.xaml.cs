using System;
using System.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace By_ProV2
{
    public partial class BelgeSorgulama : Window
    {
        public BelgeSorgulama()
        {
            InitializeComponent();
            txtBelgeKodu.KeyDown += TxtBelgeKodu_KeyDown;
            btnSorgula.Click += btnSorgula_Click;
        }

        private async void btnSorgula_Click(object sender, RoutedEventArgs e)
        {
            string belgeKodu = txtBelgeKodu.Text.Trim();

            if (string.IsNullOrWhiteSpace(belgeKodu))
            {
                MessageBox.Show("Lütfen bir belge kodu giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (await BelgeVarMiAsync(belgeKodu))
            {
                // Açık EskiSiparisFormu pencereleri arasında kontrol et
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is EskiSiparisFormu eskiForm && eskiForm.BelgeKodu == belgeKodu)
                    {
                        eskiForm.Activate(); // Pencereyi ön plana getir
                        return; // Yeni pencere açma
                    }
                }

                // Eğer yoksa yeni pencere aç
                var siparisFormu = new EskiSiparisFormu();
                siparisFormu.BelgeKodu = belgeKodu;
                siparisFormu.Show();

                this.Close(); // Bu pencereyi kapat
            }
            else
            {
                MessageBox.Show("Belge bulunamadı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private async void TxtBelgeKodu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SorgulaVeYonlendir();
            }
        }

        private async Task SorgulaVeYonlendir()
        {
            string belgeKodu = txtBelgeKodu.Text.Trim();

            if (string.IsNullOrWhiteSpace(belgeKodu))
            {
                MessageBox.Show("Lütfen bir belge kodu giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (await BelgeVarMiAsync(belgeKodu))
            {
                // Sipariş formunu aç
                var siparisFormu = new EskiSiparisFormu();
                siparisFormu.BelgeKodu = belgeKodu;
                siparisFormu.Show();

                this.Close(); // Bu pencereyi kapat
            }
            else
            {
                MessageBox.Show("Belge bulunamadı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task<bool> BelgeVarMiAsync(string belgeKodu)
        {
            string connStr = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();

                string sql = "SELECT COUNT(1) FROM SiparisMaster WHERE BelgeKodu = @BelgeKodu";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@BelgeKodu", belgeKodu);
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }
    }
}