using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{

    public class UsuarioManager : GenericManager<usuario>
    {
        public UsuarioManager(AbstractValidator<usuario> validador) : base(validador)
        {
        }

        public override Task<usuario> Agregar(usuario entidad)
        {
            try
            {
                // Validaciones y configuraciones iniciales
                if (string.IsNullOrEmpty(entidad.clave_usuario))
                {
                    Error = "La contraseña no puede estar vacía";
                    return null;
                }

                if (string.IsNullOrEmpty(entidad.nombre_usuario_login))
                {
                    Error = "El nombre de usuario no puede estar vacío";
                    return null;
                }

                return base.Agregar(entidad);
            }
            catch (Exception ex)
            {
                Error = $"Error al agregar usuario: {ex.Message}";
                return null;
            }
        }
    }

}
