using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using By_ProV2.Models;
using System.Linq;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;
using OxyPlot.Axes;


namespace By_ProV2
{
    public class PdfRaporOlusturucu
    {
        private const int SatirBaslangicY = 100;
        private const int SatirAraligi = 20;
        private const int SayfaBaslikY = 40;

        private XFont baslikFont = new XFont("Arial", 16);
        private XFont normalFont = new XFont("Arial", 9);
        private XFont kalinFont = new XFont("Arial", 9);

        public void Olustur(List<RaporSatiri> veriler, string dosyaYolu, DateTime baslangic, DateTime bitis)
        {
            PdfDocument doc = new PdfDocument();

            AddDetailPages(doc, veriler, baslangic, bitis);

            var icmalListesi = veriler
                .GroupBy(x => new { x.CariKodu, x.CariAdi })
                .Select(g => new CariIcmal
                {
                    CariKodu = g.Key.CariKodu,
                    CariAdi = g.Key.CariAdi,
                    Adet = g.Count(),
                    ToplamAlis = g.Sum(x => x.AlisTutari),
                    ToplamSatis = g.Sum(x => x.SatisTutari),
                    ToplamBrutKar = g.Sum(x => x.BrutKar)
                })
                .ToList();


            EkleIcmalSayfasi(doc, icmalListesi);
           

            doc.Save(dosyaYolu);
            Process.Start("explorer.exe", dosyaYolu);
        }



        private void AddDetailPages(PdfDocument doc, List<RaporSatiri> veriler, DateTime baslangic, DateTime bitis)
        {
            PdfPage page = doc.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            int sayfaIndex = 1;
            int yPos = SatirBaslangicY;

            EkleBaslik(gfx, page, baslangic, bitis, sayfaIndex++);

            yPos += 20;
            EkleKolonBasliklari(gfx, yPos);
            yPos += SatirAraligi;

            // ➕ Toplamlar için değişkenler
            decimal toplamAlis = 0;
            decimal toplamSatis = 0;
            decimal toplamKar = 0;
            int toplamAdet = 0;

            foreach (var satir in veriler)
            {
                if (yPos + SatirAraligi > page.MediaBox.Height - 70) // Son satıra yer bırak
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPos = SatirBaslangicY;

                    EkleBaslik(gfx, page, baslangic, bitis, sayfaIndex++);
                    yPos += 20;
                    EkleKolonBasliklari(gfx, yPos);
                    yPos += SatirAraligi;
                }

                EkleVeriSatiri(gfx, yPos, satir);
                yPos += SatirAraligi;

                // ➕ Toplamlara ekle
                toplamAlis += satir.AlisTutari;
                toplamSatis += satir.SatisTutari;
                toplamKar += satir.BrutKar;
                toplamAdet++;
            }

            // ✅ Son sayfanın altına toplamlar eklenecek
            yPos += 10;
            gfx.DrawLine(XPens.Black, 30, yPos, 580, yPos);
            yPos += 10;

            gfx.DrawString("TOPLAM", kalinFont, XBrushes.Black, 220, yPos);
            gfx.DrawString($"{toplamAlis:N2}", kalinFont, XBrushes.Black, 350, yPos);
            gfx.DrawString($"{toplamSatis:N2}", kalinFont, XBrushes.Black, 415, yPos);
            gfx.DrawString($"{toplamKar:N2}", kalinFont, XBrushes.Black, 480, yPos);

            decimal ortalamaKarYuzdesi = toplamSatis != 0 ? (toplamKar / toplamSatis) * 100 : 0;
            gfx.DrawString($"{ortalamaKarYuzdesi:N2}%", kalinFont, XBrushes.Black, 540, yPos);
        }


        private void EkleBaslik(XGraphics gfx, PdfPage page, DateTime baslangic, DateTime bitis, int sayfaNo = 1)
        {
            string tarihAraligi = $"Tarih Aralığı: {baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy}";
            gfx.DrawString("SATIŞ RAPORU", baslikFont, XBrushes.Black, new XRect(0, SayfaBaslikY, page.MediaBox.Width, 20), XStringFormats.TopCenter);
            gfx.DrawString(tarihAraligi, normalFont, XBrushes.Black, new XRect(0, SayfaBaslikY + 25, page.MediaBox.Width, 20), XStringFormats.TopCenter);
            gfx.DrawString($"Sayfa: {sayfaNo}", normalFont, XBrushes.Gray, new XRect(40, SayfaBaslikY + 25, 100, 20), XStringFormats.TopLeft);
        }

