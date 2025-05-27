using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class usuarioValidator : CamposControlValidator<usuario>
    {
        public usuarioValidator()
        {
            RuleFor(u => u.nombre_usuario_login)
                .NotEmpty()
                .MaximumLength(50)
                .MinimumLength(4);

            RuleFor(u => u.clave_usuario)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(255)
                .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula")
                .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una minúscula")
                .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número");
        }
    }
}
