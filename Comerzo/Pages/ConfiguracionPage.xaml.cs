using BIZ;
using COMMON.Entidades;
using COMMON.Validadores;

namespace Comerzo.Pages;

public partial class ConfiguracionPage : ContentPage
{
    private InformacionComercioManager infoComercioManager;
    private informacion_comercio infoComercio;

    public ConfiguracionPage()
    {
        InitializeComponent();
        infoComercioManager = new InformacionComercioManager(new informacionComercioValidator());

        CargarInformacion();
    }

    private async void CargarInformacion()
    {
        infoComercio = await infoComercioManager.ObtenerInformacionComercio();

        if (infoComercio == null)
        {
            SinInformacionLayout.IsVisible = true;
            InformacionLayout.IsVisible = false;
        }
        else
        {
            lblNombreComercio.Text = infoComercio.nombre_comercio;
            lblRazonSocial.Text = string.IsNullOrWhiteSpace(infoComercio.razon_social) ? "No especificado" : infoComercio.razon_social;
            lblFechaCreacion.Text = infoComercio.fecha_creacion.ToString("dd/MM/yyyy");
            lblDescripcion.Text = infoComercio.descripcion;
            lblEncargado.Text = infoComercio.encargado.ToString();

            SinInformacionLayout.IsVisible = false;
            InformacionLayout.IsVisible = true;
        }
    }
 private async void OnEditarClicked(object sender, EventArgs e)
    {
        if (infoComercio == null)
        {
            // If no existing info, create a new instance to pass to the form
            // Or use the parameterless constructor of InformacionComercioFormPage
            await Navigation.PushAsync(new InformacionComercioFormPage());
        }
        else
        {
            // Pass the existing info to the form for editing
            await Navigation.PushAsync(new InformacionComercioFormPage(infoComercio));
        }
    }
}
