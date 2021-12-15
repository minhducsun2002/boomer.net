using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pepper.Commons.Osu;

namespace Pepper.Database.OsuUsernameProviders
{
    [Table("osu-usernames")]
    public class Username
    {
        [Key]
        [Column("discord_user_id")] public string DiscordUserId { get; set; }
        [Column("osu_username")] public string? OsuUsername { get; set; }
        [Column("ripple_username")] public string? RippleUsername { get; set; }
        [Column("default_mode")] public string? DefaultServerString { get; set; }

        public GameServer DefaultServer
        {
            get
            {
                if (DefaultServerString != null)
                {
                    return Enum.Parse<GameServer>(DefaultServerString);
                }
                return GameServer.Osu;
            }
        }

        public string? GetUsername(GameServer server)
        {
            return server switch
            {
                GameServer.Osu => OsuUsername,
                GameServer.Ripple => RippleUsername,
                _ => throw new ArgumentOutOfRangeException(nameof(server), server, null)
            };
        }
    }
}