using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using MEEY.Database;

namespace MEEY.Reports
{
    public partial class RaporBase : UserControl
    {
        public class DataItem : INotifyPropertyChanged
        {
            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected == value) return;
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }

            public int Id { get; set; }
            public string DisplayText { get; set; } = "";
            public object Data { get; set; } = null!;

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<DataItem> dataItems;
        private string reportType;
        private string? currentPdfPath;
        private CoreWebView2Environment? pdfPreviewEnvironment;

        private readonly List<(string Sorun, string Cozum, string Sonuc)> GunlukTaslaklar = new()
        {
            ("İşletmedeki yoğun iş temposu nedeniyle teorik bilgilerin pratiğe aktarılmasında yaşanan zaman darlığı.", "İşletme yetkilisiyle görüşülerek öğrenciye müfredat konularını deneyimlemesi için özel zaman dilimleri ayrılmıştır.", "Öğrencinin yoğun çalışma temposuna uyum sağlama yeteneği ve stres yönetimi gelişmiştir."),
            ("Okulda kullanılan eğitim materyalleri ile işletmedeki endüstriyel cihazlar arasındaki teknolojik farklar.", "Usta öğretici tarafından işletmedeki cihazların kullanımı üzerine yerinde ve uygulamalı teknik eğitim verilmiştir.", "Öğrencinin güncel sektörel teknolojileri kavrama ve uygulama becerisi standartların üzerindedir."),
            ("Öğrencinin işletme hiyerarşisi ve kurumsal iletişim süreçlerine alışma sürecinde yaşadığı tereddütler.", "Koordinatör öğretmen rehberliğinde kurumsal iletişim becerileri üzerine bilgilendirme ve rol-play çalışmaları yapılmıştır.", "Öğrencinin mesleki yönden iletişim becerileri gelişmiş olup takım çalışmasına yatkınlığı artmıştır."),
            ("Okuldaki ders programı ile vardiyalı işletme çalışma saatleri arasındaki uyum sorunları.", "Öğrencinin haftalık izin günleri okul programına göre yeniden düzenlenerek kesintisiz dinlenme sağlanmıştır.", "Öğrencinin fiziksel dayanıklılığı artmış olup iş güvenliği kurallarına uyumu tamdır."),
            ("Güvenlik protokollerinin tam kavranamayarak mesleki risklerin göz ardı edilme ihtimali.", "Öğrenciye İş Sağlığı ve Güvenliği uzmanı tarafından işletmeye özel uygulamalı ISG eğitimi verilmiştir.", "Öğrencinin iş güvenliği bilinci üst düzeydedir ve riskli durumlarda erken tedbir alma davranışı göstermektedir."),
            ("İşletmede kullanılan spesifik paket programlara veya ara yüzlere yabancılık çekilmesi.", "Ekran okuma ve program kullanma arayüzü üzerine ek eğitimler düzenlenmiş, usta öğreticiyle eşli uygulamalar yapılmıştır.", "Öğrenci işletmenin bilişim altyapısına hızlıca adaptasyon sağlamış, yazılım kullanma becerisinde gözle görülür ilerleme sağlanmıştır."),
            ("Müşteri veya üçüncü taraflarla yaşanan ilk karşılaşmalardaki iletişim zafiyeti ve özgüven eksikliği.", "Usta öğretici eşliğinde kontrollü müşteri görüşmelerine katılımı sağlanarak pratik iletişim simülasyonları yapıldı.", "Öğrencinin dış paydaşlarla iletişimi belirgin şekilde düzelmiş, mesleki özgüveni yeterli seviyeye ulaşmıştır."),
            ("Bazı el aletleri veya teknik ekipmanların doğru açı, hız ve güvenlik limitlerinde kullanılamaması.", "Tehlike içermeyen ortamda hurda materyallerle el becerisini artırmak üzere tekrar odaklı deneme çalışmaları yapılmıştır.", "El pratikliği hızla artmış olan öğrenci, malzeme firesini minimize etmede dikkate değer bir başarı göstermektedir."),
            ("Endüstriyel atık veya üretim hurdalarının doğru tasnif edilmesi ve sıfır atık ilkelerine uyum zorluğu.", "Birim amiri ile işletmenin çevresel atık yönetimi prosedürleri üstünden tekrar geçilip farkındalık oluşturuldu.", "Çevre ve geri dönüşüm duyarlılığı yüksek olup, atık yönetimi konusunda başarılı bir çalışma sergilemektedir."),
            ("Yeni işbaşı yapan diğer personelle entegrasyon veya rekabet esnasında yaşanabilen motivasyon düşüklüğü.", "Koordinatör öğretmen tarafından birebir mesleki rehberlik görüşmesi yapılmış, kişisel hedef oluşturma motivasyonu sağlanmıştır.", "Motivasyonu yüksek, ekip arkadaşlarıyla uyumlu ve verilen görevleri zamanında, sorumluluk bilinciyle yerine getiren bir öğrencidir.")
        };

        public RaporBase()
        {
            dataItems = new ObservableCollection<DataItem>();
            reportType = "";
            currentPdfPath = null;

            InitializeComponent();
            DataList.ItemsSource = dataItems;
            
            // Varsayılan tarihler - Bu Ay
            var today = DateTime.Today;
            var firstDay = new DateTime(today.Year, today.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            
            dpBaslangic.SelectedDate = firstDay;
            dpBitis.SelectedDate = lastDay;
            dpBaslangic.IsEnabled = false;
            dpBitis.IsEnabled = false;
            
            dpBelgeTarihi.SelectedDate = DateTime.Today;
            dpFesihSozlesmeTarihi.SelectedDate = null;
            dpFesihIptalTarihi.SelectedDate = null;
            if (cmbNotFisiTuru != null)
                cmbNotFisiTuru.SelectedIndex = 0;
            if (cmbNotSayfaDuzeni != null)
                cmbNotSayfaDuzeni.SelectedIndex = 0;
            if (cmbNotDonem != null)
                cmbNotDonem.SelectedIndex = 0;

            if (cmbHerSayfada != null)
                cmbHerSayfada.SelectedIndex = 0;

            if (cmbHaftalikAy != null && cmbHaftalikAy.Items.Count > today.Month)
                cmbHaftalikAy.SelectedIndex = today.Month;

            AutoSelectCurrentWeekForHaftalik();
        }

        private int GetCurrentWeekOfMonth()
        {
            var day = DateTime.Today.Day;
            var week = ((day - 1) / 7) + 1;
            return Math.Max(1, Math.Min(5, week));
        }

        private void AutoSelectCurrentWeekForHaftalik()
        {
            if (chkHaftalik1 is null || chkHaftalik2 is null || chkHaftalik3 is null || chkHaftalik4 is null || chkHaftalik5 is null)
                return;

            var selectedMonth = cmbHaftalikAy?.SelectedIndex ?? 0;
            if (selectedMonth != DateTime.Today.Month)
                return;

            chkHaftalik1.IsChecked = false;
            chkHaftalik2.IsChecked = false;
            chkHaftalik3.IsChecked = false;
            chkHaftalik4.IsChecked = false;
            chkHaftalik5.IsChecked = false;

            switch (GetCurrentWeekOfMonth())
            {
                case 1: chkHaftalik1.IsChecked = true; break;
                case 2: chkHaftalik2.IsChecked = true; break;
                case 3: chkHaftalik3.IsChecked = true; break;
                case 4: chkHaftalik4.IsChecked = true; break;
                default: chkHaftalik5.IsChecked = true; break;
            }
        }

        private void CmbHaftalikAy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AutoSelectCurrentWeekForHaftalik();
        }

