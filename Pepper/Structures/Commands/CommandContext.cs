using System;
using Discord.WebSocket;
using Pepper.Services.Main;

namespace Pepper.Structures
{
    public class CommandContext : Qmmands.CommandContext
    {
        public CommandContext(IServiceProvider services) : base(services) {}
        public SocketUser Author;
        public DiscordSocketClient Client;
        public ISocketMessageChannel Channel;
        public SocketUserMessage Message;
        public CommandService CommandService;
        public string Prefix;
    }
}