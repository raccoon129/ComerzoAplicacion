using BIZ;
using COMMON.Entidades;
using COMMON.Validadores;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Diagnostics;
using System.Text;

namespace Comerzo.Pages;

public partial class VentaPage : ContentPage, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Propiedades de Binding

    // Resumen del día
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

    // Ultimas ventas
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
            CalcularResumenVentas(); // Recalcular resumen al cambiar las ventas
        }
    }

    // Estados de UI
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

    private bool _showNoSalesMessage = true;
    public bool ShowNoSalesMessage
    {
        get => _showNoSalesMessage;
        set { _showNoSalesMessage = value; OnPropertyChanged(); }
    }

    // Propiedades para el resumen total de ventas
    private int _totalVentas;
    public int TotalVentas
    {
        get => _totalVentas;
        set { _totalVentas = value; OnPropertyChanged(); }
    }

    private decimal _montoTotalVentas;
    public decimal MontoTotalVentas
    {
        get => _montoTotalVentas;
        set { _montoTotalVentas = value; OnPropertyChanged(); }
    }

    private decimal _promedioVentas;
    public decimal PromedioVentas
    {
        get => _promedioVentas;
        set { _promedioVentas = value; OnPropertyChanged(); }
    }

    #endregion

    #region Comandos

    public ICommand CargarDatosCommand { get; }
    public ICommand VerDetalleCommand { get; }

    #endregion

    // Managers
    private VentaManager _ventaManager;
    private ProductoManager _productoManager;
    private InventarioManager _inventarioManager;
    private VentaDetalleManager _ventaDetalleManager;

    public VentaPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Inicializar comandos
        CargarDatosCommand = new Command(async () => await CargarDatos());
        VerDetalleCommand = new Command<venta>(async (v) => await NavegarADetalleVenta(v));

        try
        {
            // Inicializar validadores y managers
            var productoValidator = new productoValidator();
            var ventaValidator = new ventaValidator();
            var ventaDetalleValidator = new ventaDetalleValidator();
            var inventarioValidator = new inventarioValidator();

            _productoManager = new ProductoManager(productoValidator);
            _inventarioManager = new InventarioManager(inventarioValidator, _productoManager);
            _ventaManager = new VentaManager(ventaValidator, _inventarioManager);
            _ventaDetalleManager = new VentaDetalleManager(ventaDetalleValidator, _productoManager, _inventarioManager);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al inicializar managers: {ex.Message}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarDatos();
    }

    /// <summary>
    /// Carga todos los datos necesarios para la página
    /// </summary>
    private async Task CargarDatos()
    {
        Cargando = true;
        try
        {
            // Cargar datos en paralelo para mejorar rendimiento
            var tareasCargas = new List<Task>
            {
                CargarResumenVentas(),
                CargarUltimasVentas()
            };

            await Task.WhenAll(tareasCargas);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar datos: {ex.Message}", "OK");
        }
        finally
        {
            Cargando = false;
        }
    }

    /// <summary>
    /// Carga el resumen de ventas del día actual
    /// </summary>
    private async Task CargarResumenVentas()
    {
        try
        {
            // Verificar inicialización de managers
            if (_ventaManager == null || _ventaDetalleManager == null)
            {
                Debug.WriteLine("Error: Managers no inicializados correctamente");
                return;
            }

            var resumen = await _ventaManager.ObtenerResumenVentasDiario(_ventaDetalleManager);
            TotalVentasDia = resumen.MontoTotal;
            TotalProductosVendidos = resumen.ProductosVendidos;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar resumen de ventas: {ex.Message}");
            // Establecer valores por defecto en caso de error
            TotalVentasDia = 0;
            TotalProductosVendidos = 0;
        }
    }

    /// <summary>
    /// Carga las últimas ventas realizadas
    /// </summary>
    private async Task CargarUltimasVentas()
    {
        try
        {
            // Verificar inicialización de manager
            if (_ventaManager == null)
            {
                Debug.WriteLine("Error: VentaManager no inicializado correctamente");
                return;
            }

            var ventasRecientes = await _ventaManager.ObtenerUltimasVentas(10);

            // Crear una nueva colección para garantizar que se disparen los eventos
            var tempUltimasVentas = new ObservableCollection<venta>();
            if (ventasRecientes != null)
            {
                foreach (var v in ventasRecientes)
                {
                    tempUltimasVentas.Add(v);
                }
            }

            UltimasVentas = tempUltimasVentas; // Esto actualiza ShowSalesList y ShowNoSalesMessage
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar últimas ventas: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar últimas ventas: {ex.Message}", "OK");
            UltimasVentas = new ObservableCollection<venta>(); // Asegurar estado limpio en error
        }
    }

    /// <summary>
    /// Calcula el resumen de ventas para mostrar en la parte inferior
    /// </summary>
    private void CalcularResumenVentas()
    {
        if (UltimasVentas == null || !UltimasVentas.Any())
        {
            TotalVentas = 0;
            MontoTotalVentas = 0;
            PromedioVentas = 0;
            return;
        }

        TotalVentas = UltimasVentas.Count;
        MontoTotalVentas = UltimasVentas.Sum(v => v.monto_total_venta);
        PromedioVentas = MontoTotalVentas / TotalVentas;
    }

    /// <summary>
    /// Navega a la página de detalle de venta cuando se selecciona una venta
    /// </summary>
    private async Task NavegarADetalleVenta(venta venta)
    {
        if (venta == null) return;

        try
        {
            Cargando = true;

            // Verificar que la venta existe antes de navegar
            var ventaVerificada = await _ventaManager.ObtenerPorId(venta.id_venta);
            if (ventaVerificada == null)
            {
                await DisplayAlert("Error", "No se pudo encontrar la venta seleccionada.", "OK");
                return;
            }

            // Navegar a la página de detalle de venta indicando que venimos de VentaPage
            await Navigation.PushAsync(new DetalleVenta(venta.id_venta, DetalleVenta.PaginaOrigen.VentaPage));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al navegar al detalle de la venta: {ex.Message}", "OK");
        }
        finally
        {
            Cargando = false;
        }
    }

    // El método MostrarDetalleVenta ya no es necesario, lo hemos reemplazado por NavegarADetalleVenta
}