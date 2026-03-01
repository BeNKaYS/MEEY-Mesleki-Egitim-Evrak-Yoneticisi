using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MEEY.Reports
{
    public static class NotCizelgesiRaporu
    {
        static NotCizelgesiRaporu()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public sealed record NotOgrenciInfo(
            int Id,
            string Sinif,
            string OkulNo,
            string AdSoyad,
            string Alan,
            string Dal
        );

        public sealed record NotCizelgesiInput(
            SchoolInfo School,
            BusinessInfo Business,
            string DonemText,
            DateTime Tarih,
            string KoordinatorOgretmen,
            string KoordinatorMudurYrd,
            IReadOnlyList<NotOgrenciInfo> Students
        );

        public static byte[] OlusturPdf(IReadOnlyList<NotCizelgesiInput> inputs, int perPage = 1)
        {
            if (inputs is null || inputs.Count == 0)
                throw new Exception("Not çizelgesi için veri bulunamadı.");

            var doc = new Belge(inputs, perPage <= 1 ? 1 : 2);
            return doc.GeneratePdf();
        }

        private sealed class Belge : IDocument
        {
            private readonly IReadOnlyList<NotCizelgesiInput> _inputs;
            private readonly int _perPage;

            public Belge(IReadOnlyList<NotCizelgesiInput> inputs, int perPage)
            {
                _inputs = inputs;
                _perPage = perPage;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    var pageSize = _perPage == 1 ? PageSizes.A4.Landscape() : PageSizes.A4;
                    var margin = _perPage == 1 ? 16f : 10f;

                    page.Size(pageSize);
                    page.Margin(margin);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(_perPage == 1 ? 9.2f : 8.2f));

                    var contentHeight = pageSize.Height - (margin * 2);
                    const float sectionSpacing = 14f;
                    var sectionHeight = _perPage == 1 ? contentHeight : (contentHeight - sectionSpacing) / 2f;

                    page.Content().Column(doc =>
                    {
                        for (var i = 0; i < _inputs.Count; i += _perPage)
                        {
                            var chunk = _inputs.Skip(i).Take(_perPage).ToList();

                            doc.Item().Column(col =>
                            {
                                col.Spacing(sectionSpacing);

                                foreach (var item in chunk)
                                {
                                    col.Item()
                                        .Height(sectionHeight)
                                        .Padding(2)
                                        .ScaleToFit()
                                        .Element(c => ComposeForm(c, item, _perPage));
                                }
                            });

                            if (i + _perPage < _inputs.Count)
                                doc.Item().PageBreak();
                        }
                    });
                });
            }

            private static void ComposeForm(IContainer container, NotCizelgesiInput input, int nup)
            {
                var y1 = input.Tarih.Month >= 9 ? input.Tarih.Year : input.Tarih.Year - 1;
                var ogretimYili = $"{y1}-{y1 + 1}";

                container.Column(col =>
                {
                    col.Spacing(nup == 2 ? 5 : 8);

                    col.Item().Border(1.2f).PaddingVertical(nup == 2 ? 6 : 8).PaddingHorizontal(10).AlignCenter()
                        .Text($"İŞLETMELERDE MESLEK EĞİTİMİ GÖREN ÖĞRENCİLERE AİT {input.DonemText} PUAN FİŞİ")
                        .FontSize(nup == 2 ? 12.5f : 15).SemiBold();

                    col.Item().Border(1.0f).Padding(nup == 2 ? 5 : 7).Table(t =>
                    {
                        t.ColumnsDefinition(cd =>
                        {
                            cd.ConstantColumn(nup == 2 ? 120 : 145);
                            cd.RelativeColumn();
                            cd.ConstantColumn(nup == 2 ? 110 : 135);
                            cd.RelativeColumn();
                        });

                        IContainer Key(IContainer c) => c.Border(0.6f).Background(Colors.Grey.Lighten4).PaddingVertical(nup == 2 ? 3 : 4).PaddingHorizontal(6);
                        IContainer Val(IContainer c) => c.Border(0.6f).PaddingVertical(nup == 2 ? 3 : 4).PaddingHorizontal(6);

                        void Row(string k1, string v1, string k2, string v2)
                        {
                            t.Cell().Element(Key).Text(k1).SemiBold().FontSize(nup == 2 ? 7.6f : 8.2f);
                            t.Cell().Element(Val).Text(v1 ?? string.Empty).FontSize(nup == 2 ? 7.6f : 8.2f);
                            t.Cell().Element(Key).Text(k2).SemiBold().FontSize(nup == 2 ? 7.6f : 8.2f);
                            t.Cell().Element(Val).Text(v2 ?? string.Empty).FontSize(nup == 2 ? 7.6f : 8.2f);
                        }

                        Row("Okul/Kurum Adı :", input.School.Ad, "Dersin Adı :", "İşletmelerde Meslek Eğitimi");
                        Row("Öğretim Yılı :", ogretimYili, "Tarih :", input.Tarih.ToString("dd.MM.yyyy"));
                        Row("İşletmenin Adı :", input.Business.Adi, "Koord. Öğrt. :", input.KoordinatorOgretmen);
                    });

                    col.Item().Border(1.0f).Padding(0).Table(t =>
                    {
                        t.ColumnsDefinition(cd =>
                        {
                            cd.ConstantColumn(nup == 2 ? 20 : 24);
                            cd.ConstantColumn(nup == 2 ? 28 : 32);

                            cd.RelativeColumn(0.85f);
                            cd.RelativeColumn(0.75f);

                            for (int i = 0; i < 12; i++)
                                cd.RelativeColumn(0.25f);

                            cd.RelativeColumn(0.32f);
                            cd.RelativeColumn(0.32f);
                            cd.RelativeColumn(0.28f);
                            cd.RelativeColumn(0.28f);
                            cd.RelativeColumn(0.28f);
                        });

                        void HeaderGroup(string text, int colSpan)
                        {
                            t.Cell().ColumnSpan((uint)colSpan)
                                .Border(0.6f).Background(Colors.Grey.Lighten3)
                                .PaddingVertical(nup == 2 ? 4 : 6).AlignCenter().AlignMiddle()
                                .Text(text).SemiBold().FontSize(nup == 2 ? 8.2f : 9.2f);
                        }

                        void HeaderCell(string text, int colSpan = 1)
                        {
                            var cell = t.Cell();
                            if (colSpan > 1)
                                cell = cell.ColumnSpan((uint)colSpan);

                            cell.Border(0.6f).Background(Colors.White)
                                .Padding(nup == 2 ? 3 : 5).AlignCenter().AlignMiddle()
                                .Text(text).SemiBold().FontSize(nup == 2 ? 7.8f : 8.6f);
                        }

                        void HeaderRotate(string text)
                        {
                            t.Cell().Border(0.6f).Background(Colors.White)
                                .MinHeight(nup == 2 ? 40 : 50)
                                .AlignCenter().AlignMiddle()
                                .RotateLeft()
                                .Text(text).SemiBold().FontSize(nup == 2 ? 7.2f : 8.0f);
                        }

                        void BodyCell(string text)
                        {
                            t.Cell().Border(0.6f)
                                .PaddingVertical(2).PaddingHorizontal(nup == 2 ? 3 : 4)
                                .AlignMiddle()
                                .Text(text ?? string.Empty)
                                .FontSize(nup == 2 ? 7.4f : 7.8f);
                        }

                        void EmptyCell() => t.Cell().Border(0.6f).MinHeight(nup == 2 ? 12 : 14);

                        HeaderGroup("Öğrencinin", 4);
                        HeaderGroup("İşletmede Verilen Puanlar", 12);
                        HeaderGroup("Okulda\nVerilen\nPuanlar", 2);
                        HeaderGroup("Dönem\nBaşarısı", 3);

                        HeaderRotate("Sınıf");
                        HeaderRotate("Numarası");
                        HeaderCell("Adı Soyadı");
                        HeaderCell("Alan Dalı");

                        HeaderCell("Temrin", 3);
                        HeaderCell("İş-Hizmet", 3);
                        HeaderCell("Proje", 3);
                        HeaderCell("Deney", 3);

                        HeaderRotate("Telafi Eğitimi\nPuanı(*)");
                        HeaderRotate("Beceri Sınav\nPuanı(*)");

                        HeaderRotate("Dönem\nPuan Ort.");
                        HeaderRotate("Rakamla");
                        HeaderRotate("Yazıyla");

                        foreach (var student in input.Students)
                        {
                            BodyCell(string.IsNullOrWhiteSpace(student.Sinif) ? "-" : student.Sinif);
                            BodyCell(student.OkulNo);
                            BodyCell(student.AdSoyad);
                            BodyCell($"{student.Alan} {student.Dal}".Trim());

                            for (int i = 0; i < 12; i++) EmptyCell();
                            EmptyCell();
                            EmptyCell();
                            EmptyCell();
                            EmptyCell();
                            EmptyCell();
                        }
                    });

                    col.Item().Border(1).Table(t =>
                    {
                        t.ColumnsDefinition(cd =>
                        {
                            cd.RelativeColumn();
                            cd.RelativeColumn();
                            cd.RelativeColumn();
                            cd.RelativeColumn();
                        });

                        void Sign(string title, string sub, string? name = null)
                        {
                            t.Cell().Border(0.6f).Height(nup == 2 ? 70 : 92).AlignCenter().AlignMiddle().Padding(8).Text(tx =>
                            {
                                tx.DefaultTextStyle(x => x.FontSize(nup == 2 ? 8.0f : 9.5f));
                                tx.AlignCenter();
                                tx.Line(title).SemiBold();
                                if (!string.IsNullOrWhiteSpace(name))
                                    tx.Line(name).FontSize(nup == 2 ? 8.0f : 9.5f);
                                if (!string.IsNullOrWhiteSpace(sub))
                                    tx.Line(sub).FontSize(nup == 2 ? 7.4f : 8.6f);
                            });
                        }

                        Sign("Usta Öğretici / İşletme Yetkilisi", "Kaşe - İmza");
                        Sign("Eğitici Personel", "Kaşe - İmza");
                        Sign("Koordinatör\nMüdür Yardımcısı", "", name: input.KoordinatorMudurYrd);
                        Sign("Okul/Kurum Müdürü", "", name: input.School.Mudur);
                    });

                    col.Item().Border(1).PaddingVertical(nup == 2 ? 4 : 8).PaddingHorizontal(10).Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(nup == 2 ? 7.4f : 9));
                        txt.Span("AÇIKLAMA : ").SemiBold();
                        txt.Span("(*) işaretli bölümler okul/kurum müdürlüğüce doldurulacak ve puan ortalaması alınarak dönem ortalaması belirlenecektir.");
                    });
                });
            }
        }
    }
}
