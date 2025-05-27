using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class ClienteManager : GenericManager<cliente>
    {
        public ClienteManager(AbstractValidator<cliente> validador) : base(validador)
        {
        }

        public override Task<cliente> Agregar(cliente entidad)
        {
            try
            {
                // Validaciones y configuraciones iniciales
                if (string.IsNullOrEmpty(entidad.telefono_cliente))
                {
                    entidad.telefono_cliente = "Sin teléfono";
                }

                if (string.IsNullOrEmpty(entidad.direccion_cliente))
                {
                    entidad.direccion_cliente = "Sin dirección";
                }

                return base.Agregar(entidad);
            }
            catch (Exception ex)
            {
                Error = $"Error al agregar cliente: {ex.Message}";
                return null;
            }
        }
    }

}
