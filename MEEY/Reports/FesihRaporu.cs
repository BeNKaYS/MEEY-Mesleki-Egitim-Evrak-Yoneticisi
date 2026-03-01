using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MEEY.Reports
{
    public static class FesihRaporu
    {
        static FesihRaporu()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public sealed record FesihGirdi(
            SchoolInfo School,
            BusinessInfo Business,
            StudentInfo Student,
            string StudentSinif,
            string KoordinatorOgretmen,
            DateTime? SozlesmeTarihi,
            DateTime? IptalTarihi,
            string IptalNedenleri,
            DateTime BelgeTarihi,
            string FormKodu
        );

        public static byte[] OlusturPdf(FesihGirdi input)
        {
            var doc = new FesihBelgesi(input);
            return doc.GeneratePdf();
        }

        private sealed class FesihBelgesi : IDocument
        {
            private readonly FesihGirdi _i;

            public FesihBelgesi(FesihGirdi input)
            {
                _i = input;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));

                    page.Content()
                        .Border(1)
                        .BorderColor(Colors.Black)
                        .Padding(28)
                        .Column(col =>
                        {
                            col.Spacing(10);

                            if (!string.IsNullOrWhiteSpace(_i.School.Ad))
                                col.Item().AlignCenter().Text((_i.School.Ad ?? string.Empty).ToUpperInvariant()).SemiBold();

                            col.Item().AlignCenter().Text("İŞLETMELERDE BECERİ EĞİTİMİ GÖREN ÖĞRENCİLERE AİT").SemiBold();
                            col.Item().AlignCenter().Text("SÖZLEŞME İPTAL TUTANAĞI").FontSize(16).SemiBold();

                            col.Item().PaddingTop(6).Table(t =>
                            {
                                t.ColumnsDefinition(cd =>
                                {
                                    cd.ConstantColumn(190);
                                    cd.ConstantColumn(20);
                                    cd.RelativeColumn();
                                });

                                void LineRow(string label, string? value, bool boldLabel = true)
                                {
                                    var labelText = t.Cell().PaddingVertical(6).Text(label);
                                    if (boldLabel)
                                        labelText.SemiBold();

                                    t.Cell().PaddingVertical(6).AlignCenter().Text(":").SemiBold();

                                    t.Cell().PaddingVertical(6)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingBottom(3)
                                        .Text(value ?? string.Empty);
                                }

                                LineRow("İŞLETMENİN ADI", _i.Business.Adi);

                                t.Cell().ColumnSpan(3).PaddingTop(8).Text(tx =>
                                {
                                    tx.Span("ÖĞRENCİNİN").SemiBold().Underline();
                                });

                                LineRow("ADI SOYADI", _i.Student.AdSoyad);
                                LineRow("ALANI", _i.Student.Alan);
                                LineRow("SINIFI", _i.StudentSinif);
                                LineRow("NUMARASI", _i.Student.OkulNo);

                                LineRow("SÖZLEŞME TARİHİ", _i.SozlesmeTarihi is null ? "……/……/20…." : _i.SozlesmeTarihi.Value.ToString("dd.MM.yyyy"));
                                LineRow("SÖZLEŞME İPTAL TARİHİ", _i.IptalTarihi is null ? "……/……/20…." : _i.IptalTarihi.Value.ToString("dd.MM.yyyy"));

                                t.Cell().PaddingTop(14).Text("İPTAL NEDENLERİ").SemiBold();
                                t.Cell().PaddingTop(14).AlignCenter().Text(":").SemiBold();
                                t.Cell().PaddingTop(10).Column(c =>
                                {
                                    c.Spacing(10);

                                    c.Item()
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingBottom(3)
                                        .Text((_i.IptalNedenleri ?? string.Empty).Trim());

                                    for (int k = 0; k < 4; k++)
                                        c.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Height(18);
                                });
                            });

                            var aciklama =
                                "Ortaöğretim Kurumları Yönetmeliği uyarınca İşletmede Beceri Eğitimi yapılan ve yukarıda " +
                                "belirtilen iş yerinden kimliği yazılı öğrencinin açıklanan nedenlerden dolayı sözleşmesi iptal edilmiştir.";

                            col.Item()
                                .PaddingTop(26)
                                .Text(aciklama)
                                .SemiBold()
                                .Justify();

                            col.Item().PaddingTop(26).Row(r =>
                            {
                                r.Spacing(24);

                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("İşletme Yetkilisi").SemiBold().FontSize(10);
                                    c.Item().PaddingTop(8).Text("").FontSize(10);
                                });

                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Koordinatör Öğretmen").SemiBold().FontSize(10);
                                    c.Item().PaddingTop(8).PaddingLeft(10).Text(_i.KoordinatorOgretmen ?? string.Empty).FontSize(10);
                                });

                                r.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().AlignRight().PaddingRight(22).Text("……/……/2026").SemiBold().FontSize(10);

                                    if (!string.IsNullOrWhiteSpace(_i.School.Mudur))
                                    {
                                        c.Item().PaddingTop(22).AlignRight().PaddingRight(10).Text(_i.School.Mudur ?? string.Empty).SemiBold().FontSize(10);
                                        c.Item().PaddingTop(4).AlignRight().PaddingRight(22).Text("Okul Müdürü").SemiBold().FontSize(10);
                                    }
                                });
                            });
                        });
                });
            }
        }
    }
}
