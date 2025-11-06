using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using By_ProV2.DataAccess;
using By_ProV2.Models;

namespace By_ProV2
{
    public partial class AuditTrailWindow : Window
    {
        private readonly AuditTrailRepository _auditRepo;
        private List<AuditTrailEntry> _allEntries;

        public AuditTrailWindow()
        {
            InitializeComponent();
            _auditRepo = new AuditTrailRepository();
            
            // Set default date range to last 30 days
            dpEndDate.SelectedDate = DateTime.Now.Date;
            dpStartDate.SelectedDate = DateTime.Now.Date.AddDays(-30);
            
            LoadAuditTrail();
        }

        private void LoadAuditTrail()
        {
            try
            {
                List<AuditTrailEntry> entries;
                
                if (dpStartDate.SelectedDate.HasValue && dpEndDate.SelectedDate.HasValue)
                {
                    var startDate = dpStartDate.SelectedDate.Value.Date;
                    var endDate = dpEndDate.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1); // End of selected day
                    entries = _auditRepo.GetAuditTrailByDateRange(startDate, endDate);
                }
                else
                {
                    entries = _auditRepo.GetSutKayitAuditTrail();
                }
                
                _allEntries = entries;
                dgAuditTrail.ItemsSource = _allEntries;
                
                // Update statistics
                txtStats.Text = $"Toplam {entries.Count} kayıt bulundu.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yüklenirken hata oluştu: {ex.Message}", 
                                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadAuditTrail();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            dpStartDate.SelectedDate = DateTime.Now.Date.AddDays(-30);
            dpEndDate.SelectedDate = DateTime.Now.Date;
            chkShowAll.IsChecked = false;
            LoadAuditTrail();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAuditTrail();
        }
    }
}