using COMMON.Entidades;
using FluentValidation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class InventarioManager : GenericManager<inventario>
    {
        private readonly ProductoManager _productoManager;

        public InventarioManager(AbstractValidator<inventario> validador, ProductoManager productoManager = null) : base(validador)
        {
            _productoManager = productoManager;
        }

        /// <summary>
        /// Registra un movimiento de entrada en el inventario
        /// </summary>
        /// <param name="idProducto">ID del producto</param>
        /// <param name="cantidad">Cantidad del producto a ingresar</param>
        /// <param name="descripcion">Descripción del movimiento (opcional)</param>
        /// <returns>Retorna el movimiento registrado o null si hay error</returns>
        public async Task<inventario> RegistrarEntrada(int idProducto, int cantidad, string descripcion = "")
        {
            try
            {
                if (cantidad <= 0)
                {
                    Error = "La cantidad debe ser mayor que cero";
                    return null;
                }

                var movimiento = new inventario
                {
                    id_producto = idProducto,
                    cantidad_movimiento = cantidad,
                    tipo_movimiento = "entrada",
                    fecha_movimiento = DateTime.Now,
                    descripcion_movimiento = string.IsNullOrEmpty(descripcion) ? "Entrada de inventario" : descripcion
                };

                // Registrar el movimiento
                var resultado = await base.Agregar(movimiento);

                // Actualizar el stock del producto si hay un ProductoManager disponible
                if (_productoManager != null)
                {
                    await ActualizarStockProducto(idProducto, cantidad, true);
                }

                return resultado;
            }
            catch (Exception ex)
            {
                Error = $"Error al registrar entrada: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Registra un movimiento de salida en el inventario
        /// </summary>
        /// <param name="idProducto">ID del producto</param>
        /// <param name="cantidad">Cantidad del producto a retirar</param>
        /// <param name="descripcion">Descripción del movimiento (opcional)</param>
        /// <returns>Retorna el movimiento registrado o null si hay error</returns>
        public async Task<inventario> RegistrarSalida(int idProducto, int cantidad, string descripcion = "")
        {
            try
            {
                if (cantidad <= 0)
                {
                    Error = "La cantidad debe ser mayor que cero";
                    return null;
                }

                // Verificar que haya stock suficiente si hay ProductoManager
                if (_productoManager != null)
                {
                    var producto = await _productoManager.ObtenerPorId(idProducto);
                    if (producto != null)
                    {
                        if (producto.stock_producto < cantidad)
                        {
                            Error = "No hay suficiente stock para realizar la salida";
                            return null;
                        }
                    }
                }

                var movimiento = new inventario
                {
                    id_producto = idProducto,
                    cantidad_movimiento = cantidad,
                    tipo_movimiento = "salida",
                    fecha_movimiento = DateTime.Now,
                    descripcion_movimiento = string.IsNullOrEmpty(descripcion) ? "Salida de inventario" : descripcion
                };

                // Registrar el movimiento
                var resultado = await base.Agregar(movimiento);

                // Actualizar el stock del producto si hay un ProductoManager disponible
                if (_productoManager != null && resultado != null)
                {
                    await ActualizarStockProducto(idProducto, cantidad, false);
                }

                return resultado;
            }
            catch (Exception ex)
            {
                Error = $"Error al registrar salida: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Obtiene todos los movimientos de un producto específico
        /// </summary>
        /// <param name="idProducto">ID del producto a consultar</param>
        /// <returns>Lista de movimientos del producto</returns>
        public async Task<List<inventario>> ObtenerMovimientosPorProducto(int idProducto)
        {
            try
            {
                var movimientos = await ObtenerTodos();
                if (movimientos == null)
                {
                    return null;
                }

                return movimientos.Where(m => m.id_producto == idProducto).ToList();
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener movimientos: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Obtiene el balance actual de un producto sumando entradas y restando salidas
        /// </summary>
        /// <param name="idProducto">ID del producto a consultar</param>
        /// <returns>Balance actual del producto</returns>
        public async Task<int> ObtenerBalanceProducto(int idProducto)
        {
            try
            {
                var movimientos = await ObtenerMovimientosPorProducto(idProducto);
                if (movimientos == null)
                {
                    return 0;
                }

                int entradas = movimientos
                    .Where(m => m.tipo_movimiento == "entrada")
                    .Sum(m => m.cantidad_movimiento);

                int salidas = movimientos
                    .Where(m => m.tipo_movimiento == "salida")
                    .Sum(m => m.cantidad_movimiento);

                return entradas - salidas;
            }
            catch (Exception ex)
            {
                Error = $"Error al calcular balance: {ex.Message}";
                return 0;
            }
        }

        /// <summary>
        /// Obtiene los últimos movimientos de todos los productos
        /// </summary>
        /// <param name="cantidad">Cantidad de movimientos a obtener</param>
        /// <returns>Lista de los últimos movimientos</returns>
        public async Task<List<inventario>> ObtenerUltimosMovimientos(int cantidad = 10)
        {
            try
            {
                var movimientos = await ObtenerTodos();
                if (movimientos == null)
                {
                    return null;
                }

                return movimientos
                    .OrderByDescending(m => m.fecha_movimiento)
                    .Take(cantidad)
                    .ToList();
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener últimos movimientos: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Actualiza el stock de un producto después de un movimiento
        /// </summary>
        /// <param name="idProducto">ID del producto</param>
        /// <param name="cantidad">Cantidad del movimiento</param>
        /// <param name="esEntrada">Indica si es entrada (true) o salida (false)</param>
        /// <returns>True si se actualizó correctamente, False en caso contrario</returns>
        private async Task<bool> ActualizarStockProducto(int idProducto, int cantidad, bool esEntrada)
        {
            try
            {
                var producto = await _productoManager.ObtenerPorId(idProducto);
                if (producto == null)
                {
                    Error = _productoManager.Error;
                    return false;
                }

                if (esEntrada)
                {
                    producto.stock_producto += cantidad;
                }
                else
                {
                    producto.stock_producto -= cantidad;
                }

                // Asegurarse que el stock no sea negativo
                if (producto.stock_producto < 0)
                {
                    producto.stock_producto = 0;
                }

                var resultado = await _productoManager.Modificar(producto);
                return resultado != null;
            }
            catch (Exception ex)
            {
                Error = $"Error al actualizar stock: {ex.Message}";
                return false;
            }
        }
    }
}