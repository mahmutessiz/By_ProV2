using System;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using By_ProV2.Models;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using By_ProV2.ViewModels;

namespace By_ProV2.Helpers
{
    public static class SqlCommandExtensions
    {
        public static SqlCommand AddParam(this SqlCommand cmd, string paramName, object value)
        {
            cmd.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
            return cmd;
        }
    }

    public static class PdfGenerator
    {


        // Sipariş Formu oluştur
        public static void OlusturSiparisFormu(List<KalemModel> kalemler, CariModel cari, CariModel teslimatCari, string dosyaAdi, string siparisNo, string belgeKodu, MainViewModel vm,  bool onizlemeModu = false)
        {
            OlusturPdf(kalemler, cari, teslimatCari, dosyaAdi, "Sipariş Formu", siparisNo,
                vm.SiparisTarihi != default ? vm.SiparisTarihi : DateTime.Now,
                vm.SevkTarihi != default ? vm.SevkTarihi : DateTime.Now.AddDays(3),
                vm.OdemeYontemi ?? "Belirtilmedi",
                vm.Aciklama1 ?? "", vm.Aciklama2 ?? "", vm.Aciklama3 ?? "", belgeKodu,
                onizlemeModu,
                vm.IsFabrikaTeslim,       // ✅ EKLENDİ (bool)
                vm.Aciklama4 ?? "",        // ✅ EKLENDİ (Şoför)
                vm.Aciklama5 ?? "",        // ✅ EKLENDİ (Plaka 1)
                vm.Aciklama6 ?? ""         // ✅ EKLENDİ (Plaka 2)
                );
        }


        // Proforma Fatura oluştur
        public static void OlusturProformaFatura(List<KalemModel> kalemler, CariModel cari, CariModel teslimatCari, string dosyaAdi, string proformaNo, string belgeKodu, MainViewModel vm, bool onizlemeModu = false)
        {
            OlusturPdf(kalemler, cari, teslimatCari, dosyaAdi, "Proforma Fatura", proformaNo,
                vm.SiparisTarihi != default ? vm.SiparisTarihi : DateTime.Now,
                vm.SevkTarihi != default ? vm.SevkTarihi : DateTime.Now.AddDays(3),
                vm.ProformaOdemeYontemi ?? "Belirtilmedi",
                vm.Aciklama1 ?? "", vm.Aciklama2 ?? "", vm.Aciklama3 ?? "", belgeKodu,
                onizlemeModu,
                vm.IsFabrikaTeslim,       // ✅ EKLENDİ
                vm.Aciklama4 ?? "",
                vm.Aciklama5 ?? "",
                vm.Aciklama6 ?? "");
        }

