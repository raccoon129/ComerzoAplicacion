using BIZ;
using COMMON.Entidades;
using COMMON.Validadores;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Comerzo.Pages;
// ESTA SIRVE PARA VER LOS DETALLES DE CADA VENTA
// SE MUESTRA CUANDO SE COMPLETA UNA VENTA EN NUEVA VENTA
// Y EN LA PÁGINA DEL INVENTARIO PARA VER LOS DETALLES DE UNA VENTA POR ID

//GRAN PARTE DE ESTA ADAPTACIÓN DE NUEVA VENTA DEL BLAZOR FUE HECHO CON IA

public partial class DetalleVenta : ContentPage, INotifyPropertyChanged
{
    // Implementación de INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Managers para operaciones de negocio
    private ProductoManager _productoManager;
    private VentaManager _ventaManager;
    private VentaDetalleManager _ventaDetalleManager;

    // ID de la venta a mostrar
    private int _ventaId;
    public int VentaId
    {
        get => _ventaId;
        set
        {
            if (_ventaId != value)
            {
                _ventaId = value;
                OnPropertyChanged();
            }
        }
    }

    // Propiedad para rastrear la página de origen de la navegación
    public enum PaginaOrigen
    {
        InicioPage,
        VentaPage,
        None
    }

    private PaginaOrigen _paginaDeOrigen = PaginaOrigen.None;
    public PaginaOrigen PaginaDeOrigen
    {
        get => _paginaDeOrigen;
        set
        {
            _paginaDeOrigen = value;
            OnPropertyChanged();
        }
    }

