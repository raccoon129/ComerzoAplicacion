using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class venta_detalle : CamposControl
    {
        public int id_detalle { get; set; }
        public int id_venta { get; set; }
        public int id_producto { get; set; }
        public int cantidad_vendida { get; set; }
        public decimal precio_unitario_venta { get; set; }
        public decimal subtotal_detalle { get; set; }

    }
}