        public static void OlusturOnizlemeBelgesi(List<KalemModel> siparisKalemler, CariModel siparisCari,
                                          List<KalemModel> proformaKalemler, CariModel proformaCari,
                                          CariModel teslimatCari, string siparisNo, string proformaNo, string belgeKodu, MainViewModel vm)
        {
            var doc = new Document();

            // 1️⃣ Sipariş Formu sayfası
            OlusturPdfSayfa(doc, siparisKalemler, siparisCari, teslimatCari, "Sipariş Formu", siparisNo, belgeKodu,
                vm.SiparisTarihi != default ? vm.SiparisTarihi : DateTime.Now,
                vm.SevkTarihi != default ? vm.SevkTarihi : DateTime.Now.AddDays(3),
                vm.OdemeYontemi ?? "Belirtilmedi",
                vm.Aciklama1, vm.Aciklama2, vm.Aciklama3,
                bankaBilgileriGoster: true, // sadece proforma'da göster
                isFabrikaTeslim: vm.IsSatisFabrikaTeslim,
                aciklama4: vm.Aciklama4,
                aciklama5: vm.Aciklama5,
                aciklama6: vm.Aciklama6
            );

            
            // 2️⃣ Proforma Fatura sayfası
            OlusturPdfSayfa(doc, proformaKalemler, proformaCari, teslimatCari, "Proforma Fatura", proformaNo, belgeKodu,
                vm.SiparisTarihi != default ? vm.SiparisTarihi : DateTime.Now,
                vm.SevkTarihi != default ? vm.SevkTarihi : DateTime.Now.AddDays(3),
                vm.ProformaOdemeYontemi ?? "Belirtilmedi",
                vm.Aciklama1, vm.Aciklama2, vm.Aciklama3,
                bankaBilgileriGoster: true, // banka bilgilerini sadece proforma'da göster
                isFabrikaTeslim: vm.IsSatisFabrikaTeslim,
                aciklama4: vm.Aciklama4,
                aciklama5: vm.Aciklama5,
                aciklama6: vm.Aciklama6
            );

            // PDF render ve geçici dosyada aç
            var renderer = new PdfDocumentRenderer()
            {
                Document = doc
            };
            renderer.RenderDocument();

            var tempFilePath = Path.Combine(Path.GetTempPath(), $"Onizleme_{DateTime.Now.Ticks}.pdf");
            renderer.PdfDocument.Save(tempFilePath);

            // Aç
            Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
        }
        private static void OlusturPdfSayfa(Document doc, List<KalemModel> kalemler, CariModel cari, CariModel teslimatCari,
    string baslik, string belgeNo, string belgeKodu, DateTime siparisTarihi, DateTime sevkTarihi, string odemeYontemi,
    string aciklama1, string aciklama2, string aciklama3,
    bool bankaBilgileriGoster = false,
    bool isFabrikaTeslim = false, string aciklama4 = "", string aciklama5 = "", string aciklama6 = "")
        {
            var section = doc.AddSection();

            section.PageSetup.TopMargin = Unit.FromCentimeter(1);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2);

            // Başlık Stili
            var headingStyle = doc.Styles["Heading1"];
            headingStyle.Font.Size = 16;
            headingStyle.Font.Bold = true;
            headingStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            // Logo ve Firma Bilgileri
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Logo1.png");
            var headerTable = section.AddTable();
            headerTable.Borders.Visible = false;
            headerTable.AddColumn(Unit.FromCentimeter(8));
            headerTable.AddColumn(Unit.FromCentimeter(9));
            var headerRow = headerTable.AddRow();

            var logoCell = headerRow.Cells[0];
            logoCell.VerticalAlignment = VerticalAlignment.Top;
            if (File.Exists(logoPath))
            {
                var logo = logoCell.AddImage(logoPath);
                logo.Width = Unit.FromCentimeter(4);
                logo.LockAspectRatio = true;
            }
            else
            {
                logoCell.AddParagraph("LOGO").Format.Font.Bold = true;
            }

            var firmaCell = headerRow.Cells[1];
            firmaCell.VerticalAlignment = VerticalAlignment.Top;
            var firmaPar = firmaCell.AddParagraph();
            firmaPar.Format.Font.Size = 8;
            firmaPar.Format.Alignment = ParagraphAlignment.Right;
            firmaPar.AddText("BERYEM Tarım Ürünleri\nGıda Nak. Tic. Ltd. Şti\nİstiklal Osb Mah. 77769 Sok. No:4/1\nÇumra Konya");

            section.AddParagraph("\n");

            // Üst Bilgi Tablosu
            var infoTable = section.AddTable();
            infoTable.AddColumn(Unit.FromCentimeter(8.5));
            infoTable.AddColumn(Unit.FromCentimeter(8.5));
            var row = infoTable.AddRow();
            row.Height = Unit.FromCentimeter(2.5);
            row.HeightRule = RowHeightRule.Exactly;

            // Cari Bilgileri
            var cariCell = row.Cells[0];
            cariCell.Borders.Width = 0.75;
            cariCell.Borders.Color = Colors.Gray;
            var cariPar = cariCell.AddParagraph();
            cariPar.Format.Font.Size = 11;
            cariPar.Format.Font.Name = "Arial";
            if (cari != null)
            {
                cariPar.AddText($"Ad: {cari.CariAdi}\n");
                cariPar.AddText($"Adres: {cari.Adres}\n");
                cariPar.AddText($"Vergi No: {cari.VergiNo}\n");
                cariPar.AddText($"Telefon: {cari.Telefon}");
            }

