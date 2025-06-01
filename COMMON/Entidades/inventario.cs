using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class inventario : CamposControl
    {
        public int id_movimiento { get; set; }
        public int id_producto { get; set; }
        public int cantidad_movimiento { get; set; }
        public enum TipoMovimiento
        {
            entrada,
            salida
        }
        public string tipo_movimiento { get; set; } = string.Empty;
        public DateTime fecha_movimiento { get; set; }
        public string? descripcion_movimiento { get; set; }
    }
}