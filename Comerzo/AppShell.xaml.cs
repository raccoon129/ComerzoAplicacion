namespace Comerzo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();


            Routing.RegisterRoute("InicioPage", typeof(Pages.InicioPage));
            Routing.RegisterRoute("VentaPage", typeof(Pages.VentaPage));
            Routing.RegisterRoute("InventarioPage", typeof(Pages.InventarioPage));
            Routing.RegisterRoute("ConfiguracionPage", typeof(Pages.ConfiguracionPage));
        }
    }
}
