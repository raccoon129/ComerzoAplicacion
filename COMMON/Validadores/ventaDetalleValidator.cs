using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class ventaDetalleValidator : CamposControlValidator<venta_detalle>
    {
        public ventaDetalleValidator()
        {
            RuleFor(vd => vd.id_venta)
                .NotEmpty()
                .GreaterThan(0);

            RuleFor(vd => vd.id_producto)
                .NotEmpty()
                .GreaterThan(0);

            RuleFor(vd => vd.cantidad_vendida)
                .NotEmpty()
                .GreaterThan(0);

            RuleFor(vd => vd.precio_unitario_venta)
                .NotEmpty()
                .GreaterThan(0)
                .LessThan(999999.99m);

            RuleFor(vd => vd.subtotal_detalle)
                .NotEmpty()
                .GreaterThan(0)
                .LessThan(999999.99m);
        }
    }
}