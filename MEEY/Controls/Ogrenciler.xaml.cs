using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MEEY.Database;

namespace MEEY.Controls
{
    public partial class Ogrenciler : UserControl
    {
        private class KoordinatorItem
        {
            public int Id { get; set; }
            public string DisplayText { get; set; } = "";
            public override string ToString() => DisplayText;
        }
        
        public class OgrenciModel
        {
            public int Id { get; set; }
            public string OkulNo { get; set; } = "";
            public string AdSoyad { get; set; } = "";
            public string Sinif { get; set; } = "";
            public string AlanDal { get; set; } = "";
            public string KoordinatorId { get; set; } = "";
            public string Koordinator { get; set; } = "";
            public string Gunler { get; set; } = "";
        }

        private ObservableCollection<OgrenciModel> ogrenciler = new ObservableCollection<OgrenciModel>();
        private int? selectedId = null;

        public Ogrenciler()
        {
            InitializeComponent();
            dgOgrenciler.ItemsSource = ogrenciler;
            LoadComboBoxData();
            LoadData();
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    
                    // Alan/Dal yükle
                    cmbAlanDal.Items.Clear();
                    string alanDalQuery = "SELECT DISTINCT Alan || ' / ' || Dal as AlanDal FROM AlanDal WHERE Dal IS NOT NULL AND Dal != '' ORDER BY AlanDal";
                    using (var command = new SQLiteCommand(alanDalQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbAlanDal.Items.Add(reader["AlanDal"].ToString());
                        }
                    }
                    
                    // Koordinatörleri yükle (ID ile birlikte)
                    cmbKoordinator.Items.Clear();
                    string koordinatorQuery = "SELECT Id, Ogretmen, Isletme FROM KoordinatorTanimlama WHERE Ogretmen IS NOT NULL AND Ogretmen != '' ORDER BY Ogretmen";
                    using (var command = new SQLiteCommand(koordinatorQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ogretmen = reader["Ogretmen"].ToString();
                            var isletme = reader["Isletme"].ToString();
                            cmbKoordinator.Items.Add(new KoordinatorItem
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                DisplayText = $"{ogretmen} - {isletme}"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ComboBox yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                ogrenciler.Clear();
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            o.Id,
                            o.OkulNo,
                            o.AdSoyad,
                            o.Sinif,
                            o.AlanDal,
                            o.Koordinator,
                            o.Gunler,
                            CASE 
                                WHEN k.Id IS NOT NULL THEN k.Ogretmen || ' - ' || k.Isletme
                                ELSE o.Koordinator
                            END AS KoordinatorDisplay
                        FROM Ogrenciler o
                        LEFT JOIN KoordinatorTanimlama k ON o.Koordinator = CAST(k.Id AS TEXT)
                        ORDER BY o.Id";
                    
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ogrenciler.Add(new OgrenciModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                OkulNo = reader["OkulNo"].ToString() ?? "",
                                AdSoyad = reader["AdSoyad"].ToString() ?? "",
                                Sinif = reader["Sinif"].ToString() ?? "",
                                AlanDal = reader["AlanDal"].ToString() ?? "",
                                KoordinatorId = reader["Koordinator"].ToString() ?? "",
                                Koordinator = reader["KoordinatorDisplay"].ToString() ?? "",
                                Gunler = reader["Gunler"].ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetSelectedGunler()
        {
            var gunler = new List<string>();
            
            if (btnPzt.IsChecked == true) gunler.Add("Pzt");
            if (btnSal.IsChecked == true) gunler.Add("Sal");
            if (btnCar.IsChecked == true) gunler.Add("Çar");
            if (btnPer.IsChecked == true) gunler.Add("Per");
            if (btnCum.IsChecked == true) gunler.Add("Cum");
            if (btnCmt.IsChecked == true) gunler.Add("Cmt");
            if (btnPaz.IsChecked == true) gunler.Add("Paz");
            
            return string.Join(", ", gunler);
        }

        private void SetSelectedGunler(string gunler)
        {
            btnPzt.IsChecked = gunler.Contains("Pzt");
            btnSal.IsChecked = gunler.Contains("Sal");
            btnCar.IsChecked = gunler.Contains("Çar");
            btnPer.IsChecked = gunler.Contains("Per");
            btnCum.IsChecked = gunler.Contains("Cum");
            btnCmt.IsChecked = gunler.Contains("Cmt");
            btnPaz.IsChecked = gunler.Contains("Paz");
        }

        private void BtnYeni_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOkulNo.Text))
            {
                MessageBox.Show("Okul No boş olamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text))
            {
                MessageBox.Show("Ad Soyad boş olamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    string query;
                    
                    if (selectedId.HasValue)
                    {
                        query = "UPDATE Ogrenciler SET OkulNo=@OkulNo, AdSoyad=@AdSoyad, Sinif=@Sinif, AlanDal=@AlanDal, Koordinator=@Koordinator, Gunler=@Gunler WHERE Id=@Id";
                    }
                    else
                    {
                        query = "INSERT INTO Ogrenciler (OkulNo, AdSoyad, Sinif, AlanDal, Koordinator, Gunler) VALUES (@OkulNo, @AdSoyad, @Sinif, @AlanDal, @Koordinator, @Gunler)";
                    }
                    
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OkulNo", DatabaseManager.NormalizeText(txtOkulNo.Text));
                        command.Parameters.AddWithValue("@AdSoyad", DatabaseManager.NormalizeText(txtAdSoyad.Text));
                        command.Parameters.AddWithValue("@Sinif", DatabaseManager.NormalizeText(txtSinif.Text));
                        command.Parameters.AddWithValue("@AlanDal", DatabaseManager.NormalizeText(cmbAlanDal.SelectedItem?.ToString()));
                        
                        // Koordinator ID'sini kaydet
                        var koordinatorId = (cmbKoordinator.SelectedItem as KoordinatorItem)?.Id.ToString() ?? "";
                        command.Parameters.AddWithValue("@Koordinator", koordinatorId);
                        
                        command.Parameters.AddWithValue("@Gunler", GetSelectedGunler());
                        
                        if (selectedId.HasValue)
                            command.Parameters.AddWithValue("@Id", selectedId.Value);
                        
                        command.ExecuteNonQuery();
                    }
                }
                
                LoadData();
                ClearForm();
                MessageBox.Show("Kayıt başarılı!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedId.HasValue)
            {
                MessageBox.Show("Lütfen silinecek kaydı seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Seçili kaydı silmek istediğinizden emin misiniz?", 
                                        "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();
                        string query = "DELETE FROM Ogrenciler WHERE Id=@Id";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Id", selectedId.Value);
                            command.ExecuteNonQuery();
                        }
                    }
                    
                    LoadData();
                    ClearForm();
                    MessageBox.Show("Kayıt silindi!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnTumunuSil_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = "Onay",
                Width = 350,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow
            };
            
            var stack = new System.Windows.Controls.StackPanel { Margin = new Thickness(20, 20, 20, 20) };
            
            stack.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = "TÜM kayıtları silmek için 'EVET' yazın:", 
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            });
            
            var textBox = new System.Windows.Controls.TextBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
            
            var btnOnay = new System.Windows.Controls.Button 
            { 
                Content = "Onayla", 
                Height = 35,
                Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0, 0, 0, 0)
            };
            
            btnOnay.Click += (s, args) =>
            {
                if (textBox.Text.Trim().ToUpper() == "EVET")
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Onaylamak için 'EVET' yazmalısınız!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            
            stack.Children.Add(textBox);
            stack.Children.Add(btnOnay);
            dialog.Content = stack;
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();
                        string query = "DELETE FROM Ogrenciler";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    
                    LoadData();
                    ClearForm();
                    MessageBox.Show("Tüm kayıtlar silindi!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgOgrenciler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOgrenciler.SelectedItem is OgrenciModel selected)
            {
                selectedId = selected.Id;
                txtOkulNo.Text = selected.OkulNo;
                txtAdSoyad.Text = selected.AdSoyad;
                txtSinif.Text = selected.Sinif;
                cmbAlanDal.SelectedItem = selected.AlanDal;
                
                // Koordinatörü ID'ye göre bul ve seç
                if (int.TryParse(selected.KoordinatorId, out int koordinatorId))
                {
                    foreach (var item in cmbKoordinator.Items)
                    {
                        if (item is KoordinatorItem kItem && kItem.Id == koordinatorId)
                        {
                            cmbKoordinator.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    cmbKoordinator.SelectedItem = null;
                }
                
                SetSelectedGunler(selected.Gunler);
            }
        }

        private void ClearForm()
        {
            selectedId = null;
            txtOkulNo.Clear();
            txtAdSoyad.Clear();
            txtSinif.Clear();
            cmbAlanDal.SelectedIndex = -1;
            cmbKoordinator.SelectedIndex = -1;
            btnPzt.IsChecked = false;
            btnSal.IsChecked = false;
            btnCar.IsChecked = false;
            btnPer.IsChecked = false;
            btnCum.IsChecked = false;
            btnCmt.IsChecked = false;
            btnPaz.IsChecked = false;
            dgOgrenciler.SelectedItem = null;
        }
    }
}
