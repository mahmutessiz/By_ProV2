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


}