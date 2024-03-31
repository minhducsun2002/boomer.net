using System;

namespace Pepper.Commons.Osu.Exceptions
{
    public class UserNotFoundException : Exception, IFriendlyException
    {
        internal UserNotFoundException() { }
        internal UserNotFoundException(string message) : base(message) { }
        internal UserNotFoundException(string message, Exception inner) : base(message, inner) { }
        public string? Username { get; internal set; }

        public string FriendlyMessage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Message))
                {
                    return "User not found!";
                }

                return "User not found! Error was " + Message;
            }
        }
    }
}