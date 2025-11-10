using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using By_ProV2.Helpers;
using By_ProV2.Models;
using By_ProV2.DataAccess;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace By_ProV2
{
    public class AlisIcmalKaydi
    {
        public DateTime Tarih { get; set; }
        public string CariKod { get; set; }
        public decimal Miktar { get; set; }
        public decimal? Kesinti { get; set; }
        public decimal? Yag { get; set; }
        public decimal? Protein { get; set; }
    }

    public partial class AlisReportWindow : Window
    {
        private readonly string _connectionString;
        private readonly string _cariKod;
        private readonly DateTime? _startDate;
        private readonly DateTime? _endDate;

        public AlisReportWindow(string cariKod, DateTime? startDate = null, DateTime? endDate = null)
        {
            InitializeComponent();
            _connectionString = ConfigurationHelper.GetConnectionString("db");
            _cariKod = cariKod;
            _startDate = startDate;
            _endDate = endDate;
            LoadReport();
        }

        private void LoadReport()
        {
            spAlislar.Children.Clear(); // Clear previous records

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    string cariAdi = "";
                    decimal sutFiyati = 0;
                    decimal nakliyeFiyati = 0;

                    if (!string.IsNullOrEmpty(_cariKod))
                    {
                        // Get Cari Name, Süt Fiyatı, and Nakliye Fiyatı
                        string getCariDataSql = "SELECT c.CariAdi, cb.SUTFIYATI, cb.NAKFIYATI FROM Cari c LEFT JOIN CASABIT cb ON c.CariKod = cb.CARIKOD WHERE c.CariKod = @CariKod";
                        using (var cmd = new SqlCommand(getCariDataSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@CariKod", _cariKod);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    cariAdi = reader["CariAdi"] as string;
                                    sutFiyati = reader.IsDBNull(reader.GetOrdinal("SUTFIYATI")) ? 0 : reader.GetDecimal(reader.GetOrdinal("SUTFIYATI"));
                                    nakliyeFiyati = reader.IsDBNull(reader.GetOrdinal("NAKFIYATI")) ? 0 : reader.GetDecimal(reader.GetOrdinal("NAKFIYATI"));
                                }
                            }
                        }
                    }
                    else
                    {
                        cariAdi = "Tüm Cari Hesaplar"; // Display text when showing all customers
                    }

                    txtCariKod.Text = string.IsNullOrEmpty(_cariKod) ? "(Tümü)" : _cariKod;
                    txtCariAdi.Text = cariAdi;
                    txtSutFiyati.Text = sutFiyati.ToString("N2");
                    txtNakliyeFiyati.Text = nakliyeFiyati.ToString("N2");

                    // Set date range text
                    string dateRangeText = "";
                    if (_startDate.HasValue && _endDate.HasValue)
                    {
                        dateRangeText = $"{_startDate.Value:dd.MM.yyyy} - {_endDate.Value:dd.MM.yyyy}";
                    }
                    else if (_startDate.HasValue)
                    {
                        dateRangeText = $"{_startDate.Value:dd.MM.yyyy} ve sonrası";
                    }
                    else if (_endDate.HasValue)
                    {
                        dateRangeText = $"{_endDate.Value:dd.MM.yyyy} ve öncesi";
                    }
                    else
                    {
                        dateRangeText = "Tüm Tarihler";
                    }
                    lblDateRange.Text = dateRangeText;

                    string sql = @"
                        SELECT 
                            sk.Tarih,
                            c.CariKod,
                            sk.Miktar,
                            sk.Kesinti,
                            sk.Yag,
                            sk.Protein
                        FROM SutKayit sk
                        LEFT JOIN Cari c ON sk.TedarikciId = c.CariId
                        WHERE (@CariKod IS NULL OR @CariKod = '' OR c.CariKod LIKE '%' + @CariKod + '%')
                        AND sk.IslemTuru = 'Depoya Alım'
                        AND (@StartDate IS NULL OR sk.Tarih >= @StartDate)
                        AND (@EndDate IS NULL OR sk.Tarih <= @EndDate)
                        ORDER BY sk.Tarih DESC";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CariKod", string.IsNullOrEmpty(_cariKod) ? (object)DBNull.Value : _cariKod);
                        cmd.Parameters.AddWithValue("@StartDate", _startDate.HasValue ? (object)_startDate.Value.Date : DBNull.Value);
                        cmd.Parameters.AddWithValue("@EndDate", _endDate.HasValue ? (object)_endDate.Value.Date.AddDays(1).AddSeconds(-1) : DBNull.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            decimal toplamNetMiktar = 0;
                            decimal toplamKesinti = 0;
                            decimal agirlikliYagToplami = 0;
                            decimal toplamSutForYag = 0;
                            decimal agirlikliProteinToplami = 0;
                            decimal toplamSutForProtein = 0;
                            int toplamKayit = 0;

                            while (reader.Read())
                            {
                                var newKayit = new AlisIcmalKaydi
                                {
                                    Tarih = reader.GetDateTime(reader.GetOrdinal("Tarih")),
                                    CariKod = reader["CariKod"] as string,
                                    Miktar = reader.GetDecimal(reader.GetOrdinal("Miktar")),
                                    Kesinti = reader.IsDBNull(reader.GetOrdinal("Kesinti")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Kesinti")),
                                    Yag = reader.IsDBNull(reader.GetOrdinal("Yag")) ? null : reader.GetDecimal(reader.GetOrdinal("Yag")),
                                    Protein = reader.IsDBNull(reader.GetOrdinal("Protein")) ? null : reader.GetDecimal(reader.GetOrdinal("Protein"))
                                };

                                // Create a grid for each record
                                var recordGrid = new Grid();
                                recordGrid.Margin = new Thickness(0, 5, 0, 5); // Small gap between records
                                recordGrid.RowDefinitions.Add(new RowDefinition());
                                recordGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120) });
                                recordGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });
                                recordGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });
                                recordGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120) });
                                recordGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120) });
                                recordGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                                // Create and add controls for each column with borders
                                var dateText = new TextBlock()
                                {
                                    Text = newKayit.Tarih.ToString("dd.MM.yyyy"),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Padding = new Thickness(3)
                                };
                                Grid.SetColumn(dateText, 0);
                                Grid.SetRow(dateText, 0);
                                recordGrid.Children.Add(dateText);

                                var miktarText = new TextBlock()
                                {
                                    Text = newKayit.Miktar.ToString("N2"),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Padding = new Thickness(3)
                                };
                                Grid.SetColumn(miktarText, 1);
                                Grid.SetRow(miktarText, 0);
                                recordGrid.Children.Add(miktarText);

                                var kesintiText = new TextBlock()
                                {
                                    Text = newKayit.Kesinti?.ToString("N2") ?? "",
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Padding = new Thickness(3)
                                };
                                Grid.SetColumn(kesintiText, 2);
                                Grid.SetRow(kesintiText, 0);
                                recordGrid.Children.Add(kesintiText);

                                var yagText = new TextBlock()
                                {
                                    Text = newKayit.Yag?.ToString("N2") ?? "",
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Padding = new Thickness(3)
                                };
                                Grid.SetColumn(yagText, 3);
                                Grid.SetRow(yagText, 0);
                                recordGrid.Children.Add(yagText);

                                var proteinText = new TextBlock()
                                {
                                    Text = newKayit.Protein?.ToString("N2") ?? "",
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Padding = new Thickness(3)
                                };
                                Grid.SetColumn(proteinText, 4);
                                Grid.SetRow(proteinText, 0);
                                recordGrid.Children.Add(proteinText);

                                // Add bottom border to the entire record using a separator
                                var bottomBorder = new Border()
                                {
                                    Height = 0,
                                    Background = System.Windows.Media.Brushes.Black,
                                    Margin = new Thickness(0, 0, 0, 0)
                                };

                                var recordContainer = new StackPanel() { Orientation = Orientation.Vertical };
                                recordContainer.Children.Add(recordGrid);
                                recordContainer.Children.Add(bottomBorder);



                                // Add the container (not recordGrid directly) to the main stack panel
                                spAlislar.Children.Add(recordContainer);

                                toplamNetMiktar += newKayit.Miktar;
                                toplamKesinti += newKayit.Kesinti ?? 0;
                                toplamKayit++;

                                if (newKayit.Yag.HasValue && newKayit.Yag.Value != 0)
                                {
                                    agirlikliYagToplami += newKayit.Yag.Value * newKayit.Miktar;
                                    toplamSutForYag += newKayit.Miktar;
                                }

                                if (newKayit.Protein.HasValue && newKayit.Protein.Value != 0)
                                {
                                    agirlikliProteinToplami += newKayit.Protein.Value * newKayit.Miktar;
                                    toplamSutForProtein += newKayit.Miktar;
                                }
                            }

                            decimal ortalamaYag = toplamSutForYag > 0 ? agirlikliYagToplami / toplamSutForYag : 0;
                            decimal ortalamaProtein = toplamSutForProtein > 0 ? agirlikliProteinToplami / toplamSutForProtein : 0;

                            lblNetMiktar.Text = $"{toplamNetMiktar:N2}";
                            lblToplamKesinti.Text = $"{toplamKesinti:N2}";
                            lblOrtalamaYag.Text = $"{ortalamaYag:N2}";
                            lblOrtalamaProtein.Text = $"{ortalamaProtein:N2}";
                            lblToplamKayit.Text = $"{toplamKayit}";
                            
                            // Set the new textblock to show the summed net miktar
                            txtNetSutOdemesi.Text = $"Toplam Net Miktar: {toplamNetMiktar:N2}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rapor yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            // Load parameter defaults when window loads
            LoadParameterDefaults();
        }
    

    private void BtnExportToPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create SaveFileDialog
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.FileName = "AlisRaporu.pdf";
                saveFileDialog.Filter = "PDF Documents (*.pdf)|*.pdf";

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Prepare report data from current display
                    var reportData = new ReportData
                    {
                        Title = "ALIŞ İCMAL RAPORU",
                        DateRange = lblDateRange.Text,
                        CustomerCode = txtCariKod.Text,
                        CustomerName = txtCariAdi.Text,
                        ColumnHeaders = new List<string> { "Tarih", "Alınan Süt", "Kesinti", "Yağ Oranı", "Protein" },
                        Items = new List<ReportItem>(),
                        Summary = new ReportSummary
                        {
                            NetMiktar = decimal.ToDouble(ParseDecimalValue(lblNetMiktar.Text)),
                            Kesinti = decimal.ToDouble(ParseDecimalValue(lblToplamKesinti.Text)),
                            OrtYag = decimal.ToDouble(ParseDecimalValue(lblOrtalamaYag.Text)),
                            OrtProtein = decimal.ToDouble(ParseDecimalValue(lblOrtalamaProtein.Text)),
                            ToplamKayit = (int)ParseDecimalValue(lblToplamKayit.Text)
                        }
                    };

                    // Collect items from the displayed records
                    ReloadDataForPDF(reportData.Items);

                    ReportGenerator.GenerateReport(reportData, saveFileDialog.FileName);

                    MessageBox.Show("PDF raporu başarıyla oluşturuldu!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF oluşturulurken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPreviewPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Prepare report data from current display
                var reportData = new ReportData
                {
                    Title = "ALIŞ İCMAL RAPORU",
                    DateRange = lblDateRange.Text,
                    CustomerCode = txtCariKod.Text,
                    CustomerName = txtCariAdi.Text,
                    ColumnHeaders = new List<string> { "Tarih", "Alınan Süt", "Kesinti", "Yağ Oranı", "Protein" },
                    Items = new List<ReportItem>(),
                    Summary = new ReportSummary
                    {
                        NetMiktar = decimal.ToDouble(ParseDecimalValue(lblNetMiktar.Text)),
                        Kesinti = decimal.ToDouble(ParseDecimalValue(lblToplamKesinti.Text)),
                        OrtYag = decimal.ToDouble(ParseDecimalValue(lblOrtalamaYag.Text)),
                        OrtProtein = decimal.ToDouble(ParseDecimalValue(lblOrtalamaProtein.Text)),
                        ToplamKayit = (int)ParseDecimalValue(lblToplamKayit.Text)
                    }
                };

                ReloadDataForPDF(reportData.Items);

                // Generate to a temporary file
                string tempPdfPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"AlisRaporu_Preview_{Guid.NewGuid()}.pdf");
                ReportGenerator.GenerateReport(reportData, tempPdfPath);

                // Open the PDF
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempPdfPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF önizlemesi oluşturulurken veya açılırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadDataForPDF(List<ReportItem> items)
        {
            // Reload data specifically for PDF generation using same query as in LoadReport
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    string sql = @"
                    SELECT
                        sk.Tarih,
                        c.CariKod,
                        sk.Miktar,
                        sk.Kesinti,
                        sk.Yag,
                        sk.Protein
                    FROM SutKayit sk
                    LEFT JOIN Cari c ON sk.TedarikciId = c.CariId
                    WHERE (@CariKod IS NULL OR @CariKod = '' OR c.CariKod LIKE '%' + @CariKod + '%')
                    AND sk.IslemTuru = 'Depoya Alım'
                    AND (@StartDate IS NULL OR sk.Tarih >= @StartDate)
                    AND (@EndDate IS NULL OR sk.Tarih <= @EndDate)
                    ORDER BY sk.Tarih DESC";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CariKod", string.IsNullOrEmpty(_cariKod) ? (object)DBNull.Value : _cariKod);
                        cmd.Parameters.AddWithValue("@StartDate", _startDate.HasValue ? (object)_startDate.Value.Date : DBNull.Value);
                        cmd.Parameters.AddWithValue("@EndDate", _endDate.HasValue ? (object)_endDate.Value.Date.AddDays(1).AddSeconds(-1) : DBNull.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ReportItem
                                {
                                    Tarih = reader.GetDateTime(reader.GetOrdinal("Tarih")).ToString("dd.MM.yyyy"),
                                    Miktar = Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("Miktar"))),
                                    Kesinti = reader.IsDBNull(reader.GetOrdinal("Kesinti")) ? 0.0 : Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("Kesinti"))),
                                    Yag = reader.IsDBNull(reader.GetOrdinal("Yag")) ? 0.0 : Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("Yag"))),
                                    Protein = reader.IsDBNull(reader.GetOrdinal("Protein")) ? 0.0 : Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("Protein")))
                                };
                                items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF verileri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal ParseDecimalValue(string text)
        {
            // Extract the numeric part from text like "Net Miktar: 1234.56" or just "1234.56"
            try
            {
                int colonIndex = text.IndexOf(':');
                if (colonIndex >= 0)
                {
                    string valuePart = text.Substring(colonIndex + 1).Trim();
                    if (decimal.TryParse(valuePart, out decimal result))
                    {
                        return result;
                    }
                }
                else
                {
                    // If no colon exists, try to parse the entire string
                    if (decimal.TryParse(text, out decimal result))
                    {
                        return result;
                    }
                }
            }
            catch
            {
                // Return 0 if parsing fails
            }
            
            return 0;
        }

        private void ChkYagKesintisi_Click(object sender, RoutedEventArgs e)
        {
            // Checkbox click event is intentionally left empty
            // Parameters are loaded on window load
        }

        private void ChkProteinKesintisi_Click(object sender, RoutedEventArgs e)
        {
            // Checkbox click event is intentionally left empty
            // Parameters are loaded on window load
        }
        
        private void UpdateDizemBasiTlVisibility()
        {
            // Method is intentionally left empty
            // Parameters are loaded on window load
        }

        private void LoadParameterDefaults()
        {
            try
            {
                var paramRepo = new ParameterRepository();
                var latestParams = paramRepo.GetLatestParametreler();

                if (latestParams != null)
                {
                    // Always set the parameter values regardless of checkbox state
                    txtYagKesintiOrani.Text = latestParams.YagKesintiParametresi?.ToString("N2") ?? "";
                    txtProteinKesintiOrani.Text = latestParams.ProteinParametresi?.ToString("N2") ?? "";
                    txtDizemBasiTl.Text = latestParams.DizemBasiTl?.ToString("N2") ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Parametreler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}