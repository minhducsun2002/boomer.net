namespace Pepper.Commons.Maimai.Structures
{
    public class LoginFailedException : Exception, IFriendlyException
    {
        public LoginFailedException() : base("Login failed. Try logging in again.") { }
        public string FriendlyMessage => "Login failed. Try logging in again.";
    }
}