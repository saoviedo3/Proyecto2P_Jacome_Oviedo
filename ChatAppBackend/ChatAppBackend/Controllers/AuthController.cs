using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ChatAppBackend.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAppBackend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ChatDbContext _context;

        public AuthController(ChatDbContext context)
        {
            _context = context;
        }

        // ✅ Registro de nuevos usuarios
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("El nombre de usuario ya está en uso.");
            }

            // Encriptar la contraseña antes de guardarla
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado correctamente.");
        }

        // ✅ Inicio de sesión (Login)
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginData)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == loginData.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginData.Password, user.Password))
            {
                return Unauthorized("Usuario o contraseña incorrectos.");
            }

            // Guardar el nombre de usuario en la sesión
            HttpContext.Session.SetString("Username", user.Username);

            return Ok(new { message = "Inicio de sesión exitoso.", username = user.Username });
        }

        // ✅ Cerrar sesión (Logout)
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            return Ok("Sesión cerrada.");
        }

        // ✅ Obtener usuario autenticado
        [HttpGet("currentUser")]
        public IActionResult GetCurrentUser()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("No hay usuario autenticado.");
            }
            return Ok(new { username });
        }
    }
}
