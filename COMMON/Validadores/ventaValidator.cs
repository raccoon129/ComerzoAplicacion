using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class ventaValidator : CamposControlValidator<venta>
    {
        public ventaValidator()
        {
            RuleFor(v => v.fecha_hora_venta)
                .NotEmpty()
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("La fecha de venta no puede ser futura");

            RuleFor(v => v.monto_total_venta)
                .NotEmpty()
                .GreaterThan(0)
                .LessThan(999999.99m);

            RuleFor(v => v.id_usuario)
                .NotEmpty()
                .GreaterThan(0);

            When(v => v.id_cliente.HasValue, () =>
            {
                RuleFor(v => v.id_cliente)
                    .GreaterThan(0);
            });
        }
    }
}
