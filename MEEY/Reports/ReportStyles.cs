using System;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace MEEY.Reports
{
    public static class ReportStyles
    {
        public static TextStyle Normal => TextStyle.Default.FontSize(9).FontFamily("Arial");
        public static TextStyle NormalBold => Normal.Bold();
        public static TextStyle Title => TextStyle.Default.FontSize(11).Bold().FontFamily("Arial");
        
        public static float Line => 0.5f;
        public static float LineOuter => 1f;

        public static string TrMonthUpper(int month)
        {
            return month switch
            {
                1 => "OCAK",
                2 => "ŞUBAT",
                3 => "MART",
                4 => "NİSAN",
                5 => "MAYIS",
                6 => "HAZİRAN",
                7 => "TEMMUZ",
                8 => "AĞUSTOS",
                9 => "EYLÜL",
                10 => "EKİM",
                11 => "KASIM",
                12 => "ARALIK",
                _ => month.ToString()
            };
        }
    }

    public record SchoolInfo(string Il, string Ad, string Mudur, string MudurYardimcisi);
    public record BusinessInfo(string Adi, string Tel, string Eposta);
    public record StudentInfo(int Id, string OkulNo, string AdSoyad, string Alan, string Dal);
    public record DateRange(DateTime StartDate, DateTime EndDate);
}
