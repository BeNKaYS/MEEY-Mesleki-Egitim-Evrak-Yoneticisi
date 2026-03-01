using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MEEY.Reports
{
    public static class GunlukRehberlikRaporu
    {
        static GunlukRehberlikRaporu()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static byte[] OlusturPdf(IReadOnlyList<GunlukRehberlikGorevGirdisi> girdiler)
            => OlusturPdf(girdiler, 1);

        public static byte[] OlusturPdf(IReadOnlyList<GunlukRehberlikGorevGirdisi> girdiler, int nup)
        {
            using var ms = new MemoryStream();
            var doc = new GunlukRehberlikBelgesi(girdiler, nup);
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }
    }

    public record GunlukRehberlikGorevGirdisi(
        string IsletmeAdi,
        int OgrenciSayisi,
        string AlanDal,
        DateTime GorevTarihi,
        string KoordinatorOgretmen,
        string KoordinatorMudurYrd,
        string SchoolName,
        string BelgeTarihi,
        string Sorun,
        string Cozum,
        string Sonuc);

    internal sealed class GunlukRehberlikBelgesi : IDocument
    {
        private readonly IReadOnlyList<GunlukRehberlikGorevGirdisi> _girdiler;
        private readonly int _nup;

        public GunlukRehberlikBelgesi(IReadOnlyList<GunlukRehberlikGorevGirdisi> girdiler, int nup)
        {
            _girdiler = girdiler;
            _nup = (nup == 2) ? 2 : 1;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var perPage = _nup;
            var contentHeight = PageSizes.A4.Height - (15 * 2);
            const float sectionSpacing = 8f;
            var sectionHeight = perPage == 1
                ? contentHeight
                : (contentHeight - sectionSpacing * (perPage - 1)) / perPage;

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15);
                page.DefaultTextStyle(ReportStyles.Normal);

                page.Content().Column(col =>
                {
                    for (int i = 0; i < _girdiler.Count; i += perPage)
                    {
                        var chunk = _girdiler.Skip(i).Take(perPage).ToList();

                        col.Item().Column(section =>
                        {
                            section.Spacing(sectionSpacing);

                            foreach (var girdi in chunk)
                            {
                                section.Item()
                                    .Height(sectionHeight)
                                    .ScaleToFit()
                                    .Element(c => Form(c, girdi));
                            }
                        });

                        if (i + perPage < _girdiler.Count)
                            col.Item().PageBreak();
                    }
                });
            });
        }

        private static void Form(IContainer c, GunlukRehberlikGorevGirdisi girdi)
        {
            c.Border(1).Column(col =>
            {
                col.Item().BorderBottom(1).PaddingVertical(8).PaddingHorizontal(6)
                    .Text("İŞLETMELERDE MESLEK EĞİTİMİ GÜNLÜK REHBERLİK GÖREV FORMU")
                    .FontSize(13).Bold().AlignCenter();

                col.Item().BorderBottom(1).Padding(6).Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(210);
                        cols.ConstantColumn(10);
                        cols.RelativeColumn();
                    });

                    void Satir(string baslik, string deger)
                    {
                        t.Cell().PaddingVertical(3).Text(baslik).FontSize(11);
                        t.Cell().PaddingVertical(3).AlignCenter().Text(":").FontSize(11);
                        t.Cell().PaddingVertical(3).Text(string.IsNullOrWhiteSpace(deger) ? "-" : deger).FontSize(11);
                    }

                    Satir("İşletmenin Adı", girdi.IsletmeAdi);
                    Satir("İzlemede Sorumlu Olduğu Öğrenci Sayısı", girdi.OgrenciSayisi.ToString());
                    Satir("Meslek Alan Dalı", girdi.AlanDal);
                    Satir("Görev Tarihi", girdi.GorevTarihi.ToString("dd.MM.yyyy"));
                });

                col.Item().BorderBottom(1).Padding(8).MinHeight(300).Column(nots =>
                {
                    nots.Item().Text("Aylık Rehberlik Formuna Göre:").Bold().FontSize(11);
                    nots.Item().PaddingTop(6).Text("İşletmede öğrenim gören öğrencilerin eğitimini olumsuz yönde etkileyen hususlar (varsa yazınız):").FontSize(11);
                    nots.Item().PaddingTop(4).Text(string.IsNullOrWhiteSpace(girdi.Sorun) ? "" : girdi.Sorun).FontSize(11).LineHeight(1.2f);
                    
                    nots.Item().PaddingTop(15).Text("Belirlenen aksaklıklarla ilgili yapılan rehberlik ve alınan önlemler:").FontSize(11);
                    nots.Item().PaddingTop(4).Text(string.IsNullOrWhiteSpace(girdi.Cozum) ? "" : girdi.Cozum).FontSize(11).LineHeight(1.2f);
                    
                    nots.Item().PaddingTop(15).Text("Aylık rehberlik formunda belirtilmesinde yarar görülen hususlar:").FontSize(11);
                    nots.Item().PaddingTop(4).Text(string.IsNullOrWhiteSpace(girdi.Sonuc) ? "" : girdi.Sonuc).FontSize(11).LineHeight(1.2f);
                });

                col.Item().BorderBottom(1).Padding(8).Row(row =>
                {
                    row.RelativeItem().Column(c1 =>
                    {
                        c1.Item().Text("İşletme Eğitim Yetkilisi").Bold().AlignCenter().FontSize(11);
                        c1.Item().PaddingTop(14).Text("İmza").AlignCenter().FontSize(11);
                    });

                    row.RelativeItem().Column(c2 =>
                    {
                        c2.Item().Text("Koordinatör Öğretmen").Bold().AlignCenter().FontSize(11);
                        c2.Item().PaddingTop(4).Text(string.IsNullOrWhiteSpace(girdi.KoordinatorOgretmen) ? "-" : girdi.KoordinatorOgretmen)
                            .AlignCenter().FontSize(11);
                        c2.Item().PaddingTop(10).Text("İmza").AlignCenter().FontSize(11);
                    });

                    row.RelativeItem().Column(c3 =>
                    {
                        c3.Item().Text("Koordinatör Müdür Yardımcısı").Bold().AlignCenter().FontSize(11);
                        c3.Item().PaddingTop(4).Text(string.IsNullOrWhiteSpace(girdi.KoordinatorMudurYrd) ? "-" : girdi.KoordinatorMudurYrd)
                            .AlignCenter().FontSize(11);
                        c3.Item().PaddingTop(10).Text("İmza").AlignCenter().FontSize(11);
                    });
                });

                col.Item().Padding(8).Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(8));
                    t.Span("Açıklamalar: Bu form koordinatör öğretmen tarafından her görev için görev haftası başında koordinatör müdür ");
                    t.Span("yardımcısından alınır. Görev sonrasında okula geldiği gün içinde imzalanıp tamamlanmış olarak koordinatör müdür yardımcısına ");
                    t.Span("teslim edilir. Bu form Aylık Rehberlik Formu'nun doldurulmasında esas alınır ve rapora eklenir.");
                });
            });
        }
    }
}
