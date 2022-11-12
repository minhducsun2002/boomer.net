namespace Pepper.Commons.Maimai.Structures.Exception
{
    public class LoginFailedException : System.Exception, IFriendlyException
    {
        public LoginFailedException() : base("Login failed. Try logging in again.") { }
        public string FriendlyMessage => "Login failed. Try logging in again.";
    }
}