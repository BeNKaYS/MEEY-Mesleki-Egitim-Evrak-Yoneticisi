using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MEEY.Database;

namespace MEEY.Controls
{
    public partial class Isletmeler : UserControl
    {
        public class IsletmeModel
        {
            public int Id { get; set; }
            public string IsletmeAdi { get; set; } = "";
            public string Telefon { get; set; } = "";
            public string Adres { get; set; } = "";
        }

        private ObservableCollection<IsletmeModel> isletmeler = new ObservableCollection<IsletmeModel>();
        private int? selectedId = null;

        public Isletmeler()
        {
            InitializeComponent();
            dgIsletmeler.ItemsSource = isletmeler;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                isletmeler.Clear();
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM Isletmeler ORDER BY Id";
                    
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            isletmeler.Add(new IsletmeModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IsletmeAdi = reader["IsletmeAdi"].ToString() ?? "",
                                Telefon = reader["Telefon"].ToString() ?? "",
                                Adres = reader["Adres"].ToString() ?? ""
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
            if (string.IsNullOrWhiteSpace(txtIsletmeAdi.Text))
            {
                MessageBox.Show("İşletme adı boş olamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        query = "UPDATE Isletmeler SET IsletmeAdi=@IsletmeAdi, Telefon=@Telefon, Adres=@Adres WHERE Id=@Id";
                    }
                    else
                    {
                        query = "INSERT INTO Isletmeler (IsletmeAdi, Telefon, Adres) VALUES (@IsletmeAdi, @Telefon, @Adres)";
                    }
                    
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IsletmeAdi", DatabaseManager.NormalizeText(txtIsletmeAdi.Text));
                        command.Parameters.AddWithValue("@Telefon", DatabaseManager.NormalizeText(txtTelefon.Text));
                        command.Parameters.AddWithValue("@Adres", DatabaseManager.NormalizeText(txtAdres.Text));
                        
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
                        string query = "DELETE FROM Isletmeler WHERE Id=@Id";
                        
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
                        string query = "DELETE FROM Isletmeler";
                        
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

        private void DgIsletmeler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgIsletmeler.SelectedItem is IsletmeModel selected)
            {
                selectedId = selected.Id;
                txtIsletmeAdi.Text = selected.IsletmeAdi;
                txtTelefon.Text = selected.Telefon;
                txtAdres.Text = selected.Adres;
            }
        }

        private void ClearForm()
        {
            selectedId = null;
            txtIsletmeAdi.Clear();
            txtTelefon.Clear();
            txtAdres.Clear();
            dgIsletmeler.SelectedItem = null;
        }
    }
}