        private void EkleKolonBasliklari(XGraphics gfx, int y)
        {
            gfx.DrawString("Tarih", kalinFont, XBrushes.Black, 30, y);
            gfx.DrawString("Belge No", kalinFont, XBrushes.Black, 80, y);
            gfx.DrawString("Cari", kalinFont, XBrushes.Black, 170, y);
            gfx.DrawString("Alış", kalinFont, XBrushes.Black, 360, y);
            gfx.DrawString("Satış", kalinFont, XBrushes.Black, 420, y);
            gfx.DrawString("Kâr", kalinFont, XBrushes.Black, 480, y);
            gfx.DrawString("Kâr %", kalinFont, XBrushes.Black, 540, y);
        }

        private void EkleVeriSatiri(XGraphics gfx, int y, RaporSatiri satir)
        {
            gfx.DrawString(satir.TeslimTarihi.ToString("dd.MM.yyyy"), normalFont, XBrushes.Black, 30, y);
            gfx.DrawString(satir.BelgeKodu, normalFont, XBrushes.Black, 80, y);
            // Cari Adı'nı 25 karakterle sınırla
            string kisaltma = satir.CariAdi.Length > 32 ? satir.CariAdi.Substring(0, 32) + "..." : satir.CariAdi;
            gfx.DrawString(kisaltma, normalFont, XBrushes.Black, 170, y);

            gfx.DrawString($"{satir.AlisTutari:N2}", normalFont, XBrushes.Black, 360, y);
            gfx.DrawString($"{satir.SatisTutari:N2}", normalFont, XBrushes.Black, 420, y);
            gfx.DrawString($"{satir.BrutKar:N2}", normalFont, XBrushes.Black, 480, y);
            gfx.DrawString($"{satir.KarYuzdesi:N2}%", normalFont, XBrushes.Black, 540, y);
        }

        private void EkleIcmalSayfasi(PdfDocument doc, List<CariIcmal> icmalListesi)
        {
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            int y = 50;
            gfx.DrawString("Cari Bazında İcmal Raporu", baslikFont, XBrushes.Black, new XRect(0, y, page.MediaBox.Width, 20), XStringFormats.TopCenter);
            y += 40;

            gfx.DrawString("Cari Kodu", kalinFont, XBrushes.Black, 30, y);
            gfx.DrawString("Cari Adı", kalinFont, XBrushes.Black, 95, y);
            gfx.DrawString("Adet", kalinFont, XBrushes.Black, 325, y);
            gfx.DrawString("Toplam Alış", kalinFont, XBrushes.Black, 360, y);
            gfx.DrawString("Toplam Satış", kalinFont, XBrushes.Black, 420, y);
            gfx.DrawString("Brüt Kar", kalinFont, XBrushes.Black, 480, y);

            y += 20;

            foreach (var icmal in icmalListesi)
            {
                gfx.DrawString(icmal.CariKodu, normalFont, XBrushes.Black, 30, y);
                string kisaltma = icmal.CariAdi.Length > 40 ? icmal.CariAdi.Substring(0, 40) + "..." : icmal.CariAdi;
                gfx.DrawString(kisaltma, normalFont, XBrushes.Black, 95, y);
                gfx.DrawString(icmal.Adet.ToString(), normalFont, XBrushes.Black, 325, y);
                gfx.DrawString($"{icmal.ToplamAlis:N2}", normalFont, XBrushes.Black, new XRect(360, y - 10, 50, 20), XStringFormats.TopRight);
                gfx.DrawString($"{icmal.ToplamSatis:N2}", normalFont, XBrushes.Black, new XRect(420, y - 10, 50, 20), XStringFormats.TopRight);
                gfx.DrawString($"{icmal.ToplamBrutKar:N2}", normalFont, XBrushes.Black, new XRect(460, y - 10, 60, 20), XStringFormats.TopRight);
                y += 20;

                if (y > page.MediaBox.Height - 70)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 50;
                }
            }

            // Toplamlar
            decimal toplamAlis = icmalListesi.Sum(x => x.ToplamAlis);
            decimal toplamSatis = icmalListesi.Sum(x => x.ToplamSatis);
            decimal toplamKar = icmalListesi.Sum(x => x.ToplamBrutKar);
            int toplamAdet = icmalListesi.Sum(x => x.Adet);

            y += 20;
            gfx.DrawLine(XPens.Black, 30, y, 540, y);
            y += 20;

            gfx.DrawString("TOPLAM", kalinFont, XBrushes.Black, 75, y);
            gfx.DrawString(toplamAdet.ToString(), kalinFont, XBrushes.Black, 325, y);
            gfx.DrawString($"{toplamAlis:N2}", kalinFont, XBrushes.Black, new XRect(360, y - 10, 50, 20), XStringFormats.TopRight);
            gfx.DrawString($"{toplamSatis:N2}", kalinFont, XBrushes.Black, new XRect(420, y - 10, 50, 20), XStringFormats.TopRight);
            gfx.DrawString($"{toplamKar:N2}", kalinFont, XBrushes.Black, new XRect(460, y - 10, 60, 20), XStringFormats.TopRight);

