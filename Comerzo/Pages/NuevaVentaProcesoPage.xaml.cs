using BIZ;
using COMMON.Entidades;
using COMMON.Validadores;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;

namespace Comerzo.Pages;
//GRAN PARTE DE ESTA ADAPTACIÓN DE NUEVA VENTA DEL BLAZOR FUE HECHO CON IA


// Extender la clase producto para tener la propiedad CantidadSeleccionada
public class ProductoExtendido : INotifyPropertyChanged
{
    public producto Producto { get; set; }

    private int _cantidadSeleccionada = 0;
    public int CantidadSeleccionada
    {
        get => _cantidadSeleccionada;
        set
        {
            if (_cantidadSeleccionada != value)
            {
                _cantidadSeleccionada = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Propiedades redirigidas para que el binding funcione directamente
    public int id_producto => Producto.id_producto;
    public string nombre_producto => Producto.nombre_producto;
    public string descripcion_producto => Producto.descripcion_producto;
    public decimal precio_producto => Producto.precio_producto;
    public int stock_producto => Producto.stock_producto;
}

public partial class NuevaVentaProcesoPage : ContentPage, INotifyPropertyChanged
{
    // Implementación de INotifyPropertyChanged
    public new event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Managers para operaciones de negocio
    private ProductoManager _productoManager;
    private VentaManager _ventaManager;
    private VentaDetalleManager _ventaDetalleManager;
    private InventarioManager _inventarioManager;

    // Colecciones para la UI
    private ObservableCollection<ProductoExtendido> _productosMostrados = new ObservableCollection<ProductoExtendido>();
    public ObservableCollection<ProductoExtendido> ProductosMostrados
    {
        get => _productosMostrados;
        set
        {
            _productosMostrados = value;
            OnPropertyChanged();
        }
    }

    // Clase para manejar productos seleccionados
    public class ProductoSeleccionado : INotifyPropertyChanged
    {
        public producto Producto { get; set; }
        
        private int _cantidad;
        public int Cantidad 
        { 
            get => _cantidad;
            set 
            {
                if (_cantidad != value)
                {
                    _cantidad = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Subtotal)); // Notificar que Subtotal cambió al cambiar Cantidad
                }
            }
        }
        
        public decimal Subtotal => Producto.precio_producto * Cantidad;
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private ObservableCollection<ProductoSeleccionado> _productosSeleccionados = new ObservableCollection<ProductoSeleccionado>();
    public ObservableCollection<ProductoSeleccionado> ProductosSeleccionados
    {
        get => _productosSeleccionados;
        set
        {
            if (_productosSeleccionados != value)
            {
                _productosSeleccionados = value;
                OnPropertyChanged();
                ActualizarPropiedadesDerivadas();
            }
        }
    }

    // Método auxiliar para actualizar todas las propiedades derivadas de ProductosSeleccionados
    private void ActualizarPropiedadesDerivadas()
    {
        OnPropertyChanged(nameof(HayProductosSeleccionados));
        OnPropertyChanged(nameof(TotalVenta));
        OnPropertyChanged(nameof(BotonCompletarHabilitado));
        OnPropertyChanged(nameof(ResumenHeight));
    }

    // Variables de control de estado
    private bool _cargando = false;
    public bool Cargando
    {
        get => _cargando;
        set
        {
            _cargando = value;
            OnPropertyChanged();
        }
    }

    private bool _procesandoVenta = false;
    public bool ProcesandoVenta
    {
        get => _procesandoVenta;
        set
        {
            _procesandoVenta = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BotonCompletarHabilitado));
        }
    }

    // Propiedades derivadas
    public bool HayProductosSeleccionados => ProductosSeleccionados.Count > 0;

    public decimal TotalVenta => ProductosSeleccionados.Sum(p => p.Subtotal);

    public bool BotonCompletarHabilitado => HayProductosSeleccionados && !ProcesandoVenta;

    public double ResumenHeight => Math.Min(ProductosSeleccionados.Count * 30, 150);

    // Comandos
    public ICommand AgregarProductoCommand { get; private set; }
    public ICommand DisminuirCantidadCommand { get; private set; }
    public ICommand CompletarVentaCommand { get; private set; }

