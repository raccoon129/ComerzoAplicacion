using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class ProductoManager : GenericManager<producto>
    {
        public ProductoManager(AbstractValidator<producto> validador) : base(validador)
        {
        }

        public override async Task<producto> Agregar(producto entidad)
        {
            try
            {
                // Validaciones y configuraciones iniciales
                if (string.IsNullOrEmpty(entidad.estado_producto))
                {
                    entidad.estado_producto = "activo";
                }

                if (string.IsNullOrEmpty(entidad.notas_producto))
                {
                    entidad.notas_producto = "Sin notas";
                }

                return await base.Agregar(entidad);
            }
            catch (Exception ex)
            {
                Error = $"Error al agregar producto: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Obtiene los productos con stock por debajo del umbral especificado
        /// </summary>
        /// <param name="umbral">Umbral mínimo de stock</param>
        /// <returns>Lista de productos con stock bajo</returns>
        public async Task<List<producto>> ObtenerProductosBajoStock(int umbral = 5)
        {
            try
            {
                var productos = await ObtenerTodos();

                if (productos == null)
                {
                    return null;
                }

                return productos.Where(p => p.stock_producto < umbral).ToList();
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener productos con bajo stock: {ex.Message}";
                return null;
            }
        }
    }
}