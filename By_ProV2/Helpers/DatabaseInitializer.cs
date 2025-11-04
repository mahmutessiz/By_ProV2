using System;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace By_ProV2.Helpers
{
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create Cari Table
                string createCariTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Cari' and xtype='U')
                CREATE TABLE Cari (
                    CariId INT PRIMARY KEY IDENTITY(1,1),
                    CariKod NVARCHAR(50) NOT NULL,
                    CariAdi NVARCHAR(255),
                    Tipi NVARCHAR(50),
                    Adres NVARCHAR(MAX),
                    VergiDairesi NVARCHAR(100),
                    VergiNo NVARCHAR(50),
                    Telefon NVARCHAR(50),
                    Yetkili NVARCHAR(100),
                    KKIsk1 DECIMAL(5, 2),
                    KKIsk2 DECIMAL(5, 2),
                    KKIsk3 DECIMAL(5, 2),
                    KKIsk4 DECIMAL(5, 2),
                    Isk1 DECIMAL(5, 2),
                    Isk2 DECIMAL(5, 2),
                    Isk3 DECIMAL(5, 2),
                    Isk4 DECIMAL(5, 2),
                    NakliyeIskonto DECIMAL(5, 2),
                    SoforAdSoyad NVARCHAR(255),
                    Plaka1 NVARCHAR(50),
                    Plaka2 NVARCHAR(50)
                )";
                using (var command = new SqlCommand(createCariTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create SutKayit Table
                string createSutKayitTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SutKayit' and xtype='U')
                CREATE TABLE SutKayit (
                    SutKayitId INT PRIMARY KEY IDENTITY(1,1),
                    BelgeNo NVARCHAR(50),
                    Tarih DATETIME NOT NULL,
                    IslemTuru NVARCHAR(50),
                    TedarikciId INT,
                    MusteriId INT,
                    Miktar DECIMAL(18, 2),
                    Yag DECIMAL(5, 2),
                    Protein DECIMAL(5, 2),
                    Laktoz DECIMAL(5, 2),
                    Fiyat DECIMAL(18, 4),
                    TKM DECIMAL(5, 2),
                    YKM DECIMAL(5, 2),
                    pH DECIMAL(4, 2),
                    Iletkenlik DECIMAL(10, 2),
                    Sicaklik DECIMAL(5, 2),
                    Yogunluk DECIMAL(10, 4),
                    Kesinti DECIMAL(18, 2),
                    Antibiyotik NVARCHAR(50),
                    Arac NVARCHAR(100),
                    Plaka NVARCHAR(50),
                    DonmaN DECIMAL(5, 3),
                    Bakteri DECIMAL(18, 0),
                    Somatik DECIMAL(18, 0),
                    Durumu NVARCHAR(50),
                    Aciklama NVARCHAR(MAX),
                    FOREIGN KEY (TedarikciId) REFERENCES Cari(CariId),
                    FOREIGN KEY (MusteriId) REFERENCES Cari(CariId)
                )";
                using (var command = new SqlCommand(createSutKayitTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Check if BelgeNo column exists, and if not, add it
                string checkColumnQuery = @"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID('SutKayit') AND name = 'BelgeNo'
                )
                BEGIN
                    ALTER TABLE SutKayit ADD BelgeNo NVARCHAR(50);
                END";
                using (var checkCmd = new SqlCommand(checkColumnQuery, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Create DepoStok Table
                string createDepoStokTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DepoStok' and xtype='U')
                CREATE TABLE DepoStok (
                    DepoStokId INT PRIMARY KEY IDENTITY(1,1),
                    Tarih DATETIME NOT NULL,
                    TedarikciId INT,
                    Miktar DECIMAL(18, 2),
                    Yag DECIMAL(5, 2),
                    Protein DECIMAL(5, 2),
                    TKM DECIMAL(5, 2),
                    FOREIGN KEY (TedarikciId) REFERENCES Cari(CariId)
                )";
                using (var command = new SqlCommand(createDepoStokTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create STOKSABITKART Table
                string createStokSabitKartTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='STOKSABITKART' and xtype='U')
                CREATE TABLE STOKSABITKART (
                    STOKID INT PRIMARY KEY IDENTITY(1,1),
                    STOKKODU NVARCHAR(50) NOT NULL,
                    STOKADI NVARCHAR(255),
                    BIRIM NVARCHAR(50),
                    AGIRLIK DECIMAL(18, 2),
                    PROTEIN DECIMAL(18, 2),
                    ENERJI DECIMAL(18, 2),
                    NEM DECIMAL(18, 2),
                    BARKOD NVARCHAR(100),
                    YEMOZELLIGI NVARCHAR(100),
                    ACIKLAMA NVARCHAR(MAX),
                    MENSEI NVARCHAR(50),
                    AKTIF BIT,
                    OLUSTURMATARIHI DATETIME
                )";
                using (var command = new SqlCommand(createStokSabitKartTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create STOKSABITFIYAT Table
                string createStokSabitFiyatTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='STOKSABITFIYAT' and xtype='U')
                CREATE TABLE STOKSABITFIYAT (
                    FIYATID INT PRIMARY KEY IDENTITY(1,1),
                    STOKID INT,
                    LISTEADI NVARCHAR(100),
                    LISTETARIHI DATETIME,
                    ALISFIYAT1 DECIMAL(18, 4),
                    ALISFIYAT2 DECIMAL(18, 4),
                    ALISFIYAT3 DECIMAL(18, 4),
                    ALISFIYAT4 DECIMAL(18, 4),
                    ALISFIYAT5 DECIMAL(18, 4),
                    KDVORANI DECIMAL(5, 2),
                    PARABIRIMI NVARCHAR(10),
                    AKTIF BIT,
                    KAYITTARIHI DATETIME,
                    OLUSTURANKULLANICI NVARCHAR(100),
                    FOREIGN KEY (STOKID) REFERENCES STOKSABITKART(STOKID)
                )";
                using (var command = new SqlCommand(createStokSabitFiyatTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create STOKSABITBELGE Table
                string createStokSabitBelgeTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='STOKSABITBELGE' and xtype='U')
                CREATE TABLE STOKSABITBELGE (
                    BELGEID INT PRIMARY KEY IDENTITY(1,1),
                    STOKID INT,
                    BELGETIPI NVARCHAR(100),
                    DOSYAYOLU NVARCHAR(MAX),
                    EKLEMETARIHI DATETIME,
                    FOREIGN KEY (STOKID) REFERENCES STOKSABITKART(STOKID)
                )";
                using (var command = new SqlCommand(createStokSabitBelgeTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create STOKSABITHAREKET Table
                string createStokSabitHareketTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='STOKSABITHAREKET' and xtype='U')
                CREATE TABLE STOKSABITHAREKET (
                    HAREKETID INT PRIMARY KEY IDENTITY(1,1),
                    STOKID INT,
                    HAREKETTURU NVARCHAR(50),
                    MIKTAR DECIMAL(18, 2),
                    BIRIM NVARCHAR(50),
                    DEPOID INT,
                    ISLEMTARIHI DATETIME,
                    FOREIGN KEY (STOKID) REFERENCES STOKSABITKART(STOKID)
                )";
                using (var command = new SqlCommand(createStokSabitHareketTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create STOKSABITTED Table
                string createStokSabitTedTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='STOKSABITTED' and xtype='U')
                CREATE TABLE STOKSABITTED (
                    TEDARIKCIID INT PRIMARY KEY IDENTITY(1,1),
                    TEDARIKCIADI NVARCHAR(255)
                )";
                using (var command = new SqlCommand(createStokSabitTedTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create SiparisMaster Table
                string createSiparisMasterTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SiparisMaster' and xtype='U')
                CREATE TABLE SiparisMaster (
                    SiparisID INT PRIMARY KEY IDENTITY(1,1),
                    BelgeKodu VARCHAR(20),
                    SiparisNo VARCHAR(20),
                    ProformaNo VARCHAR(20),
                    SiparisTarihi DATE,
                    SevkTarihi DATE,
                    CariKod VARCHAR(20),
                    SatisCariKod VARCHAR(20),
                    CariAd VARCHAR(100),
                    TeslimKod VARCHAR(50),
                    TeslimIsim VARCHAR(100),
                    AlisToplamTutar DECIMAL(18, 2),
                    SatisToplamTutar DECIMAL(18, 2),
                    VergiDairesi VARCHAR(50),
                    VergiNo VARCHAR(20),
                    Telefon VARCHAR(20),
                    CariAdres VARCHAR (200),
                    SatisCariAd VARCHAR(100),
                    SatisVergiDairesi VARCHAR(50),
                    SatisVergiNo VARCHAR (20),
                    SatisTelefon VARCHAR(20),
                    SatisCariAdres VARCHAR(200),
                    TeslimAdres VARCHAR(200),
                    TeslimTelefon VARCHAR(20),
                    YetkiliKisi VARCHAR(50),
                    OdemeNakit BIT,
                    OdemeKrediKarti BIT,
                    OdemeVade VARCHAR(50),
                    FabrikaTeslim BIT,
                    SatisOdemeNakit BIT,
                    SatisOdemeKrediKarti BIT,
                    SatisOdemeVade VARCHAR(50),
                    SatisFabrikaTeslim BIT,
                    Aciklama1 VARCHAR(200),
                    Aciklama2 VARCHAR(200),
                    Aciklama3 VARCHAR(200),
                    Aciklama4 VARCHAR(200),
                    Aciklama5 VARCHAR(200),
                    Aciklama6 VARCHAR(200)
                )";
                using (var command = new SqlCommand(createSiparisMasterTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create SiparisKalemalis Table
                string createSiparisKalemAlisTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SiparisKalemAlis' and xtype='U')
                CREATE TABLE SiparisKalemAlis (
                    KalemID INT PRIMARY KEY IDENTITY(1,1),
                    SiparisID INT,
                    StokKodu NVARCHAR(50),
                    StokAdi NVARCHAR(255),
                    Miktar DECIMAL(18, 2),
                    BirimFiyat DECIMAL(18, 4),
                    Birim VARCHAR(10),
                    KDV DECIMAL(5, 2),
                    Isk1 DECIMAL(5, 2),
                    Isk2 DECIMAL(5, 2),
                    Isk3 DECIMAL(5, 2),
                    Isk4 DECIMAL(5, 2),
                    NakliyeIskonto DECIMAL(5, 2),
                    Tutar DECIMAL(18, 2),
                    FOREIGN KEY (SiparisID) REFERENCES SiparisMaster(SiparisID)
                )";
                using (var command = new SqlCommand(createSiparisKalemAlisTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create SiparisKalemSatis Table
                string createSiparisKalemSatisTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SiparisKalemSatis' and xtype='U')
                CREATE TABLE SiparisKalemSatis (
                    KalemID INT PRIMARY KEY IDENTITY(1,1),
                    SiparisID INT,
                    StokKodu NVARCHAR(50),
                    StokAdi NVARCHAR(255),
                    Miktar DECIMAL(18, 2),
                    BirimFiyat DECIMAL(18, 4),
                     Birim VARCHAR(10),
                    KDV DECIMAL(5, 2),
                    Isk1 DECIMAL(5, 2),
                    Isk2 DECIMAL(5, 2),
                    Isk3 DECIMAL(5, 2),
                    Isk4 DECIMAL(5, 2),
                    NakliyeIskonto DECIMAL(5, 2),
                    Tutar DECIMAL(18, 2),
                    FOREIGN KEY (SiparisID) REFERENCES SiparisMaster(SiparisID)
                )";
                using (var command = new SqlCommand(createSiparisKalemSatisTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create Numarator Table
                string createNumaratorTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Numarator' and xtype='U')
                CREATE TABLE Numarator (
                    Yil NVARCHAR(4),
                    Tip NVARCHAR(10),
                    SonNumara INT,
                    PRIMARY KEY (Yil, Tip)
                )";
                using (var command = new SqlCommand(createNumaratorTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}