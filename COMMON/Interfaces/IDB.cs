using COMMON.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Interfaces
{
    /// <summary>
    /// Interfaz que define el comportamiento de una clase para la comunicacion con una Base de Datos 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDB<T> where T : CamposControl //generica
    {
        /// <summary>
        /// Obtiene el error, si es que exite al ejecutar una operacion
        /// </summary>
        string Error { get; }
        /// <summary>
        /// Obitene todos los registros de una tabla
        /// </summary>
        /// <returns> Lista de objetos que representan los registros</returns>
        List<T> ObtenerTodos();
        /// <summary>
        /// Obtiene el registro correspondiente al Id Proporcionado
        /// </summary>
        /// <param name="id"> Id del registro a obtener</param>
        /// <returns>
        /// Objeto correponiente al Id seleccionado </returns>
        T ObtenerPorId(int id);
        /// <summary>
        /// Obtiene el registro correspondiente al Id Proporcionado
        /// </summary>
        /// <param name="id"> Id del registro a obtener  </param>
        /// <returns></returns>
        bool Eliminar(T entidad);
        /// <summary>
        ///  Elimina un registro de la tabla
        /// </summary>
        /// <param name="entidad"></param>
        /// <returns></returns>
        T Insertar(T entidad);
        /// <summary>
        /// Inserta un registro en la tabla
        /// </summary>
        /// <param name="entidad"></param>
        /// <returns></returns>
        T Actualizar(T entidad);
        /// <summary>
        /// Actualiza un registro en la tabla
        /// </summary>
        /// <typeparam name="M"></typeparam>
        /// <param name="nombre"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        List<M> EjecutarProcedimiento<M>(string nombre, Dictionary<string, string> parametros) where M : class;
        /// <summary>
        /// Realiza la ejecucion de un procedimiento almacenado
        /// 
    }
}
