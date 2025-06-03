using BIZ; // Tus managers
using COMMON.Entidades; // Tus entidades como 'producto' y 'venta'
using COMMON.Validadores; // Tus validadores
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input; // Para ICommand
using System.Linq; // Para .Any()

namespace Comerzo.Pages;

public partial class VentaPage : ContentPage, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private decimal _totalVentasDia;
    public decimal TotalVentasDia
    {
        get => _totalVentasDia;
        set { _totalVentasDia = value; OnPropertyChanged(); }
    }

    private int _totalProductosVendidos;
    public int TotalProductosVendidos
    {
        get => _totalProductosVendidos;
        set { _totalProductosVendidos = value; OnPropertyChanged(); }
    }

    private ObservableCollection<venta> _ultimasVentas = new ObservableCollection<venta>();
    public ObservableCollection<venta> UltimasVentas
    {
        get => _ultimasVentas;
        set
        {
            _ultimasVentas = value;
            OnPropertyChanged();
            ShowSalesList = value != null && value.Any();
            ShowNoSalesMessage = !ShowSalesList;
        }
    }

    private bool _cargando;
    public bool Cargando
    {
        get => _cargando;
        set { _cargando = value; OnPropertyChanged(); }
    }

    private bool _showSalesList;
    public bool ShowSalesList
    {
        get => _showSalesList;
        set { _showSalesList = value; OnPropertyChanged(); }
    }

    private bool _showNoSalesMessage = true; // Iniciar como true hasta que se carguen datos
    public bool ShowNoSalesMessage
    {
        get => _showNoSalesMessage;
        set { _showNoSalesMessage = value; OnPropertyChanged(); }
    }
    public ICommand CargarDatosCommand { get; }


    private VentaManager _ventaManager;
    private ProductoManager _productoManager;
    private InventarioManager _inventarioManager;
    private VentaDetalleManager _ventaDetalleManager;


    public VentaPage()
    {
        InitializeComponent();
        BindingContext = this;

        CargarDatosCommand = new Command(async () => await CargarDatos());

        var productoValidator = new productoValidator();
        var ventaValidator = new ventaValidator();
        var ventaDetalleValidator = new ventaDetalleValidator();
        var inventarioValidator = new inventarioValidator();

        _productoManager = new ProductoManager(productoValidator);
        _inventarioManager = new InventarioManager(inventarioValidator, _productoManager);
        _ventaManager = new VentaManager(ventaValidator, _inventarioManager);
        _ventaDetalleManager = new VentaDetalleManager(ventaDetalleValidator, _productoManager, _inventarioManager);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        Cargando = true;
        try
        {
            await CargarResumenVentas();
            await CargarUltimasVentas(); // Esto actualizará UltimasVentas y, a su vez, ShowSalesList/ShowNoSalesMessage
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error General", $"Ocurrió un error al cargar datos: {ex.Message}", "OK");
        }
        finally
        {
            Cargando = false;
        }
    }

    private async Task CargarResumenVentas()
    {
        try
        {
            var resumen = await _ventaManager.ObtenerResumenVentasDiario(_ventaDetalleManager);
            TotalVentasDia = resumen.MontoTotal;
            TotalProductosVendidos = resumen.ProductosVendidos;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar hábitos: {ex.Message}", "OK");
        }
    }

    private async Task CargarUltimasVentas()
    {
        try
        {
            var ventasRecientes = await _ventaManager.ObtenerUltimasVentas(10);

            // Crear una nueva colección temporal y luego asignarla
            // es una forma de asegurar que el setter de UltimasVentas se dispare correctamente.
            var tempUltimasVentas = new ObservableCollection<venta>();
            if (ventasRecientes != null)
            {
                foreach (var v in ventasRecientes)
                {
                    tempUltimasVentas.Add(v);
                }
            }
            UltimasVentas = tempUltimasVentas; // Esto disparará el setter y la lógica de visibilidad
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Ventas", $"Error al cargar últimas ventas: {ex.Message}", "OK");
            UltimasVentas = new ObservableCollection<venta>(); // Asegurar estado limpio en error
        }
    }
}