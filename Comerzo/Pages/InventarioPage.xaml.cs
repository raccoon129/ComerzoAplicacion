using BIZ;
using COMMON.Entidades;
using COMMON.Validadores;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input; // Required for ICommand

namespace Comerzo.Pages;

public partial class InventarioPage : ContentPage, INotifyPropertyChanged
{
    // Implementación de INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Propiedades públicas para el binding
    private ObservableCollection<producto> _productos = new ObservableCollection<producto>();
    public ObservableCollection<producto> Productos
    {
        get => _productos;
        set
        {
            _productos = value;
            OnPropertyChanged();
        }
    }

    private bool _cargando;
    public bool Cargando
    {
        get => _cargando;
        set
        {
            _cargando = value;
            OnPropertyChanged();
        }
    }

    public ICommand CargarDatosCommand { get; }

    private ProductoManager _productoManager;

    public InventarioPage()
    {
        InitializeComponent();
        // Establecer el BindingContext a esta misma página
        BindingContext = this;

        // Asumiendo que tienes un validador de producto como en tus otros managers
        var productoValidator = new productoValidator();
        _productoManager = new ProductoManager(productoValidator);

        CargarDatosCommand = new Command(async () => await CargarInventario());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarInventario();
    }

    private async Task CargarInventario()
    {
        if (Cargando)
            return;

        Cargando = true;
        try
        {
            // Obtener todos los productos
            var listaProductos = await _productoManager.ObtenerTodos();

            Productos.Clear();
            if (listaProductos != null)
            {
                foreach (var prod in listaProductos)
                {
                    Productos.Add(prod);
                }
            }
        }
        catch (Exception ex)
        {
            // Manejo de errores, similar a InicioPage
            await DisplayAlert("Error", $"Error al cargar el inventario: {ex.Message}", "OK");
        }
        finally
        {
            Cargando = false;
        }
    }
}