# Net Miktar Hesaplama Yöntemi - Türkiye Süt Sanayii Standartları

## Genel Bakış
Bu belge, By_ProV2 uygulamasında uygulanan Net Miktar hesaplama yöntemini açıklamaktadır. Hesaplama yöntemi Türkiye'de süt sanayiinde uygulanan mevzuata ve standartlara uygun olarak geliştirilmiştir.

## Türkiye'deki Mevzuat ve Standartlar

### 1. Donma Noktası Kriterleri
- **Standart donma noktası**: -0.525°C (saf süt için)
- **Kabul edilebilir limit**: -0.515°C (±0.010°C tolerans)
- **Dilüsyon (seyreltme) hesaplaması**: 
  - Her 0.001°C sapma = %0.22 su ilavesi

### 2. Kalite Parametreleri
- **Yağ oranı**: Minimum %3.6 (inek sütü için)
- **Protein oranı**: Minimum %3.2 (inek sütü için)
- **SNF (Katı-Mayağ-Dışı)**: Minimum standartlar geçerlidir

## Hesaplama Yöntemi

### Ana Formül
```
Net Miktar = Brüt Miktar - (Donma Noktası Sapması × Dönüşüm Katsayısı × Brüt Miktar) - Kesinti Miktarı
```

### Ayrıntılı Formül
```
Dilüsyon Oranı = ((Ölçülen Donma Noktası - (-0.515)) / 0.001) × 0.22
Dilüsyon Miktarı = (Brüt Miktar × Dilüsyon Oranı) / 100
Net Miktar = Brüt Miktar - Dilüsyon Miktarı - Kesinti
```

### Örnek Hesaplama
**Verilenler:**
- Brüt Miktar: 1000 Lt
- Donma Noktası: -0.510°C
- Kesinti: 5 Lt

**Hesaplama:**
- Donma sapması: -0.510 - (-0.515) = 0.005°C
- Dilüsyon oranı: (0.005 / 0.001) × 0.22 = 11%
- Dilüsyon miktarı: (1000 × 11) / 100 = 110 Lt
- Net Miktar: 1000 - 110 - 5 = 885 Lt

## Uygulamadaki Gerçek Zamanlı Hesaplama

### Otomatik Hesaplama Triggerları
Uygulama aşağıdaki alanlardaki değişikliklerde otomatik olarak Net Miktarı yeniden hesaplar:
- **Miktar (Lt)**: Brüt miktar
- **Donma Noktası**: Ana parametre (en önemlisi)
- **Yağ (%)**: Kalite parametresi
- **Protein (%)**: Kalite parametresi
- **Kesinti (Lt)**: Manuel kesinti miktarı

### Formül
```Net Miktar = Brüt Miktar - [((DonmaNoktası - (-0.515)) / 0.001) × 0.22% × Brüt Miktar] - Kesinti```

### Teknik Uygulama - Eski Formül (Düzeltme Tarihi: 2025-11-07)

**Eski Hesaplama Formülü (Yanlış):**
```csharp
private decimal CalculateKesinti(decimal brütMiktar, decimal? donmaNoktasi, decimal? yag, decimal? protein, decimal? somatik, decimal? bakteri, decimal? pH, decimal? yogunluk)
{
    decimal totalKesinti = 0;

    if (donmaNoktasi.HasValue)
    {
        decimal referansDeger = _latestParameters?.DonmaNoktasiReferansDegeri ?? -0.520m;
        decimal kesintiBaslangicLimiti = _latestParameters?.DonmaNoktasiKesintiAltLimit ?? -0.515m;
        
        // Yanlış hesaplama formülü:
        if (donmaNoktasi > kesintiBaslangicLimiti)
        {
            // Yanlış: (donmaNoktasi.Value * -1) + referansDeger;
            decimal donmaNoktasiFarki = (donmaNoktasi.Value * -1) + referansDeger; 
            decimal toplamYuzdeDusukluk = donmaNoktasiFarki / referansDeger * 100;
            decimal dilusyonMiktari = Math.Round(brütMiktar * toplamYuzdeDusukluk  / 100);
            if (dilusyonMiktari > 0) // Sadece pozitif kesintiler uygulanır
            {
                totalKesinti += dilusyonMiktari;
            }
        }
    }
    // ... diğer parametreler için hesaplamalar ...
    return totalKesinti;
}
```

**Yeni Düzeltme (2025-11-07) - Doğru Formül:**
```csharp
private decimal CalculateKesinti(decimal brütMiktar, decimal? donmaNoktasi, decimal? yag, decimal? protein, decimal? somatik, decimal? bakteri, decimal? pH, decimal? yogunluk)
{
    decimal totalKesinti = 0;

    if (donmaNoktasi.HasValue)
    {
        decimal referansDeger = _latestParameters?.DonmaNoktasiReferansDegeri ?? -0.520m;
        decimal kesintiBaslangicLimiti = _latestParameters?.DonmaNoktasiKesintiAltLimit ?? -0.515m;
        
        // Düzeltme: Sadece ölçüm değeri eşik değerinden büyükse kesinti uygulanır
        if (donmaNoktasi > kesintiBaslangicLimiti)
        {
            // Doğru: Gerçek farkı hesapla (ölçülen - referans)
            decimal donmaNoktasiFarki = donmaNoktasi.Value - referansDeger;
            
            // Sadece donma noktası referans değerden büyükse (seyreltme varsa) kesinti uygulanır
            if (donmaNoktasiFarki > 0)
            {
                // Yüzde oran hesaplaması için mutlak değeri kullan
                decimal yuzdeOrani = Math.Abs(donmaNoktasiFarki / referansDeger) * 100;
                
                // Yüzde orana göre kesinti miktarı hesapla
                decimal dilusyonMiktari = Math.Round(brütMiktar * yuzdeOrani / 100);
                
                // Sadece pozitif kesintiler uygulanır
                if (dilusyonMiktari > 0)
                {
                    totalKesinti += dilusyonMiktari;
                }
            }
        }
    }
    // ... diğer parametreler için hesaplamalar ...
    return totalKesinti;
}
```

