using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MEEY.Reports
{
    public static class DevamsizlikRaporu
    {
        static DevamsizlikRaporu()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static byte[] OlusturPdf(RaporGirdisi girdi)
        {
            using var ms = new MemoryStream();
            var doc = new DevamsizlikBelgesi(girdi);
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }

        public static byte[] OlusturPdf(IReadOnlyList<RaporGirdisi> girdiler)
        {
            if (girdiler == null || girdiler.Count == 0)
                throw new ArgumentException("En az bir rapor girdisi gereklidir.", nameof(girdiler));

            using var ms = new MemoryStream();
            var doc = new CokluDevamsizlikBelgesi(girdiler);
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }
    }

    internal sealed class CokluDevamsizlikBelgesi : IDocument
    {
        private readonly IReadOnlyList<RaporGirdisi> _girdiler;

        public CokluDevamsizlikBelgesi(IReadOnlyList<RaporGirdisi> girdiler)
        {
            _girdiler = girdiler;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            foreach (var girdi in _girdiler)
            {
                var tekliBelge = new DevamsizlikBelgesi(girdi);
                tekliBelge.Compose(container);
            }
        }
    }

    public class RaporGirdisi
    {
        public SchoolInfo Okul { get; set; } = new SchoolInfo("", "", "", "");
        public BusinessInfo Isletme { get; set; } = new BusinessInfo("", "", "");
        public DateTime Ay { get; set; }
        public string AyMetni { get; set; } = "";
        public string BelgeTarihi { get; set; } = "";
        public List<StudentInfo> Ogrenciler { get; set; } = new();
        public Dictionary<int, Dictionary<int, string>> DevamsizlikMap { get; set; } = new();
        public int? OkulGunu { get; set; }
        public Dictionary<int, string> OgrenciGunleri { get; set; } = new();
    }

    internal class DevamsizlikBelgesi : IDocument
    {
        private readonly RaporGirdisi _girdi;

        public DevamsizlikBelgesi(RaporGirdisi girdi)
        {
            _girdi = girdi;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15);
                page.DefaultTextStyle(ReportStyles.Normal);

                page.Content().Column(col =>
                {
                    col.Spacing(6);
                    col.Item().Element(c => BaslikBloku(c));
                    col.Item().Element(c => IsletmeBloku(c));
                    col.Item().Element(c => TabloBloku(c));
                    col.Item().Element(c => AltBilgiBloku(c));
                });
            });
        }

        private void BaslikBloku(IContainer c)
        {
            c.Column(col =>
            {
                col.Item()
                    .Border(ReportStyles.LineOuter)
                    .Padding(6)
                    .Text($"{_girdi.Okul.Ad}\nÖĞRENCİLERİNİN İŞLETMELERDE MESLEK EĞİTİMİ AYLIK DEVAM - DEVAMSIZLIK BİLDİRİM ÇİZELGESİ")
                    .Style(ReportStyles.Title)
                    .AlignCenter();
            });
        }

        private void IsletmeBloku(IContainer c)
        {
            c.Border(ReportStyles.LineOuter).Table(t =>
            {
                t.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(220);
                    cols.RelativeColumn(130);
                    cols.RelativeColumn(100);
                    cols.RelativeColumn(120);
                    cols.RelativeColumn(180);
                });

                void Baslik(string text) =>
                    t.Cell().Border(ReportStyles.Line).Padding(4).AlignCenter().AlignBottom()
                        .Text(text).Style(ReportStyles.NormalBold);

                Baslik("İŞLETME ADI");
                Baslik("İŞLETME TEL.");
                Baslik("İŞLETME E-POSTA");
                Baslik("Ait Olduğu Ay");
                Baslik("Belgenin Düzenlendiği Tarih:");

                void Deger(string text) =>
                    t.Cell().Border(ReportStyles.Line).Padding(4).AlignCenter().AlignBottom()
                        .Text(text).Style(ReportStyles.Normal);

                Deger(_girdi.Isletme.Adi);
                Deger(_girdi.Isletme.Tel);
                Deger(_girdi.Isletme.Eposta);
                Deger(_girdi.AyMetni);
                Deger(_girdi.BelgeTarihi);
            });
        }

        private void TabloBloku(IContainer c)
        {
            var ayinGunSayisi = DateTime.DaysInMonth(_girdi.Ay.Year, _girdi.Ay.Month);

            string GunAdi(DayOfWeek dow) => dow switch
            {
                DayOfWeek.Monday => "Pazartesi",
                DayOfWeek.Tuesday => "Salı",
                DayOfWeek.Wednesday => "Çarşamba",
                DayOfWeek.Thursday => "Perşembe",
                DayOfWeek.Friday => "Cuma",
                DayOfWeek.Saturday => "Cumartesi",
                DayOfWeek.Sunday => "Pazar",
                _ => ""
            };

            const float okulNoW = 42;
            const float adSoyadW = 108;
            const float alanDalW = 52;
            const float gunlerW = 24;
            const float gunW = 14;
            const float ozW = 36;
            const float ozsW = 36;
            const float toplamW = okulNoW + adSoyadW + alanDalW + gunlerW + (31 * gunW) + ozW + ozsW;

            var pageWidth = PageSizes.A4.Width;
            var margin = 15f;
            var available = pageWidth - (margin * 2);
            var scale = available / toplamW;

            float okulNoScaled = okulNoW * scale;
            float adSoyadScaled = adSoyadW * scale;
            float alanDalScaled = alanDalW * scale;
            float gunlerScaled = gunlerW * scale;
            float gunScaled = gunW * scale;
            float ozScaled = ozW * scale;
            float ozsScaled = ozsW * scale;

            float satirYuksekligi = 17f;
            float baslikYuksekligi = 54f;

            string Arkaplan(DateTime? tarih)
            {
                if (tarih == null) return Colors.White;
                if (tarih.Value.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    return Colors.Grey.Lighten3;
                return Colors.White;
            }

            c.Border(ReportStyles.LineOuter).Table(t =>
            {
                t.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(adSoyadScaled);
                    cols.ConstantColumn(okulNoScaled);
                    cols.ConstantColumn(alanDalScaled);
                    cols.ConstantColumn(gunlerScaled);

                    for (int i = 0; i < 31; i++)
                        cols.ConstantColumn(gunScaled);

                    cols.ConstantColumn(ozScaled);
                    cols.ConstantColumn(ozsScaled);
                });

                t.Header(header =>
                {
                    var grupH = 10f;
                    var gunAdH = baslikYuksekligi - grupH;

                    header.Cell().ColumnSpan(4).Border(ReportStyles.Line).MinHeight(grupH).Padding(1)
                        .AlignCenter().AlignBottom()
                        .Text("ÖĞRENCİNİN").FontSize(7.6f * scale).SemiBold();

                    for (int i = 1; i <= 31; i++)
                    {
                        var tarih = i <= ayinGunSayisi ? new DateTime(_girdi.Ay.Year, _girdi.Ay.Month, i) : (DateTime?)null;

                        var gunHucresi = header.Cell().RowSpan(2).Border(ReportStyles.Line).MinHeight(grupH + gunAdH).Padding(1)
                            .Background(Arkaplan(tarih))
                            .AlignCenter().AlignBottom().PaddingBottom(2.2f * scale);

                        if (tarih == null)
                        {
                            gunHucresi.Text("");
                        }
                        else
                        {
                            gunHucresi.RotateLeft().Text($"{i} {GunAdi(tarih.Value.DayOfWeek)}")
                                .FontSize(8.0f * scale)
                                .Bold();
                        }
                    }

                    header.Cell().ColumnSpan(2).Border(ReportStyles.Line).MinHeight(grupH).Padding(1)
                        .AlignCenter().AlignBottom()
                        .Text("Toplam Devamsızlık").FontSize(7.6f * scale).SemiBold();

                    header.Cell().Border(ReportStyles.Line).MinHeight(gunAdH).Padding(1)
                        .AlignCenter().AlignMiddle()
                        .Text("ADI SOYADI").FontSize(7.4f * scale).SemiBold();

                    void DikeyBaslik(IContainer x, string txt)
                    {
                        x.AlignCenter().AlignMiddle().RotateLeft()
                            .Text(txt).FontSize(7.4f * scale).SemiBold();
                    }

                    header.Cell().Border(ReportStyles.Line).MinHeight(gunAdH).Padding(1)
                        .Element(x => DikeyBaslik(x, "OKUL NO"));

                    header.Cell().Border(ReportStyles.Line).MinHeight(gunAdH).Padding(1)
                        .Element(x => DikeyBaslik(x, "ALAN DAL"));

                    header.Cell().Border(ReportStyles.Line).MinHeight(gunAdH).Padding(1)
                        .Element(x => DikeyBaslik(x, "GÜNLER"));

                    header.Cell().Border(ReportStyles.Line).MinHeight(gunAdH).Padding(1)
                        .Element(x => DikeyBaslik(x, "ÖZÜRLÜ"));
                    
                    header.Cell().Border(ReportStyles.Line).MinHeight(gunAdH).Padding(1)
                        .Element(x => DikeyBaslik(x, "ÖZÜRSÜZ"));
                });

                foreach (var ogr in _girdi.Ogrenciler)
                {
                    var toplamlar = ToplamHesapla(ogr.Id);

                    // ÜST SATIR (S) - Sol taraftaki hücreler RowSpan=2 ile birleştirilecek
                    
                    // Ad Soyad - 2 satır birleşik
                    t.Cell().RowSpan(2)
                        .BorderLeft(ReportStyles.Line)
                        .BorderRight(ReportStyles.Line)
                        .BorderTop(ReportStyles.Line)
                        .BorderBottom(ReportStyles.Line)
                        .PaddingLeft(2)
                        .PaddingRight(1)
                        .PaddingVertical(1)
                        .AlignMiddle()
                        .AlignLeft()
                        .Text(ogr.AdSoyad)
                        .FontSize(9.0f * scale);
                    
                    // Okul No - 2 satır birleşik
                    t.Cell().RowSpan(2)
                        .BorderLeft(ReportStyles.Line)
                        .BorderRight(ReportStyles.Line)
                        .BorderTop(ReportStyles.Line)
                        .BorderBottom(ReportStyles.Line)
                        .PaddingLeft(2)
                        .PaddingRight(1)
                        .PaddingVertical(1)
                        .AlignMiddle()
                        .AlignCenter()
                        .Text(ogr.OkulNo)
                        .FontSize(9.0f * scale);
                    
                    // Alan/Dal - 2 satır birleşik
                    t.Cell().RowSpan(2)
                        .BorderLeft(ReportStyles.Line)
                        .BorderRight(ReportStyles.Line)
                        .BorderTop(ReportStyles.Line)
                        .BorderBottom(ReportStyles.Line)
                        .PaddingLeft(2)
                        .PaddingRight(1)
                        .PaddingVertical(1)
                        .AlignLeft()
                        .AlignMiddle()
                        .Text($"{ogr.Alan} {ogr.Dal}".Trim())
                        .FontSize(7.0f * scale);

                    // S etiketi
                    t.Cell().Border(ReportStyles.Line).MinHeight(satirYuksekligi).Padding(0).AlignCenter().AlignMiddle()
                        .Text("S").Style(ReportStyles.NormalBold);

                    // Günler (S satırı)
                    for (int i = 1; i <= 31; i++)
                    {
                        var tarih = i <= ayinGunSayisi ? new DateTime(_girdi.Ay.Year, _girdi.Ay.Month, i) : (DateTime?)null;
                        var sembol = SembolGetir(ogr.Id, tarih);

                        t.Cell().Border(ReportStyles.Line).MinHeight(satirYuksekligi).Padding(0).Background(Arkaplan(tarih))
                            .AlignCenter().AlignMiddle()
                            .Element(cell =>
                            {
                                if (!string.IsNullOrWhiteSpace(sembol))
                                {
                                    cell.PaddingTop(1.6f * scale).AlignCenter().AlignMiddle()
                                        .Text(sembol)
                                        .FontSize(8.6f * scale)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                }
                            });
                    }

                    // Toplamlar (S satırı) - 2 satır birleşik
                    t.Cell().RowSpan(2)
                        .BorderLeft(ReportStyles.Line)
                        .BorderRight(ReportStyles.Line)
                        .BorderTop(ReportStyles.Line)
                        .BorderBottom(ReportStyles.Line)
                        .Padding(1)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text(toplamlar.Ozurlu == 0 ? string.Empty : toplamlar.Ozurlu.ToString())
                        .FontSize(8.2f * scale)
                        .Bold();
                    
                    t.Cell().RowSpan(2)
                        .BorderLeft(ReportStyles.Line)
                        .BorderRight(ReportStyles.Line)
                        .BorderTop(ReportStyles.Line)
                        .BorderBottom(ReportStyles.Line)
                        .Padding(1)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text(toplamlar.Ozursuz == 0 ? string.Empty : toplamlar.Ozursuz.ToString())
                        .FontSize(8.2f * scale)
                        .Bold();

                    // ALT SATIR (Ö)
                    // Sol taraftaki hücreler zaten RowSpan=2 ile yukarıda tanımlandı, tekrar eklenmeyecek

                    // Ö etiketi
                    t.Cell().Border(ReportStyles.Line).MinHeight(satirYuksekligi).Padding(0).AlignCenter().AlignMiddle()
                        .Text("Ö").Style(ReportStyles.NormalBold);

                    // Günler (Ö satırı)
                    for (int i = 1; i <= 31; i++)
                    {
                        var tarih = i <= ayinGunSayisi ? new DateTime(_girdi.Ay.Year, _girdi.Ay.Month, i) : (DateTime?)null;
                        var sembol = SembolGetir(ogr.Id, tarih);

                        t.Cell().Border(ReportStyles.Line).MinHeight(satirYuksekligi).Padding(0).Background(Arkaplan(tarih))
                            .AlignCenter().AlignMiddle()
                            .Element(cell =>
                            {
                                if (!string.IsNullOrWhiteSpace(sembol))
                                {
                                    cell.PaddingTop(1.6f * scale).AlignCenter().AlignMiddle()
                                        .Text(sembol)
                                        .FontSize(8.6f * scale)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                }
                            });
                    }
                    
                    // Toplamlar zaten RowSpan=2 ile yukarıda tanımlandı
                }
            });
        }

        private void AltBilgiBloku(IContainer c)
        {
            c.PaddingTop(6).Column(col =>
            {
                col.Spacing(3);
                col.Item().Element(ImzaVeSembolBloku);
                col.Item().Element(DipnotBloku);
            });
        }

        private void ImzaVeSembolBloku(IContainer c)
        {
            c.Row(row =>
            {
                row.RelativeItem(1).Border(ReportStyles.LineOuter).Height(85).Padding(4).Column(col =>
                {
                    col.Item().Text("İŞLETME YETKİLİSİ").Style(ReportStyles.NormalBold).AlignCenter();
                    col.Item().PaddingTop(6).Text("...../...../20.....").AlignCenter().FontSize(9);
                    col.Item().PaddingTop(12).Text("ADI SOYADI").AlignCenter().FontSize(9);
                    col.Item().PaddingTop(2).Text("Kaşe - İmza").AlignCenter().FontSize(8);
                });

                row.RelativeItem(1).Border(ReportStyles.LineOuter).Height(85).Padding(4).Column(col =>
                {
                    col.Item().Text("İNCELENDİ").Style(ReportStyles.NormalBold).AlignCenter();
                    col.Item().PaddingTop(2).Text(_girdi.Okul.MudurYardimcisi).AlignCenter().FontSize(9).Bold();
                    col.Item().PaddingTop(4).Text("Koordinatör Müdür Yardımcısı").AlignCenter().FontSize(8);
                    col.Item().PaddingTop(6).Text("...../...../20.....").AlignCenter().FontSize(9);
                    col.Item().PaddingTop(4).Text("İmza").AlignCenter().FontSize(8);
                });

                row.RelativeItem(1.15f).Column(col =>
                {
                    col.Item().Border(ReportStyles.LineOuter).Height(85).Padding(4).Column(x =>
                    {
                        x.Item().Text("Devamın Göstereceği Semboller").Style(ReportStyles.NormalBold).AlignCenter().FontSize(8);

                        x.Item().PaddingTop(2).Table(t =>
                        {
                            t.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });

                            t.Cell().Text("X: İşletmede").FontSize(7);
                            t.Cell().Text("O: Okulda").FontSize(7);
                        });

                        x.Item().PaddingTop(4).Text("Devamsızlığın Göstereceği Semboller").Style(ReportStyles.NormalBold).AlignCenter().FontSize(8);

                        x.Item().PaddingTop(2).Table(t =>
                        {
                            t.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });

                            void Satir(string sol, string sag)
                            {
                                t.Cell().Text(sol).FontSize(7);
                                t.Cell().Text(sag).FontSize(7);
                            }

                            Satir("İ: İzinli", "D: Özürsüz Devamsız");
                            Satir("H: Hasta Sevkli", "S: Sabah");
                            Satir("R: Raporlu", "Ö: Öğle");
                            Satir("T: Resmi Tatil", "");
                        });
                    });
                });
            });
        }

        private void DipnotBloku(IContainer c)
        {
            c.Border(ReportStyles.LineOuter)
                .Padding(3)
                .Text(txt =>
                {
                    txt.DefaultTextStyle(ReportStyles.Normal.FontSize(6.5f));
                    txt.Line("Bu çizelge, işletme tarafından tutulacak, öğrencinin işletmede bulunması gereken günlere ait devamsızlık durumları");
                    txt.Line("ilgili sütunda, yanda gösterilen uygun sembollerle belirtilecektir.");
                    txt.Line("(İ),(H),ve (R) ile gösterilen devamsızlıklar toplamı özürlü devamsızlık sütununa yazılacaktır.");
                });
        }

        private string SembolGetir(int ogrenciId, DateTime? tarih)
        {
            if (tarih == null) return "";
            
            if (_girdi.DevamsizlikMap.TryGetValue(ogrenciId, out var gunler))
            {
                if (gunler.TryGetValue(tarih.Value.Day, out var sembol))
                {
                    return sembol ?? "";
                }
            }
            
            return "";
        }

        private (int Ozurlu, int Ozursuz) ToplamHesapla(int ogrenciId)
        {
            int ozurlu = 0;
            int ozursuz = 0;

            if (_girdi.DevamsizlikMap.TryGetValue(ogrenciId, out var gunler))
            {
                foreach (var sembol in gunler.Values)
                {
                    if (string.IsNullOrWhiteSpace(sembol)) continue;

                    var s = sembol.Trim().ToUpper();
                    if (s == "İ" || s == "H" || s == "R")
                        ozurlu++;
                    else if (s == "D")
                        ozursuz++;
                }
            }

            return (ozurlu, ozursuz);
        }
    }
}
