using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace MEEY.Database
{
    public class DatabaseManager
    {
        private static readonly CultureInfo TrCulture = new CultureInfo("tr-TR");
        private static readonly string appDataRootDirectory;
        private static readonly string dbDirectory;
        private static readonly string dbPath;
        private static readonly string connectionString;

        static DatabaseManager()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            appDataRootDirectory = Path.Combine(localAppData, "MEEY");
            dbDirectory = Path.Combine(appDataRootDirectory, "db");
            Directory.CreateDirectory(dbDirectory);

            dbPath = Path.Combine(dbDirectory, "MEEY.db");
            connectionString = $"Data Source={dbPath};Version=3;";

            string legacyDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MEEY.db");
            if (!File.Exists(dbPath) && File.Exists(legacyDbPath))
            {
                try
                {
                    File.Copy(legacyDbPath, dbPath, true);
                }
                catch
                {
                }
            }
        }

        private static string GetLegacyDatabaseFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MEEY.db");
        }

        private static string GetPreviousAppDataDatabaseFilePath()
        {
            return Path.Combine(appDataRootDirectory, "MEEY.db");
        }

        private static int GetDatabaseDataScore(string filePath)
        {
            if (!File.Exists(filePath))
                return 0;

            int score = 0;
            string[] tableNames =
            {
                "OkulKoordinatorler",
                "Isletmeler",
                "AlanDal",
                "KoordinatorTanimlama",
                "Ogrenciler",
                "CalismaTakvimi",
                "Devamsizlik"
            };

            using (var connection = new SQLiteConnection($"Data Source={filePath};Version=3;Read Only=True;"))
            {
                connection.Open();

                foreach (string tableName in tableNames)
                {
                    using (var command = new SQLiteCommand($"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'", connection))
                    {
                        long exists = Convert.ToInt64(command.ExecuteScalar());
                        if (exists == 0)
                            continue;
                    }

                    using (var command = new SQLiteCommand($"SELECT COUNT(*) FROM {tableName}", connection))
                    {
                        score += Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }

            return score;
        }

        private static void TryMigrateLegacyDatabase()
        {
            string[] migrationCandidates =
            {
                GetLegacyDatabaseFilePath(),
                GetPreviousAppDataDatabaseFilePath()
            };

            string? bestSourceFile = null;
            int bestSourceScore = -1;

            foreach (string candidate in migrationCandidates)
            {
                if (!File.Exists(candidate))
                    continue;

                int candidateScore = GetDatabaseDataScore(candidate);
                if (candidateScore > bestSourceScore)
                {
                    bestSourceScore = candidateScore;
                    bestSourceFile = candidate;
                }
            }

            if (string.IsNullOrWhiteSpace(bestSourceFile))
                return;

            if (!File.Exists(dbPath))
            {
                File.Copy(bestSourceFile, dbPath, true);
                return;
            }

            int currentScore = GetDatabaseDataScore(dbPath);
            if (bestSourceScore > currentScore)
            {
                File.Copy(bestSourceFile, dbPath, true);
            }
        }

        public static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim().ToUpper(TrCulture);
        }

        public static void InitializeDatabase()
        {
            try
            {
                TryMigrateLegacyDatabase();
            }
            catch
            {
            }

            // Debug: Yolu dosyaya yaz
            try
            {
                File.WriteAllText(Path.Combine(dbDirectory, "db_path_log.txt"), 
                    $"Veritabanı Yolu: {dbPath}\nZaman: {DateTime.Now}\nDosya Var: {File.Exists(dbPath)}");
            }
            catch { }
            
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                
                // Profiller Tablosu
                string createProfilTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Profiller (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProfilAdi TEXT NOT NULL UNIQUE,
                        KayitTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";
                
                using (var command = new SQLiteCommand(createProfilTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                // Varsayılan profil ekle
                string insertDefaultQuery = @"
                    INSERT OR IGNORE INTO Profiller (ProfilAdi) VALUES ('Varsayılan')";
                
                using (var command = new SQLiteCommand(insertDefaultQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                // Okul & Koordinatörler Tablosu
                string createOkulTableQuery = @"
                    CREATE TABLE IF NOT EXISTS OkulKoordinatorler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OkulAdi TEXT NOT NULL,
                        Il TEXT,
                        OkulMuduru TEXT,
                        KoordMudurYrd TEXT,
                        KoordOgretmen TEXT,
                        KayitTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";
                
                using (var command = new SQLiteCommand(createOkulTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                // İşletmeler Tablosu
                string createIsletmelerTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Isletmeler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        IsletmeAdi TEXT NOT NULL,
                        Telefon TEXT,
                        Adres TEXT,
                        KayitTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";
                
                using (var command = new SQLiteCommand(createIsletmelerTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                // Alan/Dal Tablosu
                string createAlanDalTableQuery = @"
                    CREATE TABLE IF NOT EXISTS AlanDal (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Alan TEXT NOT NULL,
                        Dal TEXT,
                        KayitTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";
                
                using (var command = new SQLiteCommand(createAlanDalTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                // Koordinatör Tanımlama Tablosu
                string createKoordinatorTableQuery = @"
                    CREATE TABLE IF NOT EXISTS KoordinatorTanimlama (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Okul TEXT NOT NULL,
                        Ogretmen TEXT,
                        Isletme TEXT,
                        IsletmeYetkilisi TEXT,
                        MudurYrd TEXT,
                        Gun TEXT,
                        KoordTuru TEXT DEFAULT 'MESEM',
                        KayitTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";
                
                using (var command = new SQLiteCommand(createKoordinatorTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Mevcut tabloya KoordTuru kolonu ekleme denemesi
                try
                {
                    using (var command = new SQLiteCommand("ALTER TABLE KoordinatorTanimlama ADD COLUMN KoordTuru TEXT DEFAULT 'MESEM'", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch { }

                // Mevcut tabloya IsletmeYetkilisi kolonu ekleme denemesi
                try
                {
                    using (var command = new SQLiteCommand("ALTER TABLE KoordinatorTanimlama ADD COLUMN IsletmeYetkilisi TEXT", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch { }
                
                // Öğrenciler Tablosu
                string createOgrencilerTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Ogrenciler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OkulNo TEXT NOT NULL,
                        AdSoyad TEXT NOT NULL,
                        Sinif TEXT,
                        AlanDal TEXT,
                        Koordinator TEXT,
                        Gunler TEXT,
                        KayitTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";
                
                using (var command = new SQLiteCommand(createOgrencilerTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                // Çalışma Takvimi Tablosu
                string createCalismaTakvimiTableQuery = @"
                    CREATE TABLE IF NOT EXISTS CalismaTakvimi (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Yil INTEGER NOT NULL,
                        Baslangic TEXT NOT NULL,
                        Bitis TEXT NOT NULL,
                        Aciklama TEXT,
                        KayitTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";
                
                using (var command = new SQLiteCommand(createCalismaTakvimiTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                // Devamsızlık Tablosu
                string createDevamsizlikTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Devamsizlik (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OgrenciId INTEGER NOT NULL,
                        Tarih TEXT NOT NULL,
                        Sembol TEXT,
                        Aciklama TEXT,
                        KayitTarihi DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UNIQUE(OgrenciId, Tarih)
                    )";
                
                using (var command = new SQLiteCommand(createDevamsizlikTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        public static string GetDatabaseFilePath()
        {
            return dbPath;
        }
    }
}
