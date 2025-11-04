using System;
using System.Collections.Generic;
using System.Windows;
using ClosedXML.Excel;
using By_ProV2.Models; // FiyatGuncelleModel için
using System.Diagnostics;
using System.IO;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace By_ProV2
{
    /// <summary>
    /// IskontoGirWindow.xaml etkileşim mantığı
    /// </summary>
    /// 

    public partial class IskontoGirWindow : Window
    {
        private readonly List<FiyatGuncelleModel> _veri;

        public IskontoGirWindow(List<FiyatGuncelleModel> veri)
        {
            InitializeComponent();
            _veri = veri;
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_veri == null || _veri.Count == 0)
            {
                MessageBox.Show("Fiyat listesi boş.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryGetIskontolar(out decimal isk1, out decimal isk2, out decimal isk3, out decimal isk4))
            {
                MessageBox.Show("Lütfen geçerli iskonto değerleri girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
                FileName = "MusteriFiyatListesi.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Fiyat Listesi");

                        // Başlıklar                        
                        ws.Cell(1, 1).Value = "STOK ADI";
                        ws.Cell(1, 2).Value = "ÖN ÖDEME";
                        ws.Cell(1, 3).Value = "SÜT VADE";
                        ws.Cell(1, 4).Value = "30 GÜN";
                        ws.Cell(1, 5).Value = "45 GÜN";
                        ws.Cell(1, 6).Value = "60 GÜN";
                       

                        int row = 2;
                        foreach (var item in _veri)
                        {                            
                            ws.Cell(row, 1).Value = item.STOKADI;
                            ws.Cell(row, 2).Value = HesaplaNetFiyat(item.ALISFIYAT1, isk1, isk2, isk3, isk4);
                            ws.Cell(row, 3).Value = HesaplaNetFiyat(item.ALISFIYAT2, isk1, isk2, isk3, isk4);
                            ws.Cell(row, 4).Value = HesaplaNetFiyat(item.ALISFIYAT3, isk1, isk2, isk3, isk4);
                            ws.Cell(row, 5).Value = HesaplaNetFiyat(item.ALISFIYAT4, isk1, isk2, isk3, isk4);
                            ws.Cell(row, 6).Value = HesaplaNetFiyat(item.ALISFIYAT5, isk1, isk2, isk3, isk4);
                            
                            row++;
                        }

                        // Başlık stil
                        var headerRange = ws.Range("A1:F1");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Columns().AdjustToContents();
                        workbook.SaveAs(dialog.FileName);
                    }

                    MessageBox.Show("Excel başarıyla oluşturuldu.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Excel oluşturulurken hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private decimal HesaplaNetFiyat(decimal fiyat, decimal i1, decimal i2, decimal i3, decimal i4)
        {
            decimal carpan = (1 - i1 / 100m) * (1 - i2 / 100m) * (1 - i3 / 100m) * (1 - i4 / 100m);
            return Math.Round(fiyat * carpan, 2);
        }
        private bool TryGetIskontolar(out decimal i1, out decimal i2, out decimal i3, out decimal i4)
        {
            bool ok1 = decimal.TryParse(txtIsk1.Text, out i1);
            bool ok2 = decimal.TryParse(txtIsk2.Text, out i2);
            bool ok3 = decimal.TryParse(txtIsk3.Text, out i3);
            bool ok4 = decimal.TryParse(txtIsk4.Text, out i4);

            return ok1 && ok2 && ok3 && ok4;
        }
        private void BtnVazgec_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void BtnPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_veri == null || _veri.Count == 0)
            {
                MessageBox.Show("Fiyat listesi boş.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryGetIskontolar(out decimal isk1, out decimal isk2, out decimal isk3, out decimal isk4))
            {
                MessageBox.Show("Lütfen geçerli iskonto değerleri girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Dosyası (*.pdf)|*.pdf",
                FileName = "MusteriFiyatListesi.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // PDF oluşturucu sınıfı çağrılıyor
                    PdfFiyatListesiOlusturucu.OlusturFiyatListesiPdf(
                        _veri, isk1, isk2, isk3, isk4, dialog.FileName
                    );

                    MessageBox.Show("PDF başarıyla oluşturuldu.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PDF oluşturulurken hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
    public static class PdfFiyatListesiOlusturucu
    {
        public static void OlusturFiyatListesiPdf(List<FiyatGuncelleModel> veri,
    decimal isk1, decimal isk2, decimal isk3, decimal isk4, string pdfDosyaYolu)
        {
            var document = new Document();
            document.Info.Title = "Müşteri Fiyat Listesi";

            var section = document.AddSection();
            section.PageSetup.TopMargin = Unit.FromCentimeter(2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2.1);

            var normalFont = "Arial";

            // LOGO (Sayfanın sol üstü)
            var headerTable = section.AddTable();
            headerTable.Borders.Visible = false;
            headerTable.AddColumn(Unit.FromCentimeter(6));
            headerTable.AddColumn(Unit.FromCentimeter(10));

            var logoRow = headerTable.AddRow();

            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Logo.png");
            if (File.Exists(logoPath))
            {
                var logo = logoRow.Cells[0].AddImage(logoPath);
                logo.LockAspectRatio = true;
                logo.Width = Unit.FromCentimeter(4);
            }
            else
            {
                logoRow.Cells[0].AddParagraph("LOGO").Format.Font.Bold = true;
            }

            // Burada tarih artık yok, aşağıda ayrı paragraf olarak eklenecek
            logoRow.Cells[1].AddParagraph(); // Boş bırakıldı

            // Başlık
            var baslik = section.AddParagraph("MÜŞTERİ FİYAT LİSTESİ");
            baslik.Format.Font.Size = 14;
            baslik.Format.Font.Bold = true;
            baslik.Format.SpaceBefore = "0.5cm";
            baslik.Format.SpaceAfter = "0.3cm";
            baslik.Format.Alignment = ParagraphAlignment.Center;

            // Tarih paragrafı (tablonun hemen üstünde sağa yaslı)
            var tarihPar = section.AddParagraph("Tarih: " + DateTime.Now.ToString("dd.MM.yyyy"));
            tarihPar.Format.Alignment = ParagraphAlignment.Right;
            tarihPar.Format.Font.Name = normalFont;
            tarihPar.Format.Font.Size = 10;
            tarihPar.Format.SpaceAfter = "0.2cm";

            // TABLO OLUŞTUR (Genişlik ayarlandı, stok adı sütunu genişletildi)
            var table = section.AddTable();
            table.Borders.Width = 0.75;
            table.Borders.Color = Colors.Gray;
            table.Format.Alignment = ParagraphAlignment.Left;  // Tablo sola yaslandı

            // Toplam genişlik yaklaşık 17 cm (A4 iç alanı)
            var columns = new[]
            {
        ("Sıra", 1.0),
        ("Stok Adı", 8.0),  // %30 genişletildi (önce 7cm idi)
        ("Ön Ödeme", 1.5),
        ("Süt Vade", 1.5),
        ("30 Gün", 1.5),
        ("45 Gün", 1.5),
        ("60 Gün", 1.5),
    };

            foreach (var col in columns)
            {
                var column = table.AddColumn(Unit.FromCentimeter(col.Item2));
                column.Format.Alignment = ParagraphAlignment.Center;
            }

            // İKİ SATIRLI ÜST BİLGİ
            // İlk satır: genel kategoriler
            var headerRow1 = table.AddRow();
            headerRow1.Shading.Color = Colors.LightGray;
            headerRow1.Format.Font.Bold = true;
            headerRow1.Format.Alignment = ParagraphAlignment.Center;

            headerRow1.Cells[0].AddParagraph("Sıra");
            headerRow1.Cells[0].MergeDown = 1; // İki satırda birleştir
            headerRow1.Cells[1].AddParagraph("Stok Adı");
            headerRow1.Cells[1].MergeDown = 1; // İki satırda birleştir
            headerRow1.Cells[2].AddParagraph("Fiyatlar");
            headerRow1.Cells[2].MergeRight = 4; // Ön Ödeme'den 60 Gün'e kadar kapsar

            // İkinci satır: detaylı fiyat sütunları
            var headerRow2 = table.AddRow();
            headerRow2.Shading.Color = Colors.LightGray;
            headerRow2.Format.Font.Bold = true;
            headerRow2.Format.Alignment = ParagraphAlignment.Center;
            

            headerRow2.Cells[0].AddParagraph(""); // Boş, çünkü üstünde birleşik hücre var
            headerRow2.Cells[1].AddParagraph(""); // Boş
            headerRow2.Cells[2].AddParagraph("Ön Ödeme");
            headerRow2.Cells[3].AddParagraph("Süt Vade");
            headerRow2.Cells[4].AddParagraph("30 Gün");
            headerRow2.Cells[5].AddParagraph("45 Gün");
            headerRow2.Cells[6].AddParagraph("60 Gün");

            // Veri satırları
            int sira = 1;
            foreach (var item in veri)
            {
                var row = table.AddRow();
                row.HeightRule = RowHeightRule.AtLeast;
                row.Height = Unit.FromCentimeter(0.6);
                row.Cells[0].AddParagraph(sira.ToString());
                row.Cells[1].AddParagraph(item.STOKADI);
                row.Cells[2].AddParagraph(FiyatHesapla(item.ALISFIYAT1, isk1, isk2, isk3, isk4).ToString("N2"));
                row.Cells[3].AddParagraph(FiyatHesapla(item.ALISFIYAT2, isk1, isk2, isk3, isk4).ToString("N2"));
                row.Cells[4].AddParagraph(FiyatHesapla(item.ALISFIYAT3, isk1, isk2, isk3, isk4).ToString("N2"));
                row.Cells[5].AddParagraph(FiyatHesapla(item.ALISFIYAT4, isk1, isk2, isk3, isk4).ToString("N2"));
                row.Cells[6].AddParagraph(FiyatHesapla(item.ALISFIYAT5, isk1, isk2, isk3, isk4).ToString("N2"));

                for (int i = 0; i < row.Cells.Count; i++)
                {
                    row.Cells[i].Format.Font.Name = normalFont;
                    row.Cells[i].Format.Font.Size = 9;
                    row.Cells[i].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                    row.Cells[i].Format.Alignment = (i == 1) ? ParagraphAlignment.Left : ParagraphAlignment.Center;
                    row.Cells[i].Format.LeftIndent = Unit.FromMillimeter(2); // Hücrelerde biraz iç boşluk
                    row.Cells[i].Format.RightIndent = Unit.FromMillimeter(2);
                }

                sira++;
            }

            section.AddParagraph("\n");

            // FOOTER
            var footerPar = section.Footers.Primary.AddParagraph();
            footerPar.Format.Font.Size = 8;
            footerPar.Format.Font.Name = normalFont;
            footerPar.Format.Alignment = ParagraphAlignment.Center;
            footerPar.AddText("BERYEM Tarım Ürünleri Gıda Nak. Tic. Ltd. Şti - Çumra / KONYA");

            // PDF Render
            var pdfRenderer = new PdfDocumentRenderer()
            {
                Document = document
            };

            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(pdfDosyaYolu);

            Process.Start(new ProcessStartInfo(pdfDosyaYolu) { UseShellExecute = true });
        }


        private static decimal FiyatHesapla(decimal fiyat, decimal i1, decimal i2, decimal i3, decimal i4)
        {
            decimal carpim = (1 - i1 / 100m) * (1 - i2 / 100m) * (1 - i3 / 100m) * (1 - i4 / 100m);
            return Math.Round(fiyat * carpim, 2);
        }
    }

}