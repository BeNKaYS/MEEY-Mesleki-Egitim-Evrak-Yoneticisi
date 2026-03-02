using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data.SQLite;
using MEEY.Controls;
using MEEY.Database;
using MEEY.Reports;

namespace MEEY
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Tam ekran başlat
            this.WindowState = WindowState.Maximized;
            
            // Başlangıçta mavi renk
            SetModul1Color("#3498DB", "#AED6F1");
            LoadProfiller();
            
            // Başlangıçta Veri Girişi'ni yükle
            VeriGirisiButton_Click(null!, null!);
        }

        private void LoadProfiller()
        {
            try
            {
                cmbProfil.Items.Clear();
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT ProfilAdi FROM Profiller ORDER BY Id";
                    
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbProfil.Items.Add(reader["ProfilAdi"].ToString());
                        }
                    }
                }
                
                if (cmbProfil.Items.Count > 0)
                    cmbProfil.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Profil yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnYeniProfil_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = "Yeni Profil",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            
            var stack = new System.Windows.Controls.StackPanel { Margin = new Thickness(20, 20, 20, 20) };
            var textBox = new System.Windows.Controls.TextBox { Height = 30, Margin = new Thickness(0, 10, 0, 10) };
            var btnKaydet = new System.Windows.Controls.Button 
            { 
                Content = "Kaydet", 
                Height = 35, 
                Margin = new Thickness(0, 10, 0, 10),
                Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0, 0, 0, 0)
            };
            
            btnKaydet.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageBox.Show("Profil adı boş olamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                try
                {
                    using (var connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();
                        string query = "INSERT INTO Profiller (ProfilAdi) VALUES (@ProfilAdi)";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ProfilAdi", textBox.Text);
                            command.ExecuteNonQuery();
                        }
                    }
                    
                    LoadProfiller();
                    dialog.Close();
                    MessageBox.Show("Profil oluşturuldu!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            
            stack.Children.Add(new System.Windows.Controls.TextBlock { Text = "Profil Adı:" });
            stack.Children.Add(textBox);
            stack.Children.Add(btnKaydet);
            dialog.Content = stack;
            dialog.ShowDialog();
        }

        private void BtnDuzenleProfil_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProfil.SelectedItem == null)
            {
                MessageBox.Show("Lütfen düzenlenecek profili seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            string eskiAd = cmbProfil.SelectedItem.ToString();
            
            if (eskiAd == "Varsayılan")
            {
                MessageBox.Show("Varsayılan profil düzenlenemez!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var dialog = new Window
            {
                Title = "Profil Düzenle",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            
            var stack = new System.Windows.Controls.StackPanel { Margin = new Thickness(20, 20, 20, 20) };
            var textBox = new System.Windows.Controls.TextBox { Height = 30, Margin = new Thickness(0, 10, 0, 10), Text = eskiAd };
            var btnKaydet = new System.Windows.Controls.Button 
            { 
                Content = "Kaydet", 
                Height = 35, 
                Margin = new Thickness(0, 10, 0, 10),
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0, 0, 0, 0)
            };
            
            btnKaydet.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageBox.Show("Profil adı boş olamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                try
                {
                    using (var connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();
                        string query = "UPDATE Profiller SET ProfilAdi = @YeniAd WHERE ProfilAdi = @EskiAd";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@YeniAd", textBox.Text);
                            command.Parameters.AddWithValue("@EskiAd", eskiAd);
                            command.ExecuteNonQuery();
                        }
                    }
                    
                    LoadProfiller();
                    dialog.Close();
                    MessageBox.Show("Profil güncellendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            
            stack.Children.Add(new System.Windows.Controls.TextBlock { Text = "Yeni Profil Adı:" });
            stack.Children.Add(textBox);
            stack.Children.Add(btnKaydet);
            dialog.Content = stack;
            dialog.ShowDialog();
        }

        private void BtnSilProfil_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProfil.SelectedItem == null)
            {
                MessageBox.Show("Lütfen silinecek profili seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            string profilAdi = cmbProfil.SelectedItem.ToString();
            
            if (profilAdi == "Varsayılan")
            {
                MessageBox.Show("Varsayılan profil silinemez!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var result = MessageBox.Show($"'{profilAdi}' profilini silmek istediğinizden emin misiniz?", 
                                        "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();
                        string query = "DELETE FROM Profiller WHERE ProfilAdi = @ProfilAdi";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ProfilAdi", profilAdi);
                            command.ExecuteNonQuery();
                        }
                    }
                    
                    LoadProfiller();
                    MessageBox.Show("Profil silindi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnKaydetProfil_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Profil ayarları kaydedildi!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnYenileProfil_Click(object sender, RoutedEventArgs e)
        {
            LoadProfiller();
            MessageBox.Show("Profiller yenilendi!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetModul1Color(string mainColorHex, string lightColorHex)
        {
            var baseColor = (Color)ColorConverter.ConvertFromString(mainColorHex);

            Modul1Border.Background = new SolidColorBrush(baseColor);
            Modul2Border.Background = new SolidColorBrush(WithAlpha(LightenColor(baseColor, 0.22), 51));
            Modul3Border.Background = new SolidColorBrush(WithAlpha(LightenColor(baseColor, 0.38), 51));
            Modul4Border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6FFFFFF"));
        }

        private static Color LightenColor(Color color, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));

            byte channel(byte c) => (byte)(c + ((255 - c) * amount));

            return Color.FromRgb(channel(color.R), channel(color.G), channel(color.B));
        }

        private static Color WithAlpha(Color color, byte alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        private void ApplyDefaultLayout()
        {
            Modul2Border.Visibility = Visibility.Visible;
            Grid.SetRow(Modul4Border, 1);
            Grid.SetRowSpan(Modul4Border, 1);
        }

        private void ApplyMetinEditorLayout()
        {
            Modul2Border.Visibility = Visibility.Collapsed;
            Grid.SetRow(Modul4Border, 0);
            Grid.SetRowSpan(Modul4Border, 2);
        }

        private void VeriGirisiButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyDefaultLayout();
            Modul1ProfilPanel.Visibility = Visibility.Visible;

            // Modül 1 - Mavi renk
            Modul1Baslik.Text = "Veri Girişi";
            SetModul1Color("#3498DB", "#AED6F1");
            
            // Modül 2
            Modul2Baslik.Text = "Veri Girişi";
            Modul2Icerik.Text = "Veri girişi modüllerini kullanarak okul, işletme, öğrenci ve diğer bilgileri yönetin.";
            
            // Modül 3 - Veri Girişi menüsünü göster
            Modul3Icerik.Children.Clear();
            Modul3Icerik.Visibility = Visibility.Visible;
            
            // Veri Girişi Başlık
            var baslik = new TextBlock 
            { 
                Text = "Veritabanı Tabloları", 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
            };
            Modul3Icerik.Children.Add(baslik);
            
            // Okul ve Koordinatörler
            AddVeriGirisiButton("🎓 Okul ve Koordinatörler", "OkulKoordinatorler");
            
            // İşletmeler
            AddVeriGirisiButton("💼 İşletmeler", "Isletmeler");
            
            // Alan / Dal
            AddVeriGirisiButton("🏷️ Alan / Dal", "AlanDal");
            
            // Koordinatör Tanımlama
            AddVeriGirisiButton("⚙️ Koordinatör Tanımlama", "KoordinatorTanimlama");
            
            // Öğrenciler
            AddVeriGirisiButton("👥 Öğrenciler", "Ogrenciler");
            
            // Çalışma Takvimi
            AddVeriGirisiButton("📅 Çalışma Takvimi", "CalismaTakvimi");
            
            // Modül 4 - Varsayılan görünüm
            Modul4Container.Children.Clear();
            Modul4Container.Children.Add(Modul4Default);
            Modul4Baslik.Text = "Veri Girişi";
            Modul4Icerik.Text = "Sol menüden bir modül seçin.";
        }
        
        private void AddVeriGirisiButton(string text, string moduleName)
        {
            var btn = new Button
            {
                Height = 40,
                Margin = new Thickness(0, 2, 0, 2),
                Background = Brushes.White,
                BorderThickness = new Thickness(1, 1, 1, 1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(189, 195, 199)),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 0, 10, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Content = new TextBlock { Text = text, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)) }
            };
            
            btn.Click += (s, e) => 
            {
                switch (moduleName)
                {
                    case "OkulKoordinatorler":
                        MenuOkulKoordinatorler_Click(s, e);
                        break;
                    case "Isletmeler":
                        MenuIsletmeler_Click(s, e);
                        break;
                    case "AlanDal":
                        MenuAlanDal_Click(s, e);
                        break;
                    case "KoordinatorTanimlama":
                        MenuKoordinatorTanimlama_Click(s, e);
                        break;
                    case "Ogrenciler":
                        MenuOgrenciler_Click(s, e);
                        break;
                    case "CalismaTakvimi":
                        MenuCalismaTakvimi_Click(s, e);
                        break;
                }
            };
            
            Modul3Icerik.Children.Add(btn);
        }

        private VeriTabaniYonetimi? veriTabaniControl = null;

        private void Modul2Button_Click(object sender, RoutedEventArgs e)
        {
            ApplyDefaultLayout();
            Modul1ProfilPanel.Visibility = Visibility.Visible;

            // Modül 1 - Yeşil renk
            Modul1Baslik.Text = "Veritabanı Yönetimi";
            SetModul1Color("#27AE60", "#A9DFBF");
            
            // Modül 2
            Modul2Baslik.Text = "Veritabanı Yönetimi";
            Modul2Icerik.Text = "Tüm veritabanı tablolarını görüntüleyin, Excel'e aktarın, içe aktarın ve yedekleyin.";
            
            // Modül 3 - Veritabanı menüsünü göster
            Modul3Icerik.Children.Clear();
            Modul3Icerik.Visibility = Visibility.Visible;
            
            // Veritabanı Tabloları Başlık
            var baslik = new TextBlock 
            { 
                Text = "Veritabanı Tabloları", 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
            };
            Modul3Icerik.Children.Add(baslik);
            
            // Okul ve Koordinatörler
            AddDatabaseButton("🎓 Okul ve Koordinatörler", "OkulKoordinatorler", "Okul ve Koordinatörler");
            
            // İşletmeler
            AddDatabaseButton("💼 İşletmeler", "Isletmeler", "İşletmeler");
            
            // Alan / Dal
            AddDatabaseButton("🏷️ Alan / Dal", "AlanDal", "Alan / Dal");
            
            // Koordinatör Tanımlama
            AddDatabaseButton("⚙️ Koordinatör Tanımlama", "KoordinatorTanimlama", "Koordinatör Tanımlama");
            
            // Öğrenciler
            AddDatabaseButton("👥 Öğrenciler", "Ogrenciler", "Öğrenciler");
            
            // Çalışma Takvimi
            AddDatabaseButton("📅 Çalışma Takvimi", "CalismaTakvimi", "Çalışma Takvimi");
            
            // Araçlar Başlık
            var araclarBaslik = new TextBlock 
            { 
                Text = "Araçlar", 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 15, 0, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
            };
            Modul3Icerik.Children.Add(araclarBaslik);
            
            // Excel'e Aktar
            var btnExcel = new Button
            {
                Height = 40,
                Margin = new Thickness(0, 2, 0, 2),
                Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)),
                BorderThickness = new Thickness(0, 0, 0, 0),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 0, 10, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Content = new TextBlock { Text = "📊 Excel'e Aktar", FontSize = 12, Foreground = Brushes.White }
            };
            btnExcel.Click += (s, args) => { veriTabaniControl?.ExportToExcel(); };
            Modul3Icerik.Children.Add(btnExcel);

            // Veritabanı İçe Aktar
            var btnIceAktar = new Button
            {
                Height = 40,
                Margin = new Thickness(0, 2, 0, 2),
                Background = new SolidColorBrush(Color.FromRgb(243, 156, 18)),
                BorderThickness = new Thickness(0, 0, 0, 0),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 0, 10, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Content = new TextBlock { Text = "📥 Veritabanı İçe Aktar", FontSize = 12, Foreground = Brushes.White }
            };
            btnIceAktar.Click += (s, args) => { veriTabaniControl?.ImportDatabase(); };
            Modul3Icerik.Children.Add(btnIceAktar);
            
            // Veritabanını Yedekle
            var btnYedek = new Button
            {
                Height = 40,
                Margin = new Thickness(0, 2, 0, 2),
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                BorderThickness = new Thickness(0, 0, 0, 0),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 0, 10, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Content = new TextBlock { Text = "💾 Veritabanını Yedekle", FontSize = 12, Foreground = Brushes.White }
            };
            btnYedek.Click += (s, args) => { veriTabaniControl?.BackupDatabase(); };
            Modul3Icerik.Children.Add(btnYedek);
            
            // Modül 4 - Veritabanı Yönetimi kontrolünü yükle
            Modul4Container.Children.Clear();
            veriTabaniControl = new VeriTabaniYonetimi();
            Modul4Container.Children.Add(veriTabaniControl);
        }

        private void AddDatabaseButton(string text, string tableName, string displayName)
        {
            var btn = new Button
            {
                Height = 40,
                Margin = new Thickness(0, 2, 0, 2),
                Background = Brushes.White,
                BorderThickness = new Thickness(1, 1, 1, 1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(189, 195, 199)),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 0, 10, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Content = new TextBlock { Text = text, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)) }
            };
            btn.Click += (s, e) => { veriTabaniControl?.LoadTableData(tableName, displayName); };
            Modul3Icerik.Children.Add(btn);
        }

        private void Modul3Button_Click(object sender, RoutedEventArgs e)
        {
            ApplyDefaultLayout();
            Modul1ProfilPanel.Visibility = Visibility.Visible;

            // Modül 1 - Mor renk
            Modul1Baslik.Text = "Raporlar";
            SetModul1Color("#9B59B6", "#D7BDE2");
            
            // Modül 2
            Modul2Baslik.Text = "Raporlar";
            Modul2Icerik.Text = "Çeşitli raporları oluşturun ve görüntüleyin.";
            
            // Modül 3 - Rapor menüsünü göster
            Modul3Icerik.Children.Clear();
            Modul3Icerik.Visibility = Visibility.Visible;
            
            // Raporlar Başlık
            var baslik = new TextBlock 
            { 
                Text = "Rapor Türleri", 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
            };
            Modul3Icerik.Children.Add(baslik);
            
            // Devamsızlık
            AddReportButton("📋 Devamsızlık", "Devamsizlik");
            
            // Günlük Rehberlik
            AddReportButton("📅 Günlük Rehberlik", "GunlukRehberlik");
            
            // Aylık Rehberlik
            AddReportButton("📆 Aylık Rehberlik", "AylikRehberlik");
            
            // Fesih (İşten Çıkış)
            AddReportButton("📄 Fesih (İşten Çıkış)", "Fesih");
            
            // Mazeret Dilekçesi
            AddReportButton("📝 Mazeret Dilekçesi", "MazeretDilekcesi");
            
            // Not Çizelgesi
            AddReportButton("📊 Not Çizelgesi", "NotCizelgesi");
            
            // Haftalık Görev Takip
            AddReportButton("🗓️ Haftalık Görev Takip", "HaftalikGorevTakip");
            
            // Modül 4
            Modul4Container.Children.Clear();
            Modul4Container.Children.Add(Modul4Default);
            Modul4Baslik.Text = "Rapor Seçin";
            Modul4Icerik.Text = "Sol menüden bir rapor türü seçin.";
        }

        private void AddReportButton(string text, string reportType)
        {
            var btn = new Button
            {
                Height = 40,
                Margin = new Thickness(0, 2, 0, 2),
                Background = Brushes.White,
                BorderThickness = new Thickness(1, 1, 1, 1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(189, 195, 199)),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 0, 10, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Content = new TextBlock { Text = text, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)) }
            };
            
            btn.Click += (s, e) => { LoadReport(reportType); };
            Modul3Icerik.Children.Add(btn);
        }

        private void LoadReport(string reportType)
        {
            if (reportType == "Devamsizlik")
            {
                Modul2Baslik.Text = "Devamsızlık";
                Modul2Icerik.Text = "İşletme bazlı devamsızlık raporu oluşturulur. Tarih aralığı ve belge tarihi seçilir, Rapor Sevk Gir ile manuel semboller girilir, X/O/İ/H/R/T kutuları ile raporda hangi sembollerin gösterileceği belirlenir ve PDF önizleme panelde açılır.";
            }
            else if (reportType == "GunlukRehberlik")
            {
                Modul2Baslik.Text = "Günlük Rehberlik";
                Modul2Icerik.Text = "Günlük rehberlik raporu işletme bazlı oluşturulur. Sol üstte veri seçimi yapılır, sol alttan tarih aralığı ile belge tarihi girilir, sağ tarafta oluşturulan PDF rapor önizlenir.";
            }
            else if (reportType == "AylikRehberlik")
            {
                Modul2Baslik.Text = "Aylık Rehberlik";
                Modul2Icerik.Text = "Aylık rehberlik raporu işletme ve koordinatör bazlı oluşturulur. Tarih aralığındaki görev günleri otomatik hesaplanır, öğrenci ve alan/dal bilgileri ile birlikte PDF önizleme panelinde gösterilir.";
            }
            else if (reportType == "Fesih")
            {
                Modul2Baslik.Text = "Fesih (İşten Çıkış)";
                Modul2Icerik.Text = "Fesih raporu öğrenci bazlıdır. Sol üst veri listesinden tek bir öğrenci seçilir. Sol alttaki Fesih ayarlarında 'Formu boş oluştur (şablon)' seçeneği, sözleşme tarihi, sözleşme iptal tarihi ve iptal nedenleri girilir. Rapor Oluştur ile sözleşme iptal tutanağı PDF olarak sağ panelde önizlenir.";
            }
            else if (reportType == "MazeretDilekcesi")
            {
                Modul2Baslik.Text = "Mazeret İzin Dilekçesi";
                Modul2Icerik.Text = "Mazeret raporu öğrenci bazlıdır. Sol üst veri listesinden tek bir öğrenci seçilir. Sol alttaki Mazeret ayarlarından belge tarihi ve mazeret izin süresi (gün) girilir. Rapor Oluştur ile mazeret izin dilekçesi PDF olarak sağ panelde önizlenir.";
            }
            else if (reportType == "NotCizelgesi")
            {
                Modul2Baslik.Text = "Not Çizelgesi";
                Modul2Icerik.Text = "Not çizelgesi işletme bazlıdır. Sol üstte işletmeler seçilir. Sol alttaki ayarlardan not fişi türü, sayfa düzeni, dönem ve belge tarihi manuel girilir. Rapor Oluştur ile not çizelgesi PDF olarak sağ panelde önizlenir.";
            }
            else if (reportType == "HaftalikGorevTakip")
            {
                Modul2Baslik.Text = "Haftalık Görev Takip";
                Modul2Icerik.Text = "Koordinatör öğretmen bazlıdır. Sol üstten bir veya daha fazla öğretmen seçin. Rapor ayarlarından hafta (1–5), ay ve öğretim yılını girin. Rapor Oluştur ile seçili öğretmen(ler) için A4 yatay haftalık görev formu PDF oluşturulur.";
            }
            else
            {
                Modul2Baslik.Text = "Raporlar";
                Modul2Icerik.Text = "Çeşitli raporları oluşturun ve görüntüleyin.";
            }

            Modul4Container.Children.Clear();
            
            var raporControl = new RaporBase();
            raporControl.LoadReportType(reportType);
            Modul4Container.Children.Add(raporControl);
        }

        private void DigerEvraklarButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyDefaultLayout();
            Modul1ProfilPanel.Visibility = Visibility.Visible;

            // Modül 1 - Kırmızı renk
            Modul1Baslik.Text = "Diğer Evraklar";
            SetModul1Color("#E74C3C", "#F5B7B1");
            
            // Modül 2
            Modul2Baslik.Text = "Diğer Evraklar";
            Modul2Icerik.Text = "Raporların dışında kalan, uygulamaya gömülü diğer evrak ve şablonları oluşturun.";
            
            // Modül 3 - Diğer Evraklar menüsünü göster
            Modul3Icerik.Children.Clear();
            Modul3Icerik.Visibility = Visibility.Visible;
            
            // Başlık
            var baslik = new TextBlock 
            { 
                Text = "Boş Şablonlar", 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
            };
            Modul3Icerik.Children.Add(baslik);
            
            // Modül 4
            Modul4Container.Children.Clear();
            Modul4Container.Children.Add(new MEEY.DigerEvraklar.DigerEvraklarBase());
            Modul4Baslik.Text = "Evrak Seçin";
            Modul4Icerik.Text = "Sol menüden bir evrak şablonu seçin.";
        }

        private void MenuOkulKoordinatorler_Click(object sender, RoutedEventArgs e)
        {
            // Modül 2 açıklama
            Modul2Baslik.Text = "Okul ve Koordinatörler";
            Modul2Icerik.Text = "Okul bilgileri ile raporlarda kullanılacak koordinatör ad-soyad bilgilerini giriniz. Birden fazla kayıt ekleyebilirsiniz.";
            
            // Modül 4'e Okul & Koordinatörler kontrolünü yükle
            Modul4Container.Children.Clear();
            var okulKoordinatorlerControl = new OkulKoordinatorler();
            Modul4Container.Children.Add(okulKoordinatorlerControl);
        }

        private void MenuIsletmeler_Click(object sender, RoutedEventArgs e)
        {
            // Modül 2 açıklama
            Modul2Baslik.Text = "İşletmeler";
            Modul2Icerik.Text = "İşletme adı, telefon ve adres bilgisini giriniz. Birden fazla kayıt ekleyebilirsiniz.";
            
            // Modül 4'e İşletmeler kontrolünü yükle
            Modul4Container.Children.Clear();
            var isletmelerControl = new Isletmeler();
            Modul4Container.Children.Add(isletmelerControl);
        }

        private void MenuAlanDal_Click(object sender, RoutedEventArgs e)
        {
            // Modül 2 açıklama
            Modul2Baslik.Text = "Alan / Dal";
            Modul2Icerik.Text = "Alan ve dal bilgisini giriniz. Birden fazla kayıt ekleyebilirsiniz.";
            
            // Modül 4'e Alan/Dal kontrolünü yükle
            Modul4Container.Children.Clear();
            var alanDalControl = new AlanDal();
            Modul4Container.Children.Add(alanDalControl);
        }

        private void MenuKoordinatorTanimlama_Click(object sender, RoutedEventArgs e)
        {
            // Modül 2 açıklama
            Modul2Baslik.Text = "Koordinatör Tanımlama";
            Modul2Icerik.Text = "Okul, öğretmen, işletme ve müdür yardımcısı eşleştirmelerini yapınız. Birden fazla kayıt ekleyebilirsiniz.";
            
            // Modül 4'e Koordinatör Tanımlama kontrolünü yükle
            Modul4Container.Children.Clear();
            var koordinatorControl = new KoordinatorTanimlama();
            Modul4Container.Children.Add(koordinatorControl);
        }

        private void MenuOgrenciler_Click(object sender, RoutedEventArgs e)
        {
            // Modül 2 açıklama
            Modul2Baslik.Text = "Öğrenciler";
            Modul2Icerik.Text = "Öğrenci bilgilerini giriniz. Günler butonlarla seçilir. Birden fazla kayıt ekleyebilirsiniz.";
            
            // Modül 4'e Öğrenciler kontrolünü yükle
            Modul4Container.Children.Clear();
            var ogrencilerControl = new Ogrenciler();
            Modul4Container.Children.Add(ogrencilerControl);
        }

        private void MenuCalismaTakvimi_Click(object sender, RoutedEventArgs e)
        {
            // Modül 2 açıklama
            Modul2Baslik.Text = "Çalışma Takvimi";
            Modul2Icerik.Text = "Resmi tatil ve bayram günlerini otomatik çekebilir veya manuel olarak girebilirsiniz.";
            
            // Modül 4'e Çalışma Takvimi kontrolünü yükle
            Modul4Container.Children.Clear();
            var calismaTakvimiControl = new CalismaTakvimi();
            Modul4Container.Children.Add(calismaTakvimiControl);
        }

        private void MenuDevamsizlikGirisi_Click(object sender, RoutedEventArgs e)
        {
            // Modül 2 açıklama
            Modul2Baslik.Text = "Devamsızlık Girişi";
            Modul2Icerik.Text = "Öğrencilerin günlük devamsızlık durumlarını girin. X: İşletmede, O: Okulda, T: Tatil, İ: İzinli, D: Özürsüz, H: Hasta, R: Raporlu";
            
            // Modül 4'e Devamsızlık Girişi kontrolünü yükle
            Modul4Container.Children.Clear();
            var devamsizlikGirisiControl = new DevamsizlikGirisi();
            Modul4Container.Children.Add(devamsizlikGirisiControl);
        }

        private void MetinEditorDenemeButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyMetinEditorLayout();
            SetModul1Color("#E67E22", "#F5CBA7");

            Modul1Baslik.Text = "Metin Editörü";
            Modul1ProfilPanel.Visibility = Visibility.Visible;

            Modul2Baslik.Text = string.Empty;
            Modul2Icerik.Text = string.Empty;

            // Temizle
            Modul3Icerik.Children.Clear();

            // --- Şablonlar Expander ---
            var templatesExpander = new Expander
            {
                Header = new TextBlock 
                { 
                    Text = "Şablonlar", 
                    FontSize = 14, 
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
                },
                IsExpanded = true,
                Margin = new Thickness(0, 0, 0, 10)
            };
            
            var templatesPanel = new StackPanel();
            templatesExpander.Content = templatesPanel;
            Modul3Icerik.Children.Add(templatesExpander);

            // GEREKLİ: Editör kontrolünü başlat
            var metinEditorControl = new MetinEditorHost();
            Modul4Container.Children.Clear();
            Modul4Container.Children.Add(metinEditorControl);

            // Şablonları Yükle
            try
            {
                var templatesPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Templates");
                if (System.IO.Directory.Exists(templatesPath))
                {
                    var files = System.IO.Directory.GetFiles(templatesPath, "*.html");
                    foreach (var file in files)
                    {
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                        var btn = new Button
                        {
                            Height = 40,
                            Margin = new Thickness(0, 2, 0, 2),
                            Background = Brushes.White,
                            BorderThickness = new Thickness(1, 1, 1, 1),
                            BorderBrush = new SolidColorBrush(Color.FromRgb(189, 195, 199)),
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            Padding = new Thickness(10, 0, 10, 0),
                            Cursor = System.Windows.Input.Cursors.Hand,
                            Content = new TextBlock { Text = "📄 " + fileName, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)) }
                        };
                        
                        btn.Click += async (s, args) => 
                        {
                            try
                            {
                                var htmlContent = System.IO.File.ReadAllText(file);
                                await metinEditorControl.SetHtmlContentAsync(htmlContent);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Şablon yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        };
                        templatesPanel.Children.Add(btn);
                    }
                }
                else
                {
                    var uyari = new TextBlock 
                    { 
                        Text = "Assets/Templates klasörü bulunamadı.", 
                        FontSize = 12, 
                        Foreground = Brushes.Red,
                        TextWrapping = TextWrapping.Wrap
                    };
                    templatesPanel.Children.Add(uyari);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şablon klasörü okunamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // --- Editör Kayıtları Expander ---
            var recordsExpander = new Expander
            {
                Header = new TextBlock 
                { 
                    Text = "Kayıtlı Belgeler", 
                    FontSize = 14, 
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
                },
                IsExpanded = true,
                Margin = new Thickness(0, 0, 0, 10)
            };
            
            var recordsPanel = new StackPanel();
            recordsExpander.Content = recordsPanel;
            Modul3Icerik.Children.Add(recordsExpander);

            // Kayıtları Yükle
            try
            {
                var recordsPath = MetinEditorHost.GetEditorRecordsPath();
                
                // Klasör yoksa ilk boş bir klasör oluşturalım
                if (!System.IO.Directory.Exists(recordsPath))
                {
                    System.IO.Directory.CreateDirectory(recordsPath);
                }

                var rFiles = System.IO.Directory.GetFiles(recordsPath, "*.html");
                if (rFiles.Length > 0)
                {
                    foreach (var file in rFiles)
                    {
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                        var btn = new Button
                        {
                            Height = 40,
                            Margin = new Thickness(0, 2, 0, 2),
                            Background = Brushes.White,
                            BorderThickness = new Thickness(1, 1, 1, 1),
                            BorderBrush = new SolidColorBrush(Color.FromRgb(39, 174, 96)), // Yeşil tema
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            Padding = new Thickness(10, 0, 10, 0),
                            Cursor = System.Windows.Input.Cursors.Hand,
                            Content = new TextBlock { Text = "💾 " + fileName, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)) }
                        };
                        
                        btn.Click += async (s, args) => 
                        {
                            try
                            {
                                var htmlContent = System.IO.File.ReadAllText(file);
                                await metinEditorControl.SetHtmlContentAsync(htmlContent);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Kayıtlı belge yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        };
                        recordsPanel.Children.Add(btn);
                    }
                }
                else
                {
                    var uyari = new TextBlock 
                    { 
                        Text = "Henüz kaydedilmiş belge yok.", 
                        FontSize = 12, 
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(5),
                        TextWrapping = TextWrapping.Wrap
                    };
                    recordsPanel.Children.Add(uyari);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Editör Kayıtları klasörü okunamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void HakkindaButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyDefaultLayout();
            Modul1ProfilPanel.Visibility = Visibility.Collapsed;

            // Modül 1 - Koyu renk
            Modul1Baslik.Text = "Hakkında";
            SetModul1Color("#2C3E50", "#34495E");

            // Modül 2
            Modul2Baslik.Text = "Uygulama & Geliştirici Bilgileri";
            Modul2Icerik.Text = "Sürüm detayları, geliştirici iletişim bilgileri ve özel teşekkürler.";

            // Modül 3 Temizle
            Modul3Icerik.Children.Clear();
            Modul3Icerik.Visibility = Visibility.Collapsed;

            // Modül 4 (İçerik Alanına HakkindaControl'ü yükle)
            Modul4Container.Children.Clear();
            var hakkindaControl = new Controls.HakkindaControl();
            Modul4Container.Children.Add(hakkindaControl);
        }
    }
}
