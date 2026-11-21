namespace AutenticacionSeguraAA01_JC.Negocio.Dtos
{
    public class DtoIngreso
    {
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public bool Recordarme { get; set; }
    }
}
