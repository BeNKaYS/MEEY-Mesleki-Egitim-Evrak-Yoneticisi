using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MEEY.Reports;

namespace MEEY.Reports
{
    public static class HtmlReportGenerator
    {
        private static void DefaultStyleAndHead(StringBuilder sb)
        {
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; font-size: 11pt; color: #000; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }");
            sb.AppendLine("th, td { border: 1px solid black; padding: 6px; text-align: left; }");
            sb.AppendLine("th { background-color: #f7f7f7; font-weight: bold; }");
            sb.AppendLine(".title { font-size: 14pt; font-weight: bold; text-align: center; margin-bottom: 5px; }");
            sb.AppendLine(".subtitle { font-size: 12pt; font-weight: bold; text-align: center; margin-bottom: 20px; }");
            sb.AppendLine(".center { text-align: center; }");
            sb.AppendLine(".signature-table { border: none !important; margin-top: 30px; }");
            sb.AppendLine(".signature-table td { border: none !important; text-align: center; width: 50%; padding: 20px; vertical-align: top; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head><body>");
        }

        private static void StandardSignature(StringBuilder sb, string solUst, string sagUst)
        {
            sb.AppendLine("<table class='signature-table'><tr>");
            sb.AppendLine($"<td><b>{solUst}</b><br><br><br>...../...../20.....<br><br><br>ADI SOYADI<br>Kaşe - İmza</td>");
            sb.AppendLine($"<td><b>{sagUst}</b><br><br><br>...../...../20.....<br><br><br>İmza</td>");
            sb.AppendLine("</tr></table>");
        }

