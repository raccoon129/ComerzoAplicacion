using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class productoValidator : CamposControlValidator<producto>
    {
        public productoValidator()
        {
            RuleFor(p => p.nombre_producto)
                .NotEmpty()
                .MaximumLength(100)
                .MinimumLength(3);

            RuleFor(p => p.descripcion_producto)
                .NotEmpty()
                .MinimumLength(10);

            RuleFor(p => p.precio_producto)
                .NotEmpty()
                .GreaterThan(0)
                .LessThan(99999.99m);

            RuleFor(p => p.estado_producto)
                .NotEmpty()
                .Must(estado => estado == "activo" || estado == "inactivo")
                .WithMessage("El estado del producto debe ser 'activo' o 'inactivo'");

            RuleFor(p => p.stock_producto)
                .NotNull()
                .GreaterThanOrEqualTo(0);
        }
    }
}
