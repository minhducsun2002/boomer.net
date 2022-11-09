namespace Pepper.Commons.Maimai.Structures
{
    public class LoginFailedException : Exception, IFriendlyException
    {
        public LoginFailedException() : base("Login failed. Check the cookie again.") {}
        public string FriendlyMessage => "Login failed. Check the cookie again.";
    }
}