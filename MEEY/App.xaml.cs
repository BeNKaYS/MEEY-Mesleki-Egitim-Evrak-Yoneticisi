using System.Windows;
using MEEY.Database;

namespace MEEY
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DatabaseManager.InitializeDatabase();
        }
    }
}
