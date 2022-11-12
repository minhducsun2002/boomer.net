namespace Pepper.Commons.Maimai.Structures.Exception
{
    public class MissingCookieException : System.Exception, IFriendlyException
    {
        public MissingCookieException() : base("No cookie was provided. Try logging in.") { }
        public string FriendlyMessage => "No cookie was provided. Try logging in.";
    }
}