using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MEEY.Database;
using Microsoft.Win32;

namespace MEEY.Database
{
    public partial class VeriTabaniYonetimi : UserControl
    {
        private string? currentTableName;
        private string? currentDisplayName;

        public VeriTabaniYonetimi()
        {
            InitializeComponent();
        }

        public void LoadTableData(string tableName, string displayName)
        {
            try
            {
                currentTableName = tableName;
                currentDisplayName = displayName;
                txtTabloBaslik.Text = displayName;
                
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    string query = $"SELECT * FROM {tableName}";
                    
                    using (var adapter = new SQLiteDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        
                        dgVeriTabani.ItemsSource = dataTable.DefaultView;
                        txtKayitSayisi.Text = $"Toplam {dataTable.Rows.Count} kayıt";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yükleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExportToExcel()
        {
            if (dgVeriTabani.ItemsSource == null)
            {
                MessageBox.Show("Lütfen önce bir tablo seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Dosyası (*.csv)|*.csv",
                    FileName = $"{txtTabloBaslik.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var dataView = dgVeriTabani.ItemsSource as DataView;
                    if (dataView != null)
                    {
                        ExportToCSV(dataView.Table, saveFileDialog.FileName);
                        MessageBox.Show("Veriler başarıyla CSV formatında dışa aktarıldı!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dışa aktarma hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToCSV(DataTable dataTable, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Başlıkları yaz
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    writer.Write(dataTable.Columns[i].ColumnName);
                    if (i < dataTable.Columns.Count - 1)
                        writer.Write(",");
                }
                writer.WriteLine();

                // Verileri yaz
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        string value = row[i].ToString()?.Replace(",", ";") ?? "";
                        writer.Write(value);
                        if (i < dataTable.Columns.Count - 1)
                            writer.Write(",");
                    }
                    writer.WriteLine();
                }
            }
        }

        public void BackupDatabase()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "SQLite Veritabanı (*.db)|*.db",
                    FileName = $"MEEY_Yedek_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string sourceFile = DatabaseManager.GetDatabaseFilePath();
                    
                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, saveFileDialog.FileName, true);
                        MessageBox.Show("Veritabanı başarıyla yedeklendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Veritabanı dosyası bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yedekleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ImportDatabase()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "SQLite Veritabanı (*.db)|*.db",
                    Title = "İçe Aktarılacak Veritabanını Seçin"
                };

                if (openFileDialog.ShowDialog() != true)
                    return;

                var result = MessageBox.Show(
                    "Seçtiğiniz veritabanı mevcut veritabanının üzerine yazılacak. Devam etmek istiyor musunuz?",
                    "Veritabanı İçe Aktar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                string targetFile = DatabaseManager.GetDatabaseFilePath();
                string targetDirectory = Path.GetDirectoryName(targetFile) ?? AppDomain.CurrentDomain.BaseDirectory;
                string backupFile = Path.Combine(
                    targetDirectory,
                    $"MEEY_AutoBackup_{DateTime.Now:yyyyMMdd_HHmmss}.db");

                if (File.Exists(targetFile))
                {
                    File.Copy(targetFile, backupFile, true);
                }

                File.Copy(openFileDialog.FileName, targetFile, true);

                DatabaseManager.InitializeDatabase();

                if (!string.IsNullOrWhiteSpace(currentTableName) && !string.IsNullOrWhiteSpace(currentDisplayName))
                    LoadTableData(currentTableName, currentDisplayName);
                else
                    txtKayitSayisi.Text = "Veritabanı içe aktarıldı.";

                MessageBox.Show("Veritabanı başarıyla içe aktarıldı!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İçe aktarma hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
