using System.Collections.Generic;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MEEY.Reports
{
    // ── Veri modelleri ──────────────────────────────────────────────────── //

    public record HaftalikGorevSatiri(
        string IsletmeAdi,
        string OgrenciAdSoyad,
        string UstaAdSoyad);

    public record HaftalikGorevGirdisi(
        string KoordinatorOgretmen,
        string MudurYrd,
        string SchoolName,
        string OgretimYili,
        string KoordTuru,
        string AyMetni,
        string HaftaMetni,
        IReadOnlyList<HaftalikGorevSatiri> Satirlar);

    // ── PDF Üretici ─────────────────────────────────────────────────────── //

    public static class HaftalikGorevTakipRaporu
    {
        static HaftalikGorevTakipRaporu()
            => QuestPDF.Settings.License = LicenseType.Community;

        /// <summary>
        /// Boş şablon oluşturur (veri bağlantısı gerektirmez).
        /// </summary>
        public static byte[] OlusturBosRapor(
            string ogretmenAdi,
            string okulAdi,
            string mudurYrd,
            string ogretimYili,
            string ayMetni,
            string koordTuru,
            string haftaMetni,
            int    satirSayisi = 5)
        {
            var satirlar = new List<HaftalikGorevSatiri>();
            for (int i = 0; i < satirSayisi; i++)
                satirlar.Add(new HaftalikGorevSatiri("", "", ""));

            var girdi = new HaftalikGorevGirdisi(
                KoordinatorOgretmen: ogretmenAdi,
                MudurYrd:            mudurYrd,
                SchoolName:          okulAdi,
                OgretimYili:         ogretimYili,
                KoordTuru:           koordTuru,
                AyMetni:             ayMetni,
                HaftaMetni:          haftaMetni,
                Satirlar:            satirlar);

            return OlusturPdf(new[] { girdi });
        }

        public static byte[] OlusturPdf(IReadOnlyList<HaftalikGorevGirdisi> girdiler)
        {
            using var ms = new MemoryStream();
            new HaftalikGorevBelgesi(girdiler).GeneratePdf(ms);
            return ms.ToArray();
        }
    }

    // ── Belge Sınıfı ────────────────────────────────────────────────────── //

    internal sealed class HaftalikGorevBelgesi : IDocument
    {
        private readonly IReadOnlyList<HaftalikGorevGirdisi> _girdiler;

        public HaftalikGorevBelgesi(IReadOnlyList<HaftalikGorevGirdisi> girdiler)
            => _girdiler = girdiler;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            foreach (var g in _girdiler)
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));
                    page.Content().Element(c => Form(c, g));
                });
        }

        // ─────────────────────────────────────────────────────────────────── //
        private static void Form(IContainer root, HaftalikGorevGirdisi g)
        {
            root.Border(1).BorderColor(Colors.Black).Column(col =>
            {
                // ── BAŞLIK BLOĞU ────────────────────────────────────────── //
                col.Item().BorderBottom(1).BorderColor(Colors.Black)
                   .Padding(5)
                   .AlignCenter()
                   .Text(string.IsNullOrWhiteSpace(g.SchoolName)
                         ? "OKUL ADI"
                         : g.SchoolName.ToUpperInvariant())
                   .FontSize(11).Bold();

                col.Item().BorderBottom(1).BorderColor(Colors.Black)
                   .Padding(4).AlignCenter()
                   .Text($"İŞLETMELERDE MESLEK EĞİTİMİ ({g.KoordTuru}) UYGULAMASINDA")
                   .FontSize(9.5f).Bold();

                col.Item().BorderBottom(1).BorderColor(Colors.Black)
                   .Padding(4).AlignCenter()
                   .Text("GÖREVLİ KOORDİNATÖR ÖĞRETMEN HAFTALIK GÖREV FORMU")
                   .FontSize(9.5f).Bold();

                // ── BİLGİ SATIRI ─────────────────────────────────────────── //
                col.Item().BorderBottom(1).BorderColor(Colors.Black)
                   .Padding(5).Row(row =>
                {
                    // Sol: Koordinatör
                    row.RelativeItem(3).Row(r =>
                    {
                        r.AutoItem()
                         .Text("KOORDİNATÖR ÖĞRETMEN : ")
                         .Bold().FontSize(8.5f);

                        r.RelativeItem()
                         .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                         .PaddingLeft(4)
                         .Text(string.IsNullOrWhiteSpace(g.KoordinatorOgretmen)
                               ? "                                    "
                               : g.KoordinatorOgretmen.ToUpperInvariant())
                         .Bold().FontSize(8.5f);
                    });

                    // Orta: Öğretim Yılı
                    row.RelativeItem(2).AlignCenter().Row(r =>
                    {
                        r.AutoItem()
                         .Text("ÖĞRETİM YILI : ")
                         .Bold().FontSize(8.5f);
                        r.AutoItem()
                         .Text(string.IsNullOrWhiteSpace(g.OgretimYili) ? "........./........." : g.OgretimYili)
                         .FontSize(8.5f);
                    });

                    // Sağ: Ay + Hafta
                    row.RelativeItem(2).AlignRight().Row(r =>
                    {
                        r.AutoItem()
                         .Text($"{(string.IsNullOrWhiteSpace(g.AyMetni) ? "........" : g.AyMetni.ToUpperInvariant())} AYI  ")
                         .Bold().FontSize(8.5f);
                        r.AutoItem()
                         .Text($"{g.HaftaMetni}")
                         .Bold().FontSize(8.5f);
                    });
                });

                // ── ANA TABLO ───────────────────────────────────────────── //
                col.Item().Table(t =>
                {
                    // Sütun genişlikleri (A4 landscape ~800pt iç)
                    t.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(30);   // S.N.
                        c.RelativeColumn(3.2f); // İşletme Adı
                        c.RelativeColumn(1.8f); // Tarih
                        c.RelativeColumn(1.4f); // Giriş Saati
                        c.RelativeColumn(1.4f); // Çıkış Saati
                        c.RelativeColumn(2.5f); // Öğrenci Adı
                        c.RelativeColumn(1.6f); // Öğrenci İmza
                        c.RelativeColumn(2.5f); // Usta Adı
                        c.RelativeColumn(1.6f); // Usta İmza
                    });

                    // ── Başlık satırı 1 ───────────────────────────────── //
                    void Hdr1(string txt, uint colSpan = 1, uint rowSpan = 2)
                    {
                        t.Cell()
                         .RowSpan(rowSpan).ColumnSpan(colSpan)
                         .Border(1).BorderColor(Colors.Black)
                         .Background("#D0D0D0")
                         .Padding(3).AlignCenter().AlignMiddle()
                         .Text(txt).Bold().FontSize(7.5f);
                    }

                    // S.N. — tek hücre 2 satır
                    Hdr1("S\nN");
                    // İşletme — tek hücre 2 satır
                    Hdr1("İŞLETMENİN\nADI");
                    // Tarih — 2 satır
                    Hdr1("İŞLETMEYE\nGİTTİĞİ\nTARİH");
                    // Saat bilgileri birleşik
                    t.Cell().ColumnSpan(2)
                     .Border(1).BorderColor(Colors.Black)
                     .Background("#D0D0D0")
                     .Padding(3).AlignCenter().AlignMiddle()
                     .Text("SAATLER").Bold().FontSize(7.5f);
                    // Öğrenci grubu başlığı
                    t.Cell().ColumnSpan(2)
                     .Border(1).BorderColor(Colors.Black)
                     .Background("#D0D0D0")
                     .Padding(3).AlignCenter().AlignMiddle()
                     .Text("İŞLETMEDEKİ ÖĞRENCİNİN").Bold().FontSize(7.5f);
                    // Usta grubu başlığı
                    t.Cell().ColumnSpan(2)
                     .Border(1).BorderColor(Colors.Black)
                     .Background("#D0D0D0")
                     .Padding(3).AlignCenter().AlignMiddle()
                     .Text("USTA ÖĞRETİCİ /\nEĞİTİM SORUMLUSUNUN").Bold().FontSize(7.5f);

                    // ── Başlık satırı 2 (alt başlıklar) ───────────────── //
                    void Hdr2(string txt)
                    {
                        t.Cell()
                         .Border(1).BorderColor(Colors.Black)
                         .Background("#D0D0D0")
                         .Padding(3).AlignCenter().AlignMiddle()
                         .Text(txt).Bold().FontSize(7f);
                    }

                    Hdr2("GİRİŞ\nSAATİ");
                    Hdr2("ÇIKIŞ\nSAATİ");
                    Hdr2("ADI SOYADI");
                    Hdr2("İMZASI");
                    Hdr2("ADI SOYADI");
                    Hdr2("İMZASI");

                    // ── Veri Satırları ─────────────────────────────────── //
                    int maxRows = System.Math.Max(5, g.Satirlar.Count);

                    for (int i = 0; i < maxRows; i++)
                    {
                        var s = i < g.Satirlar.Count ? g.Satirlar[i] : null;

                        void DataCell(string text, float fontSize = 8f, bool bold = false)
                        {
                            var cell = t.Cell()
                                        .Border(1).BorderColor(Colors.Grey.Lighten1)
                                        .MinHeight(22)
                                        .PaddingHorizontal(4).PaddingVertical(2)
                                        .AlignCenter().AlignMiddle();

                            var txt = cell.Text(text).FontSize(fontSize);
                            if (bold) txt.Bold();
                        }

                        DataCell((i + 1).ToString(), 8f, true); // S.N.
                        DataCell(s?.IsletmeAdi ?? "");           // İşletme
                        DataCell("");                             // Tarih
                        DataCell("");                             // Giriş saati
                        DataCell("");                             // Çıkış saati
                        DataCell(s?.OgrenciAdSoyad ?? "");       // Öğrenci
                        DataCell("");                             // Öğrenci imza
                        DataCell(s?.UstaAdSoyad ?? "");          // Usta
                        DataCell("");                             // Usta imza
                    }
                });

                // ── İMZA SATIRI ──────────────────────────────────────────── //
                col.Item().BorderTop(1).BorderColor(Colors.Black)
                   .Padding(8).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter()
                         .Text("İMZA")
                         .FontSize(8).Bold();
                        c.Item().PaddingTop(16).AlignCenter()
                         .Text(string.IsNullOrWhiteSpace(g.KoordinatorOgretmen)
                               ? "......................................"
                               : g.KoordinatorOgretmen.ToUpperInvariant())
                         .FontSize(9).Bold();
                        c.Item().AlignCenter()
                         .Text("KOORDİNATÖR ÖĞRETMEN")
                         .FontSize(7.5f);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter()
                         .Text("İMZA")
                         .FontSize(8).Bold();
                        c.Item().PaddingTop(16).AlignCenter()
                         .Text(string.IsNullOrWhiteSpace(g.MudurYrd)
                               ? "......................................"
                               : g.MudurYrd.ToUpperInvariant())
                         .FontSize(9).Bold();
                        c.Item().AlignCenter()
                         .Text("KOORDİNATÖR MDR. YRD.")
                         .FontSize(7.5f);
                    });
                });
            });
        }
    }
}
