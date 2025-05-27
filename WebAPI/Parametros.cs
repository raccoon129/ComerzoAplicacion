using DAL;

namespace WebAPI
{
    public static class Parametros
    {
        //escribir aqui la cadena de conexion

#if DEBUG
        //public static string CadenaConexion = @"SERVER=localhost\SQLEXPRESS; Database=Comerzo; Integrated Security=True;TrustServerCertificate=True;";
        public static string CadenaConexion = @"Server=localhost;Database=drift3_comerzo;Uid=root;Pwd=;";

#else
        public static string CadenaConexion = @"Server=mysql-drift3.alwaysdata.net;Database=drift3_comerzo;Uid=drift3;Pwd=xhjMz7BuB6PwRpy;";
        //public static string CadenaConexion = @";";
#endif
        //para dejar setear el tipo de base de datos


        public static TipoDB Tipo = TipoDB.MySQL;

        public static FabricRepository FabricaRepository = new FabricRepository(CadenaConexion, Tipo);
    }
}
