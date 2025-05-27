using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class InventarioManager : GenericManager<inventario>
    {
        public InventarioManager(AbstractValidator<inventario> validador) : base(validador)
        {
        }

        public override Task<inventario> Agregar(inventario entidad)
        {
            try
            {
                // Validaciones y configuraciones iniciales
                if (entidad.cantidad_movimiento <= 0)
                {
                    Error = "La cantidad debe ser mayor a cero";
                    return null;
                }

                if (string.IsNullOrEmpty(entidad.tipo_movimiento))
                {
                    Error = "Debe seleccionar un tipo de movimiento";
                    return null;
                }

                if (entidad.fecha_movimiento == DateTime.MinValue)
                {
                    entidad.fecha_movimiento = DateTime.Now;
                }

                if (string.IsNullOrEmpty(entidad.descripcion_movimiento))
                {
                    entidad.descripcion_movimiento = "Movimiento sin descripción";
                }

                return base.Agregar(entidad);
            }
            catch (Exception ex)
            {
                Error = $"Error al registrar movimiento: {ex.Message}";
                return null;
            }
        }
    }
}
