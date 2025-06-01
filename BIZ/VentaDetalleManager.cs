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
        /// Sobrescribe el método Agregar para añadir lógica específica de detalles de venta
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
                
                // Actualizar el inventario si hay un gestor de inventario disponible
                if (_inventarioManager != null && _productoManager != null)
                {
                    // Crear movimiento de inventario de salida
                    var movimientoInventario = new inventario
                    {
                        id_producto = entidad.id_producto,
                        cantidad_movimiento = entidad.cantidad_vendida,
                        tipo_movimiento = "salida",
                        fecha_movimiento = DateTime.Now,
                        descripcion_movimiento = $"Venta #{entidad.id_venta}"
                    };
                    
                    await _inventarioManager.Agregar(movimientoInventario);
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
    }
}
