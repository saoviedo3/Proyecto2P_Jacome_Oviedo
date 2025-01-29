using Microsoft.AspNetCore.SignalR;
using ChatAppBackend.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class ChatHub : Hub
{
    private readonly ChatDbContext _context;

    public ChatHub(ChatDbContext context)
    {
        _context = context;
    }

    public async Task SendMessage(string username, string message)
    {
        if (string.IsNullOrEmpty(username))
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "Sistema", "Error: No estás autenticado.");
            return;
        }

        var newMessage = new Message
        {
            UserName = username,
            Content = message,
            Timestamp = DateTime.Now
        };

        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage", username, message);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Console.WriteLine($"Cliente conectado: {Context.ConnectionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine($"Cliente desconectado: {Context.ConnectionId}");
    }
}
