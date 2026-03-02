using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MEEY.Reports
{
    public static class AylikRehberlikRaporu
    {
        static AylikRehberlikRaporu()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static byte[] OlusturPdf(IReadOnlyList<AylikRehberlikGirdisi> girdiler, int nup = 1)
        {
            using var ms = new MemoryStream();
            nup = nup == 2 ? 2 : 1;
            new AylikRehberlikBelgesi(girdiler, nup).GeneratePdf(ms);
            return ms.ToArray();
        }
    }

    public sealed record AylikRehberlikGirdisi(
        string OkulIl,
        string OkulAdi,
        string KoordinatorOgretmen,
        string KoordinatorMudurYrd,
        string IsletmeAdiAdres,
        string AlanDal,
        IReadOnlyList<string> Ogrenciler,
        string GorevliGun,
        IReadOnlyList<DateTime> ZiyaretTarihleri,
        IReadOnlyList<string>? ZiyaretTarihiEtiketleri,
        string AyText,
        bool AutoDoldur,
        bool? UstaBelgesiVar
    );

    internal sealed class AylikRehberlikBelgesi : IDocument
    {
        private readonly IReadOnlyList<AylikRehberlikGirdisi> _girdiler;
        private readonly int _nup;

        public AylikRehberlikBelgesi(IReadOnlyList<AylikRehberlikGirdisi> girdiler, int nup)
        {
            _girdiler = girdiler;
            _nup = nup;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(_nup == 2 ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.Margin(_nup == 2 ? 10 : 18);
                page.DefaultTextStyle(ReportStyles.Normal);

                page.Content().Column(col =>
                {
                    if (_nup == 1)
                    {
                        for (int i = 0; i < _girdiler.Count; i++)
                        {
                            col.Item().Element(c => TekForm(c, _girdiler[i]));
                            if (i < _girdiler.Count - 1)
                                col.Item().PageBreak();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _girdiler.Count; i += 2)
                        {
                            var sol = _girdiler[i];
                            AylikRehberlikGirdisi? sag = (i + 1 < _girdiler.Count) ? _girdiler[i + 1] : null;

                            col.Item().Row(r =>
                            {
                                r.RelativeItem().PaddingRight(4).ScaleToFit().Element(c => TekForm(c, sol, compact: true));
                                r.RelativeItem().PaddingLeft(4).ScaleToFit().Element(c =>
                                {
                                    if (sag is not null)
                                        TekForm(c, sag, compact: true);
                                });
                            });

                            if (i + 2 < _girdiler.Count)
                                col.Item().PageBreak();
                        }
                    }
                });
            });
        }

        private static void TekForm(IContainer container, AylikRehberlikGirdisi it, bool compact = false)
        {
            var tarihler = it.ZiyaretTarihleri?.Distinct().OrderBy(x => x).ToList() ?? new List<DateTime>();
            var tarihEtiketleri = (it.ZiyaretTarihiEtiketleri?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList())
                                 ?? tarihler.Select(x => x.ToString("dd.MM.yyyy")).ToList();

            var pad = compact ? 6 : 10;
            var sp = compact ? 4 : 6;
            var titleSize = compact ? 9f : 10f;
            var headerSize = compact ? 8f : 9f;
            var bodySize = compact ? 7.2f : 8f;
            var businessSize = compact ? 7.4f : 8.2f;
            var tableHeaderSize = compact ? 7.2f : 8f;
            var tableBodySize = compact ? 6.6f : 7.6f;
            var cellPad = compact ? 2 : 4;
            var datesBoxW = compact ? 120 : 150;

            container.Border(ReportStyles.LineOuter).Padding(pad).Column(col =>
            {
                col.Spacing(sp);

                col.Item().AlignCenter().Text(
                        "İŞLETMELERDE MESLEK EĞİTİMİ KOORDİNATÖRLERİNİN İŞLETMEYE YAPACAĞI\n" +
                        "AYLIK REHBERLİK RAPOR FORMU")
                    .Style(ReportStyles.Title.FontSize(titleSize));

                col.Item().Row(r =>
                {
                    r.RelativeItem();
                    r.ConstantItem(360).AlignCenter().Text($"{it.OkulAdi} MÜDÜRLÜĞÜ'NE").FontSize(headerSize).SemiBold();
                    r.RelativeItem();
                });

                col.Item().Row(r =>
                {
                    r.RelativeItem();
                    r.AutoItem().PaddingRight(110).Text(it.OkulIl).FontSize(headerSize).SemiBold();
                });

                col.Item().PaddingRight(8).Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(bodySize));
                    t.Span("Okulumuz ");
                    t.Span(it.AlanDal).SemiBold();
                    t.Span(" alanı öğrencilerinin meslek eğitimi gördüğü işletmelerde yapmış olduğum bir aylık koordinatörlük görevlerim sırasında tespit ettiğim hususlar aşağıda belirtilmiştir.");
                });
                col.Item().PaddingRight(8).Text("Gereğini bilgilerinize arz ederim.").FontSize(bodySize);

                col.Item().AlignLeft().Text($"İşletmenin Adı ve Adresi: {it.IsletmeAdiAdres}")
                    .FontSize(businessSize);

                col.Item().PaddingTop(compact ? 6 : 10).Row(r =>
                {
                    r.RelativeItem().AlignCenter().Text("İşletme Eğitim\nYetkilisi\nAdı Soyadı").FontSize(bodySize).SemiBold();
                    r.RelativeItem().AlignCenter().Text("Koordinatör Öğretmen\nAdı Soyadı\n\n" + it.KoordinatorOgretmen).FontSize(bodySize).SemiBold();

                    r.ConstantItem(datesBoxW).Padding(compact ? 4 : 6).Column(b =>
                    {
                        b.Spacing(2);
                        b.Item().PaddingTop(2).AlignCenter().Text("GÖREV TARİHLERİ").FontSize(bodySize).SemiBold().Underline();
                        if (tarihler.Count == 0)
                        {
                            b.Item().AlignCenter().Text("").FontSize(bodySize);
                            b.Item().AlignCenter().Text("").FontSize(bodySize);
                        }
                        else
                        {
                            foreach (var tarihText in tarihEtiketleri.Take(6))
                                b.Item().AlignCenter().Text(tarihText).FontSize(bodySize);
                        }
                    });
                });

                col.Item().PaddingTop(6).Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(bodySize));
                    t.Span("Öğrencilerin adı soyadı: ");
                    var liste = it.Ogrenciler?.ToList() ?? new List<string>();
                    if (liste.Count == 0)
                    {
                        t.Span("");
                        return;
                    }

                    t.Span(string.Join(", ", liste));
                });

                col.Item().PaddingTop(4).Border(ReportStyles.Line).Table(t =>
                {
                    t.ColumnsDefinition(cd =>
                    {
                        cd.RelativeColumn(compact ? 4.2f : 3.2f);
                        cd.RelativeColumn(compact ? 1.3f : 2.2f);
                    });

                    t.Cell().BorderBottom(ReportStyles.Line).BorderRight(ReportStyles.Line).Padding(cellPad)
                        .AlignCenter().Text("Koordinatörün Rehberlik Yaptığı Konular").FontSize(tableHeaderSize).SemiBold();
                    t.Cell().BorderBottom(ReportStyles.Line).Padding(cellPad)
                        .AlignCenter().Text("Değerlendirme ve Öneriler").FontSize(tableHeaderSize).SemiBold();

                    foreach (var row in Satirlar(it.AutoDoldur, it.UstaBelgesiVar))
                    {
                        t.Cell().BorderBottom(ReportStyles.Line).BorderRight(ReportStyles.Line).Padding(cellPad).Text(row.Left).FontSize(tableBodySize);
                        t.Cell().BorderBottom(ReportStyles.Line).Padding(cellPad).Text(row.Right).FontSize(tableBodySize).Italic();
                    }

                    t.Cell().BorderRight(ReportStyles.Line).Padding(cellPad).Text("D. Açıklanması gereken diğer hususlar:").FontSize(tableBodySize).SemiBold();
                    t.Cell().Padding(cellPad).Text("").FontSize(tableBodySize);
                });

                col.Item().PaddingTop(compact ? 4 : 6).Text("Açıklama: Bu form her işletme için her ay ayrı ayrı doldurulacak, kurum idaresine verilecektir.")
                    .FontSize(tableBodySize);
            });
        }

        private static IEnumerable<(string Left, string Right)> Satirlar(bool autoDoldur, bool? ustaBelgeVar)
        {
            string Sag(string text) => autoDoldur ? text : "";

            yield return (
                "A. Mesleki ve Teknik Eğitim Yönetmeliği ile ilgili konular:\n1. Usta öğreticilik/eğitici personelin yıllık eğitim planı (Gelişim Tablosu) var mı? Uyguluyor mu? Öğrencilere sürekli aynı işlem mi, rotasyona göre mi eğitim yaptırılıyor?",
                Sag("Gelişim Tablosu var. Uygulanıyor. Rotasyona göre eğitim yaptırılıyor."));
            yield return ("2. Öğrencilerin günlük çalışmaları yıllık eğitim planına uygun olarak planlanmış mı?", Sag("Öğrencilerin çalışmaları planlanmış."));
            yield return ("3. Öğrenci devam durumu günlük olarak takip ediliyor mu?", Sag("Devamsızlık günlük olarak takip ediliyor."));
            yield return ("4. Meslek eğitimi çalışmaları puanla değerlendiriliyor mu?", Sag("Mesleki çalışmalar puanla değerlendiriliyor."));
            yield return ("5. Yapılan işlerle ilgili olarak her öğrenciye iş dosyası tutuluyor mu?", Sag("Öğrenciler iş dosyası tutturuluyor."));
            yield return ("6. Öğrencilere 3308 sayılı Kanunun 25 inci maddesine göre aylık ücret ödeniyor mu?", Sag("Aylık ücret ödeniyor."));
            yield return ("7. Meslek eğitimi, çalışma saatlerinde yapılıyor mu?", Sag("Meslek eğitimi, çalışma saatlerinde yapılıyor."));
            yield return ("8. İş güvenliği konusunda öğrencilere yeterli bilgi veriliyor ve gerekli tedbirler alınıyor mu?", Sag("Yeterli bilgi veriliyor ve gerekli tedbirler alınıyor."));
            yield return ("9. Öğrenciler disiplin, kılık-kıyafet ve işletmenin kurallarına uyuyor mu?", Sag("Öğrenciler işletmenin kurallarına uyuyor."));
            yield return ("10. Öğrencilerin telafi eğitimine alınması gerekiyor mu? Gerekiyorsa hangi konularda telafi eğitimi uygulanmalı?", Sag("Telafi eğitimine ihtiyaç yoktur."));

            var ustaText = ustaBelgeVar is null
                ? "Usta öğreticilik belgesi ........."
                : (ustaBelgeVar == true ? "Usta öğreticilik belgesi var" : "Usta öğreticilik belgesi yok");

            yield return ("B. Eğitici Personel ile ilgili konular:\n1. İşletmenin meslek eğitimi ile görevli personelinin usta öğreticilik belgesi var mı?", Sag(ustaText));
            yield return ("2. Eğitici personelin sorumlu olduğu öğrenci grubu sayısı Mesleki ve Teknik Eğitim Yönetmeliğinin 192 inci maddesine uygun mu?", Sag("Öğrenci grubu sayısı yönetmeliğe uygundur."));
            yield return ("3. Meslek eğitimi konusunda koordinatör tarafından eğitici personele yapılan rehberlik ve konusu.", Sag("3308 Sayılı Kanun hakkında bilgi verildi. Staj dosyaları hakkında bilgi verildi."));
            yield return ("4. Eğitici personelin geliştirme ve uyum kursuna ihtiyacı var mı?", Sag("Uyum kursuna ihtiyaç yoktur."));

            yield return ("C. İşletme ile ilgili konular:\n1. İşletmelerde meslek eğitimi, yıllık çalışma takvimine uygun olarak sürdürülüyor mu?", Sag("Yıllık çalışma takvimine uygundur."));
            yield return ("2. İşletmede meslek eğitiminin mevzuata göre sürdürülmesi ile ilgili gerekli tedbirler alınıyor mu? (Meslekî ve Teknik Eğitim Yönetmeliği madde 196.)", Sag("Gerekli tedbirler alınıyor."));
            yield return ("3. Okul/kurum, öğretim programlarını (Gelişim Tablosu) işletmeye verdi mi?", Sag("Gelişim tablosu işletmeye verildi."));
            yield return ("4. Öğrenciler için gelişim tablosu uygulanıyor mu?", Sag("Gelişim tablosu uygulanıyor."));
            yield return ("5. İşletme yetkililerinin meslek eğitiminin uygulanışı ve öğretim programları konusundaki görüş ve önerileri:", Sag("__________"));
        }
    }
}
