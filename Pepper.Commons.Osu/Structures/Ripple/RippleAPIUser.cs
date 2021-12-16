namespace Pepper.Commons.Osu.API.Ripple
{
    internal class RippleAPIUser : APIUser
    {
        public override string PublicUrl => $"https://ripple.moe/u/{Id}";
        public override string AvatarUrl => $"https://a.ripple.moe/{Id}";
    }
}