            // Form Bilgileri
            var formCell = row.Cells[1];
            formCell.Borders.Visible = false;
            formCell.Format.Font.Size = 11;
            formCell.Format.Font.Name = "Arial";

            var formTable = new Table();
            formTable.Borders.Visible = false;
            formTable.AddColumn(Unit.FromCentimeter(4));
            formTable.AddColumn(Unit.FromCentimeter(4.5));

            var formRows = new (string Label, string Value)[]
            {
        ($"{baslik.ToUpper()} No:", belgeNo),
        ("Belge Kodu:", belgeKodu),
        ("Sipariş Tarihi:", siparisTarihi.ToString("dd.MM.yyyy")),
        ("Sevk Tarihi:", sevkTarihi.ToString("dd.MM.yyyy")),
        ("Ödeme Yöntemi:", odemeYontemi)
            };

            for (int i = 0; i < formRows.Length; i++)
            {
                var item = formRows[i];
                var formRow = formTable.AddRow();

                var labelPar = formRow.Cells[0].AddParagraph(item.Label);
                if (i == 0)
                {
                    labelPar.Format.Font.Size = 9;
                }

                var valPar = formRow.Cells[1].AddParagraph();
                valPar.Format.Alignment = ParagraphAlignment.Right;
                valPar.AddText(item.Value);
            }

            formCell.Elements.Add(formTable);

            section.AddParagraph("\n\n");

            // Başlık
            var baslikPar = section.AddParagraph(baslik.ToUpper(), "Heading1");
            baslikPar.Format.SpaceBefore = Unit.FromCentimeter(0.5);
            baslikPar.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            // Kalemler Tablosu
            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Black;
            table.Rows.LeftIndent = Unit.FromCentimeter(-0.2);

            var columns = new[]
            {
        ("Sıra", 1.0),
        ("Stok Adı", 4.5),
        ("Birim", 1.0),
        ("Miktar", 1.5),
        ("Birim Fiyat", 2.0),
        ("İskontolar", 2.0),
        ("Nak. İsk", 1.7),
        ("KDV", 1.0),
        ("Tutar", 2.5)
    };

            foreach (var col in columns)
                table.AddColumn(Unit.FromCentimeter(col.Item2));

            var tabloheaderRow = table.AddRow();
            tabloheaderRow.Shading.Color = Colors.LightGray;
            tabloheaderRow.Format.Font.Bold = true;
            tabloheaderRow.Format.Alignment = ParagraphAlignment.Center;

            for (int i = 0; i < columns.Length; i++)
                tabloheaderRow.Cells[i].AddParagraph(columns[i].Item1);

