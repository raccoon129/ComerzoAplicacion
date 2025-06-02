using COMMON;
using COMMON.Entidades;
using COMMON.Interfaces;
using FluentValidation;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class MySQL<T> : IDB<T> where T : CamposControl
    {
        public string Error { get; private set; }
        private string cadenaDeConexion;
        private string campoId;
        private bool esAutonumerico;
        private AbstractValidator<T> validador;

        public MySQL(string cadenaDeConexion, AbstractValidator<T> validador, string campoId, bool esAutonumerico)
        {
            this.cadenaDeConexion = cadenaDeConexion;
            this.campoId = campoId;
            this.esAutonumerico = esAutonumerico;
            this.validador = validador;
            Error = "";
        }

        public T Actualizar(T entidad)
        {
            Error = "";
            try
            {
                //entidad.usuario_mod = Params.UsuarioConectado;
                entidad.fecha_mod = DateTime.Now;
                var resultadoValidacion = validador.Validate(entidad);
                if (resultadoValidacion.IsValid)
                {
                    string sql = $"UPDATE {typeof(T).Name} SET {string.Join(",",
                    entidad.GetType().GetProperties().Where(p => p.Name !=
                    campoId).Select(p => p.Name + "=@" + p.Name))} WHERE {campoId}=@Id";
                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                    foreach (var propiedad in entidad.GetType().GetProperties().Where(p => p.Name != campoId))
                    {
                        parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad) ?? DBNull.Value);
                    }
                    parametros.Add("@Id", entidad.GetType().GetProperty(campoId).GetValue(entidad));
                    var r = EjecutarComando(sql, parametros);
                    if (r == 1)
                    {
                        return entidad;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Error = string.Join(",", resultadoValidacion.Errors);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        public List<M> EjecutarProcedimiento<M>(string nombre, Dictionary<string, string> parametros) where M : class
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaDeConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(nombre, conexion))
                    {
                        comando.CommandType = System.Data.CommandType.StoredProcedure;
                        foreach (var parametro in parametros)
                        {
                            comando.Parameters.AddWithValue(parametro.Key, parametro.Value);
                        }
                        var reader = comando.ExecuteReader();
                        List<M> lista = new List<M>();
                        while (reader.Read())
                        {
                            M entidad = Activator.CreateInstance<M>();
                            foreach (var propiedad in entidad.GetType().GetProperties())
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                                    propiedad.SetValue(entidad, reader[propiedad.Name]);
                            }
                            lista.Add(entidad);
                        }
                        return lista;
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return new List<M>();
            }
        }

        public bool Eliminar(T entidad)
        {
            Error = "";
            try
            {
                string sql = $"DELETE FROM {typeof(T).Name} WHERE {campoId}=@Id";
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Id", entidad.GetType().GetProperty(campoId).GetValue(entidad));
                return EjecutarComando(sql, parametros) == 1;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return false;
            }
        }

        private int EjecutarComando(string sql, Dictionary<string, object> parametros)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaDeConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(sql, conexion))
                    {
                        foreach (var parametro in parametros)
                        {
                            comando.Parameters.AddWithValue(parametro.Key, parametro.Value);
                        }
                        return comando.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return -1;
            }
        }

        public T Insertar(T entidad)
        {
            Error = "";
            try
            {
                entidad.usuario_alta = Params.UsuarioConectado;
                entidad.fecha_alta = DateTime.Now;

                var resultadoValidacion = validador.Validate(entidad);
                if (resultadoValidacion.IsValid)
                {
                    string sql;
                    Dictionary<string, object> parametros = new Dictionary<string, object>();
                    if (esAutonumerico)
                    {
                        sql = $"INSERT INTO {typeof(T).Name} ({string.Join(",",
                            entidad.GetType().GetProperties().Where(p => p.Name !=
                            campoId).Select(p => p.Name))}) VALUES ({string.Join(",",
                            entidad.GetType().GetProperties().Where(p => p.Name !=
                            campoId).Select(p => "@" + p.Name))})";

                        foreach (var propiedad in entidad.GetType().GetProperties().Where(p => p.Name != campoId))
                        {
                            parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad) ?? DBNull.Value);
                        }
                    }
                    else
                    {
                        sql = $"INSERT INTO {typeof(T).Name} ({string.Join(",",
                            entidad.GetType().GetProperties().Select(p => p.Name))}) VALUES ({string.Join(",",
                            entidad.GetType().GetProperties().Select(p => "@" + p.Name))})";

                        foreach (var propiedad in entidad.GetType().GetProperties())
                        {
                            parametros.Add("@" + propiedad.Name, propiedad.GetValue(entidad) ?? DBNull.Value);
                        }
                    }

                    if (EjecutarComando(sql, parametros) == 1)
                    {
                        if (esAutonumerico)
                        {
                            sql = $"SELECT * FROM {typeof(T).Name} WHERE {campoId} = LAST_INSERT_ID()";
                            var consulta = EjecutarConsulta(sql, new Dictionary<string, object>());
                            if (consulta.Count == 1)
                            {
                                return consulta.First();
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return entidad;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Error = string.Join(",", resultadoValidacion.Errors);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        public T ObtenerPorId(int id)
        {
            try
            {
                string SQL = $"SELECT * FROM {typeof(T).Name} WHERE {campoId}=@Id";
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Id", id);
                return EjecutarConsulta(SQL, parametros).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        // Método adicional que no está en la interfaz pero que puede ser útil
        public T ObtenerPorID(string id)
        {
            try
            {
                string SQL = $"SELECT * FROM {typeof(T).Name} WHERE {campoId}=@Id";
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Id", id);
                return EjecutarConsulta(SQL, parametros).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        public List<T> ObtenerTodos()
        {
            try
            {
                string SQL = $"SELECT * FROM {typeof(T).Name}";
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                return EjecutarConsulta(SQL, parametros);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return null;
            }
        }

        private List<T> EjecutarConsulta(string sql, Dictionary<string, object> parametros)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaDeConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(sql, conexion))
                    {
                        foreach (var parametro in parametros)
                        {
                            comando.Parameters.AddWithValue(parametro.Key, parametro.Value);
                        }
                        var reader = comando.ExecuteReader();
                        List<T> lista = new List<T>();
                        while (reader.Read())
                        {
                            T entidad = Activator.CreateInstance<T>();
                            foreach (var propiedad in entidad.GetType().GetProperties())
                            {
                                try
                                {
                                    if (!reader.IsDBNull(reader.GetOrdinal(propiedad.Name)))
                                    {
                                        var value = reader[propiedad.Name];
                                        if (propiedad.PropertyType == typeof(int) && value is string)
                                        {
                                            propiedad.SetValue(entidad, Convert.ToInt32(value));
                                        }
                                        else if (propiedad.PropertyType == typeof(string) && value is int)
                                        {
                                            propiedad.SetValue(entidad, value.ToString());
                                        }
                                        else
                                        {
                                            propiedad.SetValue(entidad, value);
                                        }
                                    }
                                }
                                catch (Exception) { /* Ignorar errores en propiedades que no existen en la BD */ }
                            }
                            lista.Add(entidad);
                        }
                        return lista;
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return new List<T>();
            }
        }
    }
}
