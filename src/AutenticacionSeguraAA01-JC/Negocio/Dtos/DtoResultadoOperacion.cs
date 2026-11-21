namespace AutenticacionSeguraAA01_JC.Negocio.Dtos
{
    public class DtoResultadoOperacion
    {
        public bool Exitoso { get; private set; }
        public List<string> Errores { get; private set; } = new();

        public static DtoResultadoOperacion Correcto()
        {
            return new DtoResultadoOperacion { Exitoso = true };
        }

        public static DtoResultadoOperacion Fallido(IEnumerable<string> errores)
        {
            return new DtoResultadoOperacion { Exitoso = false, Errores = errores.ToList() };
        }
    }
}
