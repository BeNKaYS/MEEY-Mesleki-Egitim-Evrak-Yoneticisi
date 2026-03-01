using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MEEY.Database;

namespace MEEY.Controls
{
    public partial class KoordinatorTanimlama : UserControl
    {
        public class KoordinatorModel
        {
            public int Id { get; set; }
            public string Okul { get; set; } = "";
            public string Ogretmen { get; set; } = "";
            public string Isletme { get; set; } = "";
            public string IsletmeYetkilisi { get; set; } = "";
            public string MudurYrd { get; set; } = "";
            public string Gun { get; set; } = "";
            public string KoordTuru { get; set; } = "";
        }

        private ObservableCollection<KoordinatorModel> koordinatorler = new ObservableCollection<KoordinatorModel>();
        private int? selectedId = null;

        public KoordinatorTanimlama()
        {
            InitializeComponent();
            dgKoordinatorler.ItemsSource = koordinatorler;
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
                    
                    // Okulları yükle
                    cmbOkul.Items.Clear();
                    string okulQuery = "SELECT DISTINCT OkulAdi FROM OkulKoordinatorler ORDER BY OkulAdi";
                    using (var command = new SQLiteCommand(okulQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbOkul.Items.Add(reader["OkulAdi"].ToString());
                        }
                    }
                    
                    // Öğretmenleri yükle
                    cmbOgretmen.Items.Clear();
                    string ogretmenQuery = "SELECT DISTINCT KoordOgretmen FROM OkulKoordinatorler WHERE KoordOgretmen IS NOT NULL AND KoordOgretmen != '' ORDER BY KoordOgretmen";
                    using (var command = new SQLiteCommand(ogretmenQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbOgretmen.Items.Add(reader["KoordOgretmen"].ToString());
                        }
                    }
                    
                    // İşletmeleri yükle
                    cmbIsletme.Items.Clear();
                    string isletmeQuery = "SELECT DISTINCT IsletmeAdi FROM Isletmeler ORDER BY IsletmeAdi";
                    using (var command = new SQLiteCommand(isletmeQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbIsletme.Items.Add(reader["IsletmeAdi"].ToString());
                        }
                    }
                    
                    // Müdür Yardımcılarını yükle
                    cmbMudurYrd.Items.Clear();
                    string mudurYrdQuery = "SELECT DISTINCT KoordMudurYrd FROM OkulKoordinatorler WHERE KoordMudurYrd IS NOT NULL AND KoordMudurYrd != '' ORDER BY KoordMudurYrd";
                    using (var command = new SQLiteCommand(mudurYrdQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbMudurYrd.Items.Add(reader["KoordMudurYrd"].ToString());
                        }
                    }
                }

                ApplySingleItemAutoSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ComboBox yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySingleItemAutoSelection()
        {
            AutoSelectIfSingleItem(cmbOkul);
            AutoSelectIfSingleItem(cmbOgretmen);
            AutoSelectIfSingleItem(cmbIsletme);
            AutoSelectIfSingleItem(cmbMudurYrd);
            AutoSelectIfSingleItem(cmbGun);
        }

        private static void AutoSelectIfSingleItem(ComboBox comboBox)
        {
            if (comboBox.Items.Count == 1)
                comboBox.SelectedIndex = 0;
        }

        private void LoadData()
        {
            try
            {
                koordinatorler.Clear();
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM KoordinatorTanimlama ORDER BY Id";
                    
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            koordinatorler.Add(new KoordinatorModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Okul = reader["Okul"].ToString() ?? "",
                                Ogretmen = reader["Ogretmen"].ToString() ?? "",
                                Isletme = reader["Isletme"].ToString() ?? "",
                                IsletmeYetkilisi = reader["IsletmeYetkilisi"]?.ToString() ?? "",
                                MudurYrd = reader["MudurYrd"].ToString() ?? "",
                                Gun = reader["Gun"].ToString() ?? "",
                                KoordTuru = reader["KoordTuru"]?.ToString() ?? "MESEM"
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

        private void BtnYeni_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOkul.SelectedItem == null)
            {
                MessageBox.Show("Okul seçimi zorunludur!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        query = "UPDATE KoordinatorTanimlama SET Okul=@Okul, Ogretmen=@Ogretmen, Isletme=@Isletme, IsletmeYetkilisi=@IsletmeYetkilisi, MudurYrd=@MudurYrd, Gun=@Gun, KoordTuru=@KoordTuru WHERE Id=@Id";
                    }
                    else
                    {
                        query = "INSERT INTO KoordinatorTanimlama (Okul, Ogretmen, Isletme, IsletmeYetkilisi, MudurYrd, Gun, KoordTuru) VALUES (@Okul, @Ogretmen, @Isletme, @IsletmeYetkilisi, @MudurYrd, @Gun, @KoordTuru)";
                    }
                    
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Okul", DatabaseManager.NormalizeText(cmbOkul.SelectedItem?.ToString()));
                        command.Parameters.AddWithValue("@Ogretmen", DatabaseManager.NormalizeText(cmbOgretmen.SelectedItem?.ToString()));
                        command.Parameters.AddWithValue("@Isletme", DatabaseManager.NormalizeText(cmbIsletme.SelectedItem?.ToString()));
                        command.Parameters.AddWithValue("@IsletmeYetkilisi", DatabaseManager.NormalizeText(txtIsletmeYetkilisi.Text));
                        command.Parameters.AddWithValue("@MudurYrd", DatabaseManager.NormalizeText(cmbMudurYrd.SelectedItem?.ToString()));
                        command.Parameters.AddWithValue("@Gun", DatabaseManager.NormalizeText((cmbGun.SelectedItem as ComboBoxItem)?.Content?.ToString()));
                        command.Parameters.AddWithValue("@KoordTuru", DatabaseManager.NormalizeText((cmbKoordTuru.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "MESEM"));
                        
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
                        string query = "DELETE FROM KoordinatorTanimlama WHERE Id=@Id";
                        
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
                        string query = "DELETE FROM KoordinatorTanimlama";
                        
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

        private void DgKoordinatorler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgKoordinatorler.SelectedItem is KoordinatorModel selected)
            {
                selectedId = selected.Id;
                cmbOkul.SelectedItem = selected.Okul;
                cmbOgretmen.SelectedItem = selected.Ogretmen;
                cmbIsletme.SelectedItem = selected.Isletme;
                txtIsletmeYetkilisi.Text = selected.IsletmeYetkilisi;
                cmbMudurYrd.SelectedItem = selected.MudurYrd;
                
                foreach (ComboBoxItem item in cmbGun.Items)
                {
                    if (item.Content.ToString() == selected.Gun)
                    {
                        cmbGun.SelectedItem = item;
                        break;
                    }
                }
                
                foreach (ComboBoxItem item in cmbKoordTuru.Items)
                {
                    if (item.Content.ToString() == selected.KoordTuru)
                    {
                        cmbKoordTuru.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void ClearForm()
        {
            selectedId = null;
            cmbOkul.SelectedIndex = -1;
            cmbOgretmen.SelectedIndex = -1;
            cmbIsletme.SelectedIndex = -1;
            txtIsletmeYetkilisi.Clear();
            cmbMudurYrd.SelectedIndex = -1;
            cmbGun.SelectedIndex = -1;
            cmbKoordTuru.SelectedIndex = 0;
            dgKoordinatorler.SelectedItem = null;

            ApplySingleItemAutoSelection();
        }
    }
}
