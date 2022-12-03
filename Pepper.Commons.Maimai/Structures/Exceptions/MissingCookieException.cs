namespace Pepper.Commons.Maimai.Structures.Exceptions
{
    public class MissingCookieException : System.Exception, IFriendlyException
    {
        public MissingCookieException() : base("No cookie was provided. Try logging in via `m!login` in DM.") { }
        public string FriendlyMessage => "No cookie was provided. Try logging in via `m!login` in DM.";
    }
}