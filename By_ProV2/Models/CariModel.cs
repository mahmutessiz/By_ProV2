using System;

namespace By_ProV2.Models
{
    public class CariModel
    {
        public string CariKod { get; set; }
        public string CariAdi { get; set; }
        public string Adres { get; set; }
        public string Telefon { get; set; }
        public string Yetkili { get; set; }
        public string BagliCariKod { get; set; }
        public string VergiDairesi { get; set; }
        public string VergiNo { get; set; }
        public decimal Isk1 { get; set; }
        public decimal Isk2 { get; set; }
        public decimal Isk3 { get; set; }
        public decimal Isk4 { get; set; }
        public decimal KKIsk1 { get; set; }
        public decimal KKIsk2 { get; set; }
        public decimal KKIsk3 { get; set; }
        public decimal KKIsk4 { get; set; }
        public decimal NakliyeIskonto { get; set; } = 0;
        public string Plaka1 { get; set; }
        public string Plaka2 { get; set; }
        public string Plaka3 { get; set; }
        public string SoforAdSoyad { get; set; }
        public DateTime KayitTarihi { get; set; }
        public string TeslimKod { get; set; }
        public string TeslimatAdi { get; set; }
        public string TeslimatAdres { get; set; }
        public string TeslimatTelefon { get; set; }
        public string TeslimatYetkili { get; set; }
    }
}