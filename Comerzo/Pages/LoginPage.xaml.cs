using BIZ;
using COMMON.Entidades;
using COMMON.Modelos;

namespace Comerzo.Pages;

public partial class LoginPage : ContentPage
{
    UsuarioManager usuarioManager;
    public LoginPage()
    {
        InitializeComponent();
        usuarioManager=FabricManager.UsuarioManager;
    }

    private async void btnIniciarSesion_Clicked(object sender, EventArgs e)
    {
        string nombreUsuario = entNombre.Text?.Trim();
        string claveUsuario = entClave.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nombreUsuario) ||
            string.IsNullOrWhiteSpace(claveUsuario))
        {
            await DisplayAlert("Focus", "Por favor, complete todos los campos.", "Ok");
            return;
        }
        await OnLogin(nombreUsuario, claveUsuario);

    }
    public async Task OnLogin(string nombreUsuario, string claveUsuario)
    {
        usuario usuario = usuarioManager.Login(new LoginModel() { nombre_usuario_login = nombreUsuario, clave_usuario = claveUsuario }).Result;

        try
        {
            if (usuario !=null)
            {
                COMMON.Params.UsuarioConectado = usuario.nombre_usuario_login;
                // Navega a la p�gina principal
                await Shell.Current.GoToAsync("///InicioPage");
            }
            else
            {
                await DisplayAlert("Focus", "Nombre de usuario y/o Contrase�a incorrecta", "Ok");
            }

        }
        catch (Exception ex)
        {
            // Captura cualquier excepci�n inesperada durante la operaci�n de base de datos
            await DisplayAlert("Focus", $"Ocurri� un error inesperado: {ex.Message}", "Ok");
            // Registra los detalles de la excepci�n para depuraci�n (ej. en la consola, o un servicio de logging)
            Console.WriteLine($"Error al guardar usuario en CrearCuentaPage: {ex.Message}");
        }
    }
}