            int siraNo = 1;
            foreach (var k in kalemler)
            {
                var r = table.AddRow();
                r.Height = Unit.FromCentimeter(1.7);
                r.HeightRule = RowHeightRule.Exactly;
                r.VerticalAlignment = VerticalAlignment.Center;

                r.Cells[0].AddParagraph(siraNo.ToString()).Format.Alignment = ParagraphAlignment.Center;
                r.Cells[1].AddParagraph(k.StokAdi);
                r.Cells[2].AddParagraph(k.Birim).Format.Alignment = ParagraphAlignment.Center;
                r.Cells[3].AddParagraph(k.Miktar.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
                r.Cells[4].AddParagraph(k.BirimFiyat.ToString("C2")).Format.Alignment = ParagraphAlignment.Right;
                r.Cells[5].AddParagraph($"{k.Isk1}%\n{k.Isk2}%\n{k.Isk3}%\n{k.Isk4}%").Format.Alignment = ParagraphAlignment.Right;
                r.Cells[6].AddParagraph($"{k.NakliyeIskonto}%").Format.Alignment = ParagraphAlignment.Right;
                r.Cells[7].AddParagraph($"{k.KDV}%").Format.Alignment = ParagraphAlignment.Right;
                r.Cells[8].AddParagraph(k.Tutar.ToString("C2")).Format.Alignment = ParagraphAlignment.Right;

                foreach (var cell in r.Cells)
                {
                    r.Cells[0].Format.Font.Size = 9;
                    r.Cells[0].Format.Font.Name = "Arial";
                    r.Cells[0].VerticalAlignment = VerticalAlignment.Center;
                }

                siraNo++;
            }

            section.AddParagraph("\n");

            // Toplamlar
            var toplamPar = section.AddParagraph();
            toplamPar.Format.TabStops.AddTabStop("4.2cm", TabAlignment.Left);
            toplamPar.Format.TabStops.AddTabStop("17cm", TabAlignment.Right);
            decimal toplamMiktar = kalemler.Sum(k => k.Miktar);
            decimal toplamTutar = kalemler.Sum(k => k.Tutar);
            toplamPar.AddText("\t");
            toplamPar.AddFormattedText($"Toplam Miktar: {toplamMiktar:N2}", TextFormat.Bold);
            toplamPar.AddText("\t");
            toplamPar.AddFormattedText($"Toplam Tutar: {toplamTutar:C2}", TextFormat.Bold);

            // Sevk Bilgileri
            if (teslimatCari != null)
            {
                section.AddParagraph("\n");
                var sevkPar = section.AddParagraph("Sevk Adresi:");
                sevkPar.Format.Font.Bold = true;
                var sevkDetayPar = section.AddParagraph();
                sevkDetayPar.AddText($"Ad: {teslimatCari.TeslimatAdi}\nAdres: {teslimatCari.TeslimatAdres}\nTelefon: {teslimatCari.TeslimatTelefon}\nYetkili: {teslimatCari.TeslimatYetkili}");
            }

            // 🔽 Fabrika Teslim Bilgisi (EKLENDİ)
            if (isFabrikaTeslim)
            {
                section.AddParagraph("\n");
                var fabrikaPar = section.AddParagraph("Fabrika Teslim:");
                fabrikaPar.Format.Font.Bold = true;
                fabrikaPar.Format.Font.Size = 13;

                if (!string.IsNullOrEmpty(aciklama4))
                    section.AddParagraph($"Şoför: {aciklama4}");

                if (!string.IsNullOrEmpty(aciklama5))
                    section.AddParagraph($"Plaka 1: {aciklama5}");

                if (!string.IsNullOrEmpty(aciklama6))
                    section.AddParagraph($"Plaka 2: {aciklama6}");
            }

            // Açıklamalar
            if (!string.IsNullOrEmpty(aciklama1)) section.AddParagraph($"Açıklama 1: {aciklama1}");
            if (!string.IsNullOrEmpty(aciklama2)) section.AddParagraph($"Açıklama 2: {aciklama2}");
            if (!string.IsNullOrEmpty(aciklama3)) section.AddParagraph($"Açıklama 3: {aciklama3}");

            // Banka Bilgileri (Sadece Proforma için)
            if (bankaBilgileriGoster)
            {
                var footer = section.Footers.Primary;
                footer.Format.Font.Size = 8;
                footer.Format.Alignment = ParagraphAlignment.Center;

                var bankaTable = footer.AddTable();
                bankaTable.Borders.Width = 0.75;
                bankaTable.Borders.Color = Colors.Gray;
                bankaTable.Rows.LeftIndent = Unit.FromCentimeter(0);

                bankaTable.AddColumn(Unit.FromCentimeter(5));
                bankaTable.AddColumn(Unit.FromCentimeter(6));
                bankaTable.AddColumn(Unit.FromCentimeter(6));

                var bankaheaderRow = bankaTable.AddRow();
                bankaheaderRow.Shading.Color = Colors.LightGray;
                bankaheaderRow.Format.Font.Bold = true;
                bankaheaderRow.Format.Alignment = ParagraphAlignment.Center;

                bankaheaderRow.Cells[0].AddParagraph("Banka Adı");
                bankaheaderRow.Cells[1].AddParagraph("IBAN");
                bankaheaderRow.Cells[2].AddParagraph("Hesap");

                var row1 = bankaTable.AddRow();
                row1.Cells[0].AddParagraph("Kuveyttürk Katılım Bankası");
                row1.Cells[1].AddParagraph("TR04 0020 5000 0999 1337 2000 01");
                row1.Cells[2].AddParagraph("TL");

                var row2 = bankaTable.AddRow();
                row2.Cells[0].AddParagraph("Ziraat Katılım Bankası");
                row2.Cells[1].AddParagraph("TR98 0020 9000 0224 3871 0000 01");
                row2.Cells[2].AddParagraph("TL");

                foreach (Row r in bankaTable.Rows)
                {
                    foreach (Cell c in r.Cells)
                    {
                        c.Format.Font.Size = 9;
                        c.Format.Font.Name = "Arial";
                        c.VerticalAlignment = VerticalAlignment.Center;
                        c.Format.Alignment = ParagraphAlignment.Left;
                    }
                }

                footer.AddParagraph("\nwww.beryem.com.tr | info@beryem.com | Tel: 0501 546 42 41");
            }
        }


