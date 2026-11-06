
namespace Banking.Domain.Excepciones;


    /// <summary>Datos de entrada incorrectos. Se traduce a HTTP 400.</summary>
    public class ValidacionException : ExcepcionDominio
    {
        public ValidacionException(string mensaje) : base(mensaje)
        {

        }
    }
