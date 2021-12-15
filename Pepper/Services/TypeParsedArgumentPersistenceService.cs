using System;
using System.Collections.Generic;
using commandContextType = Disqord.Bot.DiscordCommandContext;

namespace Pepper.Services
{
    /// <summary>
    /// This service is for usage in parameter checks, should they want to access all type-parsed arguments.
    /// </summary>
    /// <remarks>
    /// Currently, the <c>Argument</c> property of <see cref="commandContextType"/> is not set before all checks are run.
    ///
    /// A (partial) workaround is to set the parsed value here after parsing.
    /// </remarks>
    public class TypeParsedArgumentPersistenceService
    {
        private Dictionary<Type, object> storage = new();

        public T Get<T>() => (T) storage[typeof(T)];

        public void Set<T>(T value)
        {
            if (storage.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"A value of type {typeof(T)} already exists!");
            }
            storage[typeof(T)] = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}