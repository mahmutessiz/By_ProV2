using System;
using System.Configuration;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using By_ProV2.Helpers;

namespace By_ProV2
{
    public partial class TedarikciEkleWindow : Window
    {
        public bool KayitBasarili { get; private set; } = false;
        public string YeniTedarikciAdi { get; private set; }

        public TedarikciEkleWindow()
        {
            InitializeComponent();
        }

        private void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            string ad = txtTedarikciAdi.Text.Trim();
            string vergiNo = txtVergiNo.Text.Trim();
            string telefon = txtTelefon.Text.Trim();
            string eposta = txtEposta.Text.Trim();
            string adres = txtAdres.Text.Trim();

            if (string.IsNullOrWhiteSpace(ad))
            {
                MessageBox.Show("Tedarikçi adı zorunludur.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                        INSERT INTO STOKSABITTED (TEDARIKCIADI, VERGINO, TELEFON, EPOSTA, ADRES)
                        VALUES (@adi, @vergiNo, @telefon, @eposta, @adres)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@adi", ad);
                        cmd.Parameters.AddWithValue("@vergiNo", vergiNo);
                        cmd.Parameters.AddWithValue("@telefon", telefon);
                        cmd.Parameters.AddWithValue("@eposta", eposta);
                        cmd.Parameters.AddWithValue("@adres", adres);
                        cmd.ExecuteNonQuery();
                    }
                }

                KayitBasarili = true;
                YeniTedarikciAdi = ad;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}