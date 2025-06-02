using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BIZ
{
    /// <summary>
    /// Administrador de ventas que hereda de GenericManager
    /// </summary>
    public class VentaManager : GenericManager<venta>
    {
        private readonly InventarioManager _inventarioManager;

        public VentaManager(AbstractValidator<venta> validador, InventarioManager inventarioManager = null) : base(validador)
        {
            _inventarioManager = inventarioManager;
        }

        /// <summary>
        /// Sobrescribe el método Agregar para añadir lógica específica de ventas
        /// </summary>
        public override async Task<venta> Agregar(venta entidad)
        {
            try
            {
                // Establecer la fecha y hora actual si no se proporcionó
                if (entidad.fecha_hora_venta == default)
                {
                    entidad.fecha_hora_venta = DateTime.Now;
                }

                // Guardar la venta usando el método base
                return await base.Agregar(entidad);
            }
            catch (Exception ex)
            {
                Error = $"Error al registrar la venta: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Método para obtener las ventas del día actual
        /// </summary>
        public async Task<List<venta>> ObtenerVentasDelDia()
        {
            try
            {
                var ventas = await ObtenerTodos();
                if (ventas == null)
                {
                    return null;
                }

                var fechaActual = DateTime.Today;
                return ventas.FindAll(v => v.fecha_hora_venta.Date == fechaActual);
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener ventas del día: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Obtiene el resumen de ventas del día actual
        /// </summary>
        /// <param name="ventaDetalleManager">Manager de detalles de venta para calcular productos vendidos</param>
        /// <returns>Tupla con el monto total de ventas y cantidad de productos vendidos</returns>
        public async Task<(decimal MontoTotal, int ProductosVendidos)> ObtenerResumenVentasDiario(VentaDetalleManager ventaDetalleManager)
        {
            try
            {
                decimal montoTotal = 0;
                int productosVendidos = 0;

                var ventasHoy = await ObtenerVentasDelDia();

                if (ventasHoy != null)
                {
                    montoTotal = ventasHoy.Sum(v => v.monto_total_venta);

                    // Calcular total de productos vendidos
                    if (ventaDetalleManager != null)
                    {
                        foreach (var venta in ventasHoy)
                        {
                            var detalles = await ventaDetalleManager.ObtenerPorVenta(venta.id_venta);
                            if (detalles != null)
                            {
                                productosVendidos += detalles.Sum(d => d.cantidad_vendida);
                            }
                        }
                    }
                }

                return (montoTotal, productosVendidos);
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener resumen de ventas: {ex.Message}";
                return (0, 0);
            }
        }

        /// <summary>
        /// Obtiene las últimas ventas realizadas
        /// </summary>
        /// <param name="cantidad">Cantidad de ventas a obtener</param>
        /// <returns>Lista de las últimas ventas</returns>
        public async Task<List<venta>> ObtenerUltimasVentas(int cantidad = 10)
        {
            try
            {
                var ventas = await ObtenerTodos();
                if (ventas == null)
                {
                    return null;
                }

                return ventas
                    .OrderByDescending(v => v.fecha_hora_venta)
                    .Take(cantidad)
                    .ToList();
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener últimas ventas: {ex.Message}";
                return null;
            }
        }

        // A PARTIR DE AQUI SON PARA ESTADÍSTICAS EN REPORTESSSSSSSSSSS

        /// <summary>
        /// Obtiene ventas dentro de un rango de fechas específico
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del periodo</param>
        /// <param name="fechaFin">Fecha de fin del periodo</param>
        /// <returns>Lista de ventas filtradas por el periodo</returns>
        public async Task<List<venta>> ObtenerVentasPorPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ventas = await ObtenerTodos();

                if (ventas == null)
                {
                    return null;
                }

                return ventas
                    .Where(v => v.fecha_hora_venta.Date >= fechaInicio.Date &&
                                v.fecha_hora_venta.Date <= fechaFin.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener ventas por periodo: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Obtiene un resumen de las ventas por periodo (monto total y cantidad de ventas)
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del periodo</param>
        /// <param name="fechaFin">Fecha de fin del periodo</param>
        /// <returns>Tupla con cantidad de ventas y el monto total</returns>
        public async Task<(int CantidadVentas, decimal MontoTotal)> ObtenerResumenVentasPorPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ventasPeriodo = await ObtenerVentasPorPeriodo(fechaInicio, fechaFin);

                if (ventasPeriodo == null || !ventasPeriodo.Any())
                {
                    return (0, 0);
                }

                return (
                    ventasPeriodo.Count,
                    ventasPeriodo.Sum(v => v.monto_total_venta)
                );
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener resumen de ventas por periodo: {ex.Message}";
                return (0, 0);
            }
        }

        /// <summary>
        /// Obtiene las ventas agrupadas por día en un periodo específico
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del periodo</param>
        /// <param name="fechaFin">Fecha de fin del periodo</param>
        /// <returns>Lista de ventas diarias con fecha y monto total</returns>
        public async Task<List<(string Fecha, decimal Total)>> ObtenerVentasDiariasPorPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ventasPeriodo = await ObtenerVentasPorPeriodo(fechaInicio, fechaFin);

                if (ventasPeriodo == null || !ventasPeriodo.Any())
                {
                    return new List<(string, decimal)>();
                }

                return ventasPeriodo
                    .GroupBy(v => v.fecha_hora_venta.Date)
                    .Select(g => (
                        Fecha: g.Key.ToString("dd/MM"),
                        Total: g.Sum(v => v.monto_total_venta)
                    ))
                    .OrderBy(v => v.Fecha)
                    .ToList();
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener ventas diarias por periodo: {ex.Message}";
                return new List<(string, decimal)>();
            }
        }
    }
}