**Değişiklik Nedeni:**
Eski formül `(donmaNoktasi.Value * -1) + referansDeger` matematiksel olarak yanlış olduğu için yanlış kesintiler oluşturuyordu. Yeni formül `donmaNoktasi.Value - referansDeger` gerçek sapmayı doğru şekilde hesaplar.

**Faydaları:**
- Dondurma noktası altındaki değerlerde (daha konsantre süt) artık hatalı kesintiler uygulanmaz
- Dondurma noktası üstündeki değerlerde (seyreltilmiş süt) doğru oranda kesintiler uygulanır
- Hesaplama Türkiye süt sanayii standartlarına daha uygun hale gelmiştir
```

## Yasa Kuralları ve Denetim

### Türkiye Gıda Kodeksi
- **Maksimum donma noktası**: -0.515°C
- Bu değerin üzerindeki sütler sulandırılmış olarak kabul edilir
- Denetimlerde donma noktası en önemli parametredir

### Sanayi Uygulamaları
- Türkiye genelinde süt toplama istasyonlarında bu yöntem kullanılır
- Ödeme hesaplamaları ve kota belirlemelerinde bu standartlar geçerlidir
- Antibiyotik testi ve diğer mikrobiyolojik testler ek faktörlerdir

## Uygulama Özellikleri

### Otomatik Hesaplama
- Form üzerindeki ana parametrelerde değişiklik yapıldığında Net Miktar anında hesaplanır
- Net Miktar alanı kullanıcı tarafından manuel olarak düzenlenemez
- Gerçek zamanlı hesaplama kullanıcı deneyimini iyileştirir

### Hatalı Girişlerin Engellenmesi
- Negatif Net Miktar değerleri 0 olarak kabul edilir
- Geçersiz donma noktası değerleri için uygun kontroller mevcuttur

## Uyarlamalar

### Şirket Politikalarına Göre Ayarlamalar
Uygulama şu şekilde özelleştirilebilir:
- Farklı kalite standartları için yağ ve protein parametrelerine göre ek ayarlamalar
- Farklı donma noktası kabul limitleri
- Ek kesinti kategorileri

### Gelecekteki Geliştirmeler
- Mikrobiyolojik parametrelere göre ek kesintiler
- Farklı hayvan türlerine (keçi, koyun) göre ayarlamalar
- Sezona göre değişen standartlar

## Otomatik Kesinti Hesaplama

### Genel Bakış
Kesinti hesaplaması, süt kalitesine göre yapılan otomatik indirimlerin toplamıdır. Türkiye'de süt satın alma işlemlerinde kalite parametrelerine göre kesintiler otomatik olarak uygulanır.

### Kesinti Türleri ve Standartları

#### 1. Donma Noktası Kaynaklı Kesinti (En Önemli)
- **Kriter**: Maksimum -0.515°C (kabul edilebilir limit)
- **Referans Değeri**: -0.525°C (saf süt için standart)
- **Hesaplama**: Her 0.010°C sapma için %5 kesinti
- **Etki**: Sulandırma tespiti ve ceza
- **Formül**: `Kesinti = Brüt Miktar × [(Ölçülen FP - Referans FP) / 0.010] × 0.05`

#### 2. Yağ Oranı Kesintisi
- **Standart**: Minimum %3.6 (inek sütü için)
- **Düşük yağ (< 3.2%)**: %0.5 kesinti
- **Orta düzey yağ (3.2%-3.6%)**: %0.2 kesinti

#### 3. Protein Oranı Kesintisi
- **Standart**: Minimum %3.2 (inek sütü için)
- **Düşük protein (< 2.8%)**: %0.5 kesinti
- **Orta düzey protein (2.8%-3.2%)**: %0.2 kesinti

#### 4. Somatik Hücre Sayısı Kesintisi
- **Standart**: Maksimum 400,000 hücre/ml
- **Yüksek (> 800,000)**: %2 kesinti
- **Orta (400,000-800,000)**: %1 kesinti

#### 5. Bakteri Sayısı Kesintisi
- **Standart**: Maksimum 100,000 CFU/ml
- **Çok yüksek (> 1,000,000)**: %3 kesinti
- **Yüksek (300,000-1,000,000)**: %1.5 kesinti
- **Limit (100,000-300,000)**: %0.5 kesinti

#### 6. pH Değeri Kesintisi
- **Standart**: 6.5-6.7 aralığı
- **Dışında değer**: %0.5 kesinti

#### 7. Yoğunluk Kesintisi
- **Standart**: 1.028-1.034 g/cm³
- **Dışında değer**: %0.5 kesinti

### Uygulama Özellikleri

#### Otomatik Hesaplama
- Tüm kesintiler form parametreleri girildikçe otomatik olarak hesaplanır
- Kullanıcı manuel kesinti giremez
- Gerçek zamanlı toplam kesinti gösterilir

#### Entegre Sistem
- Net miktar = Brüt miktar - Toplam kesinti
- Toplam kesinti = Donma + Yağ + Protein + Somatik + Bakteri + pH + Yoğunluk

---

**Not:** Bu hesaplama yöntemi Türkiye Cumhuriyeti Tarım ve Orman Bakanlığı'nın süt ve süt ürünleri mevzuatına uygun olarak geliştirilmiştir. Uygulamada yapılan hesaplamalar sadece bilgi amaçlıdır ve resmi denetim amaçlı değildir.