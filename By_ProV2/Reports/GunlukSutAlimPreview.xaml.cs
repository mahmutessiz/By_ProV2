using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using By_ProV2.DataAccess;
using By_ProV2.Models;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Controls;

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

            // Genel toplam
            if (sutKayitlar.Any())
            {
                var tumMiktar = sutKayitlar.Sum(x => x.Miktar);
                var genelYag = WeightedAverage(sutKayitlar.Select(x => x.Yag), sutKayitlar.Select(x => x.Miktar));
                var genelProtein = WeightedAverage(sutKayitlar.Select(x => x.Protein), sutKayitlar.Select(x => x.Miktar));
                var genelTKM = WeightedAverage(sutKayitlar.Select(x => x.TKM), sutKayitlar.Select(x => x.Miktar));
                var genelLaktoz = WeightedAverage(sutKayitlar.Select(x => x.Laktoz), sutKayitlar.Select(x => x.Miktar));
                var genelpH = WeightedAverage(sutKayitlar.Select(x => x.pH), sutKayitlar.Select(x => x.Miktar));
                var genelIletkenlik = WeightedAverage(sutKayitlar.Select(x => x.Iletkenlik), sutKayitlar.Select(x => x.Miktar));
                var genelDonma = WeightedAverage(sutKayitlar.Select(x => x.DonmaN), sutKayitlar.Select(x => x.Miktar));
                var tumKesinti = sutKayitlar.Sum(x => x.Kesinti);

                Paragraph genelToplam = new Paragraph(new Bold(new Run(
                    $"GENEL TOPLAM — Miktar: {tumMiktar:N2} lt | Yağ: {genelYag:N2}% | Protein: {genelProtein:N2}% | TKM: {genelTKM:N2}% | Laktoz: {genelLaktoz:N2}% | pH: {genelpH:N2} | İletkenlik: {genelIletkenlik:N2}mS | Donma Noktası: {genelDonma:N2}°C | Kesinti: {tumKesinti:N2}lt"
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

            // Kolonları oluştur ve genişliklerini ayarla
            TableColumn col1 = new TableColumn { Width = new GridLength(400) }; // Tedarikçi
            TableColumn col2 = new TableColumn { Width = new GridLength(80) };  // Miktar
            TableColumn col3 = new TableColumn { Width = new GridLength(60) };  // Yağ
            TableColumn col4 = new TableColumn { Width = new GridLength(60) };  // Protein
            TableColumn col5 = new TableColumn { Width = new GridLength(60) };  // TKM
            TableColumn col6 = new TableColumn { Width = new GridLength(60) };  // Laktoz
            TableColumn col7 = new TableColumn { Width = new GridLength(60) };  // pH
            TableColumn col8 = new TableColumn { Width = new GridLength(60) };  // İletkenlik
            TableColumn col9 = new TableColumn { Width = new GridLength(60) };  // Donma Noktası
            TableColumn col10 = new TableColumn { Width = new GridLength(60) }; // Kesinti
            TableColumn col11 = new TableColumn { Width = new GridLength(60) }; // Antibiyotik
            TableColumn col12 = new TableColumn { Width = new GridLength(75) };  // Durum
            TableColumn col13 = new TableColumn { Width = new GridLength(200) }; // Açıklama

            table.Columns.Add(col1);
            table.Columns.Add(col2);
            table.Columns.Add(col3);
            table.Columns.Add(col4);
            table.Columns.Add(col5);
            table.Columns.Add(col6);
            table.Columns.Add(col7);
            table.Columns.Add(col8);
            table.Columns.Add(col9);
            table.Columns.Add(col10);
            table.Columns.Add(col11);
            table.Columns.Add(col12);
            table.Columns.Add(col13);

            // Başlık satırı
            TableRowGroup headerGroup = new TableRowGroup();
            TableRow headerRow = new TableRow();
            string[] headers = { "Tedarikçi", "Miktar (lt)", "Yağ (%)", "Protein (%)", "TKM (%)", "Laktoz (%)", "pH ", "İletkenlik (mS)", "Donma Noktası (°C)", "Kesinti (lt)", "Antibiyotik", "Durum", "Açıklama" };

            foreach (var h in headers)
            {
                headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(h))))
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(5)
                });
            }
            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            // Veri satırları
            TableRowGroup dataGroup = new TableRowGroup();
            foreach (var k in kayitlar)
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.TedarikciAdi ?? "-"))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Miktar.ToString("N2")))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Yag?.ToString("N2") ?? "-"))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Protein?.ToString("N2") ?? "-"))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.TKM?.ToString("N2") ?? "-"))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Laktoz?.ToString("N2") ?? "-"))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.pH?.ToString("N2") ?? "-"))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Iletkenlik?.ToString("N2") ?? "-"))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.DonmaN?.ToString("N2") ?? "-"))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Kesinti.ToString("N2")))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Antibiyotik ?? "-"))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Durumu ?? "-"))) { Padding = new Thickness(5) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(k.Aciklama ?? "-"))) { Padding = new Thickness(5) });
                dataGroup.Rows.Add(row);
            }

            // Alt toplam satırı
            decimal toplamMiktar = kayitlar.Sum(x => x.Miktar);
            decimal ortYag = WeightedAverage(kayitlar.Select(x => x.Yag), kayitlar.Select(x => x.Miktar));
            decimal ortProtein = WeightedAverage(kayitlar.Select(x => x.Protein), kayitlar.Select(x => x.Miktar));
            decimal ortTKM = WeightedAverage(kayitlar.Select(x => x.TKM), kayitlar.Select(x => x.Miktar));
            decimal ortLaktoz = WeightedAverage(kayitlar.Select(x => x.Laktoz), kayitlar.Select(x => x.Miktar));
            decimal ortpH = WeightedAverage(kayitlar.Select(x => x.pH), kayitlar.Select(x => x.Miktar));
            decimal ortIletkenlik = WeightedAverage(kayitlar.Select(x => x.Iletkenlik), kayitlar.Select(x => x.Miktar));
            decimal ortDonma = WeightedAverage(kayitlar.Select(x => x.DonmaN), kayitlar.Select(x => x.Miktar));
            decimal toplamKesinti = kayitlar.Sum(x => x.Kesinti);

            TableRow toplamRow = new TableRow();
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Toplam / Ortalama")))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(toplamMiktar.ToString("N2"))))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortYag.ToString("N2"))))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortProtein.ToString("N2"))))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortTKM.ToString("N2"))))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortLaktoz.ToString("N2"))))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortpH.ToString("N2"))))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortIletkenlik.ToString("N2"))))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(ortDonma.ToString("N2"))))) { Padding = new Thickness(5) });
            toplamRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(toplamKesinti.ToString("N2"))))) { Padding = new Thickness(5) });

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

                    // FlowDocument'e uygula
                    document.PageWidth = pageWidth;
                    document.PageHeight = pageHeight;
                    document.PagePadding = new Thickness(50);
                    document.ColumnWidth = pageWidth; // tek sütun
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

    }
}