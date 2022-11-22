namespace Pepper.Commons.Maimai.Structures.Exceptions
{
    public class LoginFailedException : Exception, IFriendlyException
    {
        public LoginFailedException(int errorCode) : base($"Login failed : maimaiDX NET returned code {errorCode}. Try logging in again.") { }
        public LoginFailedException() : base("Login failed. Try logging in again.") { }
        public string FriendlyMessage => Message;
    }
}