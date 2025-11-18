using System.ComponentModel;
using System;

namespace By_ProV2.Models
{
    public class KalemModel : INotifyPropertyChanged
    {
         public int Id { get; set; } // Benzersiz sayı ID (tek veya çift)

        private string stokKodu = "";
        public string StokKodu
        {
            get => stokKodu;
            set
            {
                if (stokKodu != value)
                {
                    stokKodu = value;
                    OnPropertyChanged(nameof(StokKodu));
                }
            }
        }

        private string stokAdi;
        public string StokAdi
        {
            get => stokAdi;
            set
            {
                if (stokAdi != value)
                {
                    stokAdi = value;
                    OnPropertyChanged(nameof(StokAdi));
                }
            }
        }

        private string birim;
        public string Birim
        {
            get => birim;
            set
            {
                if (birim != value)
                {
                    birim = value;
                    OnPropertyChanged(nameof(Birim));
                }
            }
        }

        private decimal miktar;
        public decimal Miktar
        {
            get => miktar;
            set
            {
                if (miktar != value)
                {
                    miktar = value;
                    OnPropertyChanged(nameof(Miktar));
                    HesaplaTutar();
                }
            }
        }

        private decimal birimFiyat;
        public decimal BirimFiyat
        {
            get => birimFiyat;
            set
            {
                if (birimFiyat != value)
                {
                    birimFiyat = value;
                    OnPropertyChanged(nameof(BirimFiyat));
                    HesaplaTutar();
                }
            }
        }

        private decimal isk1;
        public decimal Isk1
        {
            get => isk1;
            set
            {
                if (isk1 != value)
                {
                    isk1 = value;
                    OnPropertyChanged(nameof(Isk1));
                    HesaplaTutar();
                }
            }
        }

        private decimal isk2;
        public decimal Isk2
        {
            get => isk2;
            set
            {
                if (isk2 != value)
                {
                    isk2 = value;
                    OnPropertyChanged(nameof(Isk2));
                    HesaplaTutar();
                }
            }
        }

        private decimal isk3;
        public decimal Isk3
        {
            get => isk3;
            set
            {
                if (isk3 != value)
                {
                    isk3 = value;
                    OnPropertyChanged(nameof(Isk3));
                    HesaplaTutar();
                }
            }
        }

        private decimal isk4;
        public decimal Isk4
        {
            get => isk4;
            set
            {
                if (isk4 != value)
                {
                    isk4 = value;
                    OnPropertyChanged(nameof(Isk4));
                    HesaplaTutar();
                }
            }
        }
        private decimal nakliyeIskonto;
        public decimal NakliyeIskonto
        {
            get => nakliyeIskonto;
            set
            {
                if (nakliyeIskonto != value)
                {
                    nakliyeIskonto = value;
                    OnPropertyChanged(nameof(NakliyeIskonto));
                    HesaplaTutar();
                }
            }
        }


        private decimal kdv;
        public decimal KDV
        {
            get => kdv;
            set
            {
                if (kdv != value)
                {
                    kdv = value;
                    OnPropertyChanged(nameof(KDV));
                    HesaplaTutar();
                }
            }
        }

        private decimal tutar;
        public decimal Tutar
        {
            get => tutar;
            private set
            {
                if (tutar != value)
                {
                    tutar = value;
                    OnPropertyChanged(nameof(Tutar));
                }
            }
        }

        private bool isAlisFabrikaTeslim;
        public bool IsAlisFabrikaTeslim
        {
            get => isAlisFabrikaTeslim;
            set
            {
                if (isAlisFabrikaTeslim != value)
                {
                    isAlisFabrikaTeslim = value;
                    OnPropertyChanged(nameof(IsAlisFabrikaTeslim));
                    HesaplaTutar(); // ✔ Tutarı güncelle
                }
            }
        }

        private bool isSatisFabrikaTeslim;
        public bool IsSatisFabrikaTeslim
        {
            get => isSatisFabrikaTeslim;
            set
            {
                if (isSatisFabrikaTeslim != value)
                {
                    isSatisFabrikaTeslim = value;
                    OnPropertyChanged(nameof(IsSatisFabrikaTeslim));
                    HesaplaTutar(); // ✔ Tutarı güncelle
                }
            }
        }

        public void HesaplaTutar()
        {
            decimal tutarHesap = Miktar * BirimFiyat;

            tutarHesap *= (1 - Isk1 / 100);
            tutarHesap *= (1 - Isk2 / 100);
            tutarHesap *= (1 - Isk3 / 100);
            tutarHesap *= (1 - Isk4 / 100);

            // ✔ Nakliye iskontosu yalnızca fabrika teslimde uygulanır
            if (IsAlisFabrikaTeslim || IsSatisFabrikaTeslim)
            {
                tutarHesap *= (1 - NakliyeIskonto / 100);
            }

            tutarHesap *= (1 + KDV / 100);

            Tutar = Math.Round(tutarHesap, 2);
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


    public class SatisIcmalKaydi
    {
        public DateTime Tarih { get; set; }
        public string CariKod { get; set; }
        public decimal Miktar { get; set; }
        public decimal? Kesinti { get; set; }
        public decimal? Yag { get; set; }
        public decimal? Protein { get; set; }
    }

    // Classes for Satis Report
    public class SatisReportData
    {
        public string Title { get; set; }
        public string DateRange { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public List<string> ColumnHeaders { get; set; }
        public List<SatisReportItem> Items { get; set; }
        public SatisReportPaymentSummary Summary { get; set; }
    }

    public class SatisReportItem
    {
        public string Tarih { get; set; }
        public double Miktar { get; set; }
        public double Kesinti { get; set; }
        public double Yag { get; set; }
        public double Protein { get; set; }
    }

    public class SatisReportPaymentSummary
    {
        public double NetMiktar { get; set; }
        public double Kesinti { get; set; }
        public double OrtYag { get; set; }
        public double OrtProtein { get; set; }
        public double SutFiyati { get; set; }
        public double NakliyeFiyati { get; set; }
        public double YagKesintiTutari { get; set; }
        public double ProteinKesintiTutari { get; set; }
        public double NetSutOdemesi { get; set; }
        public bool IsYagKesintisiApplied { get; set; }
        public bool IsProteinKesintisiApplied { get; set; }
    }

    // Classes for Alis Report (for consistency)
    public class AlisReportData
    {
        public string Title { get; set; }
        public string DateRange { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public List<string> ColumnHeaders { get; set; }
        public List<AlisReportItem> Items { get; set; }
        public AlisReportPaymentSummary PaymentSummary { get; set; }
    }

    public class AlisReportItem
    {
        public string Tarih { get; set; }
        public double Miktar { get; set; }
        public double Kesinti { get; set; }
        public double Yag { get; set; }
        public double Protein { get; set; }
    }

    public class AlisReportPaymentSummary
    {
        public double NetMiktar { get; set; }
        public double Kesinti { get; set; }
        public double OrtYag { get; set; }
        public double OrtProtein { get; set; }
        public double SutFiyati { get; set; }
        public double NakliyeFiyati { get; set; }
        public double YagKesintiTutari { get; set; }
        public double ProteinKesintiTutari { get; set; }
        public double NetSutOdemesi { get; set; }
        public bool IsYagKesintisiApplied { get; set; }
        public bool IsProteinKesintisiApplied { get; set; }
    }
}