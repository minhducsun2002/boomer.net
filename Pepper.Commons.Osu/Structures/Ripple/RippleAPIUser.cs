namespace Pepper.Commons.Osu.API.Ripple
{
    internal class RippleAPIUser : APIUser
    {
        public override string GetPublicUrl => $"https://ripple.moe/u/{Id}";
        public new string AvatarUrl => $"https://a.ripple.moe/{Id}";
    }
}