using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace BIZ
{
    /// <summary>
    /// Administrador de detalles de venta que hereda de GenericManager
    /// </summary>
    public class VentaDetalleManager : GenericManager<venta_detalle>
    {
        // Aunque _productoManager e _inventarioManager no se usen directamente en Agregar
        // tras el cambio, se mantienen por si son necesarios para otras funcionalidades
        // o futuras extensiones del VentaDetalleManager.
        private readonly ProductoManager _productoManager;
        private readonly InventarioManager _inventarioManager;

        public VentaDetalleManager(AbstractValidator<venta_detalle> validador,
                                  ProductoManager productoManager = null,
                                  InventarioManager inventarioManager = null) : base(validador)
        {
            _productoManager = productoManager;
            _inventarioManager = inventarioManager;
        }

        /// <summary>
        /// Sobrescribe el método Agregar para añadir lógica específica de detalles de venta.
        /// La actualización del inventario y stock del producto se maneja externamente
        /// (por ejemplo, en el proceso de finalizar venta que llama a InventarioManager.RegistrarSalida).
        /// </summary>
        public override async Task<venta_detalle> Agregar(venta_detalle entidad)
        {
            try
            {
                // Validar que la cantidad vendida sea mayor que cero
                if (entidad.cantidad_vendida <= 0)
                {
                    Error = "La cantidad vendida debe ser mayor que cero";
                    return null;
                }

                // Calcular el subtotal si no se proporcionó
                if (entidad.subtotal_detalle == 0)
                {
                    entidad.subtotal_detalle = entidad.precio_unitario_venta * entidad.cantidad_vendida;
                }

                // Guardar el detalle de venta usando el método base
                return await base.Agregar(entidad);
            }
            catch (Exception ex)
            {
                Error = $"Error al registrar detalle de venta: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Método para obtener todos los detalles de una venta específica
        /// </summary>
        public async Task<List<venta_detalle>> ObtenerDetallesPorVenta(int idVenta)
        {
            try
            {
                var detalles = await ObtenerTodos();
                if (detalles == null)
                {
                    return null;
                }

                return detalles.FindAll(d => d.id_venta == idVenta);
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener detalles de venta: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Obtiene los detalles de una venta específica
        /// </summary>
        /// <param name="idVenta">ID de la venta</param>
        /// <returns>Lista de detalles de la venta</returns>
        public async Task<List<venta_detalle>> ObtenerPorVenta(int idVenta)
        {
            try
            {
                var todos = await ObtenerTodos();
                if (todos == null)
                {
                    return null;
                }

                return todos.Where(d => d.id_venta == idVenta).ToList();
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener detalles de venta: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Obtiene los productos más populares vendidos en el día actual
        /// </summary>
        /// <param name="ventaManager">Manager de ventas para obtener las ventas del día</param>
        /// <param name="productoManager">Manager de productos para obtener información de los productos</param>
        /// <param name="limite">Cantidad máxima de productos a retornar</param>
        /// <returns>Lista de productos populares con su nombre, cantidad y porcentaje de ventas</returns>
        public async Task<List<(string Nombre, int Cantidad, int PorcentajeVentas)>> ObtenerProductosPopularesDiarios(
            VentaManager ventaManager, ProductoManager productoManager, int limite = 5)
        {
            try
            {
                if (ventaManager == null || productoManager == null)
                {
                    Error = "Se requieren los managers de ventas y productos";
                    return null;
                }

                var resultado = new List<(string Nombre, int Cantidad, int PorcentajeVentas)>();
                var ventasHoy = await ventaManager.ObtenerVentasDelDia();
                Dictionary<int, int> productosCantidad = new Dictionary<int, int>();

                if (ventasHoy == null || !ventasHoy.Any())
                {
                    return resultado;
                }

                // Acumular cantidades por producto
                foreach (var venta in ventasHoy)
                {
                    var detalles = await ObtenerPorVenta(venta.id_venta);
                    if (detalles != null)
                    {
                        foreach (var detalle in detalles)
                        {
                            if (!productosCantidad.ContainsKey(detalle.id_producto))
                            {
                                productosCantidad[detalle.id_producto] = 0;
                            }
                            productosCantidad[detalle.id_producto] += detalle.cantidad_vendida;
                        }
                    }
                }

                // Si no hay productos vendidos, retornar lista vacía
                if (!productosCantidad.Any())
                {
                    return resultado;
                }

                // Calcular el total de productos vendidos
                var totalVendido = productosCantidad.Values.Sum();

                // Obtener la información de los productos
                var productos = await productoManager.ObtenerTodos();

                if (productos != null)
                {
                    // Ordenar por cantidad y tomar los primeros según el límite
                    resultado = productosCantidad
                        .OrderByDescending(p => p.Value)
                        .Take(limite)
                        .Select(p => {
                            var prod = productos.FirstOrDefault(x => x.id_producto == p.Key);
                            return (
                                Nombre: prod?.nombre_producto ?? $"Producto #{p.Key}",
                                Cantidad: p.Value,
                                PorcentajeVentas: (int)Math.Round(p.Value * 100.0 / totalVendido)
                            );
                        })
                        .ToList();
                }

                return resultado;
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener productos populares: {ex.Message}";
                return null;
            }
        }

        // PARA ESTADISTICAAAAAAAAAAAAAAAAAAAAAAS



        /// <summary>
        /// Obtiene los productos más populares vendidos en un período específico
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del período</param>
        /// <param name="fechaFin">Fecha de fin del período</param>
        /// <param name="ventaManager">Manager de ventas para obtener las ventas del periodo</param>
        /// <param name="productoManager">Manager de productos para obtener información de los productos</param>
        /// <param name="limite">Cantidad máxima de productos a retornar</param>
        /// <returns>Lista de productos populares con su nombre y cantidad</returns>
        public async Task<List<(string Nombre, int Cantidad)>> ObtenerProductosPopularesPorPeriodo(
            DateTime fechaInicio,
            DateTime fechaFin,
            VentaManager ventaManager,
            ProductoManager productoManager,
            int limite = 5)
        {
            try
            {
                if (ventaManager == null || productoManager == null)
                {
                    Error = "Se requieren los managers de ventas y productos";
                    return null;
                }

                // Obtener las ventas del período
                var ventasPeriodo = await ventaManager.ObtenerVentasPorPeriodo(fechaInicio, fechaFin);

                if (ventasPeriodo == null || !ventasPeriodo.Any())
                {
                    return new List<(string, int)>();
                }

                // Obtener los ID de las ventas del período
                var idsVentasPeriodo = ventasPeriodo.Select(v => v.id_venta).ToList();

                // Obtener todos los detalles de venta
                var todosDetalles = await ObtenerTodos();
                if (todosDetalles == null)
                {
                    return new List<(string, int)>();
                }

                // Filtrar solo los detalles correspondientes a las ventas del período
                var detallesFiltrados = todosDetalles
                    .Where(d => idsVentasPeriodo.Contains(d.id_venta))
                    .ToList();

                if (!detallesFiltrados.Any())
                {
                    return new List<(string, int)>();
                }

                // Agrupar por producto y sumar cantidades
                var productosAgrupados = detallesFiltrados
                    .GroupBy(d => d.id_producto)
                    .Select(g => (
                        IdProducto: g.Key,
                        Cantidad: g.Sum(d => d.cantidad_vendida)
                    ))
                    .OrderByDescending(p => p.Cantidad)
                    .Take(limite)
                    .ToList();

                // Obtener todos los productos
                var productos = await productoManager.ObtenerTodos();

                // Mapear IDs a nombres de producto
                return productosAgrupados
                    .Select(p => {
                        var producto = productos?.FirstOrDefault(pr => pr.id_producto == p.IdProducto);
                        return (
                            Nombre: producto?.nombre_producto ?? $"Producto #{p.IdProducto}",
                            Cantidad: p.Cantidad
                        );
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener productos populares por período: {ex.Message}";
                return new List<(string, int)>();
            }
        }

        /// <summary>
        /// Obtiene el rendimiento de ventas de productos en un período específico
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del período</param>
        /// <param name="fechaFin">Fecha de fin del período</param>
        /// <param name="ventaManager">Manager de ventas</param>
        /// <param name="productoManager">Manager de productos</param>
        /// <returns>Lista con el rendimiento de cada producto (nombre, cantidad, total y porcentaje)</returns>
        public async Task<List<(string Nombre, int CantidadVendida, decimal TotalVendido, int PorcentajeTotal)>>
            ObtenerRendimientoVentasPorPeriodo(
                DateTime fechaInicio,
                DateTime fechaFin,
                VentaManager ventaManager,
                ProductoManager productoManager)
        {
            try
            {
                if (ventaManager == null || productoManager == null)
                {
                    Error = "Se requieren los managers de ventas y productos";
                    return null;
                }

                // Obtener las ventas del período
                var ventasPeriodo = await ventaManager.ObtenerVentasPorPeriodo(fechaInicio, fechaFin);

                if (ventasPeriodo == null || !ventasPeriodo.Any())
                {
                    return new List<(string, int, decimal, int)>();
                }

                // Obtener los IDs de las ventas del período
                var idsVentasPeriodo = ventasPeriodo.Select(v => v.id_venta).ToList();

                // Obtener todos los detalles de venta
                var todosDetalles = await ObtenerTodos();
                if (todosDetalles == null)
                {
                    return new List<(string, int, decimal, int)>();
                }

                // Filtrar solo los detalles correspondientes a las ventas del período
                var detallesFiltrados = todosDetalles
                    .Where(d => idsVentasPeriodo.Contains(d.id_venta))
                    .ToList();

                if (!detallesFiltrados.Any())
                {
                    return new List<(string, int, decimal, int)>();
                }

                // Calcular el total general de ventas
                var totalGeneralVentas = detallesFiltrados.Sum(d => d.subtotal_detalle);

                // Obtener todos los productos
                var productos = await productoManager.ObtenerTodos();

                // Agrupar por producto y calcular métricas
                return detallesFiltrados
                    .GroupBy(d => d.id_producto)
                    .Select(g => {
                        var producto = productos?.FirstOrDefault(p => p.id_producto == g.Key);
                        var cantidadVendida = g.Sum(d => d.cantidad_vendida);
                        var totalVendido = g.Sum(d => d.subtotal_detalle);
                        var porcentaje = totalGeneralVentas > 0
                            ? (int)Math.Round(totalVendido * 100 / totalGeneralVentas)
                            : 0;

                        return (
                            Nombre: producto?.nombre_producto ?? $"Producto #{g.Key}",
                            CantidadVendida: cantidadVendida,
                            TotalVendido: totalVendido,
                            PorcentajeTotal: porcentaje
                        );
                    })
                    .OrderByDescending(p => p.TotalVendido)
                    .ToList();
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener rendimiento de ventas por período: {ex.Message}";
                return new List<(string, int, decimal, int)>();
            }
        }
    }
}