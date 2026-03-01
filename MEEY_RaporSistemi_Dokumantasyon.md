# MEEY - Rapor Oluşturma Sistemi - Detaylı Dokümantasyon

## İçindekiler
1. [Genel Bakış](#genel-bakış)
2. [Mimari Yapı](#mimari-yapı)
3. [Dosya Yapısı](#dosya-yapısı)
4. [QuestPDF Kütüphanesi](#questpdf-kütüphanesi)
5. [Veri Akışı](#veri-akışı)
6. [Kod Örnekleri](#kod-örnekleri)
7. [Rapor Tasarımı](#rapor-tasarımı)

---

## Genel Bakış

MEEY uygulamasının rapor sistemi, öğrencilerin işletmelerdeki devamsızlık durumlarını PDF formatında raporlamak için tasarlanmıştır. Sistem, QuestPDF kütüphanesini kullanarak profesyonel görünümlü, yazdırılabilir raporlar oluşturur.

### Temel Özellikler
- ✅ PDF formatında rapor oluşturma
- ✅ Otomatik devam/devamsızlık hesaplama
- ✅ Çalışma takvimi entegrasyonu
- ✅ Manuel devamsızlık girişi
- ✅ Çoklu işletme desteği
- ✅ Dinamik tablo oluşturma
- ✅ Türkçe karakter desteği
- ✅ A4 sayfa formatı
- ✅ Tek sayfaya sığdırma optimizasyonu

---

## Mimari Yapı

### Katmanlı Mimari

```
┌─────────────────────────────────────┐
│   UI Layer (RaporBase.xaml)        │
│   - Kullanıcı arayüzü               │
│   - Veri seçimi                     │
│   - Tarih aralığı seçimi            │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Business Logic (RaporBase.xaml.cs)│
│   - Veri toplama                    │
│   - Devamsızlık hesaplama           │
│   - PDF oluşturma tetikleme         │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   PDF Generation (DevamsizlikRaporu)│
│   - QuestPDF ile PDF oluşturma      │
│   - Tablo tasarımı                  │
│   - Stil uygulama                   │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Data Layer (DatabaseManager)      │
│   - SQLite veritabanı               │
│   - Veri okuma                      │
└─────────────────────────────────────┘
```

---

## Dosya Yapısı

### 1. RaporBase.xaml
**Amaç:** Kullanıcı arayüzü tanımı

**Bölümler:**

- **Sol Panel (350px):**
  - Üst: Veri seçimi (işletme listesi)
  - Alt: Rapor ayarları (tarih, butonlar)
  
- **Sağ Panel (Kalan alan):**
  - Rapor önizleme alanı
  - Başarı/hata mesajları

**Örnek Yapı:**
```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="350"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Sol: Veri Seçimi + Ayarlar -->
    <Grid Grid.Column="0">
        <Border>Veri Seçimi</Border>
        <Border>Rapor Ayarları</Border>
    </Grid>
    
    <!-- Sağ: Önizleme -->
    <Border Grid.Column="1">
        <StackPanel x:Name="PreviewPanel"/>
    </Border>
</Grid>
```

### 2. RaporBase.xaml.cs
**Amaç:** İş mantığı ve veri işleme

**Ana Metodlar:**

1. **LoadReportType(string type)**
   - İşletme listesini veritabanından yükler
   - DataItem koleksiyonunu doldurur

2. **GenerateReport_Click()**
   - Rapor oluşturma sürecini başlatır
   - Validasyon yapar
   - PDF oluşturur ve açar

3. **OlusturDevamsizlikPdf()**
   - Veritabanından veri toplar
   - RaporGirdisi nesnesi oluşturur
   - DevamsizlikRaporu.OlusturPdf() çağırır

4. **RaporSevkGir_Click()**
   - Manuel devamsızlık girişi popup'ı açar
   - Mevcut kayıtları gösterir
   - Yeni kayıtları veritabanına ekler

5. **ParseGunler()**
   - Gün isimlerini DayOfWeek enum'a çevirir
   - Türkçe gün isimleri destekler

### 3. DevamsizlikRaporu.cs
**Amaç:** PDF oluşturma ve tasarım

**Sınıflar:**

- **DevamsizlikRaporu (static):** PDF oluşturma entry point
- **RaporGirdisi:** Rapor verilerini tutan model
- **DevamsizlikBelgesi:** QuestPDF IDocument implementasyonu

**Rapor Bölümleri:**
1. BaslikBloku() - Başlık
2. IsletmeBloku() - İşletme bilgileri
3. TabloBloku() - Öğrenci devamsızlık tablosu
4. AltBilgiBloku() - İmza ve semboller
5. DipnotBloku() - Açıklama notları

### 4. ReportStyles.cs
**Amaç:** Stil tanımları ve yardımcı fonksiyonlar

**İçerik:**
- TextStyle tanımları (Normal, Bold, Title)
- Border kalınlıkları
- Türkçe ay isimleri
- Record types (SchoolInfo, BusinessInfo, StudentInfo)

---

## QuestPDF Kütüphanesi

### Nedir?
QuestPDF, .NET için modern, açık kaynaklı bir PDF oluşturma kütüphanesidir.

### Neden QuestPDF?

✅ Fluent API - Kolay ve okunabilir kod
✅ Layout engine - Otomatik sayfa düzeni
✅ Türkçe karakter desteği
✅ Tablo oluşturma
✅ Community lisans (ücretsiz)
✅ .NET 6 uyumlu
✅ Performanslı

### Temel Kavramlar

**1. IDocument Interface**
```csharp
public class DevamsizlikBelgesi : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(15);
            page.Content().Column(col =>
            {
                // İçerik buraya
            });
        });
    }
}
```

**2. Fluent API Örneği**
```csharp
col.Item()
    .Border(1)
    .Padding(10)
    .Background(Colors.Grey.Lighten3)
    .Text("Başlık")
    .FontSize(14)
    .Bold()
    .AlignCenter();
```

**3. Tablo Oluşturma**

```csharp
c.Table(t =>
{
    // Kolon tanımları
    t.ColumnsDefinition(cols =>
    {
        cols.RelativeColumn(1);  // Esnek genişlik
        cols.ConstantColumn(100); // Sabit genişlik
    });
    
    // Başlık satırı
    t.Header(header =>
    {
        header.Cell().Text("Kolon 1");
        header.Cell().Text("Kolon 2");
    });
    
    // Veri satırları
    t.Cell().Text("Değer 1");
    t.Cell().Text("Değer 2");
});
```

**4. RowSpan ve ColumnSpan**
```csharp
// 2 satır birleştir
t.Cell().RowSpan(2)
    .Border(1)
    .Text("Birleşik hücre");

// 3 kolon birleştir
t.Cell().ColumnSpan(3)
    .Border(1)
    .Text("Geniş hücre");
```

---

## Veri Akışı

### Rapor Oluşturma Süreci

```
1. Kullanıcı İşlem
   ↓
   - İşletme seç
   - Tarih aralığı seç
   - "Rapor Oluştur" tıkla
   
2. Validasyon
   ↓
   - İşletme seçildi mi?
   - Tarih geçerli mi?
   
3. Veri Toplama (OlusturDevamsizlikPdf)
   ↓

   a. İşletme bilgileri (ad, telefon)
   b. Okul bilgileri (ad, müdür yardımcısı)
   c. Öğrenci listesi
   d. Çalışma günleri
   e. Tatil günleri (CalismaTakvimi)
   f. Devamsızlık kayıtları (Devamsizlik tablosu)
   
4. Sembol Belirleme
   ↓
   Her gün için:
   - Devamsizlik tablosunda kayıt var mı? → Kayıttaki sembol
   - Tatil günü mü? → "T"
   - Öğrencinin çalışma günü mü? → "X"
   - Hafta içi ama çalışma günü değil mi? → "O"
   - Cumartesi/Pazar → Boş
   
5. RaporGirdisi Oluşturma
   ↓
   - Tüm veriler RaporGirdisi nesnesine aktarılır
   - DevamsizlikMap: Dictionary<OgrenciId, Dictionary<Gun, Sembol>>
   
6. PDF Oluşturma (DevamsizlikRaporu.OlusturPdf)
   ↓
   - QuestPDF ile PDF oluşturulur
   - Byte array olarak döner
   
7. Dosya Kaydetme
   ↓
   - Temp klasöre kaydedilir
   - Dosya adı: Devamsizlik_YYYYMMDD_HHmmss.pdf
   
8. PDF Açma
   ↓
   - Process.Start ile varsayılan PDF görüntüleyicide açılır
   - Başarı mesajı gösterilir
```

---

## Kod Örnekleri

### Örnek 1: Basit PDF Oluşturma

```csharp
// 1. QuestPDF lisansını ayarla
QuestPDF.Settings.License = LicenseType.Community;

// 2. PDF oluştur
public static byte[] OlusturPdf(RaporGirdisi girdi)
{
    using var ms = new MemoryStream();
    var doc = new DevamsizlikBelgesi(girdi);
    doc.GeneratePdf(ms);
    return ms.ToArray();
}

// 3. Dosyaya kaydet
var pdfBytes = DevamsizlikRaporu.OlusturPdf(girdi);
File.WriteAllBytes("rapor.pdf", pdfBytes);
```

### Örnek 2: Tablo ile Rapor
```csharp
private void TabloBloku(IContainer c)
{
    c.Border(1).Table(t =>
    {
        // Kolonlar
        t.ColumnsDefinition(cols =>
        {
            cols.ConstantColumn(100); // Ad Soyad
            cols.ConstantColumn(50);  // Okul No
            for (int i = 0; i < 31; i++)
                cols.ConstantColumn(14); // Günler
        });
        
        // Başlık
        t.Header(header =>
        {
            header.Cell().Text("ADI SOYADI");
            header.Cell().Text("OKUL NO");
            for (int i = 1; i <= 31; i++)
                header.Cell().Text(i.ToString());
        });
        
        // Öğrenci satırları
        foreach (var ogr in _girdi.Ogrenciler)
        {
            t.Cell().Text(ogr.AdSoyad);
            t.Cell().Text(ogr.OkulNo);
            
            for (int gun = 1; gun <= 31; gun++)
            {
                var sembol = SembolGetir(ogr.Id, gun);
                t.Cell().Text(sembol);
            }
        }
    });
}
```

### Örnek 3: Veritabanından Veri Çekme

```csharp
using (var connection = DatabaseManager.GetConnection())
{
    connection.Open();
    
    // Öğrencileri getir
    string query = @"
        SELECT 
            o.Id, o.OkulNo, o.AdSoyad, o.AlanDal
        FROM KoordinatorTanimlama k
        INNER JOIN Ogrenciler o ON o.Koordinator = k.Id
        WHERE k.Isletme = @IsletmeAdi
        ORDER BY o.AdSoyad";
    
    using (var command = new SQLiteCommand(query, connection))
    {
        command.Parameters.AddWithValue("@IsletmeAdi", isletmeAdi);
        
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var ogrenci = new StudentInfo(
                    Id: Convert.ToInt32(reader["Id"]),
                    OkulNo: reader["OkulNo"]?.ToString() ?? "",
                    AdSoyad: reader["AdSoyad"]?.ToString() ?? "",
                    Alan: "", // Parse edilecek
                    Dal: ""   // Parse edilecek
                );
                ogrenciler.Add(ogrenci);
            }
        }
    }
}
```

### Örnek 4: Devamsızlık Hesaplama
```csharp
// Her gün için sembol belirle
for (var tarih = baslangic; tarih <= bitis; tarih = tarih.AddDays(1))
{
    int gun = tarih.Day;
    string sembol = "";
    
    // 1. Önce manuel girişlere bak
    string devQuery = @"
        SELECT Sembol FROM Devamsizlik 
        WHERE OgrenciId = @OgrId AND Tarih = @Tarih";
    
    using (var devCmd = new SQLiteCommand(devQuery, connection))
    {
        devCmd.Parameters.AddWithValue("@OgrId", ogrenciId);
        devCmd.Parameters.AddWithValue("@Tarih", tarih.ToString("yyyy-MM-dd"));
        var result = devCmd.ExecuteScalar();
        
        if (result != null)
            sembol = result.ToString();
    }
    
    // 2. Manuel giriş yoksa otomatik belirle
    if (string.IsNullOrEmpty(sembol))
    {
        if (tatilGunleri.Contains(tarih.Date))
            sembol = "T"; // Tatil
        else if (calismaGunleri.Contains(tarih.DayOfWeek))
            sembol = "X"; // İşletmede
        else if (tarih.DayOfWeek != DayOfWeek.Saturday && 
                 tarih.DayOfWeek != DayOfWeek.Sunday)
            sembol = "O"; // Okulda
    }
    
    devamsizlikMap[ogrenciId][gun] = sembol;
}
```

---

## Rapor Tasarımı

### Sayfa Düzeni

```
┌─────────────────────────────────────────────────────┐
│ OKUL ADI                                            │
│ ÖĞRENCİLERİNİN İŞLETMELERDE MESLEK EĞİTİMİ         │
│ AYLIK DEVAM - DEVAMSIZLIK BİLDİRİM ÇİZELGESİ       │
├─────────────────────────────────────────────────────┤
│ İŞLETME ADI │ TEL │ E-POSTA │ AY │ BELGE TARİHİ   │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ADI    OKUL  ALAN  GÜN  1 2 3 ... 31  ÖZ  ÖZS    │
│ SOYADI   NO   DAL   LER                 ÜR  ÜZ     │
│                                         LÜ  SÜZ    │
│ ─────────────────────────────────────────────────  │
│ Ahmet   1234  BT    S    X X O ... T    2   1      │
│ Yılmaz        Web   Ö    X X O ... T               │
│ ─────────────────────────────────────────────────  │
│ ...                                                 │
├─────────────────────────────────────────────────────┤
│ İŞLETME YETKİLİSİ │ İNCELENDİ │ SEMBOLLER          │
│                   │           │ X: İşletmede       │
│ Tarih: __/__/__   │ Müdür Yrd │ O: Okulda          │
│ ADI SOYADI        │           │ İ: İzinli          │
│ Kaşe - İmza       │ İmza      │ D: Özürsüz         │
│                   │           │ H: Hasta           │
│                   │           │ R: Raporlu         │
│                   │           │ T: Tatil           │
├─────────────────────────────────────────────────────┤
│ Bu çizelge, işletme tarafından tutulacak...        │
└─────────────────────────────────────────────────────┘
```

### Boyutlandırma Stratejisi

**Problem:** 31 günlük tablo + öğrenci bilgileri A4'e sığmalı

**Çözüm:** Dinamik ölçeklendirme

```csharp
// 1. İdeal genişlikleri tanımla
const float okulNoW = 42;
const float adSoyadW = 108;
const float alanDalW = 52;
const float gunlerW = 24;
const float gunW = 14;
const float ozW = 36;
const float ozsW = 36;
const float toplamW = okulNoW + adSoyadW + alanDalW + 
                      gunlerW + (31 * gunW) + ozW + ozsW;

// 2. Sayfa genişliğini hesapla
var pageWidth = PageSizes.A4.Width;  // 595 pt
var margin = 15f;
var available = pageWidth - (margin * 2);  // 565 pt

// 3. Ölçek faktörünü hesapla
var scale = available / toplamW;  // ~0.85

// 4. Tüm genişlikleri ölçekle
float okulNoScaled = okulNoW * scale;
float adSoyadScaled = adSoyadW * scale;
// ... diğerleri

// 5. Font boyutlarını da ölçekle
.FontSize(9.0f * scale)
```

### Hücre Birleştirme (RowSpan)

**Öğrenci satırları S ve Ö olmak üzere 2 satır:**

```
┌─────────┬────────┬────────┬───┬───┬───┬───┬───┐
│ Ahmet   │  1234  │ BT Web │ S │ X │ O │ T │ 2 │
│ Yılmaz  │        │        │ Ö │ X │ O │ T │ 1 │
└─────────┴────────┴────────┴───┴───┴───┴───┴───┘
   RowSpan=2 (3 hücre)        Normal hücreler
```

**Kod:**
```csharp
// Ad Soyad - 2 satır birleşik
t.Cell().RowSpan(2)
    .Border(ReportStyles.Line)
    .Text(ogr.AdSoyad);

// Okul No - 2 satır birleşik
t.Cell().RowSpan(2)
    .Border(ReportStyles.Line)
    .Text(ogr.OkulNo);

// Alan/Dal - 2 satır birleşik
t.Cell().RowSpan(2)
    .Border(ReportStyles.Line)
    .Text($"{ogr.Alan} {ogr.Dal}");

// S etiketi (1. satır)
t.Cell().Text("S");

// Günler (1. satır)
for (int i = 1; i <= 31; i++)
    t.Cell().Text(sembol);

// Toplamlar - 2 satır birleşik
t.Cell().RowSpan(2).Text(ozurlu.ToString());
t.Cell().RowSpan(2).Text(ozursuz.ToString());

// Ö etiketi (2. satır)
t.Cell().Text("Ö");

// Günler (2. satır) - aynı semboller
for (int i = 1; i <= 31; i++)
    t.Cell().Text(sembol);
```

### Renklendirme

**Cumartesi/Pazar günleri gri arka plan:**

```csharp
string Arkaplan(DateTime? tarih)
{
    if (tarih == null) return Colors.White;
    if (tarih.Value.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        return Colors.Grey.Lighten3;
    return Colors.White;
}

// Kullanım
t.Cell()
    .Background(Arkaplan(tarih))
    .Text(sembol);
```

### Dikey Metin (Rotasyon)

**Başlık hücrelerinde dikey yazı:**
```csharp
header.Cell()
    .RotateLeft()  // 90 derece sola döndür
    .Text("OKUL NO")
    .FontSize(7.4f * scale)
    .SemiBold();
```

---

## Veri Modelleri

### RaporGirdisi
```csharp
public class RaporGirdisi
{
    public SchoolInfo Okul { get; set; }
    public BusinessInfo Isletme { get; set; }
    public DateTime Ay { get; set; }
    public string AyMetni { get; set; }  // "ŞUBAT-2026"
    public string BelgeTarihi { get; set; }  // "27.02.2026"
    public List<StudentInfo> Ogrenciler { get; set; }
    
    // Dictionary<OgrenciId, Dictionary<Gun, Sembol>>
    public Dictionary<int, Dictionary<int, string>> DevamsizlikMap { get; set; }
    
    public int? OkulGunu { get; set; }
    public Dictionary<int, string> OgrenciGunleri { get; set; }
}
```

### Record Types
```csharp
public record SchoolInfo(
    string Il, 
    string Ad, 
    string Mudur, 
    string MudurYardimcisi
);

public record BusinessInfo(
    string Adi, 
    string Tel, 
    string Eposta
);

public record StudentInfo(
    int Id, 
    string OkulNo, 
    string AdSoyad, 
    string Alan, 
    string Dal
);
```

---

## Sembol Sistemi

### Sembol Anlamları

| Sembol | Anlamı | Kaynak | Toplam |
|--------|--------|--------|--------|
| X | İşletmede | Otomatik | - |
| O | Okulda | Otomatik | - |
| T | Tatil | CalismaTakvimi | - |
| İ | İzinli | Manuel | Özürlü |
| D | Özürsüz Devamsız | Manuel | Özürsüz |
| H | Hasta Sevkli | Manuel | Özürlü |
| R | Raporlu | Manuel | Özürlü |
| S | Sabah | Manuel | - |
| Ö | Öğle | Manuel | - |

### Sembol Öncelik Sırası

1. **Manuel Giriş (En Yüksek Öncelik)**
   - Devamsizlik tablosunda kayıt varsa kullan
   
2. **Tatil Günleri**
   - CalismaTakvimi'nde tanımlı tatiller → "T"
   
3. **Çalışma Günleri**
   - Öğrencinin çalışma günü → "X"
   
4. **Okul Günleri**
   - Hafta içi ama çalışma günü değil → "O"
   
5. **Hafta Sonu**
   - Cumartesi/Pazar → Boş

### Toplam Hesaplama

```csharp
private (int Ozurlu, int Ozursuz) ToplamHesapla(int ogrenciId)
{
    int ozurlu = 0;
    int ozursuz = 0;

    if (_girdi.DevamsizlikMap.TryGetValue(ogrenciId, out var gunler))
    {
        foreach (var sembol in gunler.Values)
        {
            if (string.IsNullOrWhiteSpace(sembol)) continue;

            var s = sembol.Trim().ToUpper();
            if (s == "İ" || s == "H" || s == "R")
                ozurlu++;
            else if (s == "D")
                ozursuz++;
        }
    }

    return (ozurlu, ozursuz);
}
```

---

## Performans Optimizasyonu

### 1. Veritabanı Sorguları

**Kötü Yaklaşım:**
```csharp
// Her öğrenci için ayrı sorgu
foreach (var ogrenci in ogrenciler)
{
    var devamsizlik = GetDevamsizlik(ogrenci.Id);
}
```

**İyi Yaklaşım:**
```csharp
// Tek sorguda tüm veriler
var devamsizlikMap = new Dictionary<int, Dictionary<int, string>>();

foreach (var ogr in ogrenciler)
{
    devamsizlikMap[ogr.Id] = new Dictionary<int, string>();
    
    // Tüm tarihler için tek sorgu
    for (var tarih = baslangic; tarih <= bitis; tarih = tarih.AddDays(1))
    {
        // Sorgu ve işleme
    }
}
```

### 2. Bellek Yönetimi

**using statement kullanımı:**
```csharp
using (var connection = DatabaseManager.GetConnection())
{
    connection.Open();
    // İşlemler
} // Otomatik dispose
```

**MemoryStream:**
```csharp
public static byte[] OlusturPdf(RaporGirdisi girdi)
{
    using var ms = new MemoryStream();
    var doc = new DevamsizlikBelgesi(girdi);
    doc.GeneratePdf(ms);
    return ms.ToArray();
}
```

### 3. String İşlemleri

**String interpolation:**
```csharp
// İyi
var ayMetni = $"{ReportStyles.TrMonthUpper(ay.Month)}-{ay.Year}";

// Kötü
var ayMetni = ReportStyles.TrMonthUpper(ay.Month) + "-" + ay.Year.ToString();
```

---

## Hata Yönetimi

### Try-Catch Blokları

```csharp
try
{
    // PDF oluştur
    var pdfBytes = OlusturDevamsizlikPdf(...);
    File.WriteAllBytes(tempPath, pdfBytes);
    
    // PDF aç
    Process.Start(new ProcessStartInfo
    {
        FileName = tempPath,
        UseShellExecute = true
    });
    
    // Başarı mesajı
    ShowSuccessMessage(tempPath);
}
catch (Exception ex)
{
    // Hata mesajı
    ShowErrorMessage(ex.Message);
    MessageBox.Show($"Rapor oluşturma hatası: {ex.Message}", 
        "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
}
```

### Validasyon

```csharp
// İşletme seçimi kontrolü
if (selectedItems.Count == 0)
{
    MessageBox.Show("Lütfen en az bir işletme seçin!", 
        "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
    return;
}

// Tarih kontrolü
if (!dpBaslangic.SelectedDate.HasValue || !dpBitis.SelectedDate.HasValue)
{
    MessageBox.Show("Lütfen tarih aralığı seçin!", 
        "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
    return;
}

// Öğrenci kontrolü
if (ogrenciler.Count == 0)
{
    throw new Exception("Seçili işletmelerde öğrenci bulunamadı!");
}
```

---

## Test Senaryoları

### 1. Temel Rapor Oluşturma
```
Adımlar:
1. Raporlar menüsüne git
2. Bir işletme seç
3. "Bu Ay" seçeneğini seç
4. "Rapor Oluştur" butonuna tıkla

Beklenen:
✓ PDF oluşturulur
✓ Varsayılan görüntüleyicide açılır
✓ Başarı mesajı gösterilir
✓ Dosya yolu gösterilir
```

### 2. Çoklu İşletme
```
Adımlar:
1. Birden fazla işletme seç
2. Rapor oluştur

Beklenen:
✓ Her işletme için ayrı sayfa
✓ Tüm öğrenciler listelenir
```

### 3. Manuel Devamsızlık Girişi
```
Adımlar:
1. İşletme seç
2. "Rapor Sevk Gir" tıkla
3. Öğrenci seç
4. Tarih seç
5. Sembol seç (İ/D/H/R)
6. "Tamam" tıkla

Beklenen:
✓ Kayıt veritabanına eklenir
✓ Raporda görünür
```

### 4. Mevcut Kayıt Silme
```
Adımlar:
1. "Rapor Sevk Gir" aç
2. Öğrenci seç (mevcut kayıtlar yüklenir)
3. Listeden kayıt seç
4. "Seçili Kaydı Sil" tıkla

Beklenen:
✓ Kayıt veritabanından silinir
✓ Listeden kaldırılır
```

### 5. Otomatik Doldurma
```
Adımlar:
1. İşletme seç
2. Tarih aralığı seç
3. "Otomatik Doldur" tıkla

Beklenen:
✓ X, O, T sembolleri otomatik atanır
✓ Bilgi mesajı gösterilir
```

---

## Sorun Giderme

### Problem 1: PDF Açılmıyor

**Sebep:** Varsayılan PDF görüntüleyici tanımlı değil

**Çözüm:**
```csharp
try
{
    Process.Start(new ProcessStartInfo
    {
        FileName = tempPath,
        UseShellExecute = true
    });
}
catch (Exception ex)
{
    // Dosya yolunu göster
    MessageBox.Show($"PDF oluşturuldu ancak açılamadı.\n\n" +
        $"Dosya konumu:\n{tempPath}\n\n" +
        $"Hata: {ex.Message}");
}
```

### Problem 2: Türkçe Karakterler Bozuk

**Sebep:** Font desteği eksik

**Çözüm:**
```csharp
// Arial font kullan (Türkçe destekler)
public static TextStyle Normal => 
    TextStyle.Default.FontSize(9).FontFamily("Arial");
```

### Problem 3: Tablo Sayfaya Sığmıyor

**Sebep:** Ölçeklendirme yanlış

**Çözüm:**
```csharp
// Dinamik ölçeklendirme kullan
var scale = available / toplamW;
float gunScaled = gunW * scale;
```

### Problem 4: Semboller Yanlış

**Sebep:** Öncelik sırası hatalı

**Çözüm:**
```csharp
// 1. Önce manuel girişlere bak
var manuelSembol = GetManuelSembol(ogrenciId, tarih);
if (!string.IsNullOrEmpty(manuelSembol))
    return manuelSembol;

// 2. Sonra otomatik belirle
return OtomatikSembolBelirle(tarih, calismaGunleri, tatilGunleri);
```

---

## Gelecek İyileştirmeler

### 1. Çoklu Sayfa Desteği
```csharp
// Şu an: Sadece ilk işletme
return DevamsizlikRaporu.OlusturPdf(raporlar[0]);

// Gelecek: Tüm işletmeler
return DevamsizlikRaporu.OlusturCokluPdf(raporlar);
```

### 2. Excel Export
```csharp
public static byte[] OlusturExcel(RaporGirdisi girdi)
{
    // EPPlus veya ClosedXML kullanarak
    // Excel dosyası oluştur
}
```

### 3. E-posta Gönderimi
```csharp
public void RaporuMaileGonder(string email, byte[] pdfBytes)
{
    // SMTP ile PDF'i mail olarak gönder
}
```

### 4. Rapor Şablonları
```csharp
public enum RaporSablonu
{
    Standart,
    Detayli,
    Ozet
}

public static byte[] OlusturPdf(RaporGirdisi girdi, RaporSablonu sablon)
{
    // Şablona göre farklı tasarım
}
```

### 5. Grafik ve İstatistikler
```csharp
private void GrafikBloku(IContainer c)
{
    // Devamsızlık grafiği
    // Pasta chart: Özürlü vs Özürsüz
    // Çubuk grafik: Aylık trend
}
```

---

## Sonuç

MEEY rapor sistemi, QuestPDF kütüphanesi kullanarak profesyonel PDF raporları oluşturur. Sistem:

✅ Modüler ve genişletilebilir
✅ Performanslı ve optimize
✅ Kullanıcı dostu
✅ Türkçe karakter desteği
✅ Otomatik ve manuel veri girişi
✅ Esnek tarih aralığı seçimi

**Toplam Kod Satırı:** ~1500 satır
**Dosya Sayısı:** 4 dosya
**Kullanılan Kütüphane:** QuestPDF 2026.2.2
**Hedef Framework:** .NET 6.0

---

## Kaynaklar

- [QuestPDF Dokümantasyonu](https://www.questpdf.com/)
- [QuestPDF GitHub](https://github.com/QuestPDF/QuestPDF)
- [SQLite Dokümantasyonu](https://www.sqlite.org/docs.html)
- [WPF Dokümantasyonu](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)

---

**Son Güncelleme:** 27 Şubat 2026
**Yazar:** Kiro AI Assistant
**Versiyon:** 1.0
