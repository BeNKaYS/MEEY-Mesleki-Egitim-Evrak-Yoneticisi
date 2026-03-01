using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MEEY.Database;

namespace MEEY.Controls
{
    public partial class DevamsizlikGirisi : UserControl
    {
        private ObservableCollection<DevamsizlikSatir> satirlar = new ObservableCollection<DevamsizlikSatir>();
        private string secilenSembol = "";
        private int secilenAy = DateTime.Now.Month;
        private int secilenYil = DateTime.Now.Year;
        
        public DevamsizlikGirisi()
        {
            InitializeComponent();
            LoadIsletmeler();
            LoadAylar();
            LoadYillar();
        }
        
        private void LoadIsletmeler()
        {
            try
            {
                cmbIsletme.Items.Clear();
                
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT IsletmeAdi FROM Isletmeler ORDER BY IsletmeAdi";
                    
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbIsletme.Items.Add(reader["IsletmeAdi"].ToString());
                        }
                    }
                }
                
                if (cmbIsletme.Items.Count > 0)
                    cmbIsletme.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İşletme yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadAylar()
        {
            string[] aylar = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", 
                              "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
            
            for (int i = 0; i < aylar.Length; i++)
            {
                cmbAy.Items.Add($"{i + 1} - {aylar[i]}");
            }
            
            cmbAy.SelectedIndex = DateTime.Now.Month - 1;
        }
        