        public void LoadReportType(string type)
        {
            reportType = type;
            dataItems.Clear();
            ConfigureUiForReportType();
            
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    if (reportType == "HaftalikGorevTakip")
                    {
                        // Öğretmenleri (koordinatör) yükle
                        string ogretmenQuery = @"
                            SELECT Id, KoordOgretmen
                            FROM OkulKoordinatorler
                            ORDER BY KoordOgretmen";

                        using (var command = new SQLiteCommand(ogretmenQuery, connection))
                        using (var reader  = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var id      = Convert.ToInt32(reader["Id"]);
                                var adSoyad = reader["KoordOgretmen"]?.ToString() ?? "Bilinmeyen Öğretmen";
                                dataItems.Add(new DataItem
                                {
                                    Id = id, DisplayText = adSoyad,
                                    IsSelected = false, Data = adSoyad
                                });
                            }
                        }
                    }
                    else if (reportType == "MazeretDilekcesi" || reportType == "Fesih")
                    {
                        string ogrenciQuery = @"
                            SELECT
                                o.Id,
                                o.AdSoyad,
                                o.OkulNo,
                                COALESCE(k.Isletme, '') AS IsletmeAdi
                            FROM Ogrenciler o
                            LEFT JOIN KoordinatorTanimlama k
                                ON CAST(o.Koordinator AS TEXT) = CAST(k.Id AS TEXT)
                            ORDER BY o.AdSoyad, o.OkulNo";

                        using (var command = new SQLiteCommand(ogrenciQuery, connection))
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var ogrenciId = Convert.ToInt32(reader["Id"]);
                                var adSoyad = reader["AdSoyad"]?.ToString() ?? "Bilinmeyen Öğrenci";
                                var okulNo = reader["OkulNo"]?.ToString() ?? "-";
                                var isletmeAdi = reader["IsletmeAdi"]?.ToString() ?? string.Empty;

                                var displayText = string.IsNullOrWhiteSpace(isletmeAdi)
                                    ? $"{adSoyad} ({okulNo})"
                                    : $"{adSoyad} ({okulNo}) - {isletmeAdi}";

                                dataItems.Add(new DataItem
                                {
                                    Id = ogrenciId,
                                    DisplayText = displayText,
                                    IsSelected = false,
                                    Data = adSoyad
                                });
                            }
                        }
                    }
                    else
                    {
                        // Tüm işletmeleri getir (sadece ad)
                        string query = @"
                            SELECT 
                                Id,
                                IsletmeAdi
                            FROM Isletmeler
                            ORDER BY IsletmeAdi";

                        using (var command = new SQLiteCommand(query, connection))
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var isletmeId = Convert.ToInt32(reader["Id"]);
                                var isletmeAdi = reader["IsletmeAdi"]?.ToString() ?? "Bilinmeyen";

                                dataItems.Add(new DataItem
                                {
                                    Id = isletmeId,
                                    DisplayText = isletmeAdi,
                                    IsSelected = false,
                                    Data = isletmeAdi
                                });
                            }
                        }
                    }
                }

                if (reportType == "Fesih")
                    SyncFesihOptionsState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureUiForReportType()
        {
            var isDevamsizlik      = reportType == "Devamsizlik";
            var isAylik            = reportType == "AylikRehberlik";
            var isFesih            = reportType == "Fesih";
            var isMazeret          = reportType == "MazeretDilekcesi";
            var isNotCizelgesi     = reportType == "NotCizelgesi";
            var isGunlukRehberlik  = reportType == "GunlukRehberlik";
            var isHaftalikGorev    = reportType == "HaftalikGorevTakip";

            btnRaporSevkGir.Visibility              = isDevamsizlik  ? Visibility.Visible : Visibility.Collapsed;
            DevamsizlikOptionsPanel.Visibility      = isDevamsizlik  ? Visibility.Visible : Visibility.Collapsed;
            AylikOptionsPanel.Visibility            = isAylik        ? Visibility.Visible : Visibility.Collapsed;
            GunlukRehberlikOptionsPanel.Visibility  = isGunlukRehberlik ? Visibility.Visible : Visibility.Collapsed;
            MazeretOptionsPanel.Visibility          = isMazeret      ? Visibility.Visible : Visibility.Collapsed;
            FesihOptionsPanel.Visibility            = isFesih        ? Visibility.Visible : Visibility.Collapsed;
            NotCizelgesiOptionsPanel.Visibility     = isNotCizelgesi ? Visibility.Visible : Visibility.Collapsed;
            HaftalikGorevOptionsPanel.Visibility    = isHaftalikGorev ? Visibility.Visible : Visibility.Collapsed;

            var hideTimeRange = isFesih || isMazeret || isNotCizelgesi || isHaftalikGorev;
            TimeRangePanel.Visibility  = hideTimeRange ? Visibility.Collapsed : Visibility.Visible;
            DateRangePanel.Visibility  = hideTimeRange ? Visibility.Collapsed : Visibility.Visible;
            NupPanel.Visibility        = (isFesih || isMazeret || isDevamsizlik || isNotCizelgesi || isHaftalikGorev)
                                         ? Visibility.Collapsed : Visibility.Visible;
            BelgeTarihiPanel.Visibility = (isFesih || isHaftalikGorev)
                                         ? Visibility.Collapsed : Visibility.Visible;

            if (isAylik)
            {
                if (chkAylikAutoDoldur is not null && chkAylikAutoDoldur.IsChecked != true)
                    chkAylikAutoDoldur.IsChecked = true;
                SyncAylikOptionsState();
            }

            if (isDevamsizlik)
            {
                Grid.SetColumn(btnGenerateReport, 4);
                Grid.SetColumnSpan(btnGenerateReport, 1);
                btnGenerateReport.FontSize = 14;
            }
            else
            {
                Grid.SetColumn(btnGenerateReport, 2);
                Grid.SetColumnSpan(btnGenerateReport, 3);
                btnGenerateReport.FontSize = 13;

                Grid.SetColumn(btnHtmlKaydet, 0);
                Grid.SetColumnSpan(btnHtmlKaydet, 1);
            }

            if (isFesih) SyncFesihOptionsState();
            if (isHaftalikGorev) AutoSelectCurrentWeekForHaftalik();
        }

        private void AylikAutoDoldur_Changed(object sender, RoutedEventArgs e)
        {
            SyncAylikOptionsState();
        }

        private void GunlukTaslak_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbGunlukTaslak == null || txtGunlukSorun == null || txtGunlukCozum == null || txtGunlukSonuc == null)
                return;

            int index = cmbGunlukTaslak.SelectedIndex;
            if (index > 0 && index <= GunlukTaslaklar.Count)
            {
                var taslak = GunlukTaslaklar[index - 1];
                txtGunlukSorun.Text = taslak.Sorun;
                txtGunlukCozum.Text = taslak.Cozum;
                txtGunlukSonuc.Text = taslak.Sonuc;
            }
            else if (index == 0) // Manuel Giriş
            {
                txtGunlukSorun.Text = "";
                txtGunlukCozum.Text = "";
                txtGunlukSonuc.Text = "";
            }
        }

        private void SyncAylikOptionsState()
        {
            if (chkAylikAutoDoldur is null || chkUstaBelgesi is null)
                return;

            var auto = chkAylikAutoDoldur.IsChecked == true;
            chkUstaBelgesi.IsEnabled = auto;
            if (!auto)
                chkUstaBelgesi.IsChecked = false;
        }

        private void FesihBosSablon_Changed(object sender, RoutedEventArgs e)
        {
            SyncFesihOptionsState();
        }

        private void SyncFesihOptionsState()
        {
            if (chkFesihBosSablon is null)
                return;
        }

        private int GetSelectedNup()
        {
            if (cmbHerSayfada?.SelectedItem is ComboBoxItem item)
            {
                var raw = (item.Content?.ToString() ?? string.Empty).Trim();
                if (raw.StartsWith("2"))
                    return 2;
            }

            return 1;
        }

        private void SelectAll_Changed(object sender, RoutedEventArgs e)
        {
            bool isChecked = chkSelectAll.IsChecked == true;
            foreach (var item in dataItems)
            {
                item.IsSelected = isChecked;
            }
        }

        private void TimeRange_Changed(object sender, RoutedEventArgs e)
        {
            if (dpBaslangic == null || dpBitis == null) return;
            
            if (rbBuHafta?.IsChecked == true)
            {
                // Bu haftanın pazartesi ve cumasi
                var today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var monday = today.AddDays(-diff);
                var friday = monday.AddDays(4);
                
                dpBaslangic.SelectedDate = monday;
                dpBitis.SelectedDate = friday;
                dpBaslangic.IsEnabled = false;
                dpBitis.IsEnabled = false;
            }
            else if (rbBuAy?.IsChecked == true)
            {
                // Bu ayın ilk ve son günü
                var today = DateTime.Today;
                var firstDay = new DateTime(today.Year, today.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);
                
                dpBaslangic.SelectedDate = firstDay;
                dpBitis.SelectedDate = lastDay;
                dpBaslangic.IsEnabled = false;
                dpBitis.IsEnabled = false;
            }
            else if (rbZamanAraligi?.IsChecked == true)
            {
                dpBaslangic.IsEnabled = true;
                dpBitis.IsEnabled = true;
            }
        }

        private void OtomatikDoldur_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = dataItems.Where(x => x.IsSelected).ToList();
            
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir işletme seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (!dpBaslangic.SelectedDate.HasValue || !dpBitis.SelectedDate.HasValue)
            {
                MessageBox.Show("Lütfen tarih aralığı seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    
                    // Tatil günlerini Çalışma Takvimi'nden al
                    var tatilGunleri = new HashSet<DateTime>();
                    string tatilQuery = @"
                        SELECT Baslangic, Bitis 
                        FROM CalismaTakvimi 
                        WHERE (Baslangic <= @Bitis AND Bitis >= @Baslangic)";
                    
                    using (var tatilCmd = new SQLiteCommand(tatilQuery, connection))
                    {
                        tatilCmd.Parameters.AddWithValue("@Baslangic", dpBitis.SelectedDate.Value.ToString("yyyy-MM-dd"));
                        tatilCmd.Parameters.AddWithValue("@Bitis", dpBaslangic.SelectedDate.Value.ToString("yyyy-MM-dd"));
                        
                        using (var tatilReader = tatilCmd.ExecuteReader())
                        {
                            while (tatilReader.Read())
                            {
                                var tatilBaslangic = DateTime.Parse(tatilReader["Baslangic"].ToString()!);
                                var tatilBitis = DateTime.Parse(tatilReader["Bitis"].ToString()!);
                                
                                // Tatil aralığındaki tüm günleri ekle
                                for (var tarih = tatilBaslangic; tarih <= tatilBitis; tarih = tarih.AddDays(1))
                                {
                                    tatilGunleri.Add(tarih.Date);
                                }
                            }
                        }
                    }
                    
                    int toplamGuncelleme = 0;
                    
                    foreach (var isletme in selectedItems)
                    {
                        var isletmeAdi = isletme.Data?.ToString() ?? "";
                        
                        // Bu işletmedeki öğrencileri ve çalışma günlerini al
                        string query = @"
                            SELECT DISTINCT
                                o.Id as OgrenciId,
                                o.Gunler as OgrenciGunleri,
                                k.Gun as KoordinatorGunu
                            FROM KoordinatorTanimlama k
                            INNER JOIN Ogrenciler o ON (o.Koordinator = CAST(k.Id AS TEXT) OR o.Koordinator = k.Id)
                            WHERE k.Isletme = @IsletmeAdi";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@IsletmeAdi", isletmeAdi);
                            
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var ogrenciId = Convert.ToInt32(reader["OgrenciId"]);
                                    var ogrenciGunleri = reader["OgrenciGunleri"]?.ToString() ?? "";
                                    var koordinatorGunu = reader["KoordinatorGunu"]?.ToString() ?? "";
                                    
                                    // Öğrencinin çalışma günlerini belirle
                                    var calismaGunleri = new HashSet<DayOfWeek>();
                                    
                                    // Önce öğrencinin kendi günlerini kontrol et
                                    if (!string.IsNullOrEmpty(ogrenciGunleri))
                                    {
                                        ParseGunler(ogrenciGunleri, calismaGunleri);
                                    }
                                    // Yoksa koordinatörün gününü kullan
                                    else if (!string.IsNullOrEmpty(koordinatorGunu))
                                    {
                                        ParseGunler(koordinatorGunu, calismaGunleri);
                                    }
                                    
                                    // Tarih aralığındaki her gün için sembol belirle
                                    for (var tarih = dpBaslangic.SelectedDate.Value; 
                                         tarih <= dpBitis.SelectedDate.Value; 
                                         tarih = tarih.AddDays(1))
                                    {
                                        // Sembol belirleme (sadece sayım için)
                                        // Çalışma Takvimi'nden gelen tatil günü ise T
                                        // Öğrencinin çalışma günü ise X
                                        // Hafta içi ama çalışma günü değilse O (okulda)
                                        // Cumartesi/Pazar - boş bırak
                                        
                                        toplamGuncelleme++;
                                    }
                                }
                            }
                        }
                    }
                    
                    MessageBox.Show($"Otomatik doldurma tamamlandı!\n\n" +
                                  $"İşletme sayısı: {selectedItems.Count}\n" +
                                  $"Tarih aralığı: {dpBaslangic.SelectedDate:dd.MM.yyyy} - {dpBitis.SelectedDate:dd.MM.yyyy}\n\n" +
                                  $"Semboller:\n" +
                                  $"X: İşletmede (öğrencinin çalışma günleri)\n" +
                                  $"O: Okulda (hafta içi diğer günler)\n" +
                                  $"T: Tatil (Çalışma Takvimi'nden)",
                        "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Otomatik doldurma sırasında hata oluştu:\n{ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void RaporSevkGir_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = dataItems.Where(x => x.IsSelected).ToList();
            
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir işletme seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Popup penceresi oluştur
            var popup = new Window
            {
                Title = "Rapor Sevk Girişi",
                Width = 950,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                ResizeMode = ResizeMode.NoResize
            };
            
            var mainGrid = new Grid { Margin = new Thickness(20) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(320) }); // Sabit yükseklik
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Liste için
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Tamam butonu
            
            // Üst kısım - 3 kolon
            var topGrid = new Grid();
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            
            // Sol - Öğrenci Listesi
            var leftStack = new StackPanel { Margin = new Thickness(0, 0, 15, 0) };
            
            var ogrenciLabel = new TextBlock 
            { 
                Text = "Öğrenci Seçin:", 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            leftStack.Children.Add(ogrenciLabel);
            
            var ogrenciListBox = new ListBox
            {
                Height = 280,
                SelectionMode = SelectionMode.Multiple
            };
            
            // Öğrencileri yükle
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    
                    foreach (var isletme in selectedItems)
                    {
                        var isletmeAdi = isletme.Data?.ToString() ?? "";
                        
                        string query = @"
                            SELECT DISTINCT
                                o.Id as OgrenciId,
                                o.AdSoyad,
                                o.OkulNo
                            FROM KoordinatorTanimlama k
                            INNER JOIN Ogrenciler o ON (o.Koordinator = CAST(k.Id AS TEXT) OR o.Koordinator = k.Id)
                            WHERE k.Isletme = @IsletmeAdi
                            ORDER BY o.AdSoyad";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@IsletmeAdi", isletmeAdi);
                            
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var ogrenciId = Convert.ToInt32(reader["OgrenciId"]);
                                    var adSoyad = reader["AdSoyad"]?.ToString() ?? "";
                                    var okulNo = reader["OkulNo"]?.ToString() ?? "";
                                    
                                    var item = new ListBoxItem
                                    {
                                        Content = $"{adSoyad} ({okulNo})",
                                        Tag = ogrenciId
                                    };
                                    ogrenciListBox.Items.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Öğrenci yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            leftStack.Children.Add(ogrenciListBox);
            Grid.SetColumn(leftStack, 0);
            topGrid.Children.Add(leftStack);
            
            // Orta - Takvim
            var centerStack = new StackPanel { Margin = new Thickness(0, 0, 15, 0) };
            
            var calendarLabel = new TextBlock 
            { 
                Text = "Tarih Seçin:", 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            centerStack.Children.Add(calendarLabel);
            
            var calendar = new System.Windows.Controls.Calendar
            {
                Height = 280,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                SelectionMode = CalendarSelectionMode.MultipleRange
            };
            centerStack.Children.Add(calendar);
            
            Grid.SetColumn(centerStack, 1);
            topGrid.Children.Add(centerStack);
            
            // Sağ - Sembol seçimi
            var rightStack = new StackPanel();
            
            var sembolLabel = new TextBlock 
            { 
                Text = "Sembol Seçin:", 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            rightStack.Children.Add(sembolLabel);
            
            // Butonlar için container - takvimle aynı yükseklik
            var buttonContainer = new StackPanel { Height = 280 };
            
            var btnI = new Button 
            { 
                Content = "İ - İzinli", 
                Height = 65, 
                Margin = new Thickness(0, 0, 0, 5),
                Background = new SolidColorBrush(Color.FromRgb(243, 156, 18)),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Tag = "İ"
            };
            
            var btnD = new Button 
            { 
                Content = "D - Özürsüz Devamsız", 
                Height = 65, 
                Margin = new Thickness(0, 0, 0, 5),
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Tag = "D"
            };
            
            var btnH = new Button 
            { 
                Content = "H - Hasta Sevkli", 
                Height = 65, 
                Margin = new Thickness(0, 0, 0, 5),
                Background = new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Tag = "H"
            };
            
            var btnR = new Button 
            { 
                Content = "R - Raporlu", 
                Height = 65, 
                Margin = new Thickness(0, 0, 0, 5),
                Background = new SolidColorBrush(Color.FromRgb(26, 188, 156)),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Tag = "R"
            };
            
            // Seçilen kayıtları tutan liste
            var secimListesi = new List<(DateTime Tarih, string Sembol, int? Id)>();
            
            // Liste kutusunu önceden tanımla (SembolEkle fonksiyonunda kullanılacak)
            var listeBox = new ListBox
            {
                MinHeight = 150,
                MaxHeight = 200
            };
            
            // Öğrenci seçildiğinde mevcut kayıtları yükle
            void MevcutKayitlariYukle()
            {
                if (ogrenciListBox.SelectedItems.Count == 0) return;
                
                secimListesi.Clear();
                
                try
                {
                    using (var connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();
                        
                        foreach (ListBoxItem item in ogrenciListBox.SelectedItems)
                        {
                            var ogrenciId = Convert.ToInt32(item.Tag);
                            
                            string query = @"
                                SELECT Id, Tarih, Sembol 
                                FROM Devamsizlik 
                                WHERE OgrenciId = @OgrenciId
                                ORDER BY Tarih";
                            
                            using (var command = new SQLiteCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@OgrenciId", ogrenciId);
                                
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        var id = Convert.ToInt32(reader["Id"]);
                                        var tarih = DateTime.Parse(reader["Tarih"].ToString()!);
                                        var sembol = reader["Sembol"]?.ToString() ?? "";
                                        
                                        secimListesi.Add((tarih, sembol, id));
                                    }
                                }
                            }
                        }
                    }
                    
                    // Listeyi güncelle
                    listeBox.Items.Clear();
                    foreach (var item in secimListesi.OrderBy(x => x.Tarih))
                    {
                        listeBox.Items.Add($"{item.Tarih:dd.MM.yyyy} - {item.Sembol}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Kayıt yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
            // Öğrenci seçimi değiştiğinde kayıtları yükle
            ogrenciListBox.SelectionChanged += (s, args) => MevcutKayitlariYukle();
            
            // Sembol butonlarına tıklama olayı
            void SembolEkle(string sembol)
            {
                if (calendar.SelectedDates.Count == 0)
                {
                    MessageBox.Show("Lütfen önce tarih seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                foreach (var tarih in calendar.SelectedDates)
                {
                    // Aynı tarih varsa güncelle
                    var mevcut = secimListesi.FirstOrDefault(x => x.Tarih.Date == tarih.Date);
                    if (mevcut != default)
                    {
                        secimListesi.Remove(mevcut);
                    }
                    
                    secimListesi.Add((tarih, sembol, null));
                }
                
                // Listeyi güncelle
                listeBox.Items.Clear();
                foreach (var item in secimListesi.OrderBy(x => x.Tarih))
                {
                    listeBox.Items.Add($"{item.Tarih:dd.MM.yyyy} - {item.Sembol}");
                }
                
                // Takvim seçimini temizle
                calendar.SelectedDates.Clear();
            }
            
            btnI.Click += (s, args) => SembolEkle("İ");
            btnD.Click += (s, args) => SembolEkle("D");
            btnH.Click += (s, args) => SembolEkle("H");
            btnR.Click += (s, args) => SembolEkle("R");
            
            buttonContainer.Children.Add(btnI);
            buttonContainer.Children.Add(btnD);
            buttonContainer.Children.Add(btnH);
            buttonContainer.Children.Add(btnR);
            
            rightStack.Children.Add(buttonContainer);
            
            Grid.SetColumn(rightStack, 2);
            topGrid.Children.Add(rightStack);
            
            Grid.SetRow(topGrid, 0);
            mainGrid.Children.Add(topGrid);
            
            // Alt kısım - Seçilen kayıtlar listesi
            var bottomStack = new StackPanel { Margin = new Thickness(0, 20, 0, 0) };
            
            var listeBaslikGrid = new Grid();
            listeBaslikGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            listeBaslikGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            var listeLabel = new TextBlock 
            { 
                Text = "Seçilen Kayıtlar:", 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(listeLabel, 0);
            listeBaslikGrid.Children.Add(listeLabel);
            
            // Seçili kaydı sil butonu
            var btnSil = new Button
            {
                Content = "🗑️ Seçili Kaydı Sil",
                Height = 30,
                Padding = new Thickness(10, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 11,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            
            btnSil.Click += (s, args) =>
            {
                if (listeBox.SelectedIndex < 0)
                {
                    MessageBox.Show("Lütfen silmek için bir kayıt seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var selectedIndex = listeBox.SelectedIndex;
                var kayit = secimListesi[selectedIndex];
                
                // Veritabanından sil (eğer Id varsa)
                if (kayit.Id.HasValue)
                {
                    try
                    {
                        using (var connection = DatabaseManager.GetConnection())
                        {
                            connection.Open();
                            string deleteQuery = "DELETE FROM Devamsizlik WHERE Id = @Id";
                            using (var cmd = new SQLiteCommand(deleteQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@Id", kayit.Id.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                
                // Listeden kaldır
                secimListesi.RemoveAt(selectedIndex);
                listeBox.Items.RemoveAt(selectedIndex);
            };
            
            Grid.SetColumn(btnSil, 1);
            listeBaslikGrid.Children.Add(btnSil);
            
            bottomStack.Children.Add(listeBaslikGrid);
            bottomStack.Children.Add(listeBox);
            
            Grid.SetRow(bottomStack, 1);
            mainGrid.Children.Add(bottomStack);
            
            // Tamam butonu
            var tamamButton = new Button
            {
                Content = "✓ Tamam - Kaydet ve Çık",
                Height = 50,
                Margin = new Thickness(0, 20, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)),
                Foreground = Brushes.White,
                FontSize = 15,
                FontWeight = FontWeights.Bold
            };
            
            tamamButton.Click += (s, args) =>
            {
                if (ogrenciListBox.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Lütfen en az bir öğrenci seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (secimListesi.Count == 0)
                {
                    MessageBox.Show("Kayıt listesi boş. Değişiklik yapılmadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    popup.Close();
                    return;
                }
                
                try
                {
                    using (var connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();
                        int kayitSayisi = 0;
                        
                        // Seçili öğrenciler için kayıt yap
                        foreach (ListBoxItem item in ogrenciListBox.SelectedItems)
                        {
                            var ogrenciId = Convert.ToInt32(item.Tag);
                            
                            foreach (var secim in secimListesi.Where(x => !x.Id.HasValue)) // Sadece yeni kayıtlar
                            {
                                string insertQuery = @"
                                    INSERT OR REPLACE INTO Devamsizlik (OgrenciId, Tarih, Sembol)
                                    VALUES (@OgrenciId, @Tarih, @Sembol)";
                                
                                using (var insertCmd = new SQLiteCommand(insertQuery, connection))
                                {
                                    insertCmd.Parameters.AddWithValue("@OgrenciId", ogrenciId);
                                    insertCmd.Parameters.AddWithValue("@Tarih", secim.Tarih.ToString("yyyy-MM-dd"));
                                    insertCmd.Parameters.AddWithValue("@Sembol", secim.Sembol);
                                    insertCmd.ExecuteNonQuery();
                                    kayitSayisi++;
                                }
                            }
                        }
                        
                        if (kayitSayisi > 0)
                        {
                            MessageBox.Show($"Rapor sevk kaydı başarıyla eklendi!\n\nÖğrenci sayısı: {ogrenciListBox.SelectedItems.Count}\nYeni kayıt sayısı: {kayitSayisi}", 
                                "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Yeni kayıt eklenmedi. Tüm kayıtlar zaten mevcut.", 
                                "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        
                        popup.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Kayıt hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            
            Grid.SetRow(tamamButton, 2);
            mainGrid.Children.Add(tamamButton);
            
            popup.Content = mainGrid;
            popup.ShowDialog();
        }
        
        private void ParseGunler(string gunler, HashSet<DayOfWeek> calismaGunleri)
        {
            if (string.IsNullOrEmpty(gunler)) return;
            
            var gunListesi = gunler.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var gun in gunListesi)
            {
                var gunTrim = gun.Trim().ToLowerInvariant();
                
                if (gunTrim.Contains("pzt") || gunTrim.Contains("pazartesi"))
                    calismaGunleri.Add(DayOfWeek.Monday);
                else if (gunTrim.Contains("sal") || gunTrim.Contains("salı"))
                    calismaGunleri.Add(DayOfWeek.Tuesday);
                else if (gunTrim.Contains("çar") || gunTrim.Contains("çarşamba"))
                    calismaGunleri.Add(DayOfWeek.Wednesday);
                else if (gunTrim.Contains("per") || gunTrim.Contains("perşembe"))
                    calismaGunleri.Add(DayOfWeek.Thursday);
                else if (gunTrim.Contains("cum") || gunTrim.Contains("cuma"))
                    calismaGunleri.Add(DayOfWeek.Friday);
            }
        }

        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = dataItems.Where(x => x.IsSelected).ToList();
            var isFesih = reportType == "Fesih";
            var isMazeret = reportType == "MazeretDilekcesi";
            var requiresDateRange = reportType == "Devamsizlik" || reportType == "GunlukRehberlik" || reportType == "AylikRehberlik";
            var fesihBosSablon = chkFesihBosSablon?.IsChecked == true;
            
            if (!isFesih && selectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir kayıt seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isFesih && !fesihBosSablon && selectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen soldaki listeden bir öğrenci seçin ya da 'Formu boş oluştur' seçeneğini işaretleyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isFesih && !fesihBosSablon && selectedItems.Count > 1)
            {
                MessageBox.Show("Fesih raporu için lütfen soldaki listeden yalnızca bir öğrenci seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isMazeret && selectedItems.Count != 1)
            {
                MessageBox.Show("Mazeret dilekçesi için soldaki listeden yalnızca bir öğrenci seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (requiresDateRange && (!dpBaslangic.SelectedDate.HasValue || !dpBitis.SelectedDate.HasValue))
            {
                MessageBox.Show("Lütfen tarih aralığı seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                // Önce info panelinde yükleniyor mesajı göster
                ShowInfoPanel();
                PreviewPanel.Children.Clear();
                var loadingText = new TextBlock
                {
                    Text = "Rapor oluşturuluyor...",
                    FontSize = 14,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                PreviewPanel.Children.Add(loadingText);

                var baslangicTarihi = dpBaslangic.SelectedDate ?? DateTime.Today;
                var bitisTarihi = dpBitis.SelectedDate ?? DateTime.Today;

                // PDF oluştur
                byte[] pdfBytes = reportType switch
                {
                    "Devamsizlik"       => OlusturDevamsizlikPdf(selectedItems, baslangicTarihi, bitisTarihi),
                    "GunlukRehberlik"   => OlusturGunlukRehberlikPdf(selectedItems, baslangicTarihi, bitisTarihi),
                    "AylikRehberlik"    => OlusturAylikRehberlikPdf(selectedItems, baslangicTarihi, bitisTarihi),
                    "MazeretDilekcesi"  => OlusturMazeretDilekcesiPdf(selectedItems),
                    "Fesih"             => OlusturFesihPdf(selectedItems),
                    "NotCizelgesi"      => OlusturNotCizelgesiPdf(selectedItems),
                    "HaftalikGorevTakip"    => OlusturHaftalikGorevPdf(selectedItems),
                    _ => throw new Exception("Bu rapor türü henüz uygulanmadı.")
                };
                
                // Temp klasöre kaydet
                var filePrefix = reportType switch
                {
                    "Fesih" => "SozlesmeIptal",
                    "MazeretDilekcesi" => "MazeretDilekcesi",
                    "NotCizelgesi" => "NotCizelgesi",
                    _ => "Rapor"
                };
                var tempPath = Path.Combine(Path.GetTempPath(), $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                File.WriteAllBytes(tempPath, pdfBytes);
                currentPdfPath = tempPath;

                // Önce panel içinde önizleme göstermeyi dene
                var previewShown = await ShowPdfPreviewAsync(tempPath);

                // WebView2 başarısız olursa uygulama içinde bilgilendirme göster
                if (!previewShown)
                {
                    ShowInfoPanel();
                    PreviewPanel.Children.Clear();

                    var warningText = new TextBlock
                    {
                        Text = "⚠ Rapor oluşturuldu ancak uygulama içi önizleme başlatılamadı.",
                        FontSize = 13,
                        Foreground = System.Windows.Media.Brushes.OrangeRed,
                        Margin = new Thickness(20),
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeights.SemiBold
                    };
                    PreviewPanel.Children.Add(warningText);

                    var pathText = new TextBlock
                    {
                        Text = $"Dosya konumu:\n{tempPath}",
                        FontSize = 11,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(20, 0, 20, 0),
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    PreviewPanel.Children.Add(pathText);
                }
            }
            catch (Exception ex)
            {
                ShowInfoPanel();
                PreviewPanel.Children.Clear();
                var errorText = new TextBlock
                {
                    Text = $"❌ Rapor oluşturma hatası:\n\n{ex.Message}",
                    FontSize = 12,
                    Foreground = System.Windows.Media.Brushes.Red,
                    Margin = new Thickness(0, 20, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };
                PreviewPanel.Children.Add(errorText);
                
                MessageBox.Show($"Rapor oluşturma hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnHtmlKaydet_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = dataItems.Where(x => x.IsSelected).ToList();
            var isFesih = reportType == "Fesih";
            var isMazeret = reportType == "MazeretDilekcesi";
            var requiresDateRange = reportType == "Devamsizlik" || reportType == "GunlukRehberlik" || reportType == "AylikRehberlik";
            var fesihBosSablon = chkFesihBosSablon?.IsChecked == true;
            
            if (!isFesih && selectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir kayıt seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isFesih && !fesihBosSablon && selectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen soldaki listeden bir öğrenci seçin ya da 'Formu boş oluştur' seçeneğini işaretleyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isFesih && !fesihBosSablon && selectedItems.Count > 1)
            {
                MessageBox.Show("Fesih raporu için lütfen soldaki listeden yalnızca bir öğrenci seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isMazeret && selectedItems.Count != 1)
            {
                MessageBox.Show("Mazeret dilekçesi için soldaki listeden yalnızca bir öğrenci seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (requiresDateRange && (!dpBaslangic.SelectedDate.HasValue || !dpBitis.SelectedDate.HasValue))
            {
                MessageBox.Show("Lütfen tarih aralığı seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                ShowInfoPanel();
                PreviewPanel.Children.Clear();
                var loadingText = new TextBlock
                {
                    Text = "HTML dosyası oluşturuluyor (Aspose)... Lütfen bekleyin.",
                    FontSize = 14,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                PreviewPanel.Children.Add(loadingText);
                
                Dispatcher.InvokeAsync(async () => 
                {
                    try
                    {
                        var baslangicTarihi = dpBaslangic.SelectedDate ?? DateTime.Today;
                        var bitisTarihi = dpBitis.SelectedDate ?? DateTime.Today;

                        // Generate HTML text inside utf8-encoded bytes
                        byte[] htmlBytes = reportType switch
                        {
                            "Devamsizlik" => OlusturDevamsizlikPdf(selectedItems, baslangicTarihi, bitisTarihi, true),
                            "GunlukRehberlik" => OlusturGunlukRehberlikPdf(selectedItems, baslangicTarihi, bitisTarihi, true),
                            "AylikRehberlik" => OlusturAylikRehberlikPdf(selectedItems, baslangicTarihi, bitisTarihi, true),
                            "MazeretDilekcesi" => OlusturMazeretDilekcesiPdf(selectedItems, true),
                            "Fesih" => OlusturFesihPdf(selectedItems, true),
                            "NotCizelgesi" => OlusturNotCizelgesiPdf(selectedItems, true),
                            _ => throw new Exception("Bu rapor türü henüz uygulanmadı.")
                        };
                        
                        var filePrefix = reportType switch
                        {
                            "Fesih" => "SozlesmeIptal",
                            "MazeretDilekcesi" => "MazeretDilekcesi",
                            "NotCizelgesi" => "NotCizelgesi",
                            _ => "Rapor"
                        };
                        
                        // Target Path
                        var targetDir = Path.Combine(AppContext.BaseDirectory, "Assets", "EditorKayitlari");
                        if (!Directory.Exists(targetDir))
                            Directory.CreateDirectory(targetDir);
                            
                        var htmlFileName = $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                        var finalHtmlPath = Path.Combine(targetDir, htmlFileName);

                        // Save raw HTML directly
                        File.WriteAllBytes(finalHtmlPath, htmlBytes);
                        
                        PreviewPanel.Children.Clear();
                        var successText = new TextBlock
                        {
                            Text = $"✓ HTML formatında rapor kaydedildi!\n\nDosya Yolu:\n{finalHtmlPath}",
                            FontSize = 14,
                            Foreground = System.Windows.Media.Brushes.Green,
                            Margin = new Thickness(20),
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontWeight = FontWeights.SemiBold
                        };
                        PreviewPanel.Children.Add(successText);
                        
                        var btnAc = new Button
                        {
                            Content = "🌐 Tarayıcıda Aç",
                            Width = 150,
                            Height = 40,
                            Margin = new Thickness(0, 10, 0, 0),
                            Cursor = System.Windows.Input.Cursors.Hand,
                            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 128, 185)),
                            Foreground = System.Windows.Media.Brushes.White,
                            FontWeight = FontWeights.Bold
                        };
                        btnAc.Click += (s, ev) => 
                        {
                            try {
                                Process.Start(new ProcessStartInfo { FileName = finalHtmlPath, UseShellExecute = true });
                            } catch {}
                        };
                        PreviewPanel.Children.Add(btnAc);
                    }
                    catch (Exception ex)
                    {
                        PreviewPanel.Children.Clear();
                        var errorText = new TextBlock
                        {
                            Text = $"❌ HTML oluşturma hatası:\n\n{ex.Message}",
                            FontSize = 12,
                            Foreground = System.Windows.Media.Brushes.Red,
                            Margin = new Thickness(0, 20, 0, 0),
                            TextWrapping = TextWrapping.Wrap
                        };
                        PreviewPanel.Children.Add(errorText);
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Başlatma hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowPdfViewer()
        {
            PdfViewerBorder.Visibility = Visibility.Visible;
            InfoScrollViewer.Visibility = Visibility.Collapsed;
        }

        private void ShowInfoPanel()
        {
            PdfViewerBorder.Visibility = Visibility.Collapsed;
            InfoScrollViewer.Visibility = Visibility.Visible;
        }

        private bool IsSymbolEnabled(string symbol)
        {
            return symbol switch
            {
                "X" => chkX.IsChecked == true,
                "O" => chkO.IsChecked == true,
                "İ" => chkI.IsChecked == true,
                "H" => chkH.IsChecked == true,
                "R" => chkR.IsChecked == true,
                "T" => chkT.IsChecked == true,
                _ => true
            };
        }

        private async Task<bool> ShowPdfPreviewAsync(string pdfPath)
        {
            try
            {
                ShowPdfViewer();

                if (PdfPreviewBrowser.CoreWebView2 == null)
                {
                    if (pdfPreviewEnvironment == null)
                    {
                        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var webViewPath = Path.Combine(appData, "MEEY", "WebView2", "ReportPreview");
                        Directory.CreateDirectory(webViewPath);
                        pdfPreviewEnvironment = await CoreWebView2Environment.CreateAsync(userDataFolder: webViewPath);
                    }

                    await PdfPreviewBrowser.EnsureCoreWebView2Async(pdfPreviewEnvironment);
                }

                PdfPreviewBrowser.Source = new Uri(Path.GetFullPath(pdfPath));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private byte[] OlusturDevamsizlikPdf(List<DataItem> isletmeler, DateTime baslangic, DateTime bitis, bool isHtml = false)
        {
            var raporlar = new List<RaporGirdisi>();
            
            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();
                
                foreach (var isletme in isletmeler)
                {
                    var isletmeAdi = isletme.Data?.ToString() ?? "";
                    
                    // İşletme telefon numarasını al
                    string isletmeTelefon = "";
                    string isletmeQuery = "SELECT Telefon FROM Isletmeler WHERE IsletmeAdi = @IsletmeAdi LIMIT 1";
                    using (var isletmeCmd = new SQLiteCommand(isletmeQuery, connection))
                    {
                        isletmeCmd.Parameters.AddWithValue("@IsletmeAdi", isletmeAdi);
                        var result = isletmeCmd.ExecuteScalar();
                        if (result != null)
                            isletmeTelefon = result.ToString() ?? "";
                    }
                    
                    string query = @"
                        SELECT DISTINCT
                            k.Id as KoordinatorId,
                            k.Okul,
                            k.MudurYrd,
                            o.Id as OgrenciId,
                            o.OkulNo, 
                            o.AdSoyad, 
                            o.AlanDal
                        FROM KoordinatorTanimlama k
                        INNER JOIN Ogrenciler o ON (o.Koordinator = CAST(k.Id AS TEXT) OR o.Koordinator = k.Id)
                        WHERE k.Isletme = @IsletmeAdi
                        ORDER BY o.AdSoyad";
                    
                    var ogrenciler = new List<StudentInfo>();
                    string okulAdi = "";
                    string mudurYardimcisi = "";
                    
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IsletmeAdi", isletmeAdi);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (string.IsNullOrEmpty(okulAdi))
                                    okulAdi = reader["Okul"]?.ToString() ?? "";
                                
                                if (string.IsNullOrEmpty(mudurYardimcisi))
                                    mudurYardimcisi = reader["MudurYrd"]?.ToString() ?? "";
                                
                                var ogrenciId = Convert.ToInt32(reader["OgrenciId"]);
                                var okulNo = reader["OkulNo"]?.ToString() ?? "";
                                var adSoyad = reader["AdSoyad"]?.ToString() ?? "";
                                var alanDal = reader["AlanDal"]?.ToString() ?? "";
                                
                                // Alan ve Dal'ı ayır
                                var alanDalParts = alanDal.Split(new[] { '-', '/' }, 2);
                                var alan = alanDalParts.Length > 0 ? alanDalParts[0].Trim() : "";
                                var dal = alanDalParts.Length > 1 ? alanDalParts[1].Trim() : "";
                                
                                ogrenciler.Add(new StudentInfo(
                                    Id: ogrenciId,
                                    OkulNo: okulNo,
                                    AdSoyad: adSoyad,
                                    Alan: alan,
                                    Dal: dal
                                ));
                            }
                        }
                    }
                    
                    if (ogrenciler.Count > 0)
                    {
                        var ay = new DateTime(baslangic.Year, baslangic.Month, 1);
                        var ayMetni = $"{ReportStyles.TrMonthUpper(ay.Month)}-{ay.Year}";
                        
                        // Tatil günlerini Çalışma Takvimi'nden al
                        var tatilGunleri = new HashSet<DateTime>();
                        string tatilQuery = @"
                            SELECT Baslangic, Bitis 
                            FROM CalismaTakvimi 
                            WHERE (Baslangic <= @Bitis AND Bitis >= @Baslangic)";
                        
                        using (var tatilCmd = new SQLiteCommand(tatilQuery, connection))
                        {
                            tatilCmd.Parameters.AddWithValue("@Baslangic", baslangic.ToString("yyyy-MM-dd"));
                            tatilCmd.Parameters.AddWithValue("@Bitis", bitis.ToString("yyyy-MM-dd"));
                            
                            using (var tatilReader = tatilCmd.ExecuteReader())
                            {
                                while (tatilReader.Read())
                                {
                                    var tatilBaslangic = DateTime.Parse(tatilReader["Baslangic"].ToString()!);
                                    var tatilBitis = DateTime.Parse(tatilReader["Bitis"].ToString()!);
                                    
                                    // Tatil aralığındaki tüm günleri ekle
                                    for (var tarih = tatilBaslangic; tarih <= tatilBitis; tarih = tarih.AddDays(1))
                                    {
                                        tatilGunleri.Add(tarih.Date);
                                    }
                                }
                            }
                        }
                        
                        // Devamsızlık verilerini hazırla
                        var devamsizlikMap = new Dictionary<int, Dictionary<int, string>>();
                        var ogrenciGunleriMap = new Dictionary<int, string>();
                        
                        // Her öğrenci için çalışma günlerini ve devamsızlık bilgilerini al
                        foreach (var ogr in ogrenciler)
                        {
                            devamsizlikMap[ogr.Id] = new Dictionary<int, string>();
                            
                            // Öğrencinin çalışma günlerini al
                            string ogrGunQuery = @"
                                SELECT o.Gunler, k.Gun 
                                FROM Ogrenciler o
                                LEFT JOIN KoordinatorTanimlama k ON (o.Koordinator = CAST(k.Id AS TEXT) OR o.Koordinator = k.Id)
                                WHERE o.Id = @OgrId";
                            
                            using (var gunCmd = new SQLiteCommand(ogrGunQuery, connection))
                            {
                                gunCmd.Parameters.AddWithValue("@OgrId", ogr.Id);
                                using (var gunReader = gunCmd.ExecuteReader())
                                {
                                    if (gunReader.Read())
                                    {
                                        var ogrGunler = gunReader["Gunler"]?.ToString() ?? "";
                                        var koordinatorGun = gunReader["Gun"]?.ToString() ?? "";
                                        
                                        // Önce öğrencinin kendi günlerini, yoksa koordinatörün gününü kullan
                                        var gunler = !string.IsNullOrEmpty(ogrGunler) ? ogrGunler : koordinatorGun;
                                        ogrenciGunleriMap[ogr.Id] = gunler;
                                        
                                        // Çalışma günlerini belirle
                                        var calismaGunleri = new HashSet<DayOfWeek>();
                                        ParseGunler(gunler, calismaGunleri);
                                        
                                        // Tarih aralığındaki her gün için sembol belirle
                                        for (var tarih = baslangic; tarih <= bitis; tarih = tarih.AddDays(1))
                                        {
                                            int gun = tarih.Day;
                                            string sembol = "";
                                            
                                            // Önce Devamsizlik tablosundan kontrol et
                                            string devQuery = @"
                                                SELECT Sembol FROM Devamsizlik 
                                                WHERE OgrenciId = @OgrId AND Tarih = @Tarih";
                                            
                                            using (var devCmd = new SQLiteCommand(devQuery, connection))
                                            {
                                                devCmd.Parameters.AddWithValue("@OgrId", ogr.Id);
                                                devCmd.Parameters.AddWithValue("@Tarih", tarih.ToString("yyyy-MM-dd"));
                                                var devResult = devCmd.ExecuteScalar();
                                                
                                                if (devResult != null)
                                                {
                                                    sembol = devResult.ToString() ?? "";
                                                    if (!string.IsNullOrEmpty(sembol) && !IsSymbolEnabled(sembol))
                                                        sembol = "";
                                                }
                                            }
                                            
                                            // Devamsizlik tablosunda yoksa otomatik belirle
                                            if (string.IsNullOrEmpty(sembol))
                                            {
                                                // Çalışma Takvimi'nden gelen tatil günü ise T
                                                if (tatilGunleri.Contains(tarih.Date))
                                                {
                                                    sembol = IsSymbolEnabled("T") ? "T" : "";
                                                }
                                                // Öğrencinin çalışma günü ise X
                                                else if (calismaGunleri.Contains(tarih.DayOfWeek))
                                                {
                                                    sembol = IsSymbolEnabled("X") ? "X" : "";
                                                }
                                                // Hafta içi ama çalışma günü değilse O (okulda)
                                                else if (tarih.DayOfWeek != DayOfWeek.Saturday && tarih.DayOfWeek != DayOfWeek.Sunday)
                                                {
                                                    sembol = IsSymbolEnabled("O") ? "O" : "";
                                                }
                                            }
                                            
                                            devamsizlikMap[ogr.Id][gun] = sembol;
                                        }
                                    }
                                }
                            }
                        }
                        
                        raporlar.Add(new RaporGirdisi
                        {
                            Okul = new SchoolInfo("", okulAdi, "", mudurYardimcisi),
                            Isletme = new BusinessInfo(isletmeAdi, isletmeTelefon, ""),
                            Ay = ay,
                            AyMetni = ayMetni,
                            BelgeTarihi = dpBelgeTarihi.SelectedDate?.ToString("dd.MM.yyyy") ?? "",
                            Ogrenciler = ogrenciler,
                            DevamsizlikMap = devamsizlikMap,
                            OkulGunu = null,
                            OgrenciGunleri = ogrenciGunleriMap
                        });
                    }
                }
            }
            
            if (raporlar.Count == 0)
            {
                throw new Exception("Seçili işletmelerde öğrenci bulunamadı!");
            }
            
            if (isHtml) return System.Text.Encoding.UTF8.GetBytes(HtmlReportGenerator.GenerateDevamsizlikHtml(raporlar));
            return DevamsizlikRaporu.OlusturPdf(raporlar);
        }

        private byte[] OlusturGunlukRehberlikPdf(List<DataItem> isletmeler, DateTime baslangic, DateTime bitis, bool isHtml = false)
        {
            var girdiler = new List<GunlukRehberlikGorevGirdisi>();

            var nup = GetSelectedNup();

            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();

                foreach (var isletme in isletmeler)
                {
                    var isletmeAdi = isletme.Data?.ToString() ?? "";
                    string query = @"
                        SELECT DISTINCT
                            k.Id as KoordinatorId,
                            k.Okul,
                            k.Ogretmen,
                            k.MudurYrd,
                            k.Gun
                        FROM KoordinatorTanimlama k
                        WHERE k.Isletme = @IsletmeAdi
                        ORDER BY k.Ogretmen";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IsletmeAdi", isletmeAdi);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var koordinatorId = Convert.ToInt32(reader["KoordinatorId"]);
                                var okulAdi = reader["Okul"]?.ToString() ?? "";
                                var ogretmen = reader["Ogretmen"]?.ToString() ?? "";
                                var mudurYrd = reader["MudurYrd"]?.ToString() ?? "";
                                var gun = reader["Gun"]?.ToString() ?? "";

                                var ogrenciSayisi = 0;
                                var alanDalListesi = new List<string>();

                                string ogrenciQuery = @"
                                    SELECT AlanDal
                                    FROM Ogrenciler
                                    WHERE Koordinator = @KoordinatorId";

                                using (var ogrenciCmd = new SQLiteCommand(ogrenciQuery, connection))
                                {
                                    ogrenciCmd.Parameters.AddWithValue("@KoordinatorId", koordinatorId.ToString());

                                    using (var ogrReader = ogrenciCmd.ExecuteReader())
                                    {
                                        while (ogrReader.Read())
                                        {
                                            ogrenciSayisi++;
                                            var alanDal = ogrReader["AlanDal"]?.ToString() ?? "";
                                            if (!string.IsNullOrWhiteSpace(alanDal) && !alanDalListesi.Contains(alanDal))
                                                alanDalListesi.Add(alanDal);
                                        }
                                    }
                                }

                                if (ogrenciSayisi == 0)
                                    continue;

                                var schoolDay = ParseSchoolDay(gun);
                                if (!schoolDay.HasValue)
                                    continue;

                                 for (var tarih = baslangic.Date; tarih <= bitis.Date; tarih = tarih.AddDays(1))
                                 {
                                     if (tarih.DayOfWeek != schoolDay.Value)
                                         continue;

                                     girdiler.Add(new GunlukRehberlikGorevGirdisi(
                                         IsletmeAdi: isletmeAdi,
                                         OgrenciSayisi: ogrenciSayisi,
                                         AlanDal: string.Join(", ", alanDalListesi),
                                         GorevTarihi: tarih,
                                         KoordinatorOgretmen: ogretmen,
                                         KoordinatorMudurYrd: mudurYrd,
                                         SchoolName: okulAdi,
                                         BelgeTarihi: dpBelgeTarihi.SelectedDate?.ToString("dd.MM.yyyy") ?? "",
                                         Sorun: txtGunlukSorun.Text,
                                         Cozum: txtGunlukCozum.Text,
                                         Sonuc: txtGunlukSonuc.Text
                                     ));
                                 }
                            }
                        }
                    }
                }
            }

            if (girdiler.Count == 0)
                throw new Exception("Seçilen tarih aralığında günlük rehberlik görev günü bulunamadı!");

            if (isHtml) return System.Text.Encoding.UTF8.GetBytes(HtmlReportGenerator.GenerateGunlukRehberlikHtml(girdiler));
            return GunlukRehberlikRaporu.OlusturPdf(girdiler, nup);
        }

        private DayOfWeek? ParseSchoolDay(string gun)
        {
            if (string.IsNullOrWhiteSpace(gun))
                return null;

            var g = gun.Trim().ToLowerInvariant();

            if (g.Contains("pzt") || g.Contains("pazartesi") || g == "1") return DayOfWeek.Monday;
            if (g.Contains("sal") || g.Contains("salı") || g == "2") return DayOfWeek.Tuesday;
            if (g.Contains("çar") || g.Contains("car") || g.Contains("çarşamba") || g == "3") return DayOfWeek.Wednesday;
            if (g.Contains("per") || g.Contains("perşembe") || g == "4") return DayOfWeek.Thursday;
            if (g.Contains("cum") || g.Contains("cuma") || g == "5") return DayOfWeek.Friday;
            if (g.Contains("cmt") || g.Contains("cumartesi") || g == "6") return DayOfWeek.Saturday;
            if (g.Contains("paz") || g == "7") return DayOfWeek.Sunday;

            return null;
        }

        private byte[] OlusturAylikRehberlikPdf(List<DataItem> isletmeler, DateTime baslangic, DateTime bitis, bool isHtml = false)
        {
            var girdiler = new List<AylikRehberlikGirdisi>();
            var nup = GetSelectedNup();
            var autoDoldur = chkAylikAutoDoldur?.IsChecked == true;
            bool? ustaBelgesiVar = autoDoldur ? (chkUstaBelgesi?.IsChecked == true) : null;

            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();

                foreach (var isletme in isletmeler)
                {
                    var isletmeAdi = isletme.Data?.ToString() ?? "";

                    const string koordinatorQuery = @"
                        SELECT DISTINCT
                            k.Id as KoordinatorId,
                            k.Okul,
                            k.Ogretmen,
                            k.MudurYrd,
                            k.Gun,
                            i.Adres as IsletmeAdres
                        FROM KoordinatorTanimlama k
                        LEFT JOIN Isletmeler i ON i.IsletmeAdi = k.Isletme
                        WHERE k.Isletme = @IsletmeAdi
                        ORDER BY k.Ogretmen";

                    using (var koordinatorCmd = new SQLiteCommand(koordinatorQuery, connection))
                    {
                        koordinatorCmd.Parameters.AddWithValue("@IsletmeAdi", isletmeAdi);

                        using (var koordinatorReader = koordinatorCmd.ExecuteReader())
                        {
                            while (koordinatorReader.Read())
                            {
                                var koordinatorId = Convert.ToInt32(koordinatorReader["KoordinatorId"]);
                                var okulAdi = koordinatorReader["Okul"]?.ToString() ?? "";
                                var ogretmen = koordinatorReader["Ogretmen"]?.ToString() ?? "";
                                var mudurYrd = koordinatorReader["MudurYrd"]?.ToString() ?? "";
                                var gunText = koordinatorReader["Gun"]?.ToString() ?? "";
                                var isletmeAdres = koordinatorReader["IsletmeAdres"]?.ToString() ?? "";

                                var schoolDay = ParseSchoolDay(gunText);
                                if (!schoolDay.HasValue)
                                    continue;

                                var ziyaretTarihleri = Enumerable
                                    .Range(0, (bitis.Date - baslangic.Date).Days + 1)
                                    .Select(offset => baslangic.Date.AddDays(offset))
                                    .Where(tarih => tarih.DayOfWeek == schoolDay.Value)
                                    .ToList();

                                if (ziyaretTarihleri.Count == 0)
                                    continue;

                                const string okulIlQuery = @"
                                    SELECT Il
                                    FROM OkulKoordinatorler
                                    WHERE OkulAdi = @OkulAdi
                                    LIMIT 1";

                                string okulIl;
                                using (var ilCmd = new SQLiteCommand(okulIlQuery, connection))
                                {
                                    ilCmd.Parameters.AddWithValue("@OkulAdi", okulAdi);
                                    okulIl = ilCmd.ExecuteScalar()?.ToString() ?? "";
                                }

                                var ogrenciler = new List<string>();
                                var alanDalSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                                const string ogrenciQuery = @"
                                    SELECT OkulNo, AdSoyad, AlanDal
                                    FROM Ogrenciler
                                    WHERE Koordinator = @KoordinatorId
                                       OR Koordinator = CAST(@KoordinatorId AS TEXT)
                                    ORDER BY AdSoyad";

                                using (var ogrenciCmd = new SQLiteCommand(ogrenciQuery, connection))
                                {
                                    ogrenciCmd.Parameters.AddWithValue("@KoordinatorId", koordinatorId.ToString());

                                    using (var ogrenciReader = ogrenciCmd.ExecuteReader())
                                    {
                                        while (ogrenciReader.Read())
                                        {
                                            var okulNo = ogrenciReader["OkulNo"]?.ToString() ?? "";
                                            var adSoyad = ogrenciReader["AdSoyad"]?.ToString() ?? "";
                                            var alanDal = ogrenciReader["AlanDal"]?.ToString() ?? "";

                                            if (!string.IsNullOrWhiteSpace(adSoyad))
                                                ogrenciler.Add($"{okulNo} {adSoyad}".Trim());

                                            if (!string.IsNullOrWhiteSpace(alanDal))
                                                alanDalSet.Add(alanDal.Trim());
                                        }
                                    }
                                }

                                if (ogrenciler.Count == 0)
                                    continue;

                                girdiler.Add(new AylikRehberlikGirdisi(
                                    OkulAdi: okulAdi,
                                    OkulIl: okulIl,
                                    IsletmeAdiAdres: string.IsNullOrWhiteSpace(isletmeAdres) ? isletmeAdi : $"{isletmeAdi} / {isletmeAdres}",
                                    AlanDal: alanDalSet.Count == 0 ? "-" : string.Join(", ", alanDalSet),
                                    GorevliGun: TrGun(schoolDay.Value),
                                    ZiyaretTarihleri: ziyaretTarihleri,
                                    Ogrenciler: ogrenciler,
                                    KoordinatorOgretmen: ogretmen,
                                    KoordinatorMudurYrd: mudurYrd,
                                    AyText: $"{ReportStyles.TrMonthUpper(baslangic.Month)}-{baslangic.Year}",
                                    AutoDoldur: autoDoldur,
                                    UstaBelgesiVar: ustaBelgesiVar
                                ));
                            }
                        }
                    }
                }
            }

            if (girdiler.Count == 0)
                throw new Exception("Seçilen tarih aralığında aylık rehberlik için uygun kayıt bulunamadı!");

            if (isHtml) return System.Text.Encoding.UTF8.GetBytes(HtmlReportGenerator.GenerateAylikRehberlikHtml(girdiler));
            return AylikRehberlikRaporu.OlusturPdf(girdiler, nup);
        }

        private byte[] OlusturMazeretDilekcesiPdf(List<DataItem> selectedItems, bool isHtml = false)
        {
            if (selectedItems.Count != 1)
                throw new Exception("Mazeret dilekçesi için tek öğrenci seçiniz.");

            var secilen = selectedItems[0];

            using var connection = DatabaseManager.GetConnection();
            connection.Open();

            const string query = @"
                SELECT
                    o.Id,
                    o.OkulNo,
                    o.AdSoyad,
                    o.Sinif,
                    COALESCE(k.Okul, '') AS OkulAdi,
                    COALESCE(k.Isletme, '') AS IsletmeAdi,
                    COALESCE(i.Adres, '') AS IsletmeAdres,
                    COALESCE(i.Telefon, '') AS IsletmeTelefon,
                    '' AS IsletmeEposta,
                    COALESCE(ok.OkulMuduru, '') AS OkulMuduru
                FROM Ogrenciler o
                LEFT JOIN KoordinatorTanimlama k
                    ON (o.Koordinator = CAST(k.Id AS TEXT) OR o.Koordinator = k.Id)
                LEFT JOIN Isletmeler i
                    ON i.IsletmeAdi = k.Isletme
                LEFT JOIN OkulKoordinatorler ok
                    ON ok.OkulAdi = k.Okul
                WHERE o.Id = @OgrenciId
                LIMIT 1";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@OgrenciId", secilen.Id);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                throw new Exception("Seçilen öğrenciye ait mazeret dilekçesi verisi bulunamadı.");

            var okulAdi = reader["OkulAdi"]?.ToString() ?? "";
            var isletmeAdi = reader["IsletmeAdi"]?.ToString() ?? "";
            var isletmeAdres = reader["IsletmeAdres"]?.ToString() ?? "";
            var isletmeTelefon = reader["IsletmeTelefon"]?.ToString() ?? "";
            var isletmeEposta = reader["IsletmeEposta"]?.ToString() ?? "";
            var sinif = reader["Sinif"]?.ToString() ?? "";
            var okulNo = reader["OkulNo"]?.ToString() ?? "";
            var adSoyad = reader["AdSoyad"]?.ToString() ?? "";
            var okulMuduru = reader["OkulMuduru"]?.ToString() ?? "";
            var izinGun = (txtMazeretIzinGun.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(izinGun))
                izinGun = "......";

            var input = new MazeretDilekcesiRaporu.MazeretDilekceGirdi(
                OkulAdi: okulAdi,
                IsletmeAdi: isletmeAdi,
                IsletmeAdresi: isletmeAdres,
                IsletmeTelefon: isletmeTelefon,
                IsletmeEposta: isletmeEposta,
                OgrenciSinif: sinif,
                OgrenciNo: okulNo,
                OgrenciAdSoyad: adSoyad,
                IzinGun: izinGun,
                OkulMuduru: okulMuduru,
                BelgeTarihi: dpBelgeTarihi.SelectedDate ?? DateTime.Today
            );

            if (isHtml) return System.Text.Encoding.UTF8.GetBytes(HtmlReportGenerator.GenerateMazeretDilekcesiHtml(input));
            return MazeretDilekcesiRaporu.OlusturPdf(input);
        }

        private byte[] OlusturFesihPdf(List<DataItem> selectedItems, bool isHtml = false)
        {
            var bosSablon = chkFesihBosSablon?.IsChecked == true;

            SchoolInfo okul;
            BusinessInfo isletme;
            StudentInfo ogrenci;
            string sinif;
            string koordinatorOgretmen;

            if (bosSablon)
            {
                okul = new SchoolInfo("", "", "", "");
                isletme = new BusinessInfo("", "", "");
                ogrenci = new StudentInfo(0, "", "", "", "");
                sinif = "";
                koordinatorOgretmen = "";
            }
            else
            {
                if (selectedItems.Count == 0)
                    throw new Exception("Lütfen öğrenci seçin ya da 'Formu boş oluştur' seçeneğini işaretleyin.");

                var secilen = selectedItems[0];

                using var connection = DatabaseManager.GetConnection();
                connection.Open();

                const string query = @"
                    SELECT
                        o.Id,
                        o.OkulNo,
                        o.AdSoyad,
                        o.Sinif,
                        o.AlanDal,
                        COALESCE(k.Okul, '') AS OkulAdi,
                        COALESCE(k.Ogretmen, '') AS KoordinatorOgretmen,
                        COALESCE(k.Isletme, '') AS IsletmeAdi,
                        COALESCE(i.Telefon, '') AS IsletmeTelefon,
                        COALESCE(ok.Il, '') AS OkulIl,
                        COALESCE(ok.OkulMuduru, '') AS OkulMuduru,
                        COALESCE(ok.KoordOgretmen, '') AS OkulKoordOgretmen
                    FROM Ogrenciler o
                    LEFT JOIN KoordinatorTanimlama k
                        ON (o.Koordinator = CAST(k.Id AS TEXT) OR o.Koordinator = k.Id)
                    LEFT JOIN Isletmeler i
                        ON i.IsletmeAdi = k.Isletme
                    LEFT JOIN OkulKoordinatorler ok
                        ON ok.OkulAdi = k.Okul
                    WHERE o.Id = @OgrenciId
                    LIMIT 1";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@OgrenciId", secilen.Id);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                    throw new Exception("Seçilen öğrenci bilgisi bulunamadı.");

                var alanDal = reader["AlanDal"]?.ToString() ?? "";
                var alan = "";
                var dal = "";
                if (!string.IsNullOrWhiteSpace(alanDal))
                {
                    var parts = alanDal.Split(new[] { '-', '/' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    alan = parts.Length > 0 ? parts[0].Trim() : "";
                    dal = parts.Length > 1 ? parts[1].Trim() : "";
                }

                var okulAdi = reader["OkulAdi"]?.ToString() ?? "";
                var okulIl = reader["OkulIl"]?.ToString() ?? "";
                var okulMuduru = reader["OkulMuduru"]?.ToString() ?? "";
                var isletmeAdi = reader["IsletmeAdi"]?.ToString() ?? "";
                var isletmeTelefon = reader["IsletmeTelefon"]?.ToString() ?? "";
                var koordinator = reader["KoordinatorOgretmen"]?.ToString() ?? "";
                var okulKoord = reader["OkulKoordOgretmen"]?.ToString() ?? "";

                okul = new SchoolInfo(okulIl, okulAdi, okulMuduru, "");
                isletme = new BusinessInfo(isletmeAdi, isletmeTelefon, "");
                ogrenci = new StudentInfo(
                    secilen.Id,
                    reader["OkulNo"]?.ToString() ?? "",
                    reader["AdSoyad"]?.ToString() ?? "",
                    alan,
                    dal
                );
                sinif = reader["Sinif"]?.ToString() ?? "";
                koordinatorOgretmen = string.IsNullOrWhiteSpace(koordinator) ? okulKoord : koordinator;
            }

            var sozlesmeTarihi = dpFesihSozlesmeTarihi.SelectedDate;
            var iptalTarihi = dpFesihIptalTarihi.SelectedDate;
            var nedenler = (txtFesihIptalNedenleri.Text ?? string.Empty).Trim();
            var belgeTarihi = iptalTarihi ?? DateTime.Today;

            var input = new FesihRaporu.FesihGirdi(
                okul,
                isletme,
                ogrenci,
                sinif,
                koordinatorOgretmen,
                sozlesmeTarihi,
                iptalTarihi,
                nedenler,
                belgeTarihi,
                ""
            );

            if (isHtml) return System.Text.Encoding.UTF8.GetBytes(HtmlReportGenerator.GenerateFesihHtml(input));
            return FesihRaporu.OlusturPdf(input);
        }

        private byte[] OlusturNotCizelgesiPdf(List<DataItem> isletmeler, bool isHtml = false)
        {
            var girdiler = new List<NotCizelgesiRaporu.NotCizelgesiInput>();
            var sayfaDuzeni = cmbNotSayfaDuzeni?.SelectedItem as ComboBoxItem;
            var nup = (sayfaDuzeni?.Content?.ToString() ?? string.Empty).StartsWith("1 sayfaya 2") ? 2 : 1;
            var donem = (cmbNotDonem?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "1. Dönem";
            var belgeTarihi = dpBelgeTarihi?.SelectedDate ?? DateTime.Today;

            using var connection = DatabaseManager.GetConnection();
            connection.Open();

            foreach (var isletme in isletmeler)
            {
                var isletmeAdi = isletme.Data?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(isletmeAdi))
                    continue;

                const string koordinatorQuery = @"
                    SELECT DISTINCT
                        k.Id as KoordinatorId,
                        COALESCE(k.Okul, '') as OkulAdi,
                        COALESCE(k.Ogretmen, '') as KoordinatorOgretmen,
                        COALESCE(k.MudurYrd, '') as MudurYrd,
                        COALESCE(i.Telefon, '') as IsletmeTelefon
                    FROM KoordinatorTanimlama k
                    LEFT JOIN Isletmeler i ON i.IsletmeAdi = k.Isletme
                    WHERE k.Isletme = @IsletmeAdi
                    ORDER BY k.Ogretmen";

                using var koordinatorCmd = new SQLiteCommand(koordinatorQuery, connection);
                koordinatorCmd.Parameters.AddWithValue("@IsletmeAdi", isletmeAdi);

                using var koordinatorReader = koordinatorCmd.ExecuteReader();
                while (koordinatorReader.Read())
                {
                    var koordinatorId = Convert.ToInt32(koordinatorReader["KoordinatorId"]);
                    var okulAdi = koordinatorReader["OkulAdi"]?.ToString() ?? "";
                    var koordinatorOgretmen = koordinatorReader["KoordinatorOgretmen"]?.ToString() ?? "";
                    var mudurYrd = koordinatorReader["MudurYrd"]?.ToString() ?? "";
                    var isletmeTelefon = koordinatorReader["IsletmeTelefon"]?.ToString() ?? "";

                    var okulIl = "";
                    var okulMuduru = "";
                    if (!string.IsNullOrWhiteSpace(okulAdi))
                    {
                        const string okulQuery = @"
                            SELECT COALESCE(Il, ''), COALESCE(OkulMuduru, '')
                            FROM OkulKoordinatorler
                            WHERE OkulAdi = @OkulAdi
                            LIMIT 1";

                        using var okulCmd = new SQLiteCommand(okulQuery, connection);
                        okulCmd.Parameters.AddWithValue("@OkulAdi", okulAdi);

                        using var okulReader = okulCmd.ExecuteReader();
                        if (okulReader.Read())
                        {
                            okulIl = okulReader[0]?.ToString() ?? "";
                            okulMuduru = okulReader[1]?.ToString() ?? "";
                        }
                    }

                    const string ogrenciQuery = @"
                        SELECT
                            o.Id,
                            COALESCE(o.Sinif, '') as Sinif,
                            COALESCE(o.OkulNo, '') as OkulNo,
                            COALESCE(o.AdSoyad, '') as AdSoyad,
                            COALESCE(o.AlanDal, '') as AlanDal
                        FROM Ogrenciler o
                        WHERE o.Koordinator = @KoordinatorId
                           OR o.Koordinator = CAST(@KoordinatorId AS TEXT)
                        ORDER BY o.AdSoyad";

                    var ogrenciler = new List<NotCizelgesiRaporu.NotOgrenciInfo>();
                    using var ogrenciCmd = new SQLiteCommand(ogrenciQuery, connection);
                    ogrenciCmd.Parameters.AddWithValue("@KoordinatorId", koordinatorId.ToString());

                    using var ogrenciReader = ogrenciCmd.ExecuteReader();
                    while (ogrenciReader.Read())
                    {
                        var alanDal = ogrenciReader["AlanDal"]?.ToString() ?? "";
                        var alan = "";
                        var dal = "";
                        if (!string.IsNullOrWhiteSpace(alanDal))
                        {
                            var parts = alanDal.Split(new[] { '-', '/' }, 2, StringSplitOptions.RemoveEmptyEntries);
                            alan = parts.Length > 0 ? parts[0].Trim() : "";
                            dal = parts.Length > 1 ? parts[1].Trim() : "";
                        }

                        ogrenciler.Add(new NotCizelgesiRaporu.NotOgrenciInfo(
                            Id: Convert.ToInt32(ogrenciReader["Id"]),
                            Sinif: ogrenciReader["Sinif"]?.ToString() ?? "",
                            OkulNo: ogrenciReader["OkulNo"]?.ToString() ?? "",
                            AdSoyad: ogrenciReader["AdSoyad"]?.ToString() ?? "",
                            Alan: alan,
                            Dal: dal
                        ));
                    }

                    if (ogrenciler.Count == 0)
                        continue;

                    girdiler.Add(new NotCizelgesiRaporu.NotCizelgesiInput(
                        School: new SchoolInfo(okulIl, okulAdi, okulMuduru, mudurYrd),
                        Business: new BusinessInfo(isletmeAdi, isletmeTelefon, ""),
                        DonemText: donem,
                        Tarih: belgeTarihi,
                        KoordinatorOgretmen: koordinatorOgretmen,
                        KoordinatorMudurYrd: mudurYrd,
                        Students: ogrenciler
                    ));
                }
            }

            if (girdiler.Count == 0)
                throw new Exception("Seçilen işletmelerde not çizelgesi için öğrenci bulunamadı!");

            if (isHtml) return System.Text.Encoding.UTF8.GetBytes(HtmlReportGenerator.GenerateNotCizelgesiHtml(girdiler));
            return NotCizelgesiRaporu.OlusturPdf(girdiler, nup);
        }

        private static string TrGun(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Pazartesi",
                DayOfWeek.Tuesday => "Salı",
                DayOfWeek.Wednesday => "Çarşamba",
                DayOfWeek.Thursday => "Perşembe",
                DayOfWeek.Friday => "Cuma",
                DayOfWeek.Saturday => "Cumartesi",
                DayOfWeek.Sunday => "Pazar",
                _ => ""
            };
        }

        // ── Haftalık Görev Takip ──────────────────────────────────────── //
        // ── Haftalık Görev Takip — Boş Şablon ───────────────────────── //
        // -- Haftalık Görev Takip - DB Bağlantılı -------------------------//
        // -- Haftalık Görev Takip - DB Bağlantılı -------------------------//
        private byte[] OlusturHaftalikGorevPdf(List<DataItem> ogretmenler)
        {
            string egitimTuru = rbHaftalikMesem?.IsChecked == true ? "MESEM" : "ÖRGÜN";

            var seciliHaftalar = new List<string>();
            if (chkHaftalik1?.IsChecked == true) seciliHaftalar.Add("1.HFT");
            if (chkHaftalik2?.IsChecked == true) seciliHaftalar.Add("2.HFT");
            if (chkHaftalik3?.IsChecked == true) seciliHaftalar.Add("3.HFT");
            if (chkHaftalik4?.IsChecked == true) seciliHaftalar.Add("4.HFT");
            if (chkHaftalik5?.IsChecked == true) seciliHaftalar.Add("5.HFT");

            string ayMetni = (cmbHaftalikAy?.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "";
            if (ayMetni == "Lütfen bir ay seçin") ayMetni = "";
            string ogretimYili = txtHaftalikOgretimYili?.Text?.Trim() ?? "";

            // Seçim yoksa boş şablon üret
            if (ogretmenler.Count == 0)
            {
                var bosGirdiler = new List<HaftalikGorevGirdisi>();
                foreach (var hft in seciliHaftalar)
                {
                    bosGirdiler.Add(new HaftalikGorevGirdisi(
                        KoordinatorOgretmen: "",
                        MudurYrd:            "",
                        SchoolName:          "",
                        OgretimYili:         ogretimYili,
                        KoordTuru:           egitimTuru,
                        AyMetni:             ayMetni,
                        HaftaMetni:          hft,
                        Satirlar:            new List<HaftalikGorevSatiri> {
                            new HaftalikGorevSatiri("","",""), new HaftalikGorevSatiri("","",""),
                            new HaftalikGorevSatiri("","",""), new HaftalikGorevSatiri("","",""),
                            new HaftalikGorevSatiri("","","") 
                        }
                    ));
                }
                if (bosGirdiler.Count == 0) // hic hafta secilmemisse 1 tane bos uret
                {
                    bosGirdiler.Add(new HaftalikGorevGirdisi("", "", "", ogretimYili, egitimTuru, ayMetni, "", 
                        new List<HaftalikGorevSatiri> {
                            new HaftalikGorevSatiri("","",""), new HaftalikGorevSatiri("","",""),
                            new HaftalikGorevSatiri("","",""), new HaftalikGorevSatiri("","",""),
                            new HaftalikGorevSatiri("","","") 
                        }));
                }
                return HaftalikGorevTakipRaporu.OlusturPdf(bosGirdiler);
            }

            var girdiler = new List<HaftalikGorevGirdisi>();
            using var conn = DatabaseManager.GetConnection();
            conn.Open();

            foreach (var item in ogretmenler)
            {
                string ogretmenAdi = item.Data?.ToString() ?? item.DisplayText;

                // OkulKoordinatorler -> OkulAdi, KoordMudurYrd
                string okulAdi  = "";
                string mudurYrd = "";
                using (var cmd = new SQLiteCommand(
                    "SELECT OkulAdi, KoordMudurYrd FROM OkulKoordinatorler WHERE Id = @Id LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", item.Id);
                    using var r = cmd.ExecuteReader();
                    if (r.Read()) { okulAdi = r["OkulAdi"]?.ToString() ?? "";
                                    mudurYrd = r["KoordMudurYrd"]?.ToString() ?? ""; }
                }

                var satirlar = new List<HaftalikGorevSatiri>();

                // KoordinatorTanimlama -> Eğitim türüne göre filtrele (KoordTuru=@Tur)
                using (var cmd2 = new SQLiteCommand(@"
                    SELECT kt.Id AS KtId, kt.Isletme, kt.IsletmeYetkilisi, kt.Gun, kt.KoordTuru
                    FROM KoordinatorTanimlama kt
                    WHERE kt.Ogretmen = @Og AND kt.KoordTuru = @Tur", conn))
                {
                    cmd2.Parameters.AddWithValue("@Og", ogretmenAdi);
                    cmd2.Parameters.AddWithValue("@Tur", egitimTuru);
                    using var r2 = cmd2.ExecuteReader();
                    while (r2.Read())
                    {
                        string ktId    = r2["KtId"]?.ToString()    ?? "";
                        string isletme = r2["Isletme"]?.ToString() ?? "";
                        string gun     = r2["Gun"]?.ToString()     ?? "";
                        string yetkili = r2["IsletmeYetkilisi"]?.ToString() ?? "";

                        using var cmd3 = new SQLiteCommand(@"
                            SELECT o.AdSoyad FROM Ogrenciler o
                            WHERE CAST(o.Koordinator AS TEXT) = @KtId
                            ORDER BY o.AdSoyad", conn);
                        cmd3.Parameters.AddWithValue("@KtId", ktId);
                        using var r3 = cmd3.ExecuteReader();

                        bool hasOgr = false;
                        while (r3.Read())
                        {
                            hasOgr = true;
                            satirlar.Add(new HaftalikGorevSatiri(
                                IsletmeAdi:     isletme,
                                OgrenciAdSoyad: r3["AdSoyad"]?.ToString() ?? "",
                                UstaAdSoyad:    yetkili)); 
                        }
                        if (!hasOgr)
                            satirlar.Add(new HaftalikGorevSatiri(isletme, "", yetkili));
                    }
                }

                // Öğretmenin seçili eğitim türüne ait kaydı varsa HER HAFTA DÖNGÜSÜ KADAR form ekle
                if (satirlar.Count > 0)
                {
                    foreach (var hft in seciliHaftalar)
                    {
                        girdiler.Add(new HaftalikGorevGirdisi(
                            KoordinatorOgretmen: ogretmenAdi,
                            MudurYrd:            mudurYrd,
                            SchoolName:          okulAdi,
                            OgretimYili:         ogretimYili,
                            KoordTuru:           egitimTuru,
                            AyMetni:             ayMetni,
                            HaftaMetni:          hft,
                            Satirlar:            satirlar));
                    }
                }
            }

            if (girdiler.Count == 0)
                throw new Exception("Seçili öğretmen için seçilmiş eğitim türüne (" + egitimTuru + ") uygun kayıt bulunamadı.");

            return HaftalikGorevTakipRaporu.OlusturPdf(girdiler);
        }
    }
}
