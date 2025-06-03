using BIZ;
using COMMON.Entidades;
using COMMON.Validadores;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
        }
    }

    private ObservableCollection<producto> _productosBajoStock = new ObservableCollection<producto>();
    public ObservableCollection<producto> ProductosBajoStock
    {
        get => _productosBajoStock;
        set
        {
            _productosBajoStock = value;
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

    // Resto de tu código permanece igual...
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
        // Establecer el BindingContext a esta misma página
        BindingContext = this;
        
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
            await CargarProductosPopulares();
            await CargarInventarioBajo();
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

    private async Task CargarProductosPopulares()
    {
        try
        {
            var populares = await _ventaDetalleManager.ObtenerProductosPopularesDiarios(_ventaManager, _productoManager);

            ProductosPopulares.Clear();
            if (populares != null)
            {
                foreach (var p in populares.Select(p => new ProductoPopular
                {
                    Nombre = p.Nombre,
                    Cantidad = p.Cantidad,
                    PorcentajeVentas = p.PorcentajeVentas
                }))
                {
                    ProductosPopulares.Add(p);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar hábitos: {ex.Message}", "OK");
        }
    }

    private async Task CargarInventarioBajo()
    {
        try
        {
            var bajoStock = await _productoManager.ObtenerProductosBajoStock(5);

            ProductosBajoStock.Clear();
            if (bajoStock != null)
            {
                foreach (var p in bajoStock)
                {
                    ProductosBajoStock.Add(p);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar hábitos: {ex.Message}", "OK");
        }
    }
}