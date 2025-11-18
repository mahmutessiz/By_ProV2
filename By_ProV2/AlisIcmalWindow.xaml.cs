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
        private List<CariInfo> _allCariList = new List<CariInfo>();

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
                    // Check if the Cari Kodu exists and has purchase records
                    string sql = @"SELECT COUNT(*)
                                   FROM Cari c
                                   INNER JOIN SutKayit sk ON c.CariId = sk.TedarikciId
                                   WHERE c.CariKod = @CariKod
                                   AND sk.IslemTuru = 'Depoya Alım'";
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
            _allCariList.Clear();

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Load only suppliers who have purchase records (Tedarikci)
                    string sql = @"SELECT DISTINCT c.CariKod, c.CariAdi
                                   FROM Cari c
                                   INNER JOIN SutKayit sk ON c.CariId = sk.TedarikciId
                                   WHERE sk.IslemTuru = 'Depoya Alım'
                                   ORDER BY c.CariKod";
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
                                _allCariList.Add(cariInfo);
                            }
                        }
                    }
                }

                // Set the ItemsSource to the filtered list
                lstCariListesi.ItemsSource = _allCariList;
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

        // Overload for when called from Enter key event
        private void LstCariListesi_MouseDoubleClick(object sender, RoutedEventArgs e)
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

        private void TxtCariAra_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterCariListesi();
        }

        private void TxtCariAra_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && lstCariListesi.SelectedItem != null)
            {
                // If an item is selected and Enter is pressed, treat it like double-click
                LstCariListesi_MouseDoubleClick(sender, null);
            }
            else if (e.Key == Key.Escape)
            {
                // Close the popup if Escape is pressed
                popupCariListesi.IsOpen = false;
            }
        }

        private void FilterCariListesi()
        {
            try
            {
                string searchText = txtCariAra.Text?.ToLower() ?? "";

                if (string.IsNullOrEmpty(searchText))
                {
                    // If search text is empty, show all items
                    lstCariListesi.ItemsSource = _allCariList;
                }
                else
                {
                    // Filter the list based on the search text
                    var filteredList = _allCariList
                        .Where(cari => cari.CariKod?.ToLower().Contains(searchText) == true || 
                                       cari.CariAdi?.ToLower().Contains(searchText) == true ||
                                       cari.DisplayText?.ToLower().Contains(searchText) == true)
                        .ToList();

                    lstCariListesi.ItemsSource = filteredList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Arama sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}