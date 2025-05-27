using COMMON.Entidades;
using FluentValidation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Validadores
{
    public class clienteValidator:CamposControlValidator<cliente> //Se hereda de CamposControlValidator para validar los campos comunes de CamposControl y los campos especificos de cliente
    {
        public clienteValidator()
        {
            RuleFor(c => c.nombre_cliente).NotEmpty().MaximumLength(100).MinimumLength(5);
            RuleFor(c => c.telefono_cliente).NotEmpty().MaximumLength(20).MinimumLength(7);
            RuleFor(c => c.direccion_cliente).NotEmpty().MaximumLength(255).MinimumLength(5);
        }
    }
}
