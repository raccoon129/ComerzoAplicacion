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
    }
}