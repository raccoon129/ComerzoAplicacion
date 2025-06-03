using BIZ;
using COMMON.Entidades;
using COMMON.Validadores;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Comerzo.Pages;

public partial class InicioPage : ContentPage, INotifyPropertyChanged
{
    // Implementación de INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Propiedades públicas para el binding
    private decimal _totalVentasDia;
    public decimal TotalVentasDia
    {
        get => _totalVentasDia;
        set
        {
            _totalVentasDia = value;
            OnPropertyChanged();
        }
    }

    private int _totalProductosVendidos;
    public int TotalProductosVendidos
    {
        get => _totalProductosVendidos;
        set
        {
            _totalProductosVendidos = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<ProductoPopular> _productosPopulares = new ObservableCollection<ProductoPopular>();
    public ObservableCollection<ProductoPopular> ProductosPopulares
    {
        get => _productosPopulares;
        set
        {
            _productosPopulares = value;
            OnPropertyChanged();
            // Actualizar propiedad derivada cuando cambia la colección
            OnPropertyChanged(nameof(HayProductosPopulares));
            OnPropertyChanged(nameof(MostrarMensajeNoProductosPopulares));
        }
    }

    // Propiedad para determinar si hay productos populares
    public bool HayProductosPopulares => ProductosPopulares != null && ProductosPopulares.Count > 0;

    // Propiedad para mostrar mensaje de no hay productos populares
    public bool MostrarMensajeNoProductosPopulares => !Cargando && (ProductosPopulares == null || ProductosPopulares.Count == 0);

    private ObservableCollection<producto> _productosBajoStock = new ObservableCollection<producto>();
    public ObservableCollection<producto> ProductosBajoStock
    {
        get => _productosBajoStock;
        set
        {
            _productosBajoStock = value;
            OnPropertyChanged();
            // Actualizar propiedad derivada cuando cambia la colección
            OnPropertyChanged(nameof(HayProductosBajoStock));
        }
    }

    // Propiedad para determinar si hay productos con bajo stock
    public bool HayProductosBajoStock => ProductosBajoStock != null && ProductosBajoStock.Count > 0;

    private bool _cargando;
    public bool Cargando
    {
        get => _cargando;
        set
        {
            _cargando = value;
            OnPropertyChanged();
            // Actualizar propiedades dependientes
            OnPropertyChanged(nameof(MostrarMensajeNoProductosPopulares));
            OnPropertyChanged(nameof(HayProductosPopulares));
            OnPropertyChanged(nameof(HayProductosBajoStock));
        }
    }

    // Comando para la RefreshView
    public Command CargarDatosCommand { get; private set; }
    // En la región de propiedades, agregar el comando para nueva venta
    public ICommand NuevaVentaCommand { get; private set; }


    

    // Managers para acceso a datos
    private VentaManager _ventaManager;
    private ProductoManager _productoManager;
    private InventarioManager _inventarioManager;
    private VentaDetalleManager _ventaDetalleManager;

    public class ProductoPopular
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public int PorcentajeVentas { get; set; }
    }

    public InicioPage()
    {
        InitializeComponent();

        // Inicializar el comando para recargar datos
        CargarDatosCommand = new Command(async () => await CargarDatos());

        NuevaVentaCommand = new Command(async () => await NavegarANuevaVenta());

        // Establecer el BindingContext a esta misma página
        BindingContext = this;

        try
        {
            // Inicializar validadores
            var productoValidator = new productoValidator();
            var ventaValidator = new ventaValidator();
            var ventaDetalleValidator = new ventaDetalleValidator();
            var inventarioValidator = new inventarioValidator();

            // Inicializar managers en orden de dependencia
            _productoManager = new ProductoManager(productoValidator);
            _inventarioManager = new InventarioManager(inventarioValidator, _productoManager);
            _ventaManager = new VentaManager(ventaValidator, _inventarioManager);
            _ventaDetalleManager = new VentaDetalleManager(ventaDetalleValidator, _productoManager, _inventarioManager);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al inicializar managers: {ex.Message}");
            DisplayAlert("Error de inicialización", $"Error al inicializar los servicios: {ex.Message}", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarDatos();
    }

    /// <summary>
    /// Carga todos los datos necesarios para la página de inicio
    /// </summary>
    private async Task CargarDatos()
    {
        Cargando = true;
        try
        {
            // Verificar que los managers estén inicializados
            if (_ventaManager == null || _productoManager == null || _ventaDetalleManager == null)
            {
                throw new InvalidOperationException("Los servicios no están correctamente inicializados");
            }

            // Cargar cada sección de datos por separado para identificar mejor dónde está el error
            await CargarResumenVentas();
            await CargarProductosPopulares();
            await CargarInventarioBajo();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar datos: {ex}");
            await DisplayAlert("Error", $"Error al cargar los datos: {ex.Message}", "OK");
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
            Debug.WriteLine("Iniciando carga de resumen de ventas...");
            var resumen = await _ventaManager.ObtenerResumenVentasDiario(_ventaDetalleManager);
            TotalVentasDia = resumen.MontoTotal;
            TotalProductosVendidos = resumen.ProductosVendidos;
            Debug.WriteLine($"Resumen cargado: {TotalVentasDia:C}, {TotalProductosVendidos} productos");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar resumen: {ex}");
            await DisplayAlert("Error", $"Error al cargar resumen de ventas: {ex.Message}", "OK");
            // Establecer valores por defecto en caso de error
            TotalVentasDia = 0;
            TotalProductosVendidos = 0;
        }
    }

    /// <summary>
    /// Carga los productos más populares del día
    /// </summary>
    private async Task CargarProductosPopulares()
    {
        try
        {
            Debug.WriteLine("Iniciando carga de productos populares...");
            var populares = await _ventaDetalleManager.ObtenerProductosPopularesDiarios(_ventaManager, _productoManager);

            ProductosPopulares.Clear();

            if (populares != null && populares.Any())
            {
                Debug.WriteLine($"Se encontraron {populares.Count} productos populares");
                foreach (var p in populares)
                {
                    Debug.WriteLine($"Añadiendo: {p.Nombre}, {p.Cantidad}, {p.PorcentajeVentas}%");
                    ProductosPopulares.Add(new ProductoPopular
                    {
                        Nombre = p.Nombre,
                        Cantidad = p.Cantidad,
                        PorcentajeVentas = p.PorcentajeVentas
                    });
                }
            }
            else
            {
                Debug.WriteLine("No se encontraron productos populares");
            }

            // Forzar actualización de propiedades derivadas
            OnPropertyChanged(nameof(HayProductosPopulares));
            OnPropertyChanged(nameof(MostrarMensajeNoProductosPopulares));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar productos populares: {ex}");
            if (_ventaDetalleManager != null && !string.IsNullOrEmpty(_ventaDetalleManager.Error))
            {
                Debug.WriteLine($"Error específico del manager: {_ventaDetalleManager.Error}");
            }

            await DisplayAlert("Error", $"Error al cargar productos populares: {ex.Message}", "OK");
            ProductosPopulares.Clear();
        }
    }

    /// <summary>
    /// Carga los productos con inventario bajo (menos de 5 unidades)
    /// </summary>
    private async Task CargarInventarioBajo()
    {
        try
        {
            Debug.WriteLine("Iniciando carga de productos con bajo stock...");
            var bajoStock = await _productoManager.ObtenerProductosBajoStock(5);

            ProductosBajoStock.Clear();
            if (bajoStock != null && bajoStock.Any())
            {
                Debug.WriteLine($"Se encontraron {bajoStock.Count} productos con bajo stock");
                foreach (var p in bajoStock)
                {
                    ProductosBajoStock.Add(p);
                }
            }
            else
            {
                Debug.WriteLine("No se encontraron productos con bajo stock");
            }

            OnPropertyChanged(nameof(HayProductosBajoStock));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar inventario bajo: {ex}");
            await DisplayAlert("Error", $"Error al cargar inventario bajo: {ex.Message}", "OK");
            ProductosBajoStock.Clear();
        }
    }

    // Agregar el método para navegar a la página de nueva venta
    private async Task NavegarANuevaVenta()
    {
        try
        {
            await Navigation.PushAsync(new NuevaVentaProcesoPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al abrir página de ventas: {ex.Message}", "OK");
        }
    }
}