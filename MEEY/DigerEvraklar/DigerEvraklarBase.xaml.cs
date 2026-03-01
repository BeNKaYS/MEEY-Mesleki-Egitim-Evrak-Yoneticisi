using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MEEY.DigerEvraklar
{
    public class DosyaItem
    {
        public string FullPath    { get; set; } = "";
        public string Baslik      { get; set; } = "";
        public string Boyut       { get; set; } = "";
        public string UzantiBilgisi { get; set; } = "";

        // Renkli rozet için
        public string   RozetMetin { get; set; } = "DOC";
        public Brush    RozetRenk  { get; set; } = Brushes.Gray;
    }

    public partial class DigerEvraklarBase : UserControl
    {
        private List<DosyaItem> tumDosyalar = new();

        private static readonly string SablonlarKlasor = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "DigerEvraklar", "Sablonlar");

        public DigerEvraklarBase() => InitializeComponent();

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => YukleKlasor();

        private void YukleKlasor()
        {
            tumDosyalar.Clear();

            if (!Directory.Exists(SablonlarKlasor))
            {
                lblDosyaSayisi.Text = "Klasör bulunamadı: " + SablonlarKlasor;
                return;
            }

            foreach (var path in Directory.GetFiles(SablonlarKlasor)
                         .Where(f => !f.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
                         .OrderBy(f => Path.GetFileName(f)))
            {
                var ext  = Path.GetExtension(path).ToLowerInvariant();
                var info = new FileInfo(path);
                var (metin, renk) = GetRozet(ext);

                tumDosyalar.Add(new DosyaItem
                {
                    FullPath       = path,
                    Baslik         = Path.GetFileNameWithoutExtension(path),
                    UzantiBilgisi  = ext.TrimStart('.').ToUpper() + " belgesi",
                    Boyut          = FormatBytes(info.Length),
                    RozetMetin     = metin,
                    RozetRenk      = renk
                });
            }

            FiltreleVeGoster(txtArama?.Text ?? "");
        }

        private void FiltreleVeGoster(string arama)
        {
            var liste = tumDosyalar
                .Where(d => string.IsNullOrWhiteSpace(arama)
                         || d.Baslik.Contains(arama, StringComparison.OrdinalIgnoreCase))
                .ToList();

            lvDosyalar.ItemsSource = liste;
            lblDosyaSayisi.Text = $"Toplam {tumDosyalar.Count} dosya  |  Gösterilen: {liste.Count}";
        }

        private void txtArama_TextChanged(object sender, TextChangedEventArgs e)
            => FiltreleVeGoster(txtArama.Text);

        private void lvDosyalar_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lvDosyalar.SelectedItem is DosyaItem item) AcDosya(item.FullPath);
        }

        private void BtnAc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string path) AcDosya(path);
        }

        private void btnKlasorAc_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(SablonlarKlasor))
                Process.Start(new ProcessStartInfo { FileName = SablonlarKlasor, UseShellExecute = true });
        }

        private static void AcDosya(string path)
        {
            try { Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show("Dosya açılamadı: " + ex.Message, "Hata"); }
        }

        // Rozet: metin + arka plan rengi
        private static (string metin, Brush renk) GetRozet(string ext) => ext switch
        {
            ".pdf"  => ("PDF",  new SolidColorBrush(Color.FromRgb(192,  57,  43))),
            ".xlsx" => ("XLSX", new SolidColorBrush(Color.FromRgb( 39, 174,  96))),
            ".xls"  => ("XLS",  new SolidColorBrush(Color.FromRgb( 30, 139,  73))),
            ".docx" => ("DOCX", new SolidColorBrush(Color.FromRgb( 41, 128, 185))),
            ".doc"  => ("DOC",  new SolidColorBrush(Color.FromRgb( 31,  97, 141))),
            ".rar"  => ("RAR",  new SolidColorBrush(Color.FromRgb(125,  60, 152))),
            ".zip"  => ("ZIP",  new SolidColorBrush(Color.FromRgb(243, 156,  18))),
            _       => ("FILE", new SolidColorBrush(Color.FromRgb(127, 140, 141)))
        };

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024)           return $"{bytes} B";
            if (bytes < 1024 * 1024)    return $"{bytes / 1024} KB";
            return $"{bytes / (1024 * 1024)} MB";
        }
    }
}
