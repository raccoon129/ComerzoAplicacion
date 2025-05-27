using DAL;

namespace WebAPI
{
    public static class Parametros
    {
        //escribir aqui la cadena de conexion

#if DEBUG
        //public static string CadenaConexion = @"SERVER=localhost\SQLEXPRESS; Database=Comerzo; Integrated Security=True;TrustServerCertificate=True;";
        public static string CadenaConexion = @";";

#else
        public static string CadenaConexion = @";";
#endif
        //para dejar setear el tipo de base de datos


        public static TipoDB Tipo = TipoDB.MySQL;

        public static FabricRepository FabricaRepository = new FabricRepository(CadenaConexion, Tipo);
    }
}
