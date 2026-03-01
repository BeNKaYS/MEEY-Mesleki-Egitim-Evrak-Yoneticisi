using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MEEY.Database;

namespace MEEY.Controls
{
    public partial class CalismaTakvimi : UserControl
    {
        public class TatilModel
        {
            public int Id { get; set; }
            public int Yil { get; set; }
            public DateTime Baslangic { get; set; }
            public DateTime Bitis { get; set; }
            public string Aciklama { get; set; } = "";
            public int GunSayisi 
            { 
                get 
                { 
                    return (Bitis - Baslangic).Days + 1; 
                } 
            }
        }

        private ObservableCollection<TatilModel> tatiller = new ObservableCollection<TatilModel>();
        private int? selectedId = null;

        public CalismaTakvimi()
        {
            InitializeComponent();
            dgTatiller.ItemsSource = tatiller;
            LoadYillar();
            LoadData();
        }

        private void LoadYillar()
        {
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear - 5; i <= currentYear + 5; i++)
            {
                cmbYil.Items.Add(i);
            }
            cmbYil.SelectedItem = currentYear;
        }

        private void LoadData()
        {
            try
            {
                tatiller.Clear();
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM CalismaTakvimi ORDER BY Yil, Baslangic";
                    
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tatiller.Add(new TatilModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Yil = Convert.ToInt32(reader["Yil"]),
                                Baslangic = DateTime.Parse(reader["Baslangic"].ToString()!),
                                Bitis = DateTime.Parse(reader["Bitis"].ToString()!),
                                Aciklama = reader["Aciklama"].ToString() ?? ""
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

        private void BtnOtomatikCek_Click(object sender, RoutedEventArgs e)
        {
            if (cmbYil.SelectedItem == null)
            {
                MessageBox.Show("Lütfen yıl seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int yil = (int)cmbYil.SelectedItem;
            
            var result = MessageBox.Show($"{yil} yılı için resmi tatiller otomatik eklensin mi?", 
                                        "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var tatilListesi = GetResmiTatiller(yil);
                    int eklenenSayisi = 0;
                    int atlanSayisi = 0;
                    
                    using (var connection = DatabaseManager.GetConnection())
                    {
                        connection.Open();
                        
                        foreach (var tatil in tatilListesi)
                        {
                            // Aynı kayıt var mı kontrol et
                            string checkQuery = "SELECT COUNT(*) FROM CalismaTakvimi WHERE Yil=@Yil AND Baslangic=@Baslangic AND Bitis=@Bitis AND Aciklama=@Aciklama";
                            
                            using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                            {
                                checkCommand.Parameters.AddWithValue("@Yil", yil);
                                checkCommand.Parameters.AddWithValue("@Baslangic", tatil.Item1.ToString("yyyy-MM-dd"));
                                checkCommand.Parameters.AddWithValue("@Bitis", tatil.Item2.ToString("yyyy-MM-dd"));
                                checkCommand.Parameters.AddWithValue("@Aciklama", tatil.Item3);
                                
                                int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                                
                                if (count == 0)
                                {
                                    // Kayıt yoksa ekle
                                    string query = "INSERT INTO CalismaTakvimi (Yil, Baslangic, Bitis, Aciklama) VALUES (@Yil, @Baslangic, @Bitis, @Aciklama)";
                                    
                                    using (var command = new SQLiteCommand(query, connection))
                                    {
                                        command.Parameters.AddWithValue("@Yil", yil);
                                        command.Parameters.AddWithValue("@Baslangic", tatil.Item1.ToString("yyyy-MM-dd"));
                                        command.Parameters.AddWithValue("@Bitis", tatil.Item2.ToString("yyyy-MM-dd"));
                                        command.Parameters.AddWithValue("@Aciklama", tatil.Item3);
                                        command.ExecuteNonQuery();
                                    }
                                    eklenenSayisi++;
                                }
                                else
                                {
                                    atlanSayisi++;
                                }
                            }
                        }
                    }
                    
                    LoadData();
                    
                    string mesaj = $"{eklenenSayisi} tatil günü eklendi.";
                    if (atlanSayisi > 0)
                    {
                        mesaj += $"\n{atlanSayisi} kayıt zaten mevcut olduğu için atlandı.";
                    }
                    
                    MessageBox.Show(mesaj, "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Otomatik çekme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private List<Tuple<DateTime, DateTime, string>> GetResmiTatiller(int yil)
        {
            var tatiller = new List<Tuple<DateTime, DateTime, string>>();
            
            // Sabit Tatiller
            tatiller.Add(Tuple.Create(new DateTime(yil, 1, 1), new DateTime(yil, 1, 1), "Yılbaşı"));
            tatiller.Add(Tuple.Create(new DateTime(yil, 4, 23), new DateTime(yil, 4, 23), "Ulusal Egemenlik ve Çocuk Bayramı"));
            tatiller.Add(Tuple.Create(new DateTime(yil, 5, 1), new DateTime(yil, 5, 1), "Emek ve Dayanışma Günü"));
            tatiller.Add(Tuple.Create(new DateTime(yil, 5, 19), new DateTime(yil, 5, 19), "Atatürk'ü Anma, Gençlik ve Spor Bayramı"));
            tatiller.Add(Tuple.Create(new DateTime(yil, 7, 15), new DateTime(yil, 7, 15), "Demokrasi ve Milli Birlik Günü"));
            tatiller.Add(Tuple.Create(new DateTime(yil, 8, 30), new DateTime(yil, 8, 30), "Zafer Bayramı"));
            tatiller.Add(Tuple.Create(new DateTime(yil, 10, 29), new DateTime(yil, 10, 29), "Cumhuriyet Bayramı"));
            
            // Dini Bayramlar (Tahmini - Her yıl değişir)
            // 2026 için tahmini tarihler
            if (yil == 2026)
            {
                tatiller.Add(Tuple.Create(new DateTime(2026, 3, 20), new DateTime(2026, 3, 22), "Ramazan Bayramı"));
                tatiller.Add(Tuple.Create(new DateTime(2026, 5, 27), new DateTime(2026, 5, 30), "Kurban Bayramı"));
            }
            else if (yil == 2027)
            {
                tatiller.Add(Tuple.Create(new DateTime(2027, 3, 10), new DateTime(2027, 3, 12), "Ramazan Bayramı"));
                tatiller.Add(Tuple.Create(new DateTime(2027, 5, 17), new DateTime(2027, 5, 20), "Kurban Bayramı"));
            }
            
            return tatiller;
        }

        private void BtnYeni_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (cmbYil.SelectedItem == null)
            {
                MessageBox.Show("Yıl seçimi zorunludur!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBaslangic.Text))
            {
                MessageBox.Show("Başlangıç tarihi zorunludur!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBitis.Text))
            {
                MessageBox.Show("Bitiş tarihi zorunludur!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime baslangic, bitis;
            if (!DateTime.TryParseExact(txtBaslangic.Text, new[] { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy" }, 
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out baslangic))
            {
                MessageBox.Show("Başlangıç tarihi geçersiz! Format: gg.aa.yyyy", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateTime.TryParseExact(txtBitis.Text, new[] { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy" }, 
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out bitis))
            {
                MessageBox.Show("Bitiş tarihi geçersiz! Format: gg.aa.yyyy", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        query = "UPDATE CalismaTakvimi SET Yil=@Yil, Baslangic=@Baslangic, Bitis=@Bitis, Aciklama=@Aciklama WHERE Id=@Id";
                    }
                    else
                    {
                        query = "INSERT INTO CalismaTakvimi (Yil, Baslangic, Bitis, Aciklama) VALUES (@Yil, @Baslangic, @Bitis, @Aciklama)";
                    }
                    
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Yil", (int)cmbYil.SelectedItem);
                        command.Parameters.AddWithValue("@Baslangic", baslangic.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@Bitis", bitis.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@Aciklama", DatabaseManager.NormalizeText(txtAciklama.Text));
                        
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
                        string query = "DELETE FROM CalismaTakvimi WHERE Id=@Id";
                        
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
                        string query = "DELETE FROM CalismaTakvimi";
                        
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

        private void DgTatiller_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgTatiller.SelectedItem is TatilModel selected)
            {
                selectedId = selected.Id;
                cmbYil.SelectedItem = selected.Yil;
                txtBaslangic.Text = selected.Baslangic.ToString("dd.MM.yyyy");
                txtBitis.Text = selected.Bitis.ToString("dd.MM.yyyy");
                txtAciklama.Text = selected.Aciklama;
            }
        }

        private void ClearForm()
        {
            selectedId = null;
            cmbYil.SelectedItem = DateTime.Now.Year;
            txtBaslangic.Clear();
            txtBitis.Clear();
            txtAciklama.Clear();
            dgTatiller.SelectedItem = null;
        }
    }
}
