using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class inventarioValidator : CamposControlValidator<inventario>
    {
        public inventarioValidator()
        {
            RuleFor(i => i.id_producto)
                .NotEmpty()
                .GreaterThan(0);

            RuleFor(i => i.cantidad_movimiento)
                .NotEmpty()
                .NotEqual(0)
                .WithMessage("La cantidad del movimiento no puede ser cero");

            RuleFor(i => i.tipo_movimiento)
                .NotEmpty()
                .Must(tipo => tipo == "entrada" || tipo == "salida")
                .WithMessage("El tipo de movimiento debe ser 'entrada' o 'salida'");

            RuleFor(i => i.descripcion_movimiento)
                .MaximumLength(255);

            RuleFor(i => i.fecha_movimiento)
                .NotEmpty()
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("La fecha de movimiento no puede ser futura");
        }
    }
}
