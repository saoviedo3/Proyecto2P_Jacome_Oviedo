using Microsoft.EntityFrameworkCore;

namespace ChatAppBackend.Models
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }

    }
}
