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

                // Create Parametreler Table
                string createParametrelerTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Parametreler' and xtype='U')
                CREATE TABLE Parametreler (
                    ParametreId INT PRIMARY KEY IDENTITY(1,1),
                    YagKesintiParametresi DECIMAL(5, 2),
                    ProteinParametresi DECIMAL(5, 2),
                    DizemBasiTl DECIMAL(5, 2),
                    CreatedAt DATETIME DEFAULT GETDATE()
                )";
                using (var command = new SqlCommand(createParametrelerTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create Users Table for authentication
                string createUsersTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' and xtype='U')
                CREATE TABLE Users (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Username NVARCHAR(50) UNIQUE NOT NULL,
                    PasswordHash NVARCHAR(255) NOT NULL,
                    Email NVARCHAR(100),
                    FullName NVARCHAR(100),
                    Role NVARCHAR(50) DEFAULT 'User',
                    IsActive BIT DEFAULT 1,
                    CreatedAt DATETIME DEFAULT GETDATE(),
                    LastLoginAt DATETIME
                )";
                using (var command = new SqlCommand(createUsersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in SutKayit table, and if not, add them
                string checkUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SutKayit') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE SutKayit ADD CreatedBy INT NULL;
                    ALTER TABLE SutKayit ADD ModifiedBy INT NULL;
                    ALTER TABLE SutKayit ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE SutKayit ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in Cari table, and if not, add them
                string checkCariUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Cari') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE Cari ADD CreatedBy INT NULL;
                    ALTER TABLE Cari ADD ModifiedBy INT NULL;
                    ALTER TABLE Cari ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE Cari ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkCariUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in CASABIT table, and if not, add them
                string checkCasaBitUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CASABIT') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE CASABIT ADD CreatedBy INT NULL;
                    ALTER TABLE CASABIT ADD ModifiedBy INT NULL;
                    ALTER TABLE CASABIT ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE CASABIT ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkCasaBitUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in DepoStok table, and if not, add them
                string checkDepoStokUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DepoStok') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE DepoStok ADD CreatedBy INT NULL;
                    ALTER TABLE DepoStok ADD ModifiedBy INT NULL;
                    ALTER TABLE DepoStok ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE DepoStok ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkDepoStokUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in STOKSABITKART table, and if not, add them
                string checkStokSabitKartUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('STOKSABITKART') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE STOKSABITKART ADD CreatedBy INT NULL;
                    ALTER TABLE STOKSABITKART ADD ModifiedBy INT NULL;
                    ALTER TABLE STOKSABITKART ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE STOKSABITKART ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkStokSabitKartUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in STOKSABITFIYAT table, and if not, add them
                string checkStokSabitFiyatUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('STOKSABITFIYAT') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE STOKSABITFIYAT ADD CreatedBy INT NULL;
                    ALTER TABLE STOKSABITFIYAT ADD ModifiedBy INT NULL;
                    ALTER TABLE STOKSABITFIYAT ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE STOKSABITFIYAT ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkStokSabitFiyatUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in STOKSABITBELGE table, and if not, add them
                string checkStokSabitBelgeUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('STOKSABITBELGE') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE STOKSABITBELGE ADD CreatedBy INT NULL;
                    ALTER TABLE STOKSABITBELGE ADD ModifiedBy INT NULL;
                    ALTER TABLE STOKSABITBELGE ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE STOKSABITBELGE ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkStokSabitBelgeUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in STOKSABITHAREKET table, and if not, add them
                string checkStokSabitHareketUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('STOKSABITHAREKET') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE STOKSABITHAREKET ADD CreatedBy INT NULL;
                    ALTER TABLE STOKSABITHAREKET ADD ModifiedBy INT NULL;
                    ALTER TABLE STOKSABITHAREKET ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE STOKSABITHAREKET ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkStokSabitHareketUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in STOKSABITTED table, and if not, add them
                string checkStokSabitTedUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('STOKSABITTED') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE STOKSABITTED ADD CreatedBy INT NULL;
                    ALTER TABLE STOKSABITTED ADD ModifiedBy INT NULL;
                    ALTER TABLE STOKSABITTED ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE STOKSABITTED ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkStokSabitTedUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in SiparisMaster table, and if not, add them
                string checkSiparisMasterUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SiparisMaster') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE SiparisMaster ADD CreatedBy INT NULL;
                    ALTER TABLE SiparisMaster ADD ModifiedBy INT NULL;
                    ALTER TABLE SiparisMaster ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE SiparisMaster ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkSiparisMasterUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in SiparisKalemAlis table, and if not, add them
                string checkSiparisKalemAlisUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SiparisKalemAlis') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE SiparisKalemAlis ADD CreatedBy INT NULL;
                    ALTER TABLE SiparisKalemAlis ADD ModifiedBy INT NULL;
                    ALTER TABLE SiparisKalemAlis ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE SiparisKalemAlis ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkSiparisKalemAlisUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in SiparisKalemSatis table, and if not, add them
                string checkSiparisKalemSatisUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SiparisKalemSatis') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE SiparisKalemSatis ADD CreatedBy INT NULL;
                    ALTER TABLE SiparisKalemSatis ADD ModifiedBy INT NULL;
                    ALTER TABLE SiparisKalemSatis ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE SiparisKalemSatis ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkSiparisKalemSatisUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in Numarator table, and if not, add them
                string checkNumaratorUserTrackingColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Numarator') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE Numarator ADD CreatedBy INT NULL;
                    ALTER TABLE Numarator ADD ModifiedBy INT NULL;
                    ALTER TABLE Numarator ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE Numarator ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkNumaratorUserTrackingColumns, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in STOKSABITKART table, and if not, add them (added again after creating the table)
                string checkStokSabitKartUserTrackingColumns2 = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('STOKSABITKART') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE STOKSABITKART ADD CreatedBy INT NULL;
                    ALTER TABLE STOKSABITKART ADD ModifiedBy INT NULL;
                    ALTER TABLE STOKSABITKART ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE STOKSABITKART ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkStokSabitKartUserTrackingColumns2, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in STOKSABITFIYAT table, and if not, add them (added again after creating the table)
                string checkStokSabitFiyatUserTrackingColumns2 = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('STOKSABITFIYAT') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE STOKSABITFIYAT ADD CreatedBy INT NULL;
                    ALTER TABLE STOKSABITFIYAT ADD ModifiedBy INT NULL;
                    ALTER TABLE STOKSABITFIYAT ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE STOKSABITFIYAT ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkStokSabitFiyatUserTrackingColumns2, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in STOKSABITBELGE table, and if not, add them (added again after creating the table)
                string checkStokSabitBelgeUserTrackingColumns2 = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('STOKSABITBELGE') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE STOKSABITBELGE ADD CreatedBy INT NULL;
                    ALTER TABLE STOKSABITBELGE ADD ModifiedBy INT NULL;
                    ALTER TABLE STOKSABITBELGE ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE STOKSABITBELGE ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkStokSabitBelgeUserTrackingColumns2, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in STOKSABITHAREKET table, and if not, add them (added again after creating the table)
                string checkStokSabitHareketUserTrackingColumns2 = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('STOKSABITHAREKET') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE STOKSABITHAREKET ADD CreatedBy INT NULL;
                    ALTER TABLE STOKSABITHAREKET ADD ModifiedBy INT NULL;
                    ALTER TABLE STOKSABITHAREKET ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE STOKSABITHAREKET ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkStokSabitHareketUserTrackingColumns2, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in STOKSABITTED table, and if not, add them (added again after creating the table)
                string checkStokSabitTedUserTrackingColumns2 = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('STOKSABITTED') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE STOKSABITTED ADD CreatedBy INT NULL;
                    ALTER TABLE STOKSABITTED ADD ModifiedBy INT NULL;
                    ALTER TABLE STOKSABITTED ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE STOKSABITTED ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkStokSabitTedUserTrackingColumns2, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in SiparisMaster table, and if not, add them (added again after creating the table)
                string checkSiparisMasterUserTrackingColumns2 = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SiparisMaster') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE SiparisMaster ADD CreatedBy INT NULL;
                    ALTER TABLE SiparisMaster ADD ModifiedBy INT NULL;
                    ALTER TABLE SiparisMaster ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE SiparisMaster ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkSiparisMasterUserTrackingColumns2, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in SiparisKalemAlis table, and if not, add them (added again after creating the table)
                string checkSiparisKalemAlisUserTrackingColumns2 = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SiparisKalemAlis') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE SiparisKalemAlis ADD CreatedBy INT NULL;
                    ALTER TABLE SiparisKalemAlis ADD ModifiedBy INT NULL;
                    ALTER TABLE SiparisKalemAlis ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE SiparisKalemAlis ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkSiparisKalemAlisUserTrackingColumns2, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

                // Check if user tracking columns exist in SiparisKalemSatis table, and if not, add them (added again after creating the table)
                string checkSiparisKalemSatisUserTrackingColumns2 = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SiparisKalemSatis') AND name = 'CreatedBy')
                BEGIN
                    ALTER TABLE SiparisKalemSatis ADD CreatedBy INT NULL;
                    ALTER TABLE SiparisKalemSatis ADD ModifiedBy INT NULL;
                    ALTER TABLE SiparisKalemSatis ADD CreatedAt DATETIME DEFAULT GETDATE();
                    ALTER TABLE SiparisKalemSatis ADD ModifiedAt DATETIME DEFAULT GETDATE();
                END";
                using (var checkCmd = new SqlCommand(checkSiparisKalemSatisUserTrackingColumns2, connection))
                {
                    checkCmd.ExecuteNonQuery();
                }

            }
        }
    }
}