using System.Collections.ObjectModel;
using System.ComponentModel;
using By_ProV2.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using By_ProV2.Helpers;


namespace By_ProV2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // Alış ve Satış Kalem Listeleri
        public ObservableCollection<KalemModel> AlisKalemListesi { get; set; } = new ObservableCollection<KalemModel>();
        public ObservableCollection<KalemModel> SatisKalemListesi { get; set; } = new ObservableCollection<KalemModel>();

        private bool isFabrikaTeslim;
        public bool IsFabrikaTeslim
        {
            get => isFabrikaTeslim;
            set
            {
                if (isFabrikaTeslim != value)
                {
                    isFabrikaTeslim = value;
                    OnPropertyChanged(nameof(IsFabrikaTeslim));
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
                }
            }
        }

        private bool isAlisOnOdeme;
        public bool IsAlisOnOdeme
        {
            get => isAlisOnOdeme;
            set
            {
                if (isAlisOnOdeme != value)
                {
                    isAlisOnOdeme = value;
                    OnPropertyChanged(nameof(IsAlisOnOdeme));
                }
            }
        }

        private bool isSatisOnOdeme;
        public bool IsSatisOnOdeme
        {
            get => isSatisOnOdeme;
            set
            {
                if (isSatisOnOdeme != value)
                {
                    isSatisOnOdeme = value;
                    OnPropertyChanged(nameof(IsSatisOnOdeme));
                }
            }
        }

        private string alisVade;
        public string AlisVade
        {
            get => alisVade;
            set
            {
                if (alisVade != value)
                {
                    alisVade = value;
                    OnPropertyChanged(nameof(AlisVade));
                }
            }
        }

        private string satisVade;
        public string SatisVade
        {
            get => satisVade;
            set
            {
                if (satisVade != value)
                {
                    satisVade = value;
                    OnPropertyChanged(nameof(SatisVade));
                }
            }
        }


        private bool isAlisKrediKartiOdeme;
        public bool IsAlisKrediKartiOdeme
        {
            get => isAlisKrediKartiOdeme;
            set
            {
                if (isAlisKrediKartiOdeme != value)
                {
                    isAlisKrediKartiOdeme = value;
                    OnPropertyChanged(nameof(IsAlisKrediKartiOdeme));
                }
            }
        }

        private bool isSatisKrediKartiOdeme;
        public bool IsSatisKrediKartiOdeme
        {
            get => isSatisKrediKartiOdeme;
            set
            {
                if (isSatisKrediKartiOdeme != value)
                {
                    isSatisKrediKartiOdeme = value;
                    OnPropertyChanged(nameof(IsSatisKrediKartiOdeme));
                }
            }
        }

        public enum KalemTipi
        {
            Alis,
            Satis,
            Yok  // Henüz seçim yapılmadıysa
        }
        private KalemTipi secilenKalemTipi = KalemTipi.Yok;
        public KalemTipi SecilenKalemTipi
        {
            get => secilenKalemTipi;
            set
            {
                secilenKalemTipi = value;
                OnPropertyChanged(nameof(SecilenKalemTipi));
            }
        }
       

        // Sayaçlar (id atmak için)
        private int alisKalemIdSayaci = 1;  // Tek sayılar (1,3,5...)
        private int satisKalemIdSayaci = 2; // Çift sayılar (2,4,6...)

        // Seçilen Kalem
        private KalemModel secilenKalem;
        public KalemModel SecilenKalem
        {
            get => secilenKalem;
            set
            {
                if (secilenKalem != value)
                {
                    secilenKalem = value;
                    OnPropertyChanged(nameof(SecilenKalem));
                    if (secilenKalem != null)
                        LoadKalemToInputs(secilenKalem);
                }
            }
        }

        public int? DuzenlenenKalemId { get; set; } = null;

        // Stok Listesi
        public ObservableCollection<StokModel> StokListesi { get; set; } = new ObservableCollection<StokModel>();

        // Cari Bilgiler
        private CariModel alisCari;
        public CariModel AlisCari
        {
            get => alisCari;
            set
            {
                if (alisCari != value)
                {
                    alisCari = value;
                    OnPropertyChanged(nameof(AlisCari));
                }
            }
        }

        private CariModel satisCari;
        public CariModel SatisCari
        {
            get => satisCari;
            set
            {
                if (satisCari != value)
                {
                    satisCari = value;
                    OnPropertyChanged(nameof(SatisCari));
                }
            }
        }

        
        private DateTime siparisTarihi = DateTime.Today;
        public DateTime SiparisTarihi
        {
            get => siparisTarihi;
            set
            {
                if (siparisTarihi != value)
                {
                    siparisTarihi = value;
                    OnPropertyChanged(nameof(SiparisTarihi));
                }
            }
        }

        private DateTime sevkTarihi = DateTime.Today.AddDays(3);
        public DateTime SevkTarihi
        {
            get => sevkTarihi;
            set
            {
                if (sevkTarihi != value)
                {
                    sevkTarihi = value;
                    OnPropertyChanged(nameof(SevkTarihi));
                }
            }
        }

        private string odemeYontemi;
        public string OdemeYontemi
        {
            get => odemeYontemi;
            set
            {
                if (odemeYontemi != value)
                {
                    odemeYontemi = value;
                    OnPropertyChanged(nameof(OdemeYontemi));
                }
            }
        }

        private string ProformaodemeYontemi;
        public string ProformaOdemeYontemi
        {
            get => ProformaodemeYontemi;
            set
            {
                if (ProformaodemeYontemi != value)
                {
                    ProformaodemeYontemi = value;
                    OnPropertyChanged(nameof(ProformaOdemeYontemi));
                }
            }
        }

        private CariModel teslimatCari;
        public CariModel TeslimatCari
        {
            get => teslimatCari;
            set
            {
                if (teslimatCari != value)
                {
                    teslimatCari = value;
                    OnPropertyChanged(nameof(TeslimatCari));
                }
            }
        }

        private string aciklama1;
        public string Aciklama1
        {
            get => aciklama1;
            set
            {
                if (aciklama1 != value)
                {
                    aciklama1 = value;
                    OnPropertyChanged(nameof(Aciklama1));
                }
            }
        }

        private string aciklama2;
        public string Aciklama2
        {
            get => aciklama2;
            set
            {
                if (aciklama2 != value)
                {
                    aciklama2 = value;
                    OnPropertyChanged(nameof(Aciklama2));
                }
            }
        }

        private string aciklama3;
        public string Aciklama3
        {
            get => aciklama3;
            set
            {
                if (aciklama3 != value)
                {
                    aciklama3 = value;
                    OnPropertyChanged(nameof(Aciklama3));
                }
            }
        }
        private string aciklama4;
        public string Aciklama4
        {
            get => aciklama4;
            set
            {
                if (aciklama5 != value)
                {
                    aciklama4 = value;
                    OnPropertyChanged(nameof(Aciklama4));
                }
            }
        }

        private string aciklama5;
        public string Aciklama5
        {
            get => aciklama5;
            set
            {
                if (aciklama5 != value)
                {
                    aciklama5 = value;
                    OnPropertyChanged(nameof(Aciklama5));
                }
            }
        }

        private string aciklama6;
        public string Aciklama6
        {
            get => aciklama6;
            set
            {
                if (aciklama6 != value)
                {
                    aciklama6 = value;
                    OnPropertyChanged(nameof(Aciklama6));
                }
            }
        }

        private string iban;
        public string Iban
        {
            get => iban;
            set
            {
                if (iban != value)
                {
                    iban = value;
                    OnPropertyChanged(nameof(Iban));
                }
            }
        }


        // Constructor
        public MainViewModel()
        {
            AlisKalemListesi.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(AlisToplamTutar));
                OnPropertyChanged(nameof(AlisToplamMiktar));
                OnPropertyChanged(nameof(AlisBrutToplam));
                OnPropertyChanged(nameof(AlisKdvToplam));
                OnPropertyChanged(nameof(AlisIskontoToplam));
            };

            SatisKalemListesi.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(SatisToplamTutar));
                OnPropertyChanged(nameof(SatisToplamMiktar));
                OnPropertyChanged(nameof(SatisBrutToplam));
                OnPropertyChanged(nameof(SatisKdvToplam));
                OnPropertyChanged(nameof(SatisIskontoToplam));
            };
        }

        // Stokları yüklemek için
        public void YukleStoklari(IEnumerable<StokModel> stoklar)
        {
            StokListesi.Clear();
            foreach (var stok in stoklar)
            {
                StokListesi.Add(stok);
            }
            OnPropertyChanged(nameof(StokListesi));
        }

        // View'da override edilecek (input doldurmak için)
        public virtual void LoadKalemToInputs(KalemModel kalem)
        {
            // Override edilecek
        }

        // Kalem ekleme fonksiyonu (UI'dan çağrılacak)
        public void KalemEkle(
    string stokKodu, string stokAdi, string birim,
    decimal miktar, decimal kdv,
    decimal isk1, decimal isk2, decimal isk3, decimal isk4,
    decimal alisFiyat, decimal satisFiyat, decimal alisNakliyeIskonto, decimal satisNakliyeIskonto, bool isAlisFabrikaTeslim, bool isSatisFabrikaTeslim)
        {
            
            // === ALIŞ ===
            // GÜNCELLENECEK ALIŞ KALEMİ
            if (DuzenlenenKalemId.HasValue)
            {
                var guncellenecekAlis = AlisKalemListesi.FirstOrDefault(x => x.Id == DuzenlenenKalemId.Value);
                if (guncellenecekAlis != null)
                {
                    guncellenecekAlis.StokKodu = stokKodu;
                    guncellenecekAlis.StokAdi = stokAdi;
                    guncellenecekAlis.Birim = birim;
                    guncellenecekAlis.Miktar = miktar;
                    guncellenecekAlis.KDV = kdv;
                    guncellenecekAlis.BirimFiyat = alisFiyat;

                    guncellenecekAlis.Isk1 = isk1;
                    guncellenecekAlis.Isk2 = isk2;
                    guncellenecekAlis.Isk3 = isk3;
                    guncellenecekAlis.Isk4 = isk4;
                    guncellenecekAlis.NakliyeIskonto = alisNakliyeIskonto;
                    guncellenecekAlis.IsAlisFabrikaTeslim = isAlisFabrikaTeslim;




                    guncellenecekAlis.HesaplaTutar();
                }
            }

            else
            {
                var alis = new KalemModel
                {
                    Id = alisKalemIdSayaci,
                    StokKodu = stokKodu,
                    StokAdi = stokAdi,
                    Birim = birim,
                    Miktar = miktar,
                    BirimFiyat = alisFiyat,
                    KDV = kdv,
                    Isk1 = IsAlisKrediKartiOdeme ? AlisCari?.KKIsk1 ?? isk1 : AlisCari?.Isk1 ?? isk1,
                    Isk2 = IsAlisKrediKartiOdeme ? AlisCari?.KKIsk2 ?? isk2 : AlisCari?.Isk2 ?? isk2,
                    Isk3 = IsAlisKrediKartiOdeme ? AlisCari?.KKIsk3 ?? isk3 : AlisCari?.Isk3 ?? isk3,
                    Isk4 = IsAlisKrediKartiOdeme ? AlisCari?.KKIsk4 ?? isk4 : AlisCari?.Isk4 ?? isk4,
                    NakliyeIskonto = isAlisFabrikaTeslim ? (AlisCari?.NakliyeIskonto ?? 0) : 0,
                    IsAlisFabrikaTeslim = isAlisFabrikaTeslim   // Yeni atama


                };
                alis.HesaplaTutar();
                AlisKalemListesi.Add(alis);
                alisKalemIdSayaci += 2;
            }

            // === SATIŞ ===
            // GÜNCELLENECEK SATIŞ KALEMİ
            if (DuzenlenenKalemId.HasValue)
            {
                var guncellenecekSatis = SatisKalemListesi.FirstOrDefault(x => x.Id == DuzenlenenKalemId.Value);
                if (guncellenecekSatis != null)
                {
                    guncellenecekSatis.StokKodu = stokKodu;
                    guncellenecekSatis.StokAdi = stokAdi;
                    guncellenecekSatis.Birim = birim;
                    guncellenecekSatis.Miktar = miktar;
                    guncellenecekSatis.KDV = kdv;
                    guncellenecekSatis.BirimFiyat = satisFiyat;

                    guncellenecekSatis.Isk1 = isk1;
                    guncellenecekSatis.Isk2 = isk2;
                    guncellenecekSatis.Isk3 = isk3;
                    guncellenecekSatis.Isk4 = isk4;
                    guncellenecekSatis.NakliyeIskonto = satisNakliyeIskonto;
                    guncellenecekSatis.IsSatisFabrikaTeslim = isSatisFabrikaTeslim;



                    guncellenecekSatis.HesaplaTutar();
                }
            }

            else
            {
                var satis = new KalemModel
                {
                    Id = satisKalemIdSayaci,
                    StokKodu = stokKodu,
                    StokAdi = stokAdi,
                    Birim = birim,
                    Miktar = miktar,
                    BirimFiyat = satisFiyat,
                    KDV = kdv,
                    Isk1 = IsSatisKrediKartiOdeme ? SatisCari?.KKIsk1 ?? isk1 : SatisCari?.Isk1 ?? isk1,
                    Isk2 = IsSatisKrediKartiOdeme ? SatisCari?.KKIsk2 ?? isk2 : SatisCari?.Isk2 ?? isk2,
                    Isk3 = IsSatisKrediKartiOdeme ? SatisCari?.KKIsk3 ?? isk3 : SatisCari?.Isk3 ?? isk3,
                    Isk4 = IsSatisKrediKartiOdeme ? SatisCari?.KKIsk4 ?? isk4 : SatisCari?.Isk4 ?? isk4,
                    NakliyeIskonto = isSatisFabrikaTeslim ? (SatisCari?.NakliyeIskonto ?? 0) : 0,
                    IsSatisFabrikaTeslim = isSatisFabrikaTeslim   // Yeni atama

                };
                satis.HesaplaTutar();
                SatisKalemListesi.Add(satis);
                satisKalemIdSayaci += 2;
            }

            // Temizle
            DuzenlenenKalemId = null;
            GuncelleToplamlar();
            
            OnPropertyChanged(nameof(AlisToplamMiktar));
            OnPropertyChanged(nameof(AlisToplamTutar));
            OnPropertyChanged(nameof(SatisToplamMiktar));
            OnPropertyChanged(nameof(SatisToplamTutar));
            OnPropertyChanged(nameof(AlisBrutToplam));
            OnPropertyChanged(nameof(AlisKdvToplam));
            OnPropertyChanged(nameof(AlisIskontoToplam));
            OnPropertyChanged(nameof(SatisBrutToplam));
            OnPropertyChanged(nameof(SatisKdvToplam));
            OnPropertyChanged(nameof(SatisIskontoToplam));

        }

        public void AlisKalemEkle(
    string stokKodu, string stokAdi, string birim,
    decimal miktar, decimal kdv,
    decimal isk1, decimal isk2, decimal isk3, decimal isk4,
    decimal alisFiyat, decimal alisNakliyeIskonto, bool isAlisFabrikaTeslim)
        {
            // Satış parametrelerini boş/default bırakıyoruz
            KalemEkle(stokKodu, stokAdi, birim, miktar, kdv,
                isk1, isk2, isk3, isk4,
                alisFiyat, 0m,
                alisNakliyeIskonto, 0m,
                isAlisFabrikaTeslim, false);
        }

        public void SatisKalemEkle(
            string stokKodu, string stokAdi, string birim,
            decimal miktar, decimal kdv,
            decimal isk1, decimal isk2, decimal isk3, decimal isk4,
            decimal satisFiyat, decimal satisNakliyeIskonto, bool isSatisFabrikaTeslim)
        {
            // Alış parametrelerini boş/default bırakıyoruz
            KalemEkle(stokKodu, stokAdi, birim, miktar, kdv,
                isk1, isk2, isk3, isk4,
                0m, satisFiyat,
                0m, satisNakliyeIskonto,
                false, isSatisFabrikaTeslim);
        }


        private decimal toplamAlisTutar;
        public decimal ToplamAlisTutar
        {
            get => toplamAlisTutar;
            set
            {
                if (toplamAlisTutar != value)
                {
                    toplamAlisTutar = value;
                    OnPropertyChanged(nameof(ToplamAlisTutar));
                }
            }
        }

        private decimal toplamAlisMiktar;
        public decimal ToplamAlisMiktar
        {
            get => toplamAlisMiktar;
            set
            {
                if (toplamAlisMiktar != value)
                {
                    toplamAlisMiktar = value;
                    OnPropertyChanged(nameof(ToplamAlisMiktar));
                }
            }
        }

        private decimal toplamSatisTutar;
        public decimal ToplamSatisTutar
        {
            get => toplamSatisTutar;
            set
            {
                if (toplamSatisTutar != value)
                {
                    toplamSatisTutar = value;
                    OnPropertyChanged(nameof(ToplamSatisTutar));
                }
            }
        }

        private decimal toplamSatisMiktar;
        public decimal ToplamSatisMiktar
        {
            get => toplamSatisMiktar;
            set
            {
                if (toplamSatisMiktar != value)
                {
                    toplamSatisMiktar = value;
                    OnPropertyChanged(nameof(ToplamSatisMiktar));
                }
            }
        }
        public void GuncelleToplamlar()
        {
            ToplamAlisMiktar = AlisKalemListesi.Sum(k => k.Miktar);
            ToplamAlisTutar = AlisKalemListesi.Sum(k => k.Tutar);

            ToplamSatisMiktar = SatisKalemListesi.Sum(k => k.Miktar);
            ToplamSatisTutar = SatisKalemListesi.Sum(k => k.Tutar);
        }
        public decimal AlisToplamTutar => AlisKalemListesi.Sum(x => x.Tutar);
        public decimal AlisToplamMiktar => AlisKalemListesi.Sum(x => x.Miktar);

        public decimal SatisToplamTutar => SatisKalemListesi.Sum(x => x.Tutar);
        public decimal SatisToplamMiktar => SatisKalemListesi.Sum(x => x.Miktar);

        // Alış Kalemleri Özetleri
        public decimal AlisBrutToplam => AlisKalemListesi.Sum(k => k.Miktar * k.BirimFiyat);
        public decimal AlisNetToplam => AlisKalemListesi.Sum(k => k.Tutar);
        public decimal AlisKdvToplam => AlisNetToplam - AlisKalemListesi.Sum(k =>
        {
            decimal tutar = k.Miktar * k.BirimFiyat;
            tutar *= (1 - k.Isk1 / 100);
            tutar *= (1 - k.Isk2 / 100);
            tutar *= (1 - k.Isk3 / 100);
            tutar *= (1 - k.Isk4 / 100);
            if (k.IsAlisFabrikaTeslim)
                tutar *= (1 - k.NakliyeIskonto / 100);
            return Math.Round(tutar, 2); // KDV'siz tutar
        });

        public decimal AlisIskontoToplam => AlisBrutToplam - (AlisNetToplam / (1 + (AlisKalemListesi.FirstOrDefault()?.KDV ?? 0) / 100));

        // Satış Kalemleri Özetleri
        public decimal SatisBrutToplam => SatisKalemListesi.Sum(k => k.Miktar * k.BirimFiyat);
        public decimal SatisNetToplam => SatisKalemListesi.Sum(k => k.Tutar);
        public decimal SatisKdvToplam => SatisNetToplam - SatisKalemListesi.Sum(k =>
        {
            decimal tutar = k.Miktar * k.BirimFiyat;
            tutar *= (1 - k.Isk1 / 100);
            tutar *= (1 - k.Isk2 / 100);
            tutar *= (1 - k.Isk3 / 100);
            tutar *= (1 - k.Isk4 / 100);
            if (k.IsSatisFabrikaTeslim)
                tutar *= (1 - k.NakliyeIskonto / 100);
            return Math.Round(tutar, 2);
        });

        public decimal SatisIskontoToplam => SatisBrutToplam - (SatisNetToplam / (1 + (SatisKalemListesi.FirstOrDefault()?.KDV ?? 0) / 100));

        private string siparisNo = "000000000";
        public string SiparisNo
        {
            get => siparisNo;
            set
            {
                if (siparisNo != value)
                {
                    siparisNo = value;
                    OnPropertyChanged(nameof(SiparisNo));
                }
            }
        }

        private string proformaNo = "000000000";
        public string ProformaNo
        {
            get => proformaNo;
            set
            {
                if (proformaNo != value)
                {
                    proformaNo = value;
                    OnPropertyChanged(nameof(ProformaNo));
                }
            }
        }
        public string BelgeKodu { get; set; } = "BELGE_YOK"; // default
        public string belgeKodu
        {
            get => BelgeKodu;
            set
            {
                if (BelgeKodu != value)
                {
                    BelgeKodu = value;
                    OnPropertyChanged(nameof(belgeKodu));
                }
            }
        }
        public int SeciliSiparisID { get; set; }

        public ObservableCollection<string> OdemeSekliListe { get; set; } = new ObservableCollection<string>
        {
            "-- Seçiniz --",
            "Süt Vade",
            "30 Gün",
            "45 Gün",
            "60 Gün"
        };

        public async Task<bool> SiparisiKaydetAsync()
        {
            // Validasyon
            if (AlisCari == null || string.IsNullOrWhiteSpace(AlisCari.CariKod))
            {
                MessageBox.Show("Alış cari bilgisi boş olamaz.");
                return false;
            }

            foreach (var kalem in AlisKalemListesi)
            {
                if (string.IsNullOrWhiteSpace(kalem.StokKodu))
                {
                    MessageBox.Show("Alış kalemlerinde stok kodu boş bırakılamaz.");
                    return false;
                }
                if (kalem.Miktar <= 0)
                {
                    MessageBox.Show($"Alış kalemlerinde miktar sıfırdan büyük olmalı. StokKodu: {kalem.StokKodu}");
                    return false;
                }
            }

            foreach (var kalem in SatisKalemListesi)
            {
                if (string.IsNullOrWhiteSpace(kalem.StokKodu))
                {
                    MessageBox.Show("Satış kalemlerinde stok kodu boş bırakılamaz.");
                    return false;
                }
                if (kalem.Miktar <= 0)
                {
                    MessageBox.Show($"Satış kalemlerinde miktar sıfırdan büyük olmalı. StokKodu: {kalem.StokKodu}");
                    return false;
                }
            }

            string connStr = ConfigurationHelper.GetConnectionString("db");

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string insertMasterSql = @"
                        INSERT INTO SiparisMaster
                        (BelgeKodu, SiparisNo, SiparisTarihi, SevkTarihi, CariKod, CariAd, VergiDairesi, VergiNo, Telefon, CariAdres, 
ProformaNo, TeslimKod, TeslimIsim, TeslimAdres, TeslimTelefon, YetkiliKisi, OdemeNakit, OdemeKrediKarti, OdemeVade, FabrikaTeslim,
SatisOdemeNakit, SatisOdemeKrediKarti, SatisOdemeVade, SatisFabrikaTeslim,
SatisCariKod, SatisCariAd, SatisVergiDairesi, SatisVergiNo, SatisTelefon, SatisCariAdres, 
Aciklama1, Aciklama2, Aciklama3, Aciklama4, Aciklama5, Aciklama6, AlisToplamTutar, SatisToplamTutar)
                        VALUES
                        (@BelgeKodu, @SiparisNo, @SiparisTarihi, @SevkTarihi, @CariKod, @CariAd, @VergiDairesi, @VergiNo, @Telefon, @CariAdres, 
@ProformaNo, @TeslimKod, @Teslimisim, @TeslimAdres, @TeslimTelefon, @YetkiliKisi, @AlisOnOdeme, @AlisKrediKarti, @AlisVade, @AlisFabirkaTeslim,
@SatisOnOdeme, @SatisKrediKarti, @SatisVade, @SatisFabrikaTeslim,
@SatisCariKod, @SatisCariAd, @SatisVergiDairesi, @SatisVergiNo, @SatisTelefon, @SatisCariAdres, 
@Aciklama1, @Aciklama2, @Aciklama3, @Aciklama4, @Aciklama5, @Aciklama6, @AlisToplamTutar, @SatisToplamTutar);
                        SELECT SCOPE_IDENTITY();
                    ";

                            using (SqlCommand cmdMaster = new SqlCommand(insertMasterSql, conn, trans))
                            {
                                cmdMaster.Parameters.AddWithValue("@BelgeKodu", BelgeKodu);
                                cmdMaster.Parameters.AddWithValue("@SiparisNo", SiparisNo);
                                cmdMaster.Parameters.AddWithValue("@SiparisTarihi", SiparisTarihi);
                                cmdMaster.Parameters.AddWithValue("@SevkTarihi", (object)SevkTarihi ?? DBNull.Value);
                                cmdMaster.Parameters.AddWithValue("@CariKod", AlisCari.CariKod);
                                cmdMaster.Parameters.AddWithValue("@CariAd", AlisCari.CariAdi ?? "");
                                cmdMaster.Parameters.AddWithValue("@VergiDairesi", AlisCari.VergiDairesi);
                                cmdMaster.Parameters.AddWithValue("@VergiNo", AlisCari.VergiNo);
                                cmdMaster.Parameters.AddWithValue("@Telefon", AlisCari.Telefon);
                                cmdMaster.Parameters.AddWithValue("@CariAdres", AlisCari.Adres);
                                cmdMaster.Parameters.AddWithValue("@ProformaNo", ProformaNo);
                                cmdMaster.Parameters.AddWithValue("@Teslimisim", TeslimatCari.CariAdi);
                                cmdMaster.Parameters.AddWithValue("@TeslimKod", TeslimatCari.CariKod);
                                cmdMaster.Parameters.AddWithValue("@TeslimAdres", TeslimatCari.Adres);
                                cmdMaster.Parameters.AddWithValue("@TeslimTelefon", TeslimatCari.Telefon);
                                cmdMaster.Parameters.AddWithValue("@YetkiliKisi", TeslimatCari.Yetkili);
                                
                                cmdMaster.Parameters.AddWithValue("@AlisOnOdeme", IsAlisOnOdeme);
                                cmdMaster.Parameters.AddWithValue("@AlisKrediKarti", IsAlisKrediKartiOdeme);
                                cmdMaster.Parameters.AddWithValue("@AlisVade", string.IsNullOrEmpty(AlisVade) ? DBNull.Value : (object)AlisVade);
                                cmdMaster.Parameters.AddWithValue("@AlisFabirkaTeslim", IsAlisFabrikaTeslim);

                                cmdMaster.Parameters.AddWithValue("@SatisOnOdeme", IsSatisOnOdeme);
                                cmdMaster.Parameters.AddWithValue("@SatisKrediKarti", IsSatisKrediKartiOdeme);
                                cmdMaster.Parameters.AddWithValue("@SatisVade", string.IsNullOrEmpty(SatisVade) ? DBNull.Value : (object)SatisVade);
                                cmdMaster.Parameters.AddWithValue("@SatisFabrikaTeslim", IsSatisFabrikaTeslim);

                                cmdMaster.Parameters.AddWithValue("@SatisCariKod", SatisCari.CariKod);
                                cmdMaster.Parameters.AddWithValue("@SatisCariAd", SatisCari.CariAdi ?? "");
                                cmdMaster.Parameters.AddWithValue("@SatisVergiDairesi", SatisCari.VergiDairesi);
                                cmdMaster.Parameters.AddWithValue("@SatisVergiNo", SatisCari.VergiNo);
                                cmdMaster.Parameters.AddWithValue("@SatisTelefon", SatisCari.Telefon);
                                cmdMaster.Parameters.AddWithValue("@SatisCariAdres", SatisCari.Adres);
                                cmdMaster.Parameters.AddWithValue("@Aciklama1", string.IsNullOrWhiteSpace(Aciklama1) ? DBNull.Value : (object)Aciklama1);
                                cmdMaster.Parameters.AddWithValue("@Aciklama2", string.IsNullOrWhiteSpace(Aciklama2) ? DBNull.Value : (object)Aciklama2);
                                cmdMaster.Parameters.AddWithValue("@Aciklama3", string.IsNullOrWhiteSpace(Aciklama3) ? DBNull.Value : (object)Aciklama3);
                                cmdMaster.Parameters.AddWithValue("@Aciklama4", string.IsNullOrWhiteSpace(Aciklama4) ? DBNull.Value : (object)Aciklama4);
                                cmdMaster.Parameters.AddWithValue("@Aciklama5", string.IsNullOrWhiteSpace(Aciklama5) ? DBNull.Value : (object)Aciklama5);
                                cmdMaster.Parameters.AddWithValue("@Aciklama6", string.IsNullOrWhiteSpace(Aciklama6) ? DBNull.Value : (object)Aciklama6);


                                cmdMaster.Parameters.AddWithValue("@AlisToplamTutar", AlisToplamTutar);
                                cmdMaster.Parameters.AddWithValue("@SatisToplamTutar", SatisToplamTutar);

                                int siparisID = Convert.ToInt32(await cmdMaster.ExecuteScalarAsync());

                                // Alış kalemleri
                                string insertKalemSql = @"
                            INSERT INTO SiparisKalemAlis
                            (SiparisID, StokKodu, StokAdi, Birim, Miktar, BirimFiyat, KDV, Isk1, Isk2, Isk3, Isk4, NakliyeIskonto, Tutar)
                            VALUES
                            (@SiparisID, @StokKodu, @StokAdi, @Birim, @Miktar, @BirimFiyat, @KDV, @Isk1, @Isk2, @Isk3, @Isk4, @NakliyeIskonto, @Tutar)";

                                foreach (var kalem in AlisKalemListesi)
                                {
                                    using (SqlCommand cmd = new SqlCommand(insertKalemSql, conn, trans))
                                    {
                                        cmd.Parameters.AddWithValue("@SiparisID", siparisID);
                                        cmd.Parameters.AddWithValue("@StokKodu", kalem.StokKodu);
                                        cmd.Parameters.AddWithValue("@StokAdi", kalem.StokAdi ?? "");
                                        cmd.Parameters.AddWithValue("@Birim", kalem.Birim ?? "");
                                        cmd.Parameters.AddWithValue("@Miktar", kalem.Miktar);
                                        cmd.Parameters.AddWithValue("@BirimFiyat", kalem.BirimFiyat);
                                        cmd.Parameters.AddWithValue("@KDV", kalem.KDV);
                                        cmd.Parameters.AddWithValue("@Isk1", kalem.Isk1);
                                        cmd.Parameters.AddWithValue("@Isk2", kalem.Isk2);
                                        cmd.Parameters.AddWithValue("@Isk3", kalem.Isk3);
                                        cmd.Parameters.AddWithValue("@Isk4", kalem.Isk4);
                                        cmd.Parameters.AddWithValue("@NakliyeIskonto", kalem.NakliyeIskonto);
                                        cmd.Parameters.AddWithValue("@Tutar", kalem.Tutar);

                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }

                                // Satış kalemleri
                                string insertSatisSql = @"
                            INSERT INTO SiparisKalemSatis
                            (SiparisID, StokKodu, StokAdi, Birim, Miktar, BirimFiyat, KDV, Isk1, Isk2, Isk3, Isk4, NakliyeIskonto, Tutar)
                            VALUES
                            (@SiparisID, @StokKodu, @StokAdi, @Birim, @Miktar, @BirimFiyat, @KDV, @Isk1, @Isk2, @Isk3, @Isk4, @NakliyeIskonto, @Tutar)";

                                foreach (var kalem in SatisKalemListesi)
                                {
                                    using (SqlCommand cmd = new SqlCommand(insertSatisSql, conn, trans))
                                    {
                                        cmd.Parameters.AddWithValue("@SiparisID", siparisID);
                                        cmd.Parameters.AddWithValue("@StokKodu", kalem.StokKodu);
                                        cmd.Parameters.AddWithValue("@StokAdi", kalem.StokAdi ?? "");
                                        cmd.Parameters.AddWithValue("@Birim", kalem.Birim ?? "");
                                        cmd.Parameters.AddWithValue("@Miktar", kalem.Miktar);
                                        cmd.Parameters.AddWithValue("@BirimFiyat", kalem.BirimFiyat);
                                        cmd.Parameters.AddWithValue("@KDV", kalem.KDV);
                                        cmd.Parameters.AddWithValue("@Isk1", kalem.Isk1);
                                        cmd.Parameters.AddWithValue("@Isk2", kalem.Isk2);
                                        cmd.Parameters.AddWithValue("@Isk3", kalem.Isk3);
                                        cmd.Parameters.AddWithValue("@Isk4", kalem.Isk4);
                                        cmd.Parameters.AddWithValue("@NakliyeIskonto", kalem.NakliyeIskonto);
                                        cmd.Parameters.AddWithValue("@Tutar", kalem.Tutar);

                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }

                                trans.Commit();
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Veritabanı hatası: " + ex.Message);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantı hatası: " + ex.Message);
                return false;
            }
        }

        public void TemizleForm()
        {
            AlisCari = null;
            SatisCari = null;
            TeslimatCari = null;
            AlisKalemListesi.Clear();
            SatisKalemListesi.Clear();
            SiparisTarihi = DateTime.Today;
            SevkTarihi = DateTime.Today.AddDays(3);
            Aciklama1 = Aciklama2 = Aciklama3 = string.Empty;
            Iban = string.Empty;
            IsAlisKrediKartiOdeme = false;
            IsSatisKrediKartiOdeme = false;
            IsAlisFabrikaTeslim = false;
            IsSatisFabrikaTeslim = false;
            GuncelleToplamlar();
        }
        public async Task SiparisiYukleAsync(string belgeKodu)
        {
            string connStr = ConfigurationHelper.GetConnectionString("db");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();

                int siparisID = -1;

                string masterSql = "SELECT * FROM SiparisMaster WHERE BelgeKodu = @BelgeKodu";

                using (SqlCommand cmd = new SqlCommand(masterSql, conn))
                {
                    cmd.Parameters.AddWithValue("@BelgeKodu", belgeKodu);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            siparisID = Convert.ToInt32(reader["SiparisID"]);
                            BelgeKodu = reader["BelgeKodu"]?.ToString();
                            SiparisNo = reader["SiparisNo"]?.ToString();
                            SiparisTarihi = reader["SiparisTarihi"] as DateTime? ?? DateTime.Today;
                            SevkTarihi = reader["SevkTarihi"] as DateTime? ?? DateTime.Today.AddDays(3);
                            ProformaNo = reader["ProformaNo"]?.ToString();

                            // Alış Cari bilgileri
                            AlisCari = new CariModel
                            {
                                CariKod = reader["CariKod"]?.ToString(),
                                CariAdi = reader["CariAd"]?.ToString(),
                                VergiDairesi = reader["VergiDairesi"]?.ToString(),
                                VergiNo = reader["VergiNo"]?.ToString(),
                                Telefon = reader["Telefon"]?.ToString(),
                                Adres = reader["CariAdres"]?.ToString()
                            };

                            // Teslimat Cari bilgileri
                            TeslimatCari = new CariModel
                            {
                                CariKod = reader["TeslimKod"]?.ToString(),
                                CariAdi = reader["TeslimIsim"]?.ToString(),
                                Telefon = reader["TeslimTelefon"]?.ToString(),
                                Adres = reader["TeslimAdres"]?.ToString(),
                                Yetkili = reader["YetkiliKisi"]?.ToString()
                            };

                            // Satış Cari bilgileri
                            SatisCari = new CariModel
                            {
                                CariKod = reader["SatisCariKod"]?.ToString(),
                                CariAdi = reader["SatisCariAd"]?.ToString(),
                                VergiDairesi = reader["SatisVergiDairesi"]?.ToString(),
                                VergiNo = reader["SatisVergiNo"]?.ToString(),
                                Telefon = reader["SatisTelefon"]?.ToString(),
                                Adres = reader["SatisCariAdres"]?.ToString()
                            };

                            Aciklama1 = reader["Aciklama1"]?.ToString() ?? "";
                            Aciklama2 = reader["Aciklama2"]?.ToString() ?? "";
                            Aciklama3 = reader["Aciklama3"]?.ToString() ?? "";
                            Aciklama4 = reader["Aciklama4"]?.ToString() ?? "";
                            Aciklama5 = reader["Aciklama5"]?.ToString() ?? "";
                            Aciklama6 = reader["Aciklama6"]?.ToString() ?? "";

                            // Fabrika teslim checkbox'ları
                            IsAlisFabrikaTeslim = reader["FabrikaTeslim"] != DBNull.Value && Convert.ToBoolean(reader["FabrikaTeslim"]);
                            IsSatisFabrikaTeslim = reader["SatisFabrikaTeslim"] != DBNull.Value && Convert.ToBoolean(reader["SatisFabrikaTeslim"]);

                            // Ödeme tipleri
                            IsAlisOnOdeme = reader["OdemeNakit"] != DBNull.Value && Convert.ToBoolean(reader["OdemeNakit"]);
                            IsSatisOnOdeme = reader["SatisOdemeNakit"] != DBNull.Value && Convert.ToBoolean(reader["SatisOdemeNakit"]);

                            IsAlisKrediKartiOdeme = reader["OdemeKrediKarti"] != DBNull.Value && Convert.ToBoolean(reader["OdemeKrediKarti"]);
                            IsSatisKrediKartiOdeme = reader["SatisOdemeKrediKarti"] != DBNull.Value && Convert.ToBoolean(reader["SatisOdemeKrediKarti"]);

                            // Vadeler (ComboBox)
                            AlisVade = reader["OdemeVade"]?.ToString();
                            SatisVade = reader["SatisOdemeVade"]?.ToString();



                        }
                        else
                        {
                            MessageBox.Show("Belge bulunamadı.");
                            return;
                        }
                    }
                }

                if (siparisID > 0)
                {
                    await AlisKalemleriniYukle(conn, siparisID);
                    await SatisKalemleriniYukle(conn, siparisID);
                    
                    var tumStokKodlari = AlisKalemListesi
                        .Select(k => k.StokKodu)
                        .Concat(SatisKalemListesi.Select(k => k.StokKodu))
                        .Where(k => !string.IsNullOrWhiteSpace(k))
                        .Distinct()
                        .ToList();

                    var stokListesi = await StokBilgileriniToplucaGetir(conn, tumStokKodlari);

                    StokListesi.Clear();
                    foreach (var stok in stokListesi)
                    {
                        StokListesi.Add(stok);
                    }
                }
                
                SeciliSiparisID = siparisID;
            }
        }

        private async Task AlisKalemleriniYukle(SqlConnection conn, int siparisID)
        {
            string sql = "SELECT * FROM SiparisKalemAlis WHERE SiparisID = @SiparisID";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@SiparisID", siparisID);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    AlisKalemListesi.Clear();

                    while (await reader.ReadAsync())
                    {
                        var kalem = new KalemModel
                        {
                            Id = alisKalemIdSayaci,
                            StokKodu = reader["StokKodu"]?.ToString(),
                            StokAdi = reader["StokAdi"]?.ToString(),
                            Birim = reader["Birim"]?.ToString(),
                            Miktar = reader["Miktar"] != DBNull.Value ? Convert.ToDecimal(reader["Miktar"]) : 0,
                            BirimFiyat = reader["BirimFiyat"] != DBNull.Value ? Convert.ToDecimal(reader["BirimFiyat"]) : 0,
                            KDV = reader["KDV"] != DBNull.Value ? Convert.ToDecimal(reader["KDV"]) : 0,
                            Isk1 = reader["Isk1"] != DBNull.Value ? Convert.ToDecimal(reader["Isk1"]) : 0,
                            Isk2 = reader["Isk2"] != DBNull.Value ? Convert.ToDecimal(reader["Isk2"]) : 0,
                            Isk3 = reader["Isk3"] != DBNull.Value ? Convert.ToDecimal(reader["Isk3"]) : 0,
                            Isk4 = reader["Isk4"] != DBNull.Value ? Convert.ToDecimal(reader["Isk4"]) : 0,
                            NakliyeIskonto = reader["NakliyeIskonto"] != DBNull.Value ? Convert.ToDecimal(reader["NakliyeIskonto"]) : 0
                        };

                        // ✅ ViewModel'den fabrika teslim bilgisi aktarılıyor mu?
                        kalem.IsAlisFabrikaTeslim = this.IsAlisFabrikaTeslim;

                        // ✅ Tutarı hesapla
                        kalem.HesaplaTutar();

                        AlisKalemListesi.Add(kalem);
                        alisKalemIdSayaci += 2;
                    }
                }
            }
        }



        private async Task SatisKalemleriniYukle(SqlConnection conn, int siparisID)
        {
            string sql = "SELECT * FROM SiparisKalemSatis WHERE SiparisID = @SiparisID";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@SiparisID", siparisID);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    SatisKalemListesi.Clear();

                    while (await reader.ReadAsync())
                    {
                        var kalem = new KalemModel
                        {
                            Id = satisKalemIdSayaci,
                            StokKodu = reader["StokKodu"]?.ToString(),
                            StokAdi = reader["StokAdi"]?.ToString(),
                            Birim = reader["Birim"]?.ToString(),
                            Miktar = reader["Miktar"] != DBNull.Value ? Convert.ToDecimal(reader["Miktar"]) : 0,
                            BirimFiyat = reader["BirimFiyat"] != DBNull.Value ? Convert.ToDecimal(reader["BirimFiyat"]) : 0,
                            KDV = reader["KDV"] != DBNull.Value ? Convert.ToDecimal(reader["KDV"]) : 0,
                            Isk1 = reader["Isk1"] != DBNull.Value ? Convert.ToDecimal(reader["Isk1"]) : 0,
                            Isk2 = reader["Isk2"] != DBNull.Value ? Convert.ToDecimal(reader["Isk2"]) : 0,
                            Isk3 = reader["Isk3"] != DBNull.Value ? Convert.ToDecimal(reader["Isk3"]) : 0,
                            Isk4 = reader["Isk4"] != DBNull.Value ? Convert.ToDecimal(reader["Isk4"]) : 0,
                            NakliyeIskonto = reader["NakliyeIskonto"] != DBNull.Value ? Convert.ToDecimal(reader["NakliyeIskonto"]) : 0
                        };

                        // ✅ ViewModel'den fabrika teslim bilgisi aktarılıyor mu?
                        kalem.IsSatisFabrikaTeslim = this.IsSatisFabrikaTeslim;

                        // ✅ Tutarı hesapla
                        kalem.HesaplaTutar();

                        SatisKalemListesi.Add(kalem);
                        satisKalemIdSayaci += 2;
                    }
                }
            }
        }


        private async Task<List<StokModel>> StokBilgileriniToplucaGetir(SqlConnection conn, List<string> stokKodlari)
        {
            if (stokKodlari == null || !stokKodlari.Any())
                return new List<StokModel>();

            // Parametreleri SQL injection'dan koruyarak dinamik hale getiriyoruz
            string paramList = string.Join(", ", stokKodlari.Select((s, i) => $"@p{i}"));
            string sql = $"SELECT * FROM STOKSABITKART WHERE STOKKODU IN ({paramList})";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                for (int i = 0; i < stokKodlari.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"@p{i}", stokKodlari[i]);
                }

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    var stokList = new List<StokModel>();

                    while (await reader.ReadAsync())
                    {
                        var stok = new StokModel
                        {
                            StokKodu = reader["STOKKODU"]?.ToString(),
                            StokAdi = reader["STOKADI"]?.ToString(),
                            Birim = reader["BIRIM"]?.ToString(),
                            
                            // Diğer alanlar burada eklenebilir
                        };

                        stokList.Add(stok);
                    }

                    return stokList;
                }
            }
        }

        public async Task<bool> SiparisiGuncelleAsync(int siparisID)
        {
            string connStr = ConfigurationHelper.GetConnectionString("db");

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Kalemleri sil
                            string deleteAlisSql = "DELETE FROM SiparisKalemAlis WHERE SiparisID = @SiparisID";
                            string deleteSatisSql = "DELETE FROM SiparisKalemSatis WHERE SiparisID = @SiparisID";

                            using (SqlCommand cmd = new SqlCommand(deleteAlisSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@SiparisID", siparisID);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            using (SqlCommand cmd = new SqlCommand(deleteSatisSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@SiparisID", siparisID);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 2. Master güncelle
                            string updateMasterSql = @"
UPDATE SiparisMaster SET
    BelgeKodu = @BelgeKodu,
    SiparisNo = @SiparisNo,
    SiparisTarihi = @SiparisTarihi,
    SevkTarihi = @SevkTarihi,
    CariKod = @CariKod,
    CariAd = @CariAd,
    VergiDairesi = @VergiDairesi,
    VergiNo = @VergiNo,
    Telefon = @Telefon,
    CariAdres = @CariAdres,
    ProformaNo = @ProformaNo,
    TeslimKod = @TeslimKod,
    TeslimIsim = @Teslimisim,
    TeslimAdres = @TeslimAdres,
    TeslimTelefon = @TeslimTelefon,
    YetkiliKisi = @YetkiliKisi,
    OdemeNakit = @AlisOnOdeme,
    OdemeKrediKarti = @AlisKrediKarti,
    OdemeVade = @AlisVade,
    FabrikaTeslim = @AlisFabirkaTeslim,
    SatisOdemeNakit = @SatisOnOdeme,
    SatisOdemeKrediKarti = @SatisKrediKarti,
    SatisOdemeVade = @SatisVade,
    SatisFabrikaTeslim = @SatisFabrikaTeslim,
    SatisCariKod = @SatisCariKod,
    SatisCariAd = @SatisCariAd,
    SatisVergiDairesi = @SatisVergiDairesi,
    SatisVergiNo = @SatisVergiNo,
    SatisTelefon = @SatisTelefon,
    SatisCariAdres = @SatisCariAdres,
    Aciklama1 = @Aciklama1,
    Aciklama2 = @Aciklama2,
    Aciklama3 = @Aciklama3,
    Aciklama4 = @Aciklama4,
    Aciklama5 = @Aciklama5,
    Aciklama6 = @Aciklama6,
    AlisToplamTutar = @AlisToplamTutar,
    SatisToplamTutar = @SatisToplamTutar
WHERE SiparisID = @SiparisID";

                            using (SqlCommand cmd = new SqlCommand(updateMasterSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@SiparisID", siparisID);
                                cmd.Parameters.AddWithValue("@BelgeKodu", BelgeKodu);
                                cmd.Parameters.AddWithValue("@SiparisNo", SiparisNo);
                                cmd.Parameters.AddWithValue("@SiparisTarihi", SiparisTarihi);
                                cmd.Parameters.AddWithValue("@SevkTarihi", (object)SevkTarihi ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@CariKod", AlisCari.CariKod);
                                cmd.Parameters.AddWithValue("@CariAd", AlisCari.CariAdi ?? "");
                                cmd.Parameters.AddWithValue("@VergiDairesi", AlisCari.VergiDairesi ?? "");
                                cmd.Parameters.AddWithValue("@VergiNo", AlisCari.VergiNo ?? "");
                                cmd.Parameters.AddWithValue("@Telefon", AlisCari.Telefon ?? "");
                                cmd.Parameters.AddWithValue("@CariAdres", AlisCari.Adres ?? "");
                                cmd.Parameters.AddWithValue("@ProformaNo", ProformaNo ?? "");

                                cmd.Parameters.AddWithValue("@TeslimKod", TeslimatCari.CariKod ?? "");
                                cmd.Parameters.AddWithValue("@Teslimisim", TeslimatCari.CariAdi ?? "");
                                cmd.Parameters.AddWithValue("@TeslimAdres", TeslimatCari.Adres ?? "");
                                cmd.Parameters.AddWithValue("@TeslimTelefon", TeslimatCari.Telefon ?? "");
                                cmd.Parameters.AddWithValue("@YetkiliKisi", TeslimatCari.Yetkili ?? "");

                                cmd.Parameters.AddWithValue("@AlisOnOdeme", IsAlisOnOdeme);
                                cmd.Parameters.AddWithValue("@AlisKrediKarti", IsAlisKrediKartiOdeme);
                                cmd.Parameters.AddWithValue("@AlisVade", string.IsNullOrEmpty(AlisVade) ? DBNull.Value : (object)AlisVade);
                                cmd.Parameters.AddWithValue("@AlisFabirkaTeslim", IsAlisFabrikaTeslim);

                                cmd.Parameters.AddWithValue("@SatisOnOdeme", IsSatisOnOdeme);
                                cmd.Parameters.AddWithValue("@SatisKrediKarti", IsSatisKrediKartiOdeme);
                                cmd.Parameters.AddWithValue("@SatisVade", string.IsNullOrEmpty(SatisVade) ? DBNull.Value : (object)SatisVade);
                                cmd.Parameters.AddWithValue("@SatisFabrikaTeslim", IsSatisFabrikaTeslim);

                                cmd.Parameters.AddWithValue("@SatisCariKod", SatisCari.CariKod ?? "");
                                cmd.Parameters.AddWithValue("@SatisCariAd", SatisCari.CariAdi ?? "");
                                cmd.Parameters.AddWithValue("@SatisVergiDairesi", SatisCari.VergiDairesi ?? "");
                                cmd.Parameters.AddWithValue("@SatisVergiNo", SatisCari.VergiNo ?? "");
                                cmd.Parameters.AddWithValue("@SatisTelefon", SatisCari.Telefon ?? "");
                                cmd.Parameters.AddWithValue("@SatisCariAdres", SatisCari.Adres ?? "");

                                cmd.Parameters.AddWithValue("@Aciklama1", string.IsNullOrWhiteSpace(Aciklama1) ? DBNull.Value : (object)Aciklama1);
                                cmd.Parameters.AddWithValue("@Aciklama2", string.IsNullOrWhiteSpace(Aciklama2) ? DBNull.Value : (object)Aciklama2);
                                cmd.Parameters.AddWithValue("@Aciklama3", string.IsNullOrWhiteSpace(Aciklama3) ? DBNull.Value : (object)Aciklama3);
                                cmd.Parameters.AddWithValue("@Aciklama4", string.IsNullOrWhiteSpace(Aciklama4) ? DBNull.Value : (object)Aciklama4);
                                cmd.Parameters.AddWithValue("@Aciklama5", string.IsNullOrWhiteSpace(Aciklama5) ? DBNull.Value : (object)Aciklama5);
                                cmd.Parameters.AddWithValue("@Aciklama6", string.IsNullOrWhiteSpace(Aciklama6) ? DBNull.Value : (object)Aciklama6);

                                cmd.Parameters.AddWithValue("@AlisToplamTutar", AlisToplamTutar);
                                cmd.Parameters.AddWithValue("@SatisToplamTutar", SatisToplamTutar);

                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 3. Alış kalemlerini tekrar ekle
                            string insertAlisSql = @"
INSERT INTO SiparisKalemAlis
(SiparisID, StokKodu, StokAdi, Birim, Miktar, BirimFiyat, KDV, Isk1, Isk2, Isk3, Isk4, NakliyeIskonto, Tutar)
VALUES
(@SiparisID, @StokKodu, @StokAdi, @Birim, @Miktar, @BirimFiyat, @KDV, @Isk1, @Isk2, @Isk3, @Isk4, @NakliyeIskonto, @Tutar)";

                            foreach (var kalem in AlisKalemListesi)
                            {
                                using (SqlCommand cmd = new SqlCommand(insertAlisSql, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@SiparisID", siparisID);
                                    cmd.Parameters.AddWithValue("@StokKodu", kalem.StokKodu);
                                    cmd.Parameters.AddWithValue("@StokAdi", kalem.StokAdi ?? "");
                                    cmd.Parameters.AddWithValue("@Birim", kalem.Birim ?? "");
                                    cmd.Parameters.AddWithValue("@Miktar", kalem.Miktar);
                                    cmd.Parameters.AddWithValue("@BirimFiyat", kalem.BirimFiyat);
                                    cmd.Parameters.AddWithValue("@KDV", kalem.KDV);
                                    cmd.Parameters.AddWithValue("@Isk1", kalem.Isk1);
                                    cmd.Parameters.AddWithValue("@Isk2", kalem.Isk2);
                                    cmd.Parameters.AddWithValue("@Isk3", kalem.Isk3);
                                    cmd.Parameters.AddWithValue("@Isk4", kalem.Isk4);
                                    cmd.Parameters.AddWithValue("@NakliyeIskonto", kalem.NakliyeIskonto);
                                    cmd.Parameters.AddWithValue("@Tutar", kalem.Tutar);

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }

                            // 4. Satış kalemlerini tekrar ekle
                            string insertSatisSql = @"
INSERT INTO SiparisKalemSatis
(SiparisID, StokKodu, StokAdi, Birim, Miktar, BirimFiyat, KDV, Isk1, Isk2, Isk3, Isk4, NakliyeIskonto, Tutar)
VALUES
(@SiparisID, @StokKodu, @StokAdi, @Birim, @Miktar, @BirimFiyat, @KDV, @Isk1, @Isk2, @Isk3, @Isk4, @NakliyeIskonto, @Tutar)";

                            foreach (var kalem in SatisKalemListesi)
                            {
                                using (SqlCommand cmd = new SqlCommand(insertSatisSql, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@SiparisID", siparisID);
                                    cmd.Parameters.AddWithValue("@StokKodu", kalem.StokKodu);
                                    cmd.Parameters.AddWithValue("@StokAdi", kalem.StokAdi ?? "");
                                    cmd.Parameters.AddWithValue("@Birim", kalem.Birim ?? "");
                                    cmd.Parameters.AddWithValue("@Miktar", kalem.Miktar);
                                    cmd.Parameters.AddWithValue("@BirimFiyat", kalem.BirimFiyat);
                                    cmd.Parameters.AddWithValue("@KDV", kalem.KDV);
                                    cmd.Parameters.AddWithValue("@Isk1", kalem.Isk1);
                                    cmd.Parameters.AddWithValue("@Isk2", kalem.Isk2);
                                    cmd.Parameters.AddWithValue("@Isk3", kalem.Isk3);
                                    cmd.Parameters.AddWithValue("@Isk4", kalem.Isk4);
                                    cmd.Parameters.AddWithValue("@NakliyeIskonto", kalem.NakliyeIskonto);
                                    cmd.Parameters.AddWithValue("@Tutar", kalem.Tutar);

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }

                            // 5. Hepsi başarılı, commit
                            trans.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Güncelleme sırasında hata oluştu: " + ex.Message);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı bağlantı hatası: " + ex.Message);
                return false;
            }
        }
        public async Task<bool> SiparisiSilAsync(int siparisID)
        {
            string connStr = ConfigurationHelper.GetConnectionString("db");

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Alış kalemlerini sil
                            string deleteAlisSql = "DELETE FROM SiparisKalemAlis WHERE SiparisID = @SiparisID";
                            using (SqlCommand cmd = new SqlCommand(deleteAlisSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@SiparisID", siparisID);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 2. Satış kalemlerini sil
                            string deleteSatisSql = "DELETE FROM SiparisKalemSatis WHERE SiparisID = @SiparisID";
                            using (SqlCommand cmd = new SqlCommand(deleteSatisSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@SiparisID", siparisID);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 3. Master kaydını sil
                            string deleteMasterSql = "DELETE FROM SiparisMaster WHERE SiparisID = @SiparisID";
                            using (SqlCommand cmd = new SqlCommand(deleteMasterSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@SiparisID", siparisID);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 4. Commit
                            trans.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Silme işlemi sırasında hata oluştu: " + ex.Message);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı bağlantı hatası: " + ex.Message);
                return false;
            }
        }


    }

}