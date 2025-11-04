using System;

namespace By_ProV2.Models
{
    public class SutKaydi
    {
        public int SutKayitId { get; set; }
        public int TedarikciId { get; set; }
        public int MusteriId { get; set; }
        public int Id { get; set; }
        public string BelgeNo { get; set; }  // Document number
        public DateTime Tarih { get; set; }
        public string IslemTuru { get; set; }  // Depoya Alım, Depodan Sevk, Direkt Sevk

        // Tedarikçi ve müşteri bilgileri
        public string TedarikciKod { get; set; }
        public string TedarikciAdi { get; set; }
        public string MusteriKod { get; set; }
        public string MusteriAdi { get; set; }

        // Miktar ve fiyat
        public decimal Miktar { get; set; }
        public decimal Fiyat { get; set; }
        public decimal Kesinti { get; set; }

        // Analiz Bilgileri (nullable olabilir)
        public decimal? Yag { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Laktoz { get; set; }
        public decimal? TKM { get; set; }
        public decimal? YKM { get; set; }
        public decimal? pH { get; set; }
        public decimal? Iletkenlik { get; set; }
        public decimal? Sicaklik { get; set; }
        public decimal? Yogunluk { get; set; }
        public decimal? DonmaN { get; set; }

        // Metin analizleri
        public decimal? Bakteri { get; set; }
        public decimal? Somatik { get; set; }

        // Diğer bilgiler
        public string Antibiyotik { get; set; }
        public string AracTemizlik { get; set; }
        public string Plaka { get; set; }
        public string Durumu { get; set; }
        public string Aciklama { get; set; }

        // Hangi depoda işlendiği bilgisi (isteğe bağlı)
        public string DepoKodu { get; set; }
    }
}
