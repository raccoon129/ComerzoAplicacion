using BIZ;
using COMMON.Entidades;
using COMMON.Validadores;

namespace Comerzo.Pages;

public partial class InformacionComercioFormPage : ContentPage
{
    private InformacionComercioManager infoComercioManager;
    private informacion_comercio infoComercio;

    public InformacionComercioFormPage()
    {
        InitializeComponent();
        infoComercioManager = new InformacionComercioManager(new informacionComercioValidator());

    }

    // Constructor para editar información existente
    public InformacionComercioFormPage(informacion_comercio infoComercioToEdit)
    {
        InitializeComponent();
        infoComercio = infoComercioToEdit;
        infoComercioManager = new InformacionComercioManager(new informacionComercioValidator());

        // Populate the form fields with the existing data
        txtNombreComercio.Text = infoComercio.nombre_comercio;
        txtRazonSocial.Text = infoComercio.razon_social;
        txtDescripcion.Text = infoComercio.descripcion;
        txtEncargado.Text = infoComercio.encargado.ToString();
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        // Update the object with values from the form fields
        infoComercio.nombre_comercio = txtNombreComercio.Text;
        infoComercio.razon_social = txtRazonSocial.Text;
        infoComercio.descripcion = txtDescripcion.Text;
        if (infoComercio == null)
        {
            infoComercio = new informacion_comercio
            {
                fecha_creacion = DateTime.Now
            };
        }
        // Validate encargado ID
        if (!int.TryParse(txtEncargado.Text, out int encargadoId))
        {
            await DisplayAlert("Error de entrada", "El ID del encargado debe ser un número válido.", "OK");
            return; // Stop execution if validation fails
        }
        infoComercio.encargado = encargadoId;

        // Call the saving logic
        await GuardarInformacionComercio();
    }

    private async Task GuardarInformacionComercio() // Changed method name for clarity
    {
        // Guardar en API
        var resultado = await infoComercioManager.GuardarInformacionComercio(infoComercio);

        if (resultado != null)
        {
            await DisplayAlert("Éxito", "Información guardada correctamente", "OK");
            // Navigate back to the previous page (ConfiguracionPage)
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", infoComercioManager.Error, "OK");
        }
    }
}