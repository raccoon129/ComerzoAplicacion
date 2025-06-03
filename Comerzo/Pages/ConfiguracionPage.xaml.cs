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
        // Si no hay info existente, inicializa
        if (infoComercio == null)
        {
            infoComercio = new informacion_comercio
            {
                fecha_creacion = DateTime.Now
            };
        }

        // Navegar a una p�gina de edici�n o abrir un DisplayPromptAsync secuencial (r�pido y funcional)
        string nombre = await DisplayPromptAsync("Nombre del Comercio", "Ingresa el nombre:", initialValue: infoComercio.nombre_comercio);
        if (string.IsNullOrWhiteSpace(nombre)) return;
        infoComercio.nombre_comercio = nombre;

        string razon = await DisplayPromptAsync("Raz�n Social", "Ingresa la raz�n social:", initialValue: infoComercio.razon_social);
        infoComercio.razon_social = razon;

        string descripcion = await DisplayPromptAsync("Descripci�n", "Ingresa una descripci�n:", initialValue: infoComercio.descripcion);
        if (string.IsNullOrWhiteSpace(descripcion)) return;
        infoComercio.descripcion = descripcion;

        string encargado = await DisplayPromptAsync("Encargado", "ID del encargado:", initialValue: infoComercio.encargado.ToString());
        if (!int.TryParse(encargado, out int idEncargado)) return;
        infoComercio.encargado = idEncargado;

        // Guardar en API
        var resultado = await infoComercioManager.GuardarInformacionComercio(infoComercio);

        if (resultado != null)
        {
            await DisplayAlert("�xito", "Informaci�n guardada correctamente", "OK");
            CargarInformacion();
        }
        else
        {
            await DisplayAlert("Error", infoComercioManager.Error, "OK");
        }
    }
}