    // Datos de la venta
    private venta _venta;
    public venta Venta
    {
        get => _venta;
        set
        {
            _venta = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FechaHoraFormateada));
            OnPropertyChanged(nameof(MontoTotalFormateado));
        }
    }

    // Lista de detalles de la venta
    private ObservableCollection<DetalleVentaItem> _detalles = new ObservableCollection<DetalleVentaItem>();
    public ObservableCollection<DetalleVentaItem> Detalles
    {
        get => _detalles;
        set
        {
            _detalles = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HayDetalles));
        }
    }

    // Variables de estado
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

    // Propiedades derivadas
    public bool HayDetalles => Detalles != null && Detalles.Count > 0;
    public string FechaHoraFormateada => Venta?.fecha_hora_venta.ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
    public string MontoTotalFormateado => Venta != null ? Venta.monto_total_venta.ToString("C", CultureInfo.GetCultureInfo("es-MX")) : "$0.00";

    // Clase para mostrar detalles de venta con información del producto
    public class DetalleVentaItem : INotifyPropertyChanged
    {
        private venta_detalle _detalle;
        private string _nombreProducto;

        public venta_detalle Detalle
        {
            get => _detalle;
            set
            {
                _detalle = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PrecioUnitarioFormateado));
                OnPropertyChanged(nameof(SubtotalFormateado));
            }
        }

        public string NombreProducto
        {
            get => _nombreProducto;
            set
            {
                _nombreProducto = value;
                OnPropertyChanged();
            }
        }

        public string PrecioUnitarioFormateado => Detalle?.precio_unitario_venta.ToString("C", CultureInfo.GetCultureInfo("es-MX")) ?? "$0.00";
        public string SubtotalFormateado => Detalle?.subtotal_detalle.ToString("C", CultureInfo.GetCultureInfo("es-MX")) ?? "$0.00";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Constructor por defecto (para inicialización desde XAML)
    public DetalleVenta()
    {
        InitializeComponent();
        BindingContext = this;
        InicializarManagers();
    }

    // Constructor con ID de venta y página de origen
    public DetalleVenta(int ventaId, PaginaOrigen origen = PaginaOrigen.None)
    {
        InitializeComponent();
        BindingContext = this;
        InicializarManagers();
        VentaId = ventaId;
        PaginaDeOrigen = origen;
    }

    // Inicialización de los managers
    private void InicializarManagers()
    {
        try
        {
            // Inicializar validadores
            var productoValidator = new productoValidator();
            var ventaValidator = new ventaValidator();
            var ventaDetalleValidator = new ventaDetalleValidator();
            var inventarioValidator = new inventarioValidator();

            // Inicializar managers en orden de dependencia
            _productoManager = new ProductoManager(productoValidator);
            _ventaManager = new VentaManager(ventaValidator, null);
            _ventaDetalleManager = new VentaDetalleManager(ventaDetalleValidator, _productoManager, null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al inicializar managers: {ex.Message}");
            DisplayAlert("Error", "Error al inicializar servicios necesarios", "Aceptar");
        }
    }

    // Se ejecuta cuando la página aparece
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Si tenemos un ID de venta, cargamos los datos
        if (_ventaId > 0)
        {
            await CargarDatosVenta();
        }
    }

    // Carga los datos de la venta desde los managers
    private async Task CargarDatosVenta()
    {
        if (VentaId <= 0)
        {
            await DisplayAlert("Error", "ID de venta no válido", "Aceptar");
            return;
        }

        Cargando = true;

        try
        {
            // Verificar que los managers estén disponibles
            if (_ventaManager == null || _ventaDetalleManager == null || _productoManager == null)
            {
                await DisplayAlert("Error", "No se han inicializado los servicios correctamente", "Aceptar");
                return;
            }

            // Cargar la venta
            Venta = await _ventaManager.ObtenerPorId(VentaId);
            if (Venta == null)
            {
                await DisplayAlert("Error", $"No se pudo cargar la venta: {_ventaManager.Error}", "Aceptar");
                return;
            }

            // Actualizar título de la página
            Title = $"Venta #{Venta.id_venta}";

            // Cargar detalles
            var detallesVenta = await _ventaDetalleManager.ObtenerPorVenta(VentaId);
            if (detallesVenta == null || !detallesVenta.Any())
            {
                Debug.WriteLine($"No se encontraron detalles para la venta {VentaId}");
                return;
            }

            // Cargar información de productos
            var productos = await _productoManager.ObtenerTodos();
            if (productos == null)
            {
                Debug.WriteLine($"No se pudieron cargar los productos: {_productoManager.Error}");
                // Continuamos sin información de productos
            }

            var productosDict = productos?.ToDictionary(p => p.id_producto) ?? new Dictionary<int, producto>();

            // Crear items para la UI con información del producto
            Detalles.Clear();
            foreach (var detalle in detallesVenta)
            {
                var nombreProducto = productosDict.TryGetValue(detalle.id_producto, out var producto)
                    ? producto.nombre_producto
                    : $"Producto #{detalle.id_producto}";

                Detalles.Add(new DetalleVentaItem
                {
                    Detalle = detalle,
                    NombreProducto = nombreProducto
                });
            }

            Debug.WriteLine($"Venta cargada: ID={Venta.id_venta}, Total={Venta.monto_total_venta}, Detalles={Detalles.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar datos de venta: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "Aceptar");
        }
        finally
        {
            Cargando = false;
        }
    }

    // Manejador del evento Click del botón Volver
    private async void BotonVolver_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Usar PopToRoot si venimos de InicioPage para evitar problemas de ruta
            if (PaginaDeOrigen == PaginaOrigen.InicioPage)
            {
                // En lugar de intentar navegar de vuelta a InicioPage (que causa rutas ambiguas),
                // simplemente volvemos a la raíz de la pila de navegación
                await Navigation.PopToRootAsync();
            }
            else
            {
                // Para otras páginas, usamos el PopAsync normal
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al navegar hacia atrás: {ex.Message}");

            // Si hay error, intentamos un enfoque alternativo
            try
            {
                // Intento alternativo en caso de error
                await Navigation.PopToRootAsync();

                // Si venimos de VentaPage y necesitamos volver a ella específicamente
                if (PaginaDeOrigen == PaginaOrigen.VentaPage)
                {
                    await Shell.Current.GoToAsync("//VentaPage");
                }
            }
            catch (Exception fallbackEx)
            {
                Debug.WriteLine($"Error en navegación alternativa: {fallbackEx.Message}");
                await DisplayAlert("Error de navegación", "No se pudo volver a la página anterior.", "Aceptar");
            }
        }
    }

    // Método para actualizar el ID de la venta y cargar sus datos
    // Útil cuando la página ya está creada y se quiere cambiar la venta mostrada
    public async Task ActualizarVenta(int nuevoVentaId, PaginaOrigen origen = PaginaOrigen.None)
    {
        VentaId = nuevoVentaId;
        if (origen != PaginaOrigen.None)
        {
            PaginaDeOrigen = origen;
        }
        await CargarDatosVenta();
    }
}