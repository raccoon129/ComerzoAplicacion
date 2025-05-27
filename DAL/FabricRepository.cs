using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using COMMON.Interfaces;
using COMMON.Validadores;

namespace DAL
{
    public class FabricRepository
    {
        private string _cadenaDeConexion;
        private TipoDB _tipoDB;

        public FabricRepository(string cadenaDeConexion, TipoDB tipoDB)
        {
            _cadenaDeConexion = cadenaDeConexion;
            _tipoDB = tipoDB;
        }

        public IDB<producto> ProductoRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<producto>(_cadenaDeConexion, new productoValidator(), "id_producto", true);
                case TipoDB.SQLServer:
                    throw new NotImplementedException("SQLServer no está implementado en este proyecto");
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<inventario> InventarioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<inventario>(_cadenaDeConexion, new inventarioValidator(), "id_movimiento", true);
                case TipoDB.SQLServer:
                    throw new NotImplementedException("SQLServer no está implementado en este proyecto");
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<cliente> ClienteRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<cliente>(_cadenaDeConexion, new clienteValidator(), "id_cliente", true);
                case TipoDB.SQLServer:
                    throw new NotImplementedException("SQLServer no está implementado en este proyecto");
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<usuario> UsuarioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<usuario>(_cadenaDeConexion, new usuarioValidator(), "id_usuario", true);
                case TipoDB.SQLServer:
                    throw new NotImplementedException("SQLServer no está implementado en este proyecto");
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<venta> VentaRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<venta>(_cadenaDeConexion, new ventaValidator(), "id_venta", true);
                case TipoDB.SQLServer:
                    throw new NotImplementedException("SQLServer no está implementado en este proyecto");
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<venta_detalle> VentaDetalleRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<venta_detalle>(_cadenaDeConexion, new ventaDetalleValidator(), "id_detalle", true);
                case TipoDB.SQLServer:
                    throw new NotImplementedException("SQLServer no está implementado en este proyecto");
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<informacion_comercio> InformacionComercioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<informacion_comercio>(_cadenaDeConexion, new informacionComercioValidator(), "nombre_comercio", false);
                case TipoDB.SQLServer:
                    throw new NotImplementedException("SQLServer no está implementado en este proyecto");
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }
    }

    public enum TipoDB
    {
        MySQL,
        SQLServer
    }
}
