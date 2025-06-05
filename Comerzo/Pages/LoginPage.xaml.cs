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
        usuarioManager = FabricManager.UsuarioManager;
    }

    private async void btnIniciarSesion_Clicked(object sender, EventArgs e)
    {
        string nombreUsuario = entNombre.Text?.Trim();
        string claveUsuario = entClave.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nombreUsuario) ||
            string.IsNullOrWhiteSpace(claveUsuario))
        {
            await DisplayAlert("Comerzo", "Por favor, complete todos los campos.", "Ok");
            return;
        }

        // Deshabilitar el botón e iniciar indicador de actividad
        SetLoadingState(true);

        await OnLogin(nombreUsuario, claveUsuario);

        // Habilitar el botón y ocultar indicador de actividad
        SetLoadingState(false);
    }

    private void SetLoadingState(bool isLoading)
    {
        // Cambiar el estado del botón
        btnIniciarSesion.IsEnabled = !isLoading;
        btnIniciarSesion.Text = isLoading ? "Iniciando sesión..." : "Iniciar sesión";
        
        // Mostrar/ocultar el indicador de actividad
        loginActivityIndicator.IsRunning = isLoading;
        loginActivityIndicator.IsVisible = isLoading;
    }

    public async Task OnLogin(string nombreUsuario, string claveUsuario)
    {
        try
        {
            // Usar await correctamente con Login - evita usar .Result que puede causar bloqueos
            usuario usuario = await usuarioManager.Login(new LoginModel() 
            { 
                nombre_usuario_login = nombreUsuario, 
                clave_usuario = claveUsuario 
            });

            if (usuario != null)
            {
                COMMON.Params.UsuarioConectado = usuario.nombre_usuario_login;
                // Navega a la página principal
                await Shell.Current.GoToAsync("///InicioPage");
            }
            else
            {
                await DisplayAlert("Comerzo", "Nombre de usuario y/o Contraseña incorrecta", "Ok");
            }
        }
        catch (Exception ex)
        {
            // Captura cualquier excepción inesperada durante la operación de base de datos
            await DisplayAlert("Comerzo", $"Ocurrió un error inesperado: {ex.Message}", "Ok");
            // Registra los detalles de la excepción para depuración
            Console.WriteLine($"Error al iniciar sesión: {ex.Message}");
        }
    }
}