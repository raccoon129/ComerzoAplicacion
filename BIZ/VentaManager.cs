using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
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
    }
}
