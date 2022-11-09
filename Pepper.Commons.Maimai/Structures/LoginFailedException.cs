namespace Pepper.Commons.Maimai.Structures
{
    public class LoginFailedException : Exception
    {
        public LoginFailedException() : base("Login failed. Check the cookie again.") {}
    }
}