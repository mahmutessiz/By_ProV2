using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using By_ProV2.DataAccess;
using By_ProV2.Models;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Controls;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Diagnostics;
using System.IO;

namespace By_ProV2.Reports
{
    public partial class GunlukSutAlimPreview : Window
    {
        private readonly SutRaporRepository _sutRepo = new SutRaporRepository();

        public GunlukSutAlimPreview()
        {
            InitializeComponent();
        }

        private void btnGetir_Click(object sender, RoutedEventArgs e)
        {
            if (dpTarih.SelectedDate == null)
            {
                MessageBox.Show("Lütfen bir tarih seçin.");
                return;
            }

            DateTime secilenTarih = dpTarih.SelectedDate.Value;

            var sutKayitlar = _sutRepo.GetGunlukSutKayit(secilenTarih);

            FlowDocument doc = new FlowDocument
            {
                FontFamily = new FontFamily("Calibri"),
                FontSize = 14,
                PagePadding = new Thickness(20)
            };

            // Başlık
            Paragraph header = new Paragraph(new Bold(new Run($"GÜNLÜK SÜT ALIM RAPORU - {secilenTarih:dd.MM.yyyy}")))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 20,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(header);

            // 🥛 Depoya Alınan Sütler
            var depoAlimlar = sutKayitlar.Where(x => x.IslemTuru == "Depoya Alım").ToList();
            if (depoAlimlar.Any())
            {
                doc.Blocks.Add(CreateSectionTitle("Depoya Alınan Sütler"));
                doc.Blocks.Add(CreateTable(depoAlimlar));
            }

            // 🔹 Ayraç çizgisi
            if (depoAlimlar.Any())
            {
                doc.Blocks.Add(new BlockUIContainer(new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 1, 0, 0),
                    Margin = new Thickness(0, 20, 0, 20)
                }));
            }

            // 🚛 Direkt Sevk Yapılan Sütler
            var direktSevk = sutKayitlar.Where(x => x.IslemTuru == "Direkt Sevk").ToList();
            if (direktSevk.Any())
            {
                doc.Blocks.Add(CreateSectionTitle("Direkt Sevk Yapılan Sütler"));
                doc.Blocks.Add(CreateTable(direktSevk));
            }