        // Ortak PDF oluşturma metodu
        private static void OlusturPdf(List<KalemModel> kalemler, CariModel cari, CariModel teslimatCari, string dosyaAdi,
            string baslik, string belgeNo, DateTime siparisTarihi, DateTime sevkTarihi, string odemeYontemi,
            string aciklama1, string aciklama2, string aciklama3, string belgeKodu, bool onizlemeModu = false,
            bool isFabrikaTeslim = false, string aciklama4 = "", string aciklama5 = "", string aciklama6 = ""             
            )
        {
            var doc = new Document();
            var section = doc.AddSection();

            section.PageSetup.TopMargin = Unit.FromCentimeter(1);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2);

            // Başlık Stili
            var headingStyle = doc.Styles["Heading1"];
            headingStyle.Font.Size = 16;
            headingStyle.Font.Bold = true;
            headingStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            // Logo
            // Üstte logo ve firma bilgileri için tablo
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Logo1.png");

            var headerTable = section.AddTable();
            headerTable.Borders.Visible = false;
            headerTable.AddColumn(Unit.FromCentimeter(8));  // Logo için
            headerTable.AddColumn(Unit.FromCentimeter(9));  // Firma adı için

            var headerRow = headerTable.AddRow();

            // Sol hücre: LOGO
            var logoCell = headerRow.Cells[0];
            logoCell.VerticalAlignment = VerticalAlignment.Top;

            if (File.Exists(logoPath))
            {
                var logo = logoCell.AddImage(logoPath);
                logo.Width = Unit.FromCentimeter(4);
                logo.LockAspectRatio = true;
            }
            else
            {
                var logoPar = logoCell.AddParagraph("LOGO");
                logoPar.Format.Font.Bold = true;
                logoPar.Format.Alignment = ParagraphAlignment.Left;
            }

            section.AddParagraph("\n\n");

            // Sağ hücre: Firma Bilgileri
            var firmaCell = headerRow.Cells[1];
            firmaCell.VerticalAlignment = VerticalAlignment.Top;

            var firmaPar = firmaCell.AddParagraph();
            firmaPar.Format.Font.Size = 8;
            firmaPar.Format.Font.Name = "Arial";
            firmaPar.Format.Alignment = ParagraphAlignment.Right;
            firmaPar.AddText("\n");
            firmaPar.AddText("BERYEM Tarım Ürünleri\n");
            firmaPar.AddText("Gıda Nak. Tic. Ltd. Şti\n");
            firmaPar.AddText("İstiklal Osb Mah. 77769 Sok. No:4/1\n");
            firmaPar.AddText("Çumra Konya");


            section.AddParagraph("\n");

            
            // Üst Bilgi Tablosu
            var infoTable = section.AddTable();            
            infoTable.AddColumn(Unit.FromCentimeter(8.5));
            infoTable.AddColumn(Unit.FromCentimeter(8.5));

            var row = infoTable.AddRow();
            row.Height = Unit.FromCentimeter(2.5); // istediğin yüksekliği buraya gir
            row.HeightRule = RowHeightRule.Exactly;

            // Sol hücre: CARİ Bilgileri → kenarlıklı
            var cariCell = row.Cells[0];
            cariCell.Borders.Width = 0.75;
            cariCell.Borders.Color = Colors.Gray;
            var cariPar = cariCell.AddParagraph();
            cariPar.Format.Font.Size = 11;
            cariPar.Format.Font.Name = "Arial";

