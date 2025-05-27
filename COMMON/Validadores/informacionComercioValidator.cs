using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class informacionComercioValidator : CamposControlValidator<informacion_comercio>
    {
        public informacionComercioValidator()
        {
            RuleFor(ic => ic.nombre_comercio)
                .NotEmpty()
                .MaximumLength(100)
                .MinimumLength(3);

            RuleFor(ic => ic.descripcion)
                .NotEmpty()
                .MinimumLength(10);

            RuleFor(ic => ic.fecha_creacion)
                .NotEmpty()
                .LessThanOrEqualTo(DateTime.Now);

            RuleFor(ic => ic.razon_social)
                .MaximumLength(255);

            RuleFor(ic => ic.encargado)
                .NotEmpty()
                .GreaterThan(0);
        }
    }
}