            // Genel toplam - only include records that are actually displayed in the tables (Depoya Alım and Direkt Sevk)
            var displayedRecords = sutKayitlar.Where(x => x.IslemTuru == "Depoya Alım" || x.IslemTuru == "Direkt Sevk").ToList();
            if (displayedRecords.Any())
            {
                var tumMiktar = displayedRecords.Sum(x => x.Miktar);
                var genelYag = WeightedAverage(displayedRecords.Select(x => x.Yag), displayedRecords.Select(x => x.Miktar));
                var genelProtein = WeightedAverage(displayedRecords.Select(x => x.Protein), displayedRecords.Select(x => x.Miktar));
                var genelTKM = WeightedAverage(displayedRecords.Select(x => x.TKM), displayedRecords.Select(x => x.Miktar));
                var genelLaktoz = WeightedAverage(displayedRecords.Select(x => x.Laktoz), displayedRecords.Select(x => x.Miktar));
                var genelpH = WeightedAverage(displayedRecords.Select(x => x.pH), displayedRecords.Select(x => x.Miktar));
                var genelIletkenlik = WeightedAverage(displayedRecords.Select(x => x.Iletkenlik), displayedRecords.Select(x => x.Miktar));
                var genelDonma = WeightedAverage(displayedRecords.Select(x => x.DonmaN), displayedRecords.Select(x => x.Miktar));
                var tumKesinti = displayedRecords.Sum(x => x.Kesinti);

                Paragraph genelToplam = new Paragraph(new Bold(new Run(
                    $"GENEL TOPLAM — Miktar: {tumMiktar:N2} lt | Yağ: {genelYag:N2}% | Protein: {genelProtein:N2}% | TKM: {genelTKM:N2}% | Laktoz: {genelLaktoz:N2}% | pH: {genelpH:N2} | İletkenlik: {genelIletkenlik:N2}mS | Donma Noktası: {genelDonma:N3}°C | Kesinti: {tumKesinti:N2}lt"
                )))
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 30, 0, 0)
                };
                doc.Blocks.Add(genelToplam);
            }

            docViewer.Document = doc;
        }

        // 🔹 Bölüm başlığı oluşturma
        private Block CreateSectionTitle(string text)
        {
            return new Paragraph(new Bold(new Run(text)))
            {
                FontSize = 18,
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(0, 10, 0, 10),
                Foreground = Brushes.DarkSlateGray
            };
        }

        // 🔸 Tablo oluşturma
        private Table CreateTable(List<SutKaydi> kayitlar)
        {
            Table table = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 10) };

            // Kolonları oluştur ve genişliklerini ayarla - daha dengeli ve toplamda 1100px'ten fazla olmamalı A4 yatay için
            TableColumn col1 = new TableColumn { Width = new GridLength(180) }; // Tedarikçi
            TableColumn col2 = new TableColumn { Width = new GridLength(70) };  // Miktar
            TableColumn col3 = new TableColumn { Width = new GridLength(70) };  // Net Miktar
            TableColumn col4 = new TableColumn { Width = new GridLength(50) };  // Yağ
            TableColumn col5 = new TableColumn { Width = new GridLength(50) };  // Protein
            TableColumn col6 = new TableColumn { Width = new GridLength(50) };  // TKM
            TableColumn col7 = new TableColumn { Width = new GridLength(50) };  // Laktoz
            TableColumn col8 = new TableColumn { Width = new GridLength(45) };  // pH
            TableColumn col9 = new TableColumn { Width = new GridLength(60) };  // İletkenlik
            TableColumn col10 = new TableColumn { Width = new GridLength(70) }; // Donma Noktası
            TableColumn col11 = new TableColumn { Width = new GridLength(60) }; // Kesinti
            TableColumn col12 = new TableColumn { Width = new GridLength(60) }; // Antibiyotik
            TableColumn col13 = new TableColumn { Width = new GridLength(60) }; // Durum
            TableColumn col14 = new TableColumn { Width = new GridLength(120) }; // Açıklama

            table.Columns.Add(col1); // Tedarikçi
            table.Columns.Add(col2); // Miktar
            table.Columns.Add(col3); // Net Miktar
            table.Columns.Add(col4); // Yağ
            table.Columns.Add(col5); // Protein
            table.Columns.Add(col6); // TKM
            table.Columns.Add(col7); // Laktoz
            table.Columns.Add(col8); // pH
            table.Columns.Add(col9); // İletkenlik
            table.Columns.Add(col10); // Donma Noktası
            table.Columns.Add(col11); // Kesinti
            table.Columns.Add(col12); // Antibiyotik
            table.Columns.Add(col13); // Durum
            table.Columns.Add(col14); // Açıklama

            // Başlık satırı
            TableRowGroup headerGroup = new TableRowGroup();
            TableRow headerRow = new TableRow();
            string[] headers = { "Tedarikçi", "Miktar (lt)", "Net Miktar (lt)", "Yağ (%)", "Protein (%)", "TKM (%)", "Laktoz (%)", "pH ", "İletkenlik (mS)", "Donma Noktası (°C)", "Kesinti (lt)", "Antibiyotik", "Durum", "Açıklama" };
            TextAlignment[] headerAlignments = { TextAlignment.Left, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right, TextAlignment.Right, TextAlignment.Center, TextAlignment.Center, TextAlignment.Left };

            for (int i = 0; i < headers.Length; i++)
            {
                headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(headers[i]))))
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(5),
                    TextAlignment = headerAlignments[i]
                });
            }
            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            // Veri satırları
            TableRowGroup dataGroup = new TableRowGroup();
            foreach (var k in kayitlar)
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.TedarikciAdi ?? "-"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Left });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Miktar.ToString("N2")))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.NetMiktar.ToString("N2")))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Yag?.ToString("N2") ?? "-"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Protein?.ToString("N2") ?? "-"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.TKM?.ToString("N2") ?? "-"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Laktoz?.ToString("N2") ?? "-"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.pH?.ToString("N2") ?? "-"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Iletkenlik?.ToString("N2") ?? "-"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.DonmaN?.ToString("N3") ?? "-"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Kesinti.ToString("N2")))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Antibiyotik ?? "-"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Durumu ?? "-"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                // Açıklama sütununa uzun metinler için wrapping ekle
                var aciklamaParagraph = new Paragraph(new Run(k.Aciklama ?? "-"));
                var aciklamaCell = new TableCell(aciklamaParagraph) { Padding = new Thickness(5), TextAlignment = TextAlignment.Left };
                // FlowDocument tablosunda wrapping genellikle otomatik olarak işler, alternatif olarak max genişlik ayarlanabilir
                row.Cells.Add(aciklamaCell);
                dataGroup.Rows.Add(row);
            }

            // Alt toplam satırı
            decimal toplamMiktar = kayitlar.Sum(x => x.Miktar);
            decimal toplamNetMiktar = kayitlar.Sum(x => x.NetMiktar);
            decimal ortYag = WeightedAverage(kayitlar.Select(x => x.Yag), kayitlar.Select(x => x.Miktar));
            decimal ortProtein = WeightedAverage(kayitlar.Select(x => x.Protein), kayitlar.Select(x => x.Miktar));
            decimal ortTKM = WeightedAverage(kayitlar.Select(x => x.TKM), kayitlar.Select(x => x.Miktar));
            decimal ortLaktoz = WeightedAverage(kayitlar.Select(x => x.Laktoz), kayitlar.Select(x => x.Miktar));
            decimal ortpH = WeightedAverage(kayitlar.Select(x => x.pH), kayitlar.Select(x => x.Miktar));
            decimal ortIletkenlik = WeightedAverage(kayitlar.Select(x => x.Iletkenlik), kayitlar.Select(x => x.Miktar));
            decimal ortDonma = WeightedAverage(kayitlar.Select(x => x.DonmaN), kayitlar.Select(x => x.Miktar));
            decimal toplamKesinti = kayitlar.Sum(x => x.Kesinti);

            TableRow toplamRow = new TableRow();
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Toplam / Ortalama")))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Left });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(toplamMiktar.ToString("N2"))))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(toplamNetMiktar.ToString("N2"))))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortYag.ToString("N2"))))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortProtein.ToString("N2"))))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortTKM.ToString("N2"))))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortLaktoz.ToString("N2"))))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortpH.ToString("N2"))))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortIletkenlik.ToString("N2"))))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortDonma.ToString("N3"))))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(toplamKesinti.ToString("N2"))))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Run(""))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Run(""))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Run(""))) { Padding = new Thickness(5) });

            dataGroup.Rows.Add(toplamRow);
            table.RowGroups.Add(dataGroup);

            return table;
        }


        // 🔹 Ağırlıklı ortalama hesabı
        private decimal WeightedAverage(IEnumerable<decimal?> values, IEnumerable<decimal> weights)
        {
            decimal toplamMiktar = 0;
            decimal toplamAgirlikli = 0;

            var valList = values.ToList();
            var wList = weights.ToList();

            for (int i = 0; i < valList.Count; i++)
            {
                if (valList[i].HasValue)
                {
                    toplamAgirlikli += valList[i].Value * wList[i];
                    toplamMiktar += wList[i];
                }
            }

            return toplamMiktar > 0 ? toplamAgirlikli / toplamMiktar : 0;
        }
        private void btnYazdir_Click(object sender, RoutedEventArgs e)
        {
            if (docViewer.Document == null)
            {
                MessageBox.Show("Yazdırılacak bir belge yok. Lütfen önce raporu oluşturun.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                PrintDialog printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    FlowDocument document = docViewer.Document as FlowDocument;

                    // 🟩 Yazıcıyı yatay moda al
                    printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

                    // 🧾 Manuel olarak A4 yatay boyut (mm -> 96 DPI)
                    // A4: 210 x 297 mm  → yatayda 297 x 210 mm
                    double inch = 96.0 / 25.4; // 1 mm = 96/25.4 dpi
                    double pageWidth = 297 * inch;  // 297mm
                    double pageHeight = 210 * inch; // 210mm

                    // FlowDocument'e uygula - A4 yatay için uygun sütun genişliği
                    document.PageWidth = pageWidth;
                    document.PageHeight = pageHeight;
                    document.PagePadding = new Thickness(30); // Daha küçük kenar boşlukları
                    document.ColumnWidth = pageWidth - 60; // Kenar boşluklarını hesaba katarak sütun genişliğini ayarla
                    document.ColumnGap = 0;

                    // 🔸 Yeni paginator oluştur ve yazdır
                    IDocumentPaginatorSource idpSource = document;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "Günlük Süt Alım Raporu (Yatay)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yazdırma sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnPDF_Click(object sender, RoutedEventArgs e)
        {
            if (dpTarih.SelectedDate == null)
            {
                MessageBox.Show("Lütfen bir tarih seçin ve raporu getirin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime secilenTarih = dpTarih.SelectedDate.Value;
            var sutKayitlar = _sutRepo.GetGunlukSutKayit(secilenTarih);

            if (!sutKayitlar.Any())
            {
                MessageBox.Show("Seçilen tarih için veri bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"GunlukSutAlimRaporu_{secilenTarih:yyyyMMdd}.pdf",
                DefaultExt = ".pdf",
                Filter = "PDF documents (.pdf)|*.pdf"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    string dosyaYolu = dlg.FileName;
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = "Günlük Süt Alım Raporu";
                    document.Info.Author = "By_ProV2";
                    document.Info.Subject = $"Günlük Süt Alım Raporu - {secilenTarih:dd.MM.yyyy}";

                    // Sayfa boyutu (A4 yatay)
                    PdfPage page = document.AddPage();
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                                XFont baslikFont = new XFont("Arial", 14, XFontStyleEx.Bold);
                                XFont altBaslikFont = new XFont("Arial", 12, XFontStyleEx.Bold);
                                XFont normalFont = new XFont("Arial", 8);
                                XFont kalinFont = new XFont("Arial", 8, XFontStyleEx.Bold);
                    double yPos = 30; // Points olarak

                    // Başlık
                    string baslik = $"GÜNLÜK SÜT ALIM RAPORU - {secilenTarih:dd.MM.yyyy}";
                    gfx.DrawString(baslik, baslikFont, XBrushes.Black, new XRect(0, yPos, page.Width, 20), XStringFormats.Center);
                    yPos += 25;

                    // 🥛 Depoya Alınan Sütler
                    var depoAlimlar = sutKayitlar.Where(x => x.IslemTuru == "Depoya Alım").ToList();
                    if (depoAlimlar.Any())
                    {
                        gfx.DrawString("Depoya Alınan Sütler", altBaslikFont, XBrushes.DarkSlateGray, new XRect(0, yPos, page.Width, 20), XStringFormats.Center);
                        yPos += 20;
                
                        yPos = CreatePdfTableWithPageBreakSupport(gfx, page, document, depoAlimlar, yPos, normalFont, kalinFont);
                    }

                    // 🔹 Ayraç çizgisi
                    if (depoAlimlar.Any())
                    {
                        if (yPos + 15 > page.Height - 40)
                        {
                            page = document.AddPage();
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                            gfx = XGraphics.FromPdfPage(page);
                            yPos = 30;
                        }
                
                        gfx.DrawLine(XPens.Gray, 20, yPos, page.Width - 20, yPos);
                        yPos += 15;
                    }

                    // 🚛 Direkt Sevk Yapılan Sütler
                    var direktSevk = sutKayitlar.Where(x => x.IslemTuru == "Direkt Sevk").ToList();
                    if (direktSevk.Any())
                    {
                        if (yPos + 20 > page.Height - 40)
                        {
                            page = document.AddPage();
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                            gfx = XGraphics.FromPdfPage(page);
                            yPos = 30;
                        }
                
                        gfx.DrawString("Direkt Sevk Yapılan Sütler", altBaslikFont, XBrushes.DarkSlateGray, new XRect(0, yPos, page.Width, 20), XStringFormats.Center);
                        yPos += 20;
                
                        yPos = CreatePdfTableWithPageBreakSupport(gfx, page, document, direktSevk, yPos, normalFont, kalinFont);
                    }

                    // Genel toplam
                    if (sutKayitlar.Any())
                    {
                        if (yPos + 25 > page.Height - 40)
                        {
                            page = document.AddPage();
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                            gfx = XGraphics.FromPdfPage(page);
                            yPos = 30;
                        }
                
                        yPos += 15;
                        var displayedRecords = sutKayitlar.Where(x => x.IslemTuru == "Depoya Alım" || x.IslemTuru == "Direkt Sevk").ToList();
                        var tumMiktar = displayedRecords.Sum(x => x.Miktar);
                        var genelYag = WeightedAverage(displayedRecords.Select(x => x.Yag), displayedRecords.Select(x => x.Miktar));
                        var genelProtein = WeightedAverage(displayedRecords.Select(x => x.Protein), displayedRecords.Select(x => x.Miktar));
                        var genelTKM = WeightedAverage(displayedRecords.Select(x => x.TKM), displayedRecords.Select(x => x.Miktar));
                        var genelLaktoz = WeightedAverage(displayedRecords.Select(x => x.Laktoz), displayedRecords.Select(x => x.Miktar));
                        var genelpH = WeightedAverage(displayedRecords.Select(x => x.pH), displayedRecords.Select(x => x.Miktar));
                        var genelIletkenlik = WeightedAverage(displayedRecords.Select(x => x.Iletkenlik), displayedRecords.Select(x => x.Miktar));
                        var genelDonma = WeightedAverage(displayedRecords.Select(x => x.DonmaN), displayedRecords.Select(x => x.Miktar));
                        var tumKesinti = displayedRecords.Sum(x => x.Kesinti);

                        string genelToplam = $"GENEL TOPLAM — Miktar: {tumMiktar:N2} lt | Yağ: {genelYag:N2}% | Protein: {genelProtein:N2}% | TKM: {genelTKM:N2}% | Laktoz: {genelLaktoz:N2}% | pH: {genelpH:N2} | İletkenlik: {genelIletkenlik:N2}mS | Donma Noktası: {genelDonma:N3}°C | Kesinti: {tumKesinti:N2}lt";
                
                        XSize textSize = gfx.MeasureString(genelToplam, kalinFont);
                        if (textSize.Width > (page.Width - 40))
                        {
                            gfx.DrawString(genelToplam, kalinFont, XBrushes.Black, new XRect(20, yPos, page.Width - 40, 40), XStringFormats.TopLeft);
                            yPos += 40;
                        }
                        else
                        {
                            gfx.DrawString(genelToplam, kalinFont, XBrushes.Black, new XRect(0, yPos, page.Width, 20), XStringFormats.Center);
                            yPos += 25;
                        }
                    }

                    document.Save(dosyaYolu);
                    Process.Start("explorer.exe", dosyaYolu);
                    MessageBox.Show("PDF başarıyla oluşturuldu!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF oluşturma sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // PDF Tablosu oluşturma metodu - uygun sütun genişliklerinde ve sayfa sonu desteği
        private double CreatePdfTableWithPageBreakSupport(XGraphics gfx, PdfPage page, PdfDocument document, List<SutKaydi> kayitlar, double yPos, XFont normalFont, XFont kalinFont)
{
    // Column widths in points (A4 landscape is ~842 points wide)
    double[] colWidths = {
        100,  // Tedarikçi
        50,   // Miktar (lt)
        50,   // Net Miktar (lt)
        35,   // Yağ (%)
        35,   // Protein (%)
        35,   // TKM (%)
        35,   // Laktoz (%)
        30,   // pH 
        45,   // İletkenlik (mS)
        45,   // Donma N (°C)
        40,   // Kesinti (lt)
        40,   // Antibiyotik
        50,   // Durum
        170    // Açıklama
    };

    double startX = 40; // Sol kenar boşluğu (points)
    double rowHeight = 16;

    string[] headers = { "Tedarikçi", "Miktar", "Net Miktar", "Yağ", "Prot", "TKM", "Laktoz", "pH", "İletkenlik", "Donma", "Kesinti", "Anti", "Durum", "Açıklama" };

    // Taşma kontrolü
    if (yPos + rowHeight > page.Height - 40)
    {
        page = document.AddPage();
        page.Orientation = PdfSharp.PageOrientation.Landscape;
        gfx = XGraphics.FromPdfPage(page);
        yPos = 30;
    }

    // Sütun başlıklarını çiz
    double currentX = startX;
    for (int i = 0; i < headers.Length; i++)
    {
        gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, new XRect(currentX, yPos, colWidths[i], rowHeight));
        gfx.DrawString(headers[i], kalinFont, XBrushes.Black, new XRect(currentX + 2, yPos + 2, colWidths[i] - 4, rowHeight - 4), XStringFormats.Center);
        currentX += colWidths[i];
    }

    yPos += rowHeight;

    // Veri satırlarını çiz
    foreach (var k in kayitlar)
    {
        // Taşma kontrolü
        if (yPos + rowHeight > page.Height - 40)
        {
            page = document.AddPage();
            page.Orientation = PdfSharp.PageOrientation.Landscape;
            gfx = XGraphics.FromPdfPage(page);
            yPos = 30;
            
            // Yeni sayfada başlık satırını tekrar çiz
            currentX = startX;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, new XRect(currentX, yPos, colWidths[i], rowHeight));
                gfx.DrawString(headers[i], kalinFont, XBrushes.Black, new XRect(currentX + 2, yPos + 2, colWidths[i] - 4, rowHeight - 4), XStringFormats.Center);
                currentX += colWidths[i];
            }
            yPos += rowHeight;
        }

        var rowData = new[] {
            k.TedarikciAdi ?? "-",
            k.Miktar.ToString("N2"),
            k.NetMiktar.ToString("N2"),
            k.Yag?.ToString("N2") ?? "-",
            k.Protein?.ToString("N2") ?? "-",
            k.TKM?.ToString("N2") ?? "-",
            k.Laktoz?.ToString("N2") ?? "-",
            k.pH?.ToString("N2") ?? "-",
            k.Iletkenlik?.ToString("N2") ?? "-",
            k.DonmaN?.ToString("N3") ?? "-",
            k.Kesinti.ToString("N2"),
            k.Antibiyotik ?? "-",
            k.Durumu ?? "-",
            k.Aciklama ?? "-"
        };

        currentX = startX;
        for (int i = 0; i < rowData.Length; i++)
        {
            string cellText = rowData[i];
            if (i == 13 && cellText.Length > 40) // Açıklama sütunu - increased from 20 to 40 characters
            {
                cellText = cellText.Substring(0, 37) + "...";
            }

            gfx.DrawRectangle(XPens.Black, new XRect(currentX, yPos, colWidths[i], rowHeight));
            
            XStringFormat format = i == 0 || i == 13 ? XStringFormats.CenterLeft : XStringFormats.Center;
            gfx.DrawString(cellText, normalFont, XBrushes.Black, new XRect(currentX + 2, yPos + 2, colWidths[i] - 4, rowHeight - 4), format);
            currentX += colWidths[i];
        }
        yPos += rowHeight;
    }

    // Alt toplam satırı
    if (kayitlar.Any())
    {
        if (yPos + rowHeight > page.Height - 40)
        {
            page = document.AddPage();
            page.Orientation = PdfSharp.PageOrientation.Landscape;
            gfx = XGraphics.FromPdfPage(page);
            yPos = 30;
            
            // Yeni sayfada başlık satırını tekrar çiz
            currentX = startX;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, new XRect(currentX, yPos, colWidths[i], rowHeight));
                gfx.DrawString(headers[i], kalinFont, XBrushes.Black, new XRect(currentX + 2, yPos + 2, colWidths[i] - 4, rowHeight - 4), XStringFormats.Center);
                currentX += colWidths[i];
            }
            yPos += rowHeight;
        }
        
        decimal toplamMiktar = kayitlar.Sum(x => x.Miktar);
        decimal toplamNetMiktar = kayitlar.Sum(x => x.NetMiktar);
        decimal ortYag = WeightedAverage(kayitlar.Select(x => x.Yag), kayitlar.Select(x => x.Miktar));
        decimal ortProtein = WeightedAverage(kayitlar.Select(x => x.Protein), kayitlar.Select(x => x.Miktar));
        decimal ortTKM = WeightedAverage(kayitlar.Select(x => x.TKM), kayitlar.Select(x => x.Miktar));
        decimal ortLaktoz = WeightedAverage(kayitlar.Select(x => x.Laktoz), kayitlar.Select(x => x.Miktar));
        decimal ortpH = WeightedAverage(kayitlar.Select(x => x.pH), kayitlar.Select(x => x.Miktar));
        decimal ortIletkenlik = WeightedAverage(kayitlar.Select(x => x.Iletkenlik), kayitlar.Select(x => x.Miktar));
        decimal ortDonma = WeightedAverage(kayitlar.Select(x => x.DonmaN), kayitlar.Select(x => x.Miktar));
        decimal toplamKesinti = kayitlar.Sum(x => x.Kesinti);

        var toplamData = new[] {
            "Toplam / Ortalama",
            toplamMiktar.ToString("N2"),
            toplamNetMiktar.ToString("N2"),
            ortYag.ToString("N2"),
            ortProtein.ToString("N2"),
            ortTKM.ToString("N2"),
            ortLaktoz.ToString("N2"),
            ortpH.ToString("N2"),
            ortIletkenlik.ToString("N2"),
            ortDonma.ToString("N3"),
            toplamKesinti.ToString("N2"),
            "", "", ""
        };

        currentX = startX;
        for (int i = 0; i < toplamData.Length; i++)
        {
            gfx.DrawRectangle(XPens.Black, XBrushes.LightYellow, new XRect(currentX, yPos, colWidths[i], rowHeight));
            
            XStringFormat format = i == 0 ? XStringFormats.CenterLeft : XStringFormats.Center;
            gfx.DrawString(toplamData[i], kalinFont, XBrushes.Black, new XRect(currentX + 2, yPos + 2, colWidths[i] - 4, rowHeight - 4), format);
            currentX += colWidths[i];
        }
        yPos += rowHeight + 10;
    }
    
    return yPos;
}

    }
}