            if (cari != null)
            {
                cariPar.AddText($"Ad: {cari.CariAdi}\n");
                cariPar.AddText($"Adres: {cari.Adres}\n");
                cariPar.AddText($"Vergi No: {cari.VergiNo}\n");
                cariPar.AddText($"Telefon: {cari.Telefon}");
            }
            
            // Sağ hücre: FORM Bilgileri → kenarlık yok
            var formCell = row.Cells[1];
            formCell.Borders.Visible = false;           
            formCell.Format.Font.Size = 11;
            formCell.Format.Font.Name = "Arial";

            // Form bilgileri için iç tablo
            var formTable = new Table();
            formTable.Borders.Visible = false;
            formTable.AddColumn(Unit.FromCentimeter(4));     // Etiket sütunu
            formTable.AddColumn(Unit.FromCentimeter(4.5));   // Değer sütunu

            var formRows = new (string Label, string Value)[]
            {
                ($"{baslik.ToUpper()} No:", belgeNo),
                ("Belge Kodu:", belgeKodu),
                ("Sipariş Tarihi:", siparisTarihi != DateTime.MinValue ? siparisTarihi.ToString("dd.MM.yyyy") : "-"),
                ("Sevk Tarihi:", sevkTarihi != DateTime.MinValue ? sevkTarihi.ToString("dd.MM.yyyy") : "-"),
                ("Ödeme Yöntemi:", string.IsNullOrEmpty(odemeYontemi) ? "-" : odemeYontemi)
            };

            for (int i = 0; i < formRows.Length; i++)
            {
                var item = formRows[i];
                var formRow = formTable.AddRow();

                // Etiket
                var labelPar = formRow.Cells[0].AddParagraph(item.Label);
                labelPar.Format.Alignment = ParagraphAlignment.Left;

                // 🔽 Sadece BELGE NO (ilk satır) için puntoyu 9 yap
                if (i == 0)
                    labelPar.Format.Font.Size = 8;
                else
                    labelPar.Format.Font.Size = 11;

                // Değer
                var valuePar = formRow.Cells[1].AddParagraph();
                valuePar.Format.LeftIndent = Unit.FromCentimeter(0.2);
                valuePar.Format.RightIndent = Unit.FromCentimeter(0.2);
                valuePar.Format.Alignment = ParagraphAlignment.Right;

                // 🔥 Ödeme Yöntemi satırı için özel punto ayarı
                if (item.Label.StartsWith("Ödeme Yöntemi"))
                {
                    valuePar.Format.Font.Size = 14;
                    valuePar.Format.Font.Bold = true;
                }
                else
                {
                    valuePar.Format.Font.Size = 11;
                }

                valuePar.AddText(item.Value);
            }

            // Tablonun kendisini form hücresine ekle
            formCell.Elements.Add(formTable);

            // Araya daha fazla boşluk ekleyerek başlık alanını yükseltiyoruz
            section.AddParagraph("\n\n");


            // Başlık
            var baslikPar = section.AddParagraph(baslik.ToUpper(), "Heading1");
            baslikPar.Format.SpaceBefore = Unit.FromCentimeter(0.5);
            baslikPar.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            // Başlık altına ekstra boşluk da bırakabiliriz
            //section.AddParagraph("\n\n");


            // Kalemler Tablosu
            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Black;
            table.Rows.LeftIndent = Unit.FromCentimeter(-0.2);

            var columns = new[]
            {
                ("Sıra", 1.0),
                ("Stok Adı", 4.5),
                ("Birim", 1.0),
                ("Miktar", 1.5),
                ("Birim Fiyat", 2.0),
                ("İskontolar", 2.0),
                ("Nak. İsk", 1.7),
                ("KDV", 1.0),
                ("Tutar", 2.5)
            };

            foreach (var col in columns)
                table.AddColumn(Unit.FromCentimeter(col.Item2));

            var kalemheaderRow = table.AddRow();
            kalemheaderRow.Shading.Color = Colors.LightGray;
            kalemheaderRow.HeadingFormat = true;
            kalemheaderRow.Format.Font.Bold = true;
            kalemheaderRow.Format.Alignment = ParagraphAlignment.Center;
            kalemheaderRow.VerticalAlignment = VerticalAlignment.Center;

            for (int i = 0; i < columns.Length; i++)
                kalemheaderRow.Cells[i].AddParagraph(columns[i].Item1);

            int siraNo = 1;
            foreach (var k in kalemler)
            {
                var r = table.AddRow();
                r.Height = Unit.FromCentimeter(1.7);
                r.HeightRule = RowHeightRule.Exactly;
                r.VerticalAlignment = VerticalAlignment.Center;

                r.Cells[0].AddParagraph(siraNo.ToString()).Format.Alignment = ParagraphAlignment.Center;
                r.Cells[1].AddParagraph(k.StokAdi).Format.Alignment = ParagraphAlignment.Left;
                r.Cells[2].AddParagraph(k.Birim).Format.Alignment = ParagraphAlignment.Center;
                r.Cells[3].AddParagraph(k.Miktar.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
                r.Cells[4].AddParagraph(k.BirimFiyat.ToString("C2")).Format.Alignment = ParagraphAlignment.Right;

                var iskontoCell = r.Cells[5];
                iskontoCell.VerticalAlignment = VerticalAlignment.Center;
                iskontoCell.Format.Alignment = ParagraphAlignment.Right;
                iskontoCell.AddParagraph($"{k.Isk1}%\n{k.Isk2}%\n{k.Isk3}%\n{k.Isk4}%");

                r.Cells[6].AddParagraph($"{k.NakliyeIskonto}%").Format.Alignment = ParagraphAlignment.Right;
                r.Cells[7].AddParagraph($"{k.KDV}%").Format.Alignment = ParagraphAlignment.Right;
                r.Cells[8].AddParagraph(k.Tutar.ToString("C2")).Format.Alignment = ParagraphAlignment.Right;

                for (int i = 0; i < r.Cells.Count; i++)
                {
                    r.Cells[i].VerticalAlignment = VerticalAlignment.Center;
                    r.Cells[i].Format.Font.Size = 9;
                    r.Cells[i].Format.Font.Name = "Arial";
                }

                siraNo++;
            }

            section.AddParagraph("\n");

            // Toplam Miktar paragrafı - sola daha yakın (örneğin 10 cm'den hizalanmış)
            var par = section.AddParagraph();

            // İlk tab: sola yakın (örneğin 10cm)
            // İkinci tab: sağ kenara (örneğin 17cm, sayfa genişliğine göre ayarla)
            par.Format.TabStops.AddTabStop("4.2cm", TabAlignment.Left);
            par.Format.TabStops.AddTabStop("17cm", TabAlignment.Right);

            decimal toplamMiktar = kalemler.Sum(k => k.Miktar);
            decimal toplamTutar = kalemler.Sum(k => k.Tutar);

            // İlk \t -> 10cm konumuna gider
            par.AddText("\t");
            par.AddFormattedText($"Toplam Miktar: {toplamMiktar:N2}", TextFormat.Bold);

            // İkinci \t -> 17cm konumuna gider
            par.AddText("\t");
            par.AddFormattedText($"Toplam Tutar: {toplamTutar.ToString("C2")}", TextFormat.Bold);


            section.AddParagraph("\n\n\n");


            if (teslimatCari != null)
            {
                section.AddParagraph("\n");

                var teslimatPar = section.AddParagraph("Sevk Adresi:");
                teslimatPar.Format.Font.Bold = true;
                teslimatPar.Format.SpaceAfter = Unit.FromCentimeter(0.2);

                var teslimatDetayPar = section.AddParagraph();
                teslimatDetayPar.Format.Font.Size = 11;
                teslimatDetayPar.Format.Font.Name = "Arial";
                teslimatDetayPar.AddText($"Ad: {teslimatCari.TeslimatAdi}\n");
                teslimatDetayPar.AddText($"Adres: {teslimatCari.TeslimatAdres}\n");
                teslimatDetayPar.AddText($"Telefon: {teslimatCari.TeslimatTelefon}\n");
                teslimatDetayPar.AddText($"Yetkili: {teslimatCari.TeslimatYetkili}");
            }

            // 🔽 FABRİKA TESLİM BİLGİLERİ
            if (isFabrikaTeslim)
            {
                section.AddParagraph("\n");

                var fabrikaPar = section.AddParagraph("Fabrika Teslim:");
                fabrikaPar.Format.Font.Bold = true;
                fabrikaPar.Format.SpaceAfter = Unit.FromCentimeter(0.2);
                fabrikaPar.Format.Font.Size = 13;

                if (!string.IsNullOrEmpty(aciklama4))
                    section.AddParagraph($"Şoför: {aciklama4}");

                if (!string.IsNullOrEmpty(aciklama5))
                    section.AddParagraph($"Plaka 1: {aciklama5}");

                if (!string.IsNullOrEmpty(aciklama6))
                    section.AddParagraph($"Plaka 2: {aciklama6}");
            }


            section.AddParagraph("\n");

            if (!string.IsNullOrEmpty(aciklama1))
                section.AddParagraph($"Açıklama 1: {aciklama1}");
            if (!string.IsNullOrEmpty(aciklama2))
                section.AddParagraph($"Açıklama 2: {aciklama2}");
            if (!string.IsNullOrEmpty(aciklama3))
                section.AddParagraph($"Açıklama 3: {aciklama3}");
            
            //Banka Bilgiler asdece Proforma Faturada Gözükecek
            if (baslik == "Proforma Fatura")
            {
                var footer = section.Footers.Primary;

                var bankaTable = footer.AddTable();
                bankaTable.Borders.Width = 0.75;
                bankaTable.Borders.Color = Colors.Gray;
                bankaTable.Rows.LeftIndent = Unit.FromCentimeter(0);

                bankaTable.AddColumn(Unit.FromCentimeter(5));
                bankaTable.AddColumn(Unit.FromCentimeter(6));
                bankaTable.AddColumn(Unit.FromCentimeter(6));

                var bankaheaderRow = bankaTable.AddRow();
                bankaheaderRow.Shading.Color = Colors.LightGray;
                bankaheaderRow.Format.Font.Bold = true;
                bankaheaderRow.Format.Alignment = ParagraphAlignment.Center;

                bankaheaderRow.Cells[0].AddParagraph("Banka Adı");
                bankaheaderRow.Cells[1].AddParagraph("IBAN");
                bankaheaderRow.Cells[2].AddParagraph("Hesap");

                var row1 = bankaTable.AddRow();
                row1.Cells[0].AddParagraph("Kuveyttürk Katılım Bankası");
                row1.Cells[1].AddParagraph("TR04 0020 5000 0999 1337 2000 01");
                row1.Cells[2].AddParagraph("TL");

                var row2 = bankaTable.AddRow();
                row2.Cells[0].AddParagraph("Ziraat Katılım Bankası");
                row2.Cells[1].AddParagraph("TR98 0020 9000 0224 3871 0000 01");
                row2.Cells[2].AddParagraph("TL");

                foreach (Row r in bankaTable.Rows)
                {
                    foreach (Cell c in r.Cells)
                    {
                        c.Format.Font.Size = 9;
                        c.Format.Font.Name = "Arial";
                        c.VerticalAlignment = VerticalAlignment.Center;
                        c.Format.Alignment = ParagraphAlignment.Left;
                    }
                }
            }

            var pdfRenderer = new PdfDocumentRenderer() { Document = doc };
            pdfRenderer.RenderDocument();

            if (onizlemeModu)
            {
                // Geçici dosya oluştur ve aç
                var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");

                using (var stream = new MemoryStream())
                {
                    pdfRenderer.PdfDocument.Save(stream, false);
                    File.WriteAllBytes(tempFile, stream.ToArray());

                    Process.Start(new ProcessStartInfo(tempFile) { UseShellExecute = true });
                }
            }
            else
            {
                // Kalıcı olarak kaydet
                pdfRenderer.PdfDocument.Save(dosyaAdi);
            }

            

            

        }
    }
}



        

