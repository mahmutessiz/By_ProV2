# By_ProV2 ERP Sistemi - Mimarlık Dokümantasyonu

## İçindekiler
1. [Genel Bakış](#genel-bakış)
2. [Sistem Mimarisı](#sistem-mimarisi)
3. [Veritabanı Şeması](#veritabanı-şeması)
4. [Kullanıcı Arayüzü Bileşenleri](#kullanıcı-arayüzü-bileşenleri)
5. [İş Mantığı Bileşenleri](#iş-mantığı-bileşenleri)
6. [Yetkilendirme ve Güvenlik](#yetkilendirme-ve-güvenlik)
7. [Veri Akışı](#veri-akışı)
8. [Ana Özellikler](#ana-özellikler)

## Genel Bakış

By_ProV2 ERP sistemi, özellikle "Beryem Tarım Ürünleri Gıda Nakliye Ticaret Ltd. Şti." için tasarlanmış kapsamlı bir kurumsal kaynak planlama uygulamasıdır. Sistem aşağıdaki iş süreçlerini yönetmektedir:

- Müşteri ilişki yönetimi
- Stok yönetimi
- Süt alım ve işleme
- Yem ticareti işlemleri
- Muhasebe ve takip
- Parametre yönetimi

## Sistem Mimarisı

### Teknoloji Yığını
- **Platform**: .NET 8.0 Windows
- **Framework**: WPF (Windows Presentation Foundation)
- **Veritabanı**: Microsoft SQL Server
- **Veri Erişimi**: Microsoft.Data.SqlClient
- **UI Dili**: Türkçe (tr-TR)
- **Yetkilendirme**: Özel yetkilendirme sistemi

### Mimari Katmanlar
```
┌─────────────────────────┐
│      Kullanıcı Arayüzü  │  (XAML + Kod arkası)
├─────────────────────────┤
│    İş Mantığı           │  (Servisler, Yardımcılar)
├─────────────────────────┤
│      Veri Erişimi       │  (Depolar, Modeller)
├─────────────────────────┤
│      Veritabanı         │  (SQL Server)
└─────────────────────────┘
```

### Ana Bileşenler

#### 1. Kullanıcı Arayüzü Katmanı
- WPF Pencereleri ve Kullanıcı Denetimleri
- XAML tabanlı düzenler
- MVVM kalıpları ile veri bağlama
- Türkçe yerelleştirme desteği

#### 2. İş Mantığı Katmanı
- İş işlemleri için servis sınıfları
- Ortak işlemler için yardımcı sınıflar
- Parametre yönetimi
- Kimlik doğrulama servisleri

#### 3. Veri Erişimi Katmanı
- Depo kalıbı uygulaması
- Model sınıfları (Veri Aktarım Nesneleri)
- Veritabanı bağlantı yönetimi

#### 4. Veritabanı Katmanı
- SQL Server veritabanı
- Veri tutarlılığı için işlem desteği
- Denetim kayıtları için kullanıcı izleme alanları

## Veritabanı Şeması

### Temel Tablolar

#### 1. Cari (Müşteri/Tedarikçi)
- **Amaç**: Müşteri ve tedarikçi bilgilerini saklamak
- **Alanlar**:
  - CariId (Birincil Anahtar, Kimlik)
  - CariKod, CariAdi (Müşteri kodu ve adı)
  - Tipi (Tür: Müşteri/Tedarikçi)
  - İletişim bilgileri (adres, telefon, vergi bilgisi)
  - İskonto oranları (ISK1-4, KKISK1-4, NAKISK)
  - Araç bilgileri (plaka, şoför)
  - Kullanıcı izleme alanları: CreatedBy, ModifiedBy, CreatedAt, ModifiedAt

#### 2. SutKayit (Süt Kaydı)
- **Amaç**: Süt alımlarını, depo sevklerini ve direkt sevkleri izlemek
- **Alanlar**:
  - SutKayitId (Birincil Anahtar, Kimlik)
  - BelgeNo (Belge numarası)
  - Tarih, IslemTuru (Tarih, İşlem Türü)
  - Tedarikçi/Müşteri ID ve bilgileri
  - Süt analizleri: Yag (yağ), Protein, Laktoz, pH, vb.
  - Fiyat ve miktar alanları
  - Kullanıcı izleme alanları: CreatedBy, ModifiedBy, CreatedAt, ModifiedAt

#### 3. STOKSABITKART (Stok Kataloğu)
- **Amaç**: Ana stok kalemleri kataloğu
- **Alanlar**:
  - STOKID (Birincil Anahtar, Kimlik)
  - STOKKODU, STOKADI (Kod, Ad)
  - Birim, ağırlık, protein, enerji, nem
  - Barkod, özellikler, menşei
  - Kullanıcı izleme alanları

#### 4. STOKSABITFIYAT (Fiyat Kataloğu)
- **Amaç**: Stok kalemleri için fiyat yönetimi
- **Alanlar**:
  - FIYATID (Birincil Anahtar, Kimlik)
  - STOKID (Yabancı Anahtar)
  - Fiyat listesi bilgileri
  - Farklı ödeme vadeleri için birden fazla alış fiyatı
  - KDV oranı, para birimi
  - Kullanıcı izleme alanları

#### 5. STOKSABITBELGE (Belge Kataloğu)
- **Amaç**: Stok kalemleri için belge yönetimi
- **Alanlar**:
  - BELGEID (Birincil Anahtar, Kimlik)
  - STOKID (Yabancı Anahtar)
  - BELGETIPI (Belge türü)
  - DOSYAYOLU (Dosya yolu)
  - EKLEMETARIHI (Ekleme tarihi)
  - Kullanıcı izleme alanları

#### 6. STOKSABITHAREKET (Stok Hareketi)
- **Amaç**: Stok hareketlerini izlemek
- **Alanlar**:
  - HAREKETID (Birincil Anahtar, Kimlik)
  - STOKID (Yabancı Anahtar)
  - HAREKETTURU (Hareket türü: Giriş/Çıkış)
  - Miktar, birim, depo
  - ISLEMTARIHI (İşlem tarihi)
  - Kullanıcı izleme alanları

#### 7. STOKSABITTED (Tedarikçi Kataloğu)
- **Amaç**: Tedarikçi kataloğu
- **Alanlar**:
  - TEDARIKCIID (Birincil Anahtar, Kimlik)
  - TEDARIKCIADI (Tedarikçi adı)
  - Kullanıcı izleme alanları

#### 8. SiparisMaster (Sipariş Ana Tablosu)
- **Amaç**: Siparişler ve proformalar için ana tablo
- **Alanlar**:
  - SiparisID (Birincil Anahtar, Kimlik)
  - Belge kodları ve numaraları
  - Tarihler (sipariş, sevk)
  - Müşteri bilgileri
  - Ödeme ve teslimat terimleri
  - Kullanıcı izleme alanları

#### 9. SiparisKalemAlis/SiparisKalemSatis (Sipariş Kalemleri)
- **Amaç**: Sipariş kalemleri
- **Alanlar**:
  - KalemID (Birincil Anahtar, Kimlik)
  - SiparisID (Yabancı Anahtar)
  - Stok bilgileri, miktar, fiyat
  - İskontolar ve toplam tutarlar
  - Kullanıcı izleme alanları

#### 10. Users (Kimlik Doğrulama)
- **Amaç**: Kullanıcı kimlik doğrulama ve yetkilendirme
- **Alanlar**:
  - Id (Birincil Anahtar, Kimlik)
  - Username (Benzersiz), PasswordHash
  - Email, FullName, Role
  - IsActive, CreatedAt, LastLoginAt
  - CreatedBy, ModifiedBy, CreatedAt, ModifiedAt

#### 11. Numarator (Belge Numaralandırma)
- **Amaç**: Sıralı belge numaralandırma
- **Alanlar**:
  - Yil, Tip (Yıl, Tür)
  - SonNumara (Son numara)
  - Birincil anahtar: Yil, Tip

#### 12. DepoStok (Depo Stok)
- **Amaç**: Depo stok takibi
- **Alanlar**:
  - DepoStokId (Birincil Anahtar, Kimlik)
  - Tarih, TedarikciId
  - Miktar, yağ, protein, TKM
  - Kullanıcı izleme alanları

#### 13. Parametreler (Parametreler)
- **Amaç**: İşlem parametreleri
- **Alanlar**:
  - ParametreId (Birincil Anahtar, Kimlik)
  - YagKesintiParametresi (Yağ kesinti parametresi)
  - ProteinParametresi (Protein parametresi)
  - DizemBasiTl (Dizem başı TL)
  - CreatedAt

## Kullanıcı Arayüzü Bileşenleri

### 1. MainWindow.xaml
- **Amaç**: Ana uygulama navigasyonu
- **Özellikler**:
  - Menü ve navigasyon butonları
  - Bağlantı durumu göstergesi
  - Kullanıcı durumu ekranı
  - Alt pencereler için merkezi içerik alanı

### 2. Temel İş Pencereleri
- **CariKayitWindow**: Müşteri/Tedarikçi kayıt ve yönetimi
- **SutAlimFormu**: Süt alma işlemleri farklı modlarla
- **StokKayitWindow**: Stok kalemi yönetimi
- **SiparisFormu**: Sipariş işleme sistemi
- **ParametrelerWindow**: Parametre yönetim arayüzü

### 3. Destekleyici Pencereler
- **CariListesiWindow**: Müşteri listesi seçimi
- **StokListeWindow**: Stok listesi
- **SutDepoSevkFormu**: Depo sevk işlemleri
- **SutDirekSevkFormu**: Direkt sevk işlemleri
- **BelgeSorgulama**: Belge sorgulama
- **AuditTrailWindow**: Denetim kayıtları görüntüleyici

### 4. Kimlik Doğrulama Pencereleri
- **LoginWindow**: Kullanıcı giriş arayüzü
- **UserManagementWindow**: Admin kullanıcı yönetimi
- **FirstTimeSetupWindow**: İlk admin kurulumu

### 5. Uzmanlık Pencereleri
- **SutRaporlari**: Süt raporları
- **GunlukSutAlimPreview**: Günlük süt alımı önizlemesi
- **EskiSiparisFormu**: Eski sipariş işleme

## İş Mantığı Bileşenleri

### 1. Depo Kalıbı
- **SutRepository**: Süt kayıtlarını yönetir
- **CariRepository**: Müşteri verilerini yönetir
- **ParameterRepository**: Parametre yönetimi
- **UserRepository**: Kullanıcı yönetimi
- **DepoStokRepository**: Depo stok işlemleri

### 2. İş Servisleri
- **AuthenticationService**: Kullanıcı kimlik doğrulama ve oturum yönetimi
- **DocumentNumberGenerator**: Sıralı belge numaralandırma

### 3. Yardımcı Sınıflar
- **DatabaseInitializer**: Veritabanı oluşturma ve başlatma
- **DatabaseHelper**: Veritabanı yardımcı fonksiyonları
- **CustomFontResolver**: PDF yazı tipi yönetimi

### 4. İş Mantığı Bileşenleri
- **DepoyaAlimIslemi**: Süt alımı iş mantığı
- **DepodanSevkIslemi**: Depo sevk iş mantığı
- **DirektSevkIslemi**: Direkt sevk iş mantığı

## Yetkilendirme ve Güvenlik

### Güvenlik Özellikleri
- **Rol Tabanlı Erişim Kontrolü**: Admin/Kullanıcı rolleri
- **Kullanıcı Takibi**: Tüm işlemler yapan kullanıcıyı izler
- **Oturum Yönetimi**: Uygulama genelinde mevcut kullanıcı bağlamı
- **Güvenli Şifre Deposu**: Tuzlanmış şifrelerle

### Veritabanı Güvenliği
- Tüm büyük tablolarda kullanıcı izleme alanları
- CreatedBy/ModifiedBy Users tablosuna referanslar
- CreatedAt/ModifiedAt zaman damgaları denetim kayıtları için

## Veri Akışı

### Tipik İşlem Akışı
1. **Kullanıcı Kimlik Doğrulama**
   - LoginWindow → AuthenticationService → Oturum Bağlamı

2. **Veri Girişi**
   - UI Bileşeni → Doğrulama → Depo → Veritabanı (işlem ile)

3. **Veri Alımı**
   - UI İsteği → Depo → Veritabanı → Model Nesneleri → UI Bağlama

4. **Veri Güncelleme**
   - UI Bileşeni → Doğrulama → Depo → Veritabanı Güncelleme İşlemi

### İşlem Yönetimi
- Çoklu tablo işlemleri veritabanı işlemleri kullanır
- Stok hareketleri ve süt kayıtları koordine işlemler kullanır
- Çok aşamalı işlemlerde hata durumunda geri alma

### Belge Numaralandırma Akışı
- DocumentNumberGenerator sıralı numaralar oluşturur
- Numarator tablosu sıralama durumunu korur
- Farklı belge türleri bağımsız sıralamalara sahiptir

## Ana Özellikler

### 1. Çoklu İşlem Süt Yönetimi
- Tedarikçilerden süt alımı
- Depodan müşterilere sevk
- Tedarikçilerden doğrudan müşterilere sevk
- Tam analiz takibi (yağ, protein, pH, vb.)

### 2. Kapsamlı Müşteri Yönetimi
- Müşteri ve tedarikçi takibi
- Çeşitli iskonto türleri
- Araç ve şoför bilgileri
- Vergi ve iletişim detayları

### 3. Esnek Stok Yönetimi
- Farklı ödeme vadeleri için çoklu fiyat listeleri
- Belge ekleme desteği
- Depo hareket takibi
- Ayrıntılı ürün özellikleri

### 4. Sağlam Sipariş İşleme
- Sipariş/proforma yönetimi
- Çoklu kalemler
- İskonto hesaplamaları
- Ödeme terimi esnekliği

### 5. Parametre Yönetimi
- Sistem genelinde parametre kontrolü
- Değişiklik takibi
- Ekleme yerine güncelleme işlevselliği

### 6. Denetim Kaydı Sistemi
- Kullanıcı işlem takibi
- Tam işlem geçmişi
- Yönetimsel denetim yetenekleri

### 7. Çoklu Dil Desteği
- Türkçe yerelleştirme
- Tarih/saat biçimlendirme
- Para birimi işleme

## Sistem Entegrasyon Noktaları

### Harici Sistemler
- Microsoft SQL Server veritabanı
- Raporlar için PDF üretimi
- Belge deposu için dosya sistemi

### Dahili Bağımlılıklar
- Bağlantı dizesi yönetimi ile veritabanı bağlantısı
- Kullanıcı kimlik doğrulama bağlamı
- Belge numaralandırma sistemi
- Denetim kaydı işlevselliği

## Kullanılan Geliştirme Kalıpları

### 1. Depo Kalıbı
- Veri erişim mantığının ayrılması
- Veritabanı soyutlaması
- Test edilebilirlik iyileştirmeleri

### 2. Singleton Servisler
- AuthenticationService global kullanıcı bağlamını korur
- Uygulama genelinde tutarlı oturum

### 3. MVVM Kalıbı (Kısmen)
- XAML'de veri bağlama
- UI'da komut kalıpları
- Model ayrımı

### 4. İşlem Yönetimi
- Veritabanı işlem desteği
- Çoklu tablo tutarlılığı
- Geri alma yetenekleri

Bu mimari, denetim yetenekleri ve ileride genişletme ve bakım yapılabilirliği için modüler tasarım kalıplarıyla ERP işlemleri için kapsamlı bir çerçeve sağlar.