        private void LoadYillar()
        {
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear - 2; i <= currentYear + 2; i++)
            {
                cmbYil.Items.Add(i);
            }
            cmbYil.SelectedItem = currentYear;
        }
        
        private void Isletme_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadDevamsizlikData();
        }
        
        private void Ay_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAy.SelectedIndex >= 0)
            {
                secilenAy = cmbAy.SelectedIndex + 1;
                LoadDevamsizlikData();
            }
        }
        
        private void Yil_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cmbYil.SelectedItem != null)
            {
                secilenYil = (int)cmbYil.SelectedItem;
                LoadDevamsizlikData();
            }
        }
        
        private void LoadDevamsizlikData()
        {
            if (cmbIsletme.SelectedItem == null) return;
            
            try
            {
                string isletmeAdi = cmbIsletme.SelectedItem.ToString()!;
                satirlar.Clear();
                dgDevamsizlik.Columns.Clear();
                
                // Ay bilgisini güncelle
                string[] aylar = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", 
                                  "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
                txtBaslik.Text = $"{isletmeAdi} - {aylar[secilenAy - 1]} {secilenYil}";
                
                // Öğrencileri getir
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    
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
                                var satir = new DevamsizlikSatir
                                {
                                    OgrenciId = Convert.ToInt32(reader["OgrenciId"]),
                                    AdSoyad = reader["AdSoyad"].ToString() ?? "",
                                    OkulNo = reader["OkulNo"].ToString() ?? ""
                                };
                                
                                satirlar.Add(satir);
                            }
                        }
                    }
                    
                    // Devamsızlık verilerini yükle
                    int gunSayisi = DateTime.DaysInMonth(secilenYil, secilenAy);
                    
                    foreach (var satir in satirlar)
                    {
                        string devQuery = @"
                            SELECT Tarih, Sembol 
                            FROM Devamsizlik 
                            WHERE OgrenciId = @OgrenciId 
                            AND strftime('%Y', Tarih) = @Yil 
                            AND strftime('%m', Tarih) = @Ay";
                        
                        using (var command = new SQLiteCommand(devQuery, connection))
                        {
                            command.Parameters.AddWithValue("@OgrenciId", satir.OgrenciId);
                            command.Parameters.AddWithValue("@Yil", secilenYil.ToString());
                            command.Parameters.AddWithValue("@Ay", secilenAy.ToString("D2"));
                            
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var tarih = DateTime.Parse(reader["Tarih"].ToString()!);
                                    var sembol = reader["Sembol"]?.ToString() ?? "";
                                    satir.Gunler[tarih.Day] = sembol;
                                }
                            }
                        }
                    }
                }
                
                // DataGrid kolonlarını oluştur
                CreateDataGridColumns();
                dgDevamsizlik.ItemsSource = satirlar;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CreateDataGridColumns()
        {
            // Ad Soyad kolonu
            var colAdSoyad = new DataGridTextColumn
            {
                Header = "Ad Soyad",
                Binding = new System.Windows.Data.Binding("AdSoyad"),
                Width = 150,
                IsReadOnly = true
            };
            dgDevamsizlik.Columns.Add(colAdSoyad);
            
            // Okul No kolonu
            var colOkulNo = new DataGridTextColumn
            {
                Header = "Okul No",
                Binding = new System.Windows.Data.Binding("OkulNo"),
                Width = 100,
                IsReadOnly = true
            };
            dgDevamsizlik.Columns.Add(colOkulNo);
            
            // Gün kolonları
            int gunSayisi = DateTime.DaysInMonth(secilenYil, secilenAy);
            
            for (int gun = 1; gun <= gunSayisi; gun++)
            {
                var tarih = new DateTime(secilenYil, secilenAy, gun);
                string gunAdi = tarih.ToString("ddd", new System.Globalization.CultureInfo("tr-TR"));
                
                var col = new DataGridTextColumn
                {
                    Header = $"{gun}\n{gunAdi}",
                    Binding = new System.Windows.Data.Binding($"Gunler[{gun}]"),
                    Width = 45,
                    IsReadOnly = false
                };
                
                // Hafta sonu arka plan rengi
                if (tarih.DayOfWeek == DayOfWeek.Saturday || tarih.DayOfWeek == DayOfWeek.Sunday)
                {
                    var style = new Style(typeof(DataGridCell));
                    style.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Color.FromRgb(240, 240, 240))));
                    col.CellStyle = style;
                }
                
                dgDevamsizlik.Columns.Add(col);
            }
        }
        
        private void Sembol_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                secilenSembol = button.Tag?.ToString() ?? "";
                
                // Seçili hücreye sembolu yaz
                if (dgDevamsizlik.CurrentCell.Column != null && dgDevamsizlik.SelectedItem != null)
                {
                    var satir = dgDevamsizlik.SelectedItem as DevamsizlikSatir;
                    var columnIndex = dgDevamsizlik.CurrentCell.Column.DisplayIndex;
                    
                    // İlk iki kolon Ad Soyad ve Okul No
                    if (columnIndex >= 2 && satir != null)
                    {
                        int gun = columnIndex - 1; // -1 çünkü Ad Soyad ve Okul No var
                        satir.Gunler[gun] = secilenSembol;
                        dgDevamsizlik.Items.Refresh();
                    }
                }
            }
        }
        
        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dgDevamsizlik.CurrentCell.Column != null && dgDevamsizlik.SelectedItem != null)
            {
                var satir = dgDevamsizlik.SelectedItem as DevamsizlikSatir;
                var columnIndex = dgDevamsizlik.CurrentCell.Column.DisplayIndex;
                
                if (columnIndex >= 2 && satir != null)
                {
                    int gun = columnIndex - 1;
                    txtSeciliHucre.Text = $"{satir.AdSoyad} - {gun} {cmbAy.SelectedItem}";
                }
                else
                {
                    txtSeciliHucre.Text = "-";
                }
            }
        }
        
        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    
                    int kayitSayisi = 0;
                    
                    foreach (var satir in satirlar)
                    {
                        foreach (var gun in satir.Gunler.Keys)
                        {
                            var sembol = satir.Gunler[gun];
                            
                            if (!string.IsNullOrEmpty(sembol))
                            {
                                var tarih = new DateTime(secilenYil, secilenAy, gun);
                                
                                string query = @"
                                    INSERT OR REPLACE INTO Devamsizlik (OgrenciId, Tarih, Sembol)
                                    VALUES (@OgrenciId, @Tarih, @Sembol)";
                                
                                using (var command = new SQLiteCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@OgrenciId", satir.OgrenciId);
                                    command.Parameters.AddWithValue("@Tarih", tarih.ToString("yyyy-MM-dd"));
                                    command.Parameters.AddWithValue("@Sembol", sembol);
                                    command.ExecuteNonQuery();
                                    kayitSayisi++;
                                }
                            }
                        }
                    }
                    
                    MessageBox.Show($"Devamsızlık kayıtları başarıyla kaydedildi!\n\nKayıt sayısı: {kayitSayisi}", 
                        "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydetme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    public class DevamsizlikSatir
    {
        public int OgrenciId { get; set; }
        public string AdSoyad { get; set; } = "";
        public string OkulNo { get; set; } = "";
        public Dictionary<int, string> Gunler { get; set; } = new Dictionary<int, string>();
        
        public DevamsizlikSatir()
        {
            // 31 güne kadar boş değerler
            for (int i = 1; i <= 31; i++)
            {
                Gunler[i] = "";
            }
        }
    }
}
