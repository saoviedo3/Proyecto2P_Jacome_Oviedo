namespace ChatAppBackend.Models
{
    public class User
    {
        public int Id { get; set; } // Identificador único
        public string Username { get; set; } // Nombre de usuario (único)
        public string Password { get; set; } // Contraseña (encriptada en el futuro)
    }
}
