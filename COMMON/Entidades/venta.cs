using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class venta : CamposControl
    {
        public int id_venta { get; set; }
        public DateTime fecha_hora_venta { get; set; }
        public decimal monto_total_venta { get; set; }
        public int? id_cliente { get; set; }
        public int id_usuario { get; set; }

    }
}
