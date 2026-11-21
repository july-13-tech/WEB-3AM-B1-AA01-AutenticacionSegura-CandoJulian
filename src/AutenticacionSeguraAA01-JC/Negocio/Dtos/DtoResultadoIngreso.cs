namespace AutenticacionSeguraAA01_JC.Negocio.Dtos
{
    public class DtoResultadoIngreso
    {
        public bool Exitoso { get; private set; }
        public bool CuentaBloqueada { get; private set; }
        public string Mensaje { get; private set; } = string.Empty;

        public static DtoResultadoIngreso Correcto()
        {
            return new DtoResultadoIngreso { Exitoso = true };
        }

        public static DtoResultadoIngreso Fallido(string mensaje)
        {
            return new DtoResultadoIngreso { Exitoso = false, Mensaje = mensaje };
        }

        public static DtoResultadoIngreso Bloqueado(string mensaje)
        {
            return new DtoResultadoIngreso { Exitoso = false, CuentaBloqueada = true, Mensaje = mensaje };
        }
    }
}