            y += 60; // tablo ve legend arasında boşluk

            // Sayfa taşması kontrolü
            if (y + 300 > page.MediaBox.Height)
            {
                page = doc.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = 50;
            }

            // Pie chart oluşturuluyor
            var pieModel = new PlotModel
            {
                Title = "Cari Bazında Brüt Kar Dağılımı",
                TitleFontSize = 42,
                Background = OxyColors.White
            };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1.0,
                InsideLabelFormat = "",
                OutsideLabelFormat = "",
                FontSize = 20,
                TickHorizontalLength = 0,
                TickRadialLength = 0,
                AngleSpan = 360,
                StartAngle = 0
            };

            // Renk paleti
            var renkler = new[]
            {
                OxyColors.Red, OxyColors.Green, OxyColors.Blue,
                OxyColors.Orange, OxyColors.Purple, OxyColors.Brown,
                OxyColors.Teal, OxyColors.Pink, OxyColors.YellowGreen,
                OxyColors.SkyBlue
            };

            int renkIndex = 0;

            foreach (var icmal in icmalListesi)
            {
                if (icmal.ToplamBrutKar > 0)
                {
                    var renk = renkler[renkIndex % renkler.Length]; // Renkleri döngüyle kullan
                    renkIndex++;

                    var slice = new PieSlice(Kisalt(icmal.CariAdi, 18), (double)icmal.ToplamBrutKar)
                    {
                        Fill = renk
                    };

                    pieSeries.Slices.Add(slice);
                }
            }


            pieModel.Series.Add(pieSeries);

            // Grafik çizimi
            int grafikX = 125;
            int grafikY = y;
            int exportWidth = 1200;
            int exportHeight = 900;
            int drawWidth = 300;
            int drawHeight = 225;

            using (var ms = new MemoryStream())
            {
                var exporter = new PngExporter
                {
                    Width = exportWidth,
                    Height = exportHeight
                };
                exporter.Export(pieModel, ms);
                ms.Position = 0;

                var image = XImage.FromStream(ms);

                gfx.DrawRectangle(XPens.LightGray, grafikX - 15, grafikY - 15, drawWidth + 50, drawHeight + 50);
                gfx.DrawImage(image, grafikX, grafikY, drawWidth, drawHeight);
            }

            // Legend, tablonun altına, grafik çiziminden bağımsız
            int startX = 50;
            int startY = y + drawHeight + 50;
            int columnCount = 3;
            int columnWidth = 170;
            int rowHeight = 18;
            int kutuBoyutu = 10;
            int textOffsetY = kutuBoyutu / 2 + 1;

            double toplamBrutKar = pieSeries.Slices.Sum(s => s.Value);

            for (int i = 0; i < pieSeries.Slices.Count; i++)
            {
                var slice = pieSeries.Slices[i];
                if (slice.Value <= 0) continue;

                int col = i % columnCount;
                int row = i / columnCount;

                int x = startX + (col * columnWidth);
                int yRow = startY + (row * rowHeight);

                // Sayfa taşması kontrolü
                if (yRow > page.MediaBox.Height - 30)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    startY = 50;
                    row = 0;
                    yRow = startY;
                }

                var renk = XColor.FromArgb(slice.Fill.R, slice.Fill.G, slice.Fill.B);
                var brush = new XSolidBrush(renk);

                // Renkli kutu
                gfx.DrawRectangle(brush, x, yRow, kutuBoyutu, kutuBoyutu);

                // Etiket: Ad + yüzde (dikey ortalanmış)
                double yuzde = toplamBrutKar > 0 ? (slice.Value / toplamBrutKar) * 100 : 0;
                string labelText = $"{slice.Label} - %{yuzde:F1}";

                gfx.DrawString(labelText, normalFont, XBrushes.Black, x + kutuBoyutu + 5, yRow + textOffsetY);
            }
        }

            private string Kisalt(string metin, int uzunluk)
        {
            return metin.Length > uzunluk ? metin.Substring(0, uzunluk) + "..." : metin;
        }





    }
    public class CariIcmal
    {
        public string CariKodu { get; set; }
        public string CariAdi { get; set; }
        public int Adet { get; set; }
        public decimal ToplamAlis { get; set; }
        public decimal ToplamSatis { get; set; }
        public decimal ToplamBrutKar { get; set; }
    }
}

