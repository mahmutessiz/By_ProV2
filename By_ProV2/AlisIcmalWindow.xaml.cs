using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using By_ProV2.Helpers;
using By_ProV2.Models;

namespace By_ProV2
{
    public partial class AlisIcmalWindow : Window
    {
        public AlisIcmalWindow()
        {
            InitializeComponent();
            
            dpStartDate.SelectedDate = DateTime.Now.Date.AddDays(-30);
            dpEndDate.SelectedDate = DateTime.Now.Date;
        }

        private void BtnSorgula_Click(object sender, RoutedEventArgs e)
        {
            string cariKodu = txtCariKodu.Text.Trim();

            if (string.IsNullOrEmpty(cariKodu))
            {
                MessageBox.Show("Lütfen bir Cari Kodu giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!CariKoduExists(cariKodu))
            {
                MessageBox.Show("Girdiğiniz Cari Kodu sistemde bulunamadı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? startDate = dpStartDate.SelectedDate;
            DateTime? endDate = dpEndDate.SelectedDate;

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                MessageBox.Show("Başlangıç tarihi bitiş tarihinden büyük olamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AlisReportWindow reportWindow = new AlisReportWindow(cariKodu, startDate, endDate);
            reportWindow.ShowDialog();
        }

        private bool CariKoduExists(string cariKodu)
        {
            string connectionString = ConfigurationHelper.GetConnectionString("db");
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM Cari WHERE CariKod = @CariKod";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CariKod", cariKodu);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void BtnTumunuListele_Click(object sender, RoutedEventArgs e)
        {
            txtCariKodu.Text = "";

            DateTime? startDate = dpStartDate.SelectedDate;
            DateTime? endDate = dpEndDate.SelectedDate;

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                MessageBox.Show("Başlangıç tarihi bitiş tarihinden büyük olamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AlisReportWindow reportWindow = new AlisReportWindow("", startDate, endDate);
            reportWindow.ShowDialog();
        }

        private void TxtCariKodu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSorgula_Click(sender, e);
            }
        }

        private void BtnCariListe_Click(object sender, RoutedEventArgs e)
        {
            LoadCariListesi();
            popupCariListesi.IsOpen = true;
        }

        private void LoadCariListesi()
        {
            string connectionString = ConfigurationHelper.GetConnectionString("db");
            lstCariListesi.Items.Clear();

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT CariKod, CariAdi FROM Cari ORDER BY CariKod";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var cariInfo = new CariInfo
                                {
                                    CariKod = reader["CariKod"].ToString(),
                                    CariAdi = reader["CariAdi"].ToString(),
                                    DisplayText = $"{reader["CariKod"]} - {reader["CariAdi"]}"
                                };
                                lstCariListesi.Items.Add(cariInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cari listesi yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LstCariListesi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCariListesi.SelectedItem is CariInfo selectedCari)
            {
                txtCariKodu.Text = selectedCari.CariKod;
                popupCariListesi.IsOpen = false;
            }
        }

        private void LstCariListesi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstCariListesi.SelectedItem is CariInfo selectedCari)
            {
                txtCariKodu.Text = selectedCari.CariKod;
                popupCariListesi.IsOpen = false;

                // Open the report window with the selected supplier and current date filters
                DateTime? startDate = dpStartDate.SelectedDate;
                DateTime? endDate = dpEndDate.SelectedDate;

                if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
                {
                    MessageBox.Show("Başlangıç tarihi bitiş tarihinden büyük olamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AlisReportWindow reportWindow = new AlisReportWindow(selectedCari.CariKod, startDate, endDate);
                reportWindow.ShowDialog();
            }
        }
    }
}