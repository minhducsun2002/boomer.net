using System;

namespace Pepper.Commons.Osu.Exceptions
{
    public class UserNotFoundException : Exception
    {
        internal UserNotFoundException() { }
        internal UserNotFoundException(string message) : base(message) { }
        internal UserNotFoundException(string message, Exception inner) : base(message, inner) { }
        public string? Username { get; internal set; }
    }
}