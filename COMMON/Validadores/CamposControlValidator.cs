using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Validadores
{
    public class CamposControlValidator<T> : AbstractValidator<T> where T : CamposControl //Se agrega el where T : CamposControl para que el validador sea generico y pueda ser utilizado por cualquier clase que herede de CamposControl
    {
        public CamposControlValidator()
        {
            //Los campos nulos no son validados
            RuleFor(c => c.fecha_alta).NotEmpty().GreaterThanOrEqualTo(new DateTime(2025, 1, 1));//La fecha de alta no puede ser menor a 2025
            RuleFor(c => c.usuario_alta).NotEmpty().MaximumLength(100).MinimumLength(3);//El usuario de alta debe tener entre 5 y 50 caracteres
            RuleFor(c => c.usuario_mod).MaximumLength(100);//El usuario de modificación debe tener menos de 50 caracteres
        }
    }
}
