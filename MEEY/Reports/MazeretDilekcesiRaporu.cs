using System;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MEEY.Reports
{
    public static class MazeretDilekcesiRaporu
    {
        static MazeretDilekcesiRaporu()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public sealed record MazeretDilekceGirdi(
            string OkulAdi,
            string IsletmeAdi,
            string IsletmeAdresi,
            string IsletmeTelefon,
            string IsletmeEposta,
            string OgrenciSinif,
            string OgrenciNo,
            string OgrenciAdSoyad,
            string IzinGun,
            string OkulMuduru,
            DateTime BelgeTarihi
        );

        public static byte[] OlusturPdf(MazeretDilekceGirdi input)
        {
            var doc = new Belge(input);
            return doc.GeneratePdf();
        }

        private sealed class Belge : IDocument
        {
            private readonly MazeretDilekceGirdi _i;

            public Belge(MazeretDilekceGirdi input)
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
                    page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(11));

                    page.Content()
                        .Border(1)
                        .BorderColor(Colors.Black)
                        .Padding(26)
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("İŞLETMELERDE MESLEK EĞİTİMİ GÖREN ÖĞRENCİLER İÇİN").SemiBold().FontSize(12);
                            col.Item().PaddingTop(12).AlignCenter().Text("MAZERET İZİN DİLEKÇESİ").SemiBold().FontSize(14);

                            var mudurluk = DotIfEmpty(_i.OkulAdi, 50);
                            col.Item().PaddingTop(30).AlignCenter().Text(t =>
                            {
                                t.Span(mudurluk).SemiBold();
                                t.Span(" MÜDÜRLÜĞÜNE").SemiBold();
                            });

                            col.Item().PaddingTop(34).Text("İŞLETMENİN").SemiBold().FontSize(11.5f);

                            col.Item().PaddingTop(8).Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(150);
                                    c.ConstantColumn(16);
                                    c.RelativeColumn();
                                });

                                BilgiSatiri(t, "ADI", _i.IsletmeAdi);
                                BilgiSatiri(t, "ADRESİ", _i.IsletmeAdresi);
                                BilgiSatiri(t, "TELEFON NO", _i.IsletmeTelefon);
                                BilgiSatiri(t, "E-POSTA", _i.IsletmeEposta);
                            });

                            var ogrenciAdiEki = AddDativeSuffix(_i.OgrenciAdSoyad);
                            var sinif = DotIfEmpty(_i.OgrenciSinif, 12);
                            var no = DotIfEmpty(_i.OgrenciNo, 12);
                            var adSoyad = DotIfEmpty(_i.OgrenciAdSoyad, 30);
                            var izinGun = DotIfEmpty(_i.IzinGun, 8);

                            col.Item().PaddingTop(34).AlignCenter().Text(t =>
                            {
                                t.Span("Yukarıda adı ve adresi yazılı işletmede meslek eğitimi gören okulunuz ");
                                t.Span(sinif).SemiBold();
                                t.Span(" sınıfı, ");
                                t.Span(no).SemiBold();
                                t.Span(" numaralı öğrencisi ");
                                t.Span(adSoyad).SemiBold();
                                t.Span(ogrenciAdiEki);
                                t.Span(" mazeretinden dolayı ");
                                t.Span(izinGun).SemiBold();
                                t.Span(" gün ücretsiz izin verilmesini ve bu iznin öğrencinin devamsızlığına sayılacağını bildiğimi saygı ile arz ederim.");
                            });

                            col.Item().PaddingTop(26).AlignRight().Text(_i.BelgeTarihi.ToString("dd/MM/yyyy"));
                            col.Item().PaddingTop(18).AlignRight().Text("Öğrenci Velisi (Adı Soyadı, İmzası)").FontSize(10.5f);

                            col.Item().PaddingTop(20)
                                .Border(1)
                                .BorderColor(Colors.Black)
                                .Table(t =>
                                {
                                    t.ColumnsDefinition(c =>
                                    {
                                        c.RelativeColumn();
                                        c.RelativeColumn();
                                        c.RelativeColumn();
                                    });

                                    t.Cell().ColumnSpan(3)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Black)
                                        .PaddingVertical(6)
                                        .AlignCenter()
                                        .Text("Uygundur").SemiBold().FontSize(11.5f);

                                    ImzaKutusu(t.Cell().BorderRight(1).BorderColor(Colors.Black),
                                        "Usta Öğretici/Eğitici\nPersonel",
                                        DotIfEmpty(string.Empty, 14),
                                        "Adı Soyadı",
                                        "İmza");

                                    ImzaKutusu(t.Cell().BorderRight(1).BorderColor(Colors.Black),
                                        "İşletme Yetkilisi",
                                        DotIfEmpty(string.Empty, 14),
                                        "Adı Soyadı",
                                        "İmza");

                                    ImzaKutusu(t.Cell(),
                                        "Okul Müdürü",
                                        DotIfEmpty(string.Empty, 14),
                                        DotIfEmpty(_i.OkulMuduru, 18),
                                        "İmza",
                                        true);
                                });

                            col.Item().PaddingTop(16)
                                .Border(1)
                                .BorderColor(Colors.Black)
                                .Padding(8)
                                .Text(t =>
                                {
                                    t.Span("AÇIKLAMA: ").SemiBold().FontSize(10.5f);
                                    t.Span("Bu izin dilekçesi, işletme yetkilisi tarafından izin verilmesinin uygun görülmesi halinde imzalandıktan sonra okul/kurum müdürlüğüne öğrenci velisi ile gönderilecektir. Öğrencinin bu durumu devam-devamsızlık çizelgesine mazeret izni olarak işlenecektir. (Mesleki ve Teknik Eğitim Yönetmeliği, madde 196/k)").FontSize(10.5f);
                                });
                        });
                });
            }

            private static void BilgiSatiri(TableDescriptor t, string label, string value)
            {
                t.Cell().PaddingVertical(3).Text(label).SemiBold();
                t.Cell().PaddingVertical(3).AlignCenter().Text(":");
                t.Cell().PaddingVertical(3).Text(DotIfEmpty(value, 68));
            }

            private static void ImzaKutusu(
                IContainer container,
                string baslik,
                string tarih,
                string adSoyad,
                string imza,
                bool adSoyadBold = false)
            {
                container.Padding(8).Column(c =>
                {
                    c.Item().AlignCenter().Text(baslik).SemiBold().FontSize(11);
                    c.Item().PaddingTop(12).AlignCenter().Text(tarih);

                    var adSoyadText = c.Item().PaddingTop(12).AlignCenter().Text(adSoyad);
                    if (adSoyadBold)
                        adSoyadText.SemiBold();

                    c.Item().PaddingTop(2).AlignCenter().Text(imza);
                });
            }

            private static string DotIfEmpty(string value, int dotCount)
            {
                return string.IsNullOrWhiteSpace(value)
                    ? new string('.', dotCount)
                    : value.Trim();
            }

            private static string AddDativeSuffix(string fullName)
            {
                if (string.IsNullOrWhiteSpace(fullName))
                    return "'a";

                var lastWord = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? fullName.Trim();
                var lower = lastWord.ToLower(new System.Globalization.CultureInfo("tr-TR"));

                var vowels = "aeıioöuü";
                char lastVowel = 'a';
                for (var i = lower.Length - 1; i >= 0; i--)
                {
                    if (vowels.Contains(lower[i]))
                    {
                        lastVowel = lower[i];
                        break;
                    }
                }

                var suffixVowel = ("eiöü".Contains(lastVowel)) ? 'e' : 'a';
                var endsWithVowel = vowels.Contains(lower.Last());
                var suffix = endsWithVowel ? $"'y{suffixVowel}" : $"'{suffixVowel}";
                return suffix;
            }
        }
    }
}
