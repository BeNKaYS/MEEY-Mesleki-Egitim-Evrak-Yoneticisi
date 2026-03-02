# MEEY - Mesleki Eğitim Evrak Yöneticisi

MEEY (Mesleki Eğitim Evrak Yöneticisi), mesleki eğitimdeki evrak süreçlerini (işletme belirleme, öğrenci atama, görev takip raporları, sözleşmeler vb.) kolaylaştırmak ve okulların/koordinatörlerin iş yükünü azaltmak amacıyla geliştirilmiş modern, modüler bir WPF uygulamasıdır.

## Son Güncellemeler (v1.0.8)

- **Aylık Rehberlik Raporu**: Tatil günleri ziyaret tarihleri içinde `dd.MM.yyyy (T)` formatında işaretlenir.
- **HTML Kayıtları**: Raporlardan üretilen HTML dosyaları uygulama genelindeki ortak kayıt klasörüne alınarak görünürlük birleştirildi.
- **Kurulu Sürüm Uyumluluğu**: Veritabanı/yazma yolu yaklaşımı `LocalAppData` tabanlı akışla daha stabil hale getirildi.
- **Kurulum Paketi**: Güncel Windows kurulum dosyası `MEEY-Setup-v1.0.8-win-x64.exe` olarak üretildi.

## Mimarisi ve Özellikleri

- **Modern ve Minimalist Arayüz**: Sol menü çubuğundan ilgili modüllere kolay erişim. Kullanıcı dostu yerleşim.
- **Dinamik Raporlama**: Seçilen ay ve haftalara göre otomatik "Haftalık Görev Takip Raporu" (PDF/Word/Excel vb.) oluşturabilme.
- **Zengin Veri Yönetimi**: Öğrenciler, İşletmeler, Alan/Dallar ve Okul/Koordinatör bilgilerinin SQLite altyapısıyla güvenle ve hızla yönetilmesi.
- **Gelişmiş Metin Editörü**: Uygulama içi şablonlarla belge hazırlama ve kaydetme (Aspose.Words entegrasyonu ile).
- **Modüler Yapı**: Her bir özellik bağımsız bir arayüz (UserControl) olarak tasarlandı (Veri Girişi, Raporlar, Diğer Evraklar).
- **Dışa Aktarma**: Evrakların DOCX veya PDF olarak dışa aktarılabilmesi için altyapı desteği.

## Proje Yapısı

```
MEEY/
├── App.xaml                    # Uygulama giriş noktası
├── MainWindow.xaml             # Ana Pencere (Sol navigasyon ve kapsayıcı yapı)
├── Database/                   # SQLite veritabanı yönetimi ve tablolar
├── Models/                     # Veritabanı ve uygulama içi nesne modelleri
├── Controls/                   # Modüler arayüzler (Örn: VeriGirisi.xaml, HakkindaControl.xaml)
├── Reports/                    # Rapor oluşturma ekranları ve altyapısı (Haftalık Görev Raporları)
├── DigerEvraklar/              # Sözleşmeler, Ek formlar vb. evrak listeleri
└── Assets/                     # Resim, ikon ve editör kayıt şablonları
```

## Çalıştırma & Geliştirme

Uygulamayı derlemek ve çalıştırmak için:
```bash
cd MEEY
dotnet run
```

Visual Studio veya Visual Studio Code üzerinden projeyi açabilir, `MEEY.csproj` dosyası ile başlatabilirsiniz.

## Özel Teşekkür

Bu projenin geliştirilmesi sürecinde sağladığı kıymetli veri paylaşımları, değerli fikirleri ve yönlendirmeleri için Yılmaz ER
hocamıza en içten teşekkürlerimizi sunarız. Yaptığı katkılar uygulamanın daha doğru ve kullanılabilir olmasına büyük
ölçüde yardımcı olmuştur.

---

## 👨‍💻 Geliştirici Kartı

<div align="center">

```
▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
```

# Sercan ÖZDEMİR
**`BeNKaYS`**

*Bilişim Teknolojileri Öğretmeni · Makine Öğrenmesi · C++*

<br>

[![Mail](https://img.shields.io/badge/sercanozdemir@yandex.com-000000?style=flat-square&logo=mail.ru&logoColor=white)](mailto:sercanozdemir@yandex.com)
[![WhatsApp](https://img.shields.io/badge/WhatsApp-25D366?style=flat-square&logo=whatsapp&logoColor=white)](https://wa.me/905068858585?text=Merhaba%20bilgi%20almak%20istiyorum)

```
▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
```

</div>

<br>

## Hakkımda

Eğitim ve yazılımı birleştiren, özellikle **C++**, **algoritmalar** ve **Makine Öğrenmesi** üzerine projeler geliştiren bir yazılım öğretmeniyim. Karmaşık sistemleri sade ve anlaşılır hale getirmek temel prensibimdir.

> *"Öğretmek en iyi hata ayıklama yöntemidir."*

<br>

## Teknoloji Yığını

<div align="center">

| Alan | Teknolojiler |
|------|-------------|
| **Diller** | C++ · Python |
| **Makine Öğrenmesi** | Temel modeller · Veri işleme · OpenCV |
| **Araçlar** | Git · Linux · VSCode |

</div>

<br>

## Odak Alanlarım

```
┌─────────────────────────────────────────────────┐
│  🧠  Makine Öğrenmesi  ───────────── [ ████░ ]  │
│  ⚙️  C++ & OOP         ───────────── [ █████ ]  │
│  📐  Algoritma Tasarımı ──────────── [ ████░ ]  │
│  📚  Eğitim Odaklı Dev  ──────────── [ █████ ]  │
└─────────────────────────────────────────────────┘
```

<br>

## İletişim

Proje iş birlikleri veya sorularınız için:

- 📧 [sercanozdemir@yandex.com](mailto:sercanozdemir@yandex.com)
- 💬 [WhatsApp'tan hızlı ulaşın](https://wa.me/905068858585?text=Merhaba%20bilgi%20almak%20istiyorum)

<br>

<div align="center">

*Gündüz sınıf, gece terminal.*

![](https://komarev.com/ghpvc/?username=BeNKaYS&style=flat-square&color=555555)

</div>