    public NuevaVentaProcesoPage()
    {
        InitializeComponent();

        // Inicializar comandos
        AgregarProductoCommand = new Command<ProductoExtendido>(AgregarProducto);
        DisminuirCantidadCommand = new Command<ProductoExtendido>(DisminuirCantidad);
        CompletarVentaCommand = new Command(async () => await CompletarVenta());

        // Suscribirse a cambios en la colección
        _productosSeleccionados.CollectionChanged += (sender, e) => ActualizarPropiedadesDerivadas();

        // Establecer el BindingContext
        BindingContext = this;

        // Inicializar managers
        InicializarManagers();

        // Cargar productos iniciales
        CargarProductos();
    }

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
            _inventarioManager = new InventarioManager(inventarioValidator, _productoManager);
            _ventaManager = new VentaManager(ventaValidator, _inventarioManager);
            _ventaDetalleManager = new VentaDetalleManager(ventaDetalleValidator, _productoManager, _inventarioManager);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al inicializar managers: {ex.Message}");
            DisplayAlert("Error", $"Error al inicializar servicios: {ex.Message}", "Aceptar");
        }
    }

    private async void CargarProductos()
    {
        Cargando = true;
        try
        {
            // Verificar inicialización de manager
            if (_productoManager == null)
            {
                await DisplayAlert("Error", "El gestor de productos no está inicializado.", "Aceptar");
                return;
            }

            // Cargar todos los productos
            var productos = await _productoManager.ObtenerTodos();
            if (productos == null)
            {
                await DisplayAlert("Error", $"No se pudieron cargar los productos: {_productoManager.Error}", "Aceptar");
                return;
            }

            // Mostrar solo productos con stock disponible
            ProductosMostrados.Clear();
            foreach (var producto in productos.Where(p => p.stock_producto > 0)
                                            .OrderByDescending(p => p.nombre_producto))
            {
                // Crear un ProductoExtendido para cada producto
                ProductosMostrados.Add(new ProductoExtendido
                {
                    Producto = producto,
                    CantidadSeleccionada = 0
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar productos: {ex.Message}", "Aceptar");
        }
        finally
        {
            Cargando = false;
        }
    }

    // Método para agregar un producto a la selección
    private void AgregarProducto(ProductoExtendido productoExtendido)
    {
        if (productoExtendido == null) return;

        var producto = productoExtendido.Producto;

        var productoExistente = ProductosSeleccionados
            .FirstOrDefault(p => p.Producto.id_producto == producto.id_producto);

        if (productoExistente != null)
        {
            // Verificar que no exceda el stock disponible
            if (productoExistente.Cantidad < producto.stock_producto)
            {
                productoExistente.Cantidad++;
                // Actualizar interfaz
                OnPropertyChanged(nameof(ProductosSeleccionados));
                OnPropertyChanged(nameof(TotalVenta));
                OnPropertyChanged(nameof(HayProductosSeleccionados)); // Asegurar que se actualiza
                OnPropertyChanged(nameof(ResumenHeight)); // Actualizar altura del resumen
            }
        }
        else
        {
            // Agregar nuevo producto a la selección
            var nuevoProducto = new ProductoSeleccionado
            {
                Producto = producto,
                Cantidad = 1
            };
            ProductosSeleccionados.Add(nuevoProducto);
            
            // Notificar cambios explícitamente al agregar un nuevo producto
            OnPropertyChanged(nameof(ProductosSeleccionados));
            OnPropertyChanged(nameof(TotalVenta));
            OnPropertyChanged(nameof(HayProductosSeleccionados)); // Importante para mostrar el resumen
            OnPropertyChanged(nameof(ResumenHeight));
        }

        // Actualizar la cantidad mostrada en el producto extendido
        productoExtendido.CantidadSeleccionada = ProductosSeleccionados
            .FirstOrDefault(p => p.Producto.id_producto == producto.id_producto)?.Cantidad ?? 0;

        OnPropertyChanged(nameof(BotonCompletarHabilitado));
    }

    // Método para disminuir la cantidad de un producto
    private void DisminuirCantidad(ProductoExtendido productoExtendido)
    {
        if (productoExtendido == null) return;

        var producto = productoExtendido.Producto;

        var productoExistente = ProductosSeleccionados
            .FirstOrDefault(p => p.Producto.id_producto == producto.id_producto);

        if (productoExistente != null)
        {
            productoExistente.Cantidad--;

            if (productoExistente.Cantidad <= 0)
            {
                ProductosSeleccionados.Remove(productoExistente);
                productoExtendido.CantidadSeleccionada = 0;
            }
            else
            {
                productoExtendido.CantidadSeleccionada = productoExistente.Cantidad;
            }

            // Actualizar interfaz - notificamos todos los cambios relevantes
            OnPropertyChanged(nameof(ProductosSeleccionados));
            OnPropertyChanged(nameof(TotalVenta));
            OnPropertyChanged(nameof(HayProductosSeleccionados)); // Importante si se elimina el último producto
            OnPropertyChanged(nameof(ResumenHeight)); // Actualizar altura del resumen
        }

        OnPropertyChanged(nameof(BotonCompletarHabilitado));
    }

    /// <summary>
    /// Procesa y completa la venta actual.
    /// Este método sigue los siguientes pasos:
    /// 1. Crea un registro 'venta' con la información general de la transacción.
    /// 2. Para cada producto seleccionado:
    ///    a. Registra un 'venta_detalle'.
    ///    b. Actualiza el inventario usando InventarioManager.RegistrarSalida.
    /// 3. Muestra un mensaje de éxito y navega al detalle de la venta.
    /// </summary>
    private async Task CompletarVenta()
    {
        if (!ProductosSeleccionados.Any())
        {
            await DisplayAlert("Selección vacía", "Seleccione al menos un producto para realizar la venta", "Aceptar");
            return;
        }

        // Deshabilitar botón mientras se procesa la venta
        ProcesandoVenta = true;

        try
        {
            // Validar que todos los managers estén disponibles
            if (_ventaManager == null || _ventaDetalleManager == null || _inventarioManager == null)
            {
                await DisplayAlert("Error", "No se han inicializado los servicios correctamente", "Aceptar");
                return;
            }

            // Paso 1: Crear la entidad 'venta' (cabecera de la transacción)
            var nuevaVenta = new venta
            {
                fecha_hora_venta = DateTime.Now,
                monto_total_venta = TotalVenta,
                id_usuario = 1 // Asumiendo ID usuario predeterminado
            };

            // Guardar la venta en la base de datos
            var ventaRegistrada = await _ventaManager.Agregar(nuevaVenta);

            if (ventaRegistrada == null)
            {
                await DisplayAlert("Error", $"No se pudo registrar la venta: {_ventaManager.Error}", "Aceptar");
                return;
            }

            // Paso 2: Registrar los detalles y actualizar inventario
            foreach (var item in ProductosSeleccionados)
            {
                // 2.a: Crear y guardar el detalle de venta
                var detalle = new venta_detalle
                {
                    id_venta = ventaRegistrada.id_venta,
                    id_producto = item.Producto.id_producto,
                    cantidad_vendida = item.Cantidad,
                    precio_unitario_venta = item.Producto.precio_producto,
                    subtotal_detalle = item.Subtotal
                };

                var detalleRegistrado = await _ventaDetalleManager.Agregar(detalle);

                if (detalleRegistrado == null)
                {
                    Debug.WriteLine($"Error al registrar detalle para producto ID {item.Producto.id_producto}: {_ventaDetalleManager.Error}");
                    // Continuamos con los demás items aunque uno falle
                }

                // 2.b: Registrar la salida de inventario
                var resultadoSalida = await _inventarioManager.RegistrarSalida(
                    item.Producto.id_producto,
                    item.Cantidad,
                    $"Venta #{ventaRegistrada.id_venta}"
                );

                if (resultadoSalida == null)
                {
                    Debug.WriteLine($"Error al registrar salida de inventario para producto ID {item.Producto.id_producto}: {_inventarioManager.Error}");
                    await DisplayAlert("Advertencia",
                        $"No se pudo actualizar el inventario para {item.Producto.nombre_producto}. El stock podría ser incorrecto.",
                        "Aceptar");
                }
            }

            // Mostrar mensaje de éxito
            await DisplayAlert("Venta completada", $"Venta #{ventaRegistrada.id_venta} registrada correctamente", "Aceptar");

            // Navegar a la página de detalle de venta
            await Navigation.PushAsync(new DetalleVenta(ventaRegistrada.id_venta, DetalleVenta.PaginaOrigen.InicioPage));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error inesperado al finalizar la venta: {ex.Message}", "Aceptar");
        }
        finally
        {
            ProcesandoVenta = false;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Verificar si necesitamos recargar datos (por ejemplo, al regresar a esta página)
        if (ProductosMostrados.Count == 0 && !Cargando)
        {
            CargarProductos();
        }
    }
}