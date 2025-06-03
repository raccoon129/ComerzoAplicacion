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
    //public ICommand NuevaVentaCommand { get; }

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
        VerDetalleCommand = new Command<venta>(async (v) => await MostrarDetalleVenta(v));
        //NuevaVentaCommand = new Command(async () => await NavegarANuevaVenta());

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
    /// Muestra los detalles de una venta
    /// </summary>
    private async Task MostrarDetalleVenta(venta venta)
    {
        if (venta == null) return;

        try
        {
            Cargando = true;

            // Obtener los detalles de la venta
            var detalles = await _ventaDetalleManager.ObtenerPorVenta(venta.id_venta);

            if (detalles == null || !detalles.Any())
            {
                await DisplayAlert("Aviso", "No se encontraron detalles para esta venta", "OK");
                return;
            }

            // Preparar el mensaje con todos los detalles
            var mensaje = new StringBuilder();
            mensaje.AppendLine($"Venta #{venta.id_venta}");
            mensaje.AppendLine($"Fecha: {venta.fecha_hora_venta:dd/MM/yyyy HH:mm}");
            mensaje.AppendLine();
            mensaje.AppendLine("Productos:");

            foreach (var detalle in detalles)
            {
                // Obtener información del producto
                var producto = await _productoManager.ObtenerPorId(detalle.id_producto);
                string nombreProducto = producto?.nombre_producto ?? $"Producto #{detalle.id_producto}";

                mensaje.AppendLine($"- {nombreProducto}");
                mensaje.AppendLine($"  {detalle.cantidad_vendida} x ${detalle.precio_unitario_venta:N2} = ${detalle.subtotal_detalle:N2}");
            }

            mensaje.AppendLine();
            mensaje.AppendLine($"Total: ${venta.monto_total_venta:N2}");

            // Mostrar el detalle
            await DisplayAlert($"Detalle de Venta #{venta.id_venta}", mensaje.ToString(), "Cerrar");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar detalles de la venta: {ex.Message}", "OK");
        }
        finally
        {
            Cargando = false;
        }
    }

    /// <summary>
    /// Navega a la página para crear una nueva venta
    /// </summary>
    private async Task NavegarANuevaVenta()
    {
        // Aquí se implementaría la navegación a la página de nueva venta
        await DisplayAlert("Información", "Funcionalidad de nueva venta no implementada", "OK");

        // Cuando esté implementada la página de nueva venta:
        // await Navigation.PushAsync(new NuevaVentaPage());
    }
}