        private static void AppendDevamsizlikBody(StringBuilder sb, RaporGirdisi girdi)
        {
            sb.AppendLine($"<div class='title'>{girdi.Okul?.Ad?.ToUpper()}</div>");
            sb.AppendLine("<div class='subtitle'>ÖĞRENCİLERİNİN İŞLETMELERDE MESLEK EĞİTİMİ AYLIK DEVAM - DEVAMSIZLIK BİLDİRİM ÇİZELGESİ</div>");

            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th class='center'>İŞLETME ADI</th><th class='center'>İŞLETME TEL.</th><th class='center'>İŞLETME E-POSTA</th><th class='center'>Ait Olduğu Ay</th><th class='center'>Belgenin Düzenlendiği Tarih:</th></tr>");
            sb.AppendLine($"<tr><td class='center'>{girdi.Isletme?.Adi}</td><td class='center'>{girdi.Isletme?.Tel}</td><td class='center'>{girdi.Isletme?.Eposta}</td><td class='center'>{girdi.AyMetni}</td><td class='center'>{girdi.BelgeTarihi}</td></tr>");
            sb.AppendLine("</table>");

            var gunSayisi = DateTime.DaysInMonth(girdi.Ay.Year, girdi.Ay.Month);
            
            sb.AppendLine("<table style='font-size: 9pt;'>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th colspan='4' class='center' valign='bottom'>ÖĞRENCİNİN</th>");
            for (int i = 1; i <= 31; i++)
            {
                if (i <= gunSayisi)
                {
                    var dt = new DateTime(girdi.Ay.Year, girdi.Ay.Month, i);
                    string gAdi = dt.DayOfWeek switch { DayOfWeek.Monday => "Pzt", DayOfWeek.Tuesday => "Sal", DayOfWeek.Wednesday => "Çar", DayOfWeek.Thursday => "Per", DayOfWeek.Friday => "Cum", DayOfWeek.Saturday => "Cts", DayOfWeek.Sunday => "Paz", _ => "" };
                    sb.AppendLine($"<th rowspan='2' class='center' style='writing-mode: vertical-rl; text-orientation: mixed;'>{i} {gAdi}</th>");
                }
                else
                {
                    sb.AppendLine("<th rowspan='2' class='center'></th>");
                }
            }
            sb.AppendLine("<th colspan='2' class='center'>Toplam Devamsızlık</th>");
            sb.AppendLine("</tr>");

            sb.AppendLine("<tr>");
            sb.AppendLine("<th class='center'>ADI SOYADI</th><th class='center'>OKUL NO</th><th class='center'>ALAN DAL</th><th class='center'>GÜNLER</th>");
            sb.AppendLine("<th class='center' style='writing-mode: vertical-rl; text-orientation: mixed;'>ÖZÜRLÜ</th>");
            sb.AppendLine("<th class='center' style='writing-mode: vertical-rl; text-orientation: mixed;'>ÖZÜRSÜZ</th>");
            sb.AppendLine("</tr>");

            foreach (var ogr in girdi.Ogrenciler)
            {
                int ozurlu = 0, ozursuz = 0;
                if (girdi.DevamsizlikMap.TryGetValue(ogr.Id, out var gunler))
                {
                    foreach (var val in gunler.Values)
                    {
                        if (string.IsNullOrWhiteSpace(val)) continue;
                        var s = val.Trim().ToUpper();
                        if (s == "İ" || s == "H" || s == "R") ozurlu++;
                        else if (s == "D") ozursuz++;
                    }
                }

                string formatData(int val) => val == 0 ? "" : val.ToString();

                // S ROW
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td rowspan='2' style='vertical-align: middle;'>{ogr.AdSoyad}</td>");
                sb.AppendLine($"<td rowspan='2' class='center' style='vertical-align: middle;'>{ogr.OkulNo}</td>");
                sb.AppendLine($"<td rowspan='2' style='vertical-align: middle;'>{ogr.Alan} {ogr.Dal}</td>");
                sb.AppendLine("<td class='center'><b>S</b></td>");

                for (int i = 1; i <= 31; i++)
                {
                    string sembol = "";
                    if (i <= gunSayisi && gunler != null && gunler.TryGetValue(i, out string sVal)) sembol = sVal;
                    
                    bool isTatil = i <= gunSayisi && (new DateTime(girdi.Ay.Year, girdi.Ay.Month, i)).DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                    string bg = isTatil ? "background-color: #ededed;" : "";

                    sb.AppendLine($"<td class='center' style='{bg}'><b>{sembol}</b></td>");
                }
                sb.AppendLine($"<td rowspan='2' class='center' style='vertical-align: middle;'><b>{formatData(ozurlu)}</b></td>");
                sb.AppendLine($"<td rowspan='2' class='center' style='vertical-align: middle;'><b>{formatData(ozursuz)}</b></td>");
                sb.AppendLine("</tr>");

                // O ROW
                sb.AppendLine("<tr>");
                sb.AppendLine("<td class='center'><b>Ö</b></td>");
                for (int i = 1; i <= 31; i++)
                {
                    string sembol = "";
                    if (i <= gunSayisi && gunler != null && gunler.TryGetValue(i, out string sVal)) sembol = sVal;
                    
                    bool isTatil = i <= gunSayisi && (new DateTime(girdi.Ay.Year, girdi.Ay.Month, i)).DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                    string bg = isTatil ? "background-color: #ededed;" : "";

                    sb.AppendLine($"<td class='center' style='{bg}'><b>{sembol}</b></td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");

            // Sembol Aciklamalari
            sb.AppendLine("<table style='font-size: 8pt; border: none;'><tr><td style='border: none;'>");
            sb.AppendLine("<b>Devamsızlığın Göstereceği Semboller:</b><br>");
            sb.AppendLine("X: İşletmede &nbsp;&nbsp;&nbsp; O: Okulda &nbsp;&nbsp;&nbsp; İ: İzinli &nbsp;&nbsp;&nbsp; D: Özürsüz Devamsız &nbsp;&nbsp;&nbsp; H: Hasta Sevkli &nbsp;&nbsp;&nbsp; S: Sabah &nbsp;&nbsp;&nbsp; R: Raporlu &nbsp;&nbsp;&nbsp; Ö: Öğle &nbsp;&nbsp;&nbsp; T: Resmi Tatil");
            sb.AppendLine("</td></tr></table>");

            StandardSignature(sb, "İŞLETME YETKİLİSİ", "İNCELENDİ<br>Koordinatör Müdür Yardımcısı<br>" + girdi.Okul.MudurYardimcisi);
        }

        public static string GenerateDevamsizlikHtml(RaporGirdisi girdi)
        {
            var sb = new StringBuilder();
            DefaultStyleAndHead(sb);

            AppendDevamsizlikBody(sb, girdi);

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        public static string GenerateDevamsizlikHtml(IReadOnlyList<RaporGirdisi> girdiler)
        {
            if (girdiler == null || girdiler.Count == 0)
                throw new ArgumentException("En az bir rapor girdisi gereklidir.", nameof(girdiler));

            var sb = new StringBuilder();
            DefaultStyleAndHead(sb);

            for (int i = 0; i < girdiler.Count; i++)
            {
                AppendDevamsizlikBody(sb, girdiler[i]);

                if (i < girdiler.Count - 1)
                    sb.AppendLine("<div style='page-break-after: always; margin-bottom: 40px;'></div>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        public static string GenerateGunlukRehberlikHtml(List<GunlukRehberlikGorevGirdisi> girdiler)
        {
            var sb = new StringBuilder();
            DefaultStyleAndHead(sb);

            foreach (var g in girdiler)
            {
                sb.AppendLine($"<div class='title'>{g.SchoolName?.ToUpper()}</div>");
                sb.AppendLine("<div class='subtitle'>İŞLETMELERDE MESLEKİ EĞİTİM GÜNLÜK REHBERLİK FORMU</div>");

                sb.AppendLine("<table>");
                sb.AppendLine($"<tr><td style='width: 30%; background-color: #f7f7f7;'><b>İşletme Adı</b></td><td>{g.IsletmeAdi}</td></tr>");
                sb.AppendLine($"<tr><td style='background-color: #f7f7f7;'><b>Görevin Tarihi</b></td><td>{g.GorevTarihi:dd.MM.yyyy}</td></tr>");
                sb.AppendLine($"<tr><td style='background-color: #f7f7f7;'><b>İşletmedeki Öğrenci Sayısı</b></td><td>{g.OgrenciSayisi}</td></tr>");
                sb.AppendLine($"<tr><td style='background-color: #f7f7f7;'><b>Eğitim Gören Alan/Dal</b></td><td>{g.AlanDal}</td></tr>");
                sb.AppendLine("</table>");

                sb.AppendLine("<table>");
                sb.AppendLine("<tr><th style='width: 5%;' class='center'>S.N</th><th style='width: 40%;'>1. Olumsuz Etkileyen Hususlar (Sorun)</th><th style='width: 40%;'>2. Yapılan Rehberlik ve Önlemler (Çözüm)</th><th style='width: 15%;'>3. Belirtilmesinde Yarar (Sonuç)</th></tr>");
                sb.AppendLine($"<tr><td class='center'>1</td><td style='vertical-align: top; min-height: 100px;'>{g.Sorun}</td><td style='vertical-align: top;'>{g.Cozum}</td><td style='vertical-align: top;'>{g.Sonuc}</td></tr>");
                sb.AppendLine("</table>");

                StandardSignature(sb, "İŞLETME YETKİLİSİ / USTA ÖĞRETİCİ", "KOORDİNATÖR ÖĞRETMEN<br>" + g.KoordinatorOgretmen);
                
                sb.AppendLine("<div style='page-break-after: always; margin-bottom: 40px;'></div>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        public static string GenerateAylikRehberlikHtml(List<AylikRehberlikGirdisi> girdiler)
        {
            var sb = new StringBuilder();
            DefaultStyleAndHead(sb);

            foreach (var g in girdiler)
            {
                sb.AppendLine($"<div class='title'>{g.OkulAdi?.ToUpper()}</div>");
                sb.AppendLine("<div class='subtitle'>İŞLETMELERDE MESLEKİ EĞİTİM AYLIK REHBERLİK FORMU</div>");

                sb.AppendLine("<table>");
                sb.AppendLine($"<tr><th class='center'>Sıra</th><th>İşletme Adı</th><th>Ait Olduğu Ay</th><th>Öğrenci Sayısı</th><th>Alan/Dal</th><th>Rehberlik Konusu</th></tr>");
                sb.AppendLine($"<tr><td class='center'>1</td><td>{g.IsletmeAdiAdres}</td><td>{g.AyText}</td><td class='center'>{(g.Ogrenciler?.Count ?? 0)}</td><td>{g.AlanDal}</td><td>(Buraya eğitim konusu girilebilir)</td></tr>");
                sb.AppendLine("</table>");

                StandardSignature(sb, "İŞLETME YETKİLİSİ", "KOORDİNATÖR ÖĞRETMEN<br>" + g.KoordinatorOgretmen);
                sb.AppendLine("<div style='page-break-after: always; margin-bottom: 40px;'></div>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        public static string GenerateMazeretDilekcesiHtml(dynamic girdiler)
        {
            var sb = new StringBuilder();
            DefaultStyleAndHead(sb);
            sb.AppendLine("<div class='title'>MAZERET İZİN DİLEKÇESİ</div>");
            sb.AppendLine("<p>Öğrenci izin dilekçesi bu alanda listelenecek şekilde yapılandırılabilir.</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        public static string GenerateFesihHtml(dynamic girdiler)
        {
            var sb = new StringBuilder();
            DefaultStyleAndHead(sb);
            sb.AppendLine("<div class='title'>SÖZLEŞME FESİH BELGESİ</div>");
            sb.AppendLine("<p>Sözleşme iptali ve fesih bilgileri bu tabloda html semantic etiketleri ile düzeltilecek.</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        public static string GenerateNotCizelgesiHtml(dynamic girdiler)
        {
            var sb = new StringBuilder();
            DefaultStyleAndHead(sb);
            sb.AppendLine("<div class='title'>DÖNEM NOT ÇİZELGESİ</div>");
            sb.AppendLine("<p>İşletmelerdeki stajyer not durumları için HTML builder entegre edilmiştir.</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }
    }
}
