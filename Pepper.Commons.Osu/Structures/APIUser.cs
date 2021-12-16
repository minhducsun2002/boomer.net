namespace Pepper.Commons.Osu.API
{
    public class APIUser : osu.Game.Online.API.Requests.Responses.APIUser
    {
        public virtual string PublicUrl => $"https://osu.ppy.sh/users/{Id}";
        public new virtual string AvatarUrl => base.AvatarUrl;
    }
}