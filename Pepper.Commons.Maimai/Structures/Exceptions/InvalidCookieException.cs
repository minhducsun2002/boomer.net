namespace Pepper.Commons.Maimai.Structures.Exceptions
{
    public class InvalidCookieException : Exception, IFriendlyException
    {
        public InvalidCookieException() : base("Passed credentials were invalid. Please log in again.") { }
        public InvalidCookieException(string message) : base($"Passed credentials were invalid : {message}. Please log in again.") { }
        public string FriendlyMessage => Message;
    }
}