using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MEEY.Database;

namespace MEEY.Controls
{
    public partial class OkulKoordinatorler : UserControl
    {
        private int? selectedId = null;

        public OkulKoordinatorler()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM OkulKoordinatorler ORDER BY Id DESC";
                    
                    using (var adapter = new SQLiteDataAdapter(query, connection))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgOkullar.ItemsSource = dt.DefaultView;
                        PopulateDropdowns(dt);
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
            if (string.IsNullOrWhiteSpace(txtOkulAdi.Text))
            {
                MessageBox.Show("Okul adı zorunludur!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        // Güncelleme
                        query = @"UPDATE OkulKoordinatorler 
                                 SET OkulAdi = @OkulAdi, Il = @Il, OkulMuduru = @OkulMuduru, 
                                     KoordMudurYrd = @KoordMudurYrd, KoordOgretmen = @KoordOgretmen 
                                 WHERE Id = @Id";
                    }
                    else
                    {
                        // Yeni kayıt
                        query = @"INSERT INTO OkulKoordinatorler (OkulAdi, Il, OkulMuduru, KoordMudurYrd, KoordOgretmen) 
                                 VALUES (@OkulAdi, @Il, @OkulMuduru, @KoordMudurYrd, @KoordOgretmen)";
                    }

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        if (selectedId.HasValue)
                            command.Parameters.AddWithValue("@Id", selectedId.Value);
                        
                        command.Parameters.AddWithValue("@OkulAdi", DatabaseManager.NormalizeText(txtOkulAdi.Text));
                        command.Parameters.AddWithValue("@Il", DatabaseManager.NormalizeText(txtIl.Text));
                        command.Parameters.AddWithValue("@OkulMuduru", DatabaseManager.NormalizeText(txtOkulMuduru.Text));
                        command.Parameters.AddWithValue("@KoordMudurYrd", DatabaseManager.NormalizeText(txtKoordMudurYrd.Text));
                        command.Parameters.AddWithValue("@KoordOgretmen", DatabaseManager.NormalizeText(txtKoordOgretmen.Text));

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Kayıt başarılı!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
                ClearForm();
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
                        string query = "DELETE FROM OkulKoordinatorler WHERE Id = @Id";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Id", selectedId.Value);
                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Kayıt silindi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                    ClearForm();
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
                        string query = "DELETE FROM OkulKoordinatorler";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Tüm kayıtlar silindi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgOkullar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOkullar.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgOkullar.SelectedItem;
                selectedId = Convert.ToInt32(row["Id"]);
                txtOkulAdi.Text = row["OkulAdi"].ToString();
                txtIl.Text = row["Il"].ToString();
                txtOkulMuduru.Text = row["OkulMuduru"].ToString();
                txtKoordMudurYrd.Text = row["KoordMudurYrd"].ToString();
                txtKoordOgretmen.Text = row["KoordOgretmen"].ToString();
            }
        }

        private void PopulateDropdowns(DataTable dt)
        {
            PopulateDropdown(txtOkulAdi, dt, "OkulAdi");
            PopulateDropdown(txtIl, dt, "Il");
            PopulateDropdown(txtOkulMuduru, dt, "OkulMuduru");
            PopulateDropdown(txtKoordMudurYrd, dt, "KoordMudurYrd");
            PopulateDropdown(txtKoordOgretmen, dt, "KoordOgretmen");
        }

        private void PopulateDropdown(ComboBox comboBox, DataTable dt, string columnName)
        {
            string currentText = comboBox.Text;

            var values = dt.AsEnumerable()
                .Select(r => r[columnName]?.ToString()?.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(v => v)
                .ToList();

            comboBox.ItemsSource = values;

            if (selectedId.HasValue)
                return;

            if (values.Count == 1)
            {
                comboBox.SelectedItem = values[0];
            }
            else
            {
                comboBox.SelectedIndex = -1;
                comboBox.Text = string.Empty;

                if (!string.IsNullOrWhiteSpace(currentText) && values.Any(v => string.Equals(v, currentText, StringComparison.CurrentCultureIgnoreCase)))
                {
                    comboBox.Text = currentText;
                }
            }
        }

        private void ApplyDefaultSelections()
        {
            ApplyDefaultSelection(txtOkulAdi);
            ApplyDefaultSelection(txtIl);
            ApplyDefaultSelection(txtOkulMuduru);
            ApplyDefaultSelection(txtKoordMudurYrd);
            ApplyDefaultSelection(txtKoordOgretmen);
        }

        private void ApplyDefaultSelection(ComboBox comboBox)
        {
            if (comboBox.Items.Count == 1)
            {
                comboBox.SelectedIndex = 0;
            }
            else
            {
                comboBox.SelectedIndex = -1;
                comboBox.Text = string.Empty;
            }
        }

        private void ClearForm()
        {
            selectedId = null;
            ApplyDefaultSelections();
            dgOkullar.SelectedItem = null;
        }
    }
}
