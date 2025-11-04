namespace By_ProV1.Models
{
    public class SutRaporKaydi
    {
        public string TedarikciKod { get; set; }
        public string TedarikciAdi { get; set; }
        public decimal Miktar { get; set; }
        public decimal? Yag { get; set; }
        public decimal Fiyat { get; set; }
        public decimal Tutar => Miktar * Fiyat; // otomatik hesap
    }
}
