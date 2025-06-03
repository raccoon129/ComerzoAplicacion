namespace Comerzo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Light; // Establecer el tema de la aplicación
            MainPage = new AppShell();
        }
    }
}
