using System.Collections.Concurrent;
using Pepper.Commons.Maimai;
using Pepper.Commons.Structures;

namespace Pepper.Frontends.Maimai.Services
{
    public class CookieLockingService : ICookieConsistencyLocker
    {
        // TODO: re-implement all this
        private readonly Dictionary<string, SemaphoreSlim> locker = new();
        public void Lock(string cookie)
        {
            return;
#pragma warning disable CS0162
            SemaphoreSlim semaphore;
            lock (locker)
            {
                if (!locker.TryGetValue(cookie, out semaphore!))
                {
                    locker[cookie] = new SemaphoreSlim(1, 1);
                    semaphore = locker[cookie];
                }
            }
            semaphore.Wait();
#pragma warning restore CS0162
        }

        public void Unlock(string cookie)
        {
            return;
#pragma warning disable CS0162
            lock (locker)
            {
                if (!locker.TryGetValue(cookie, out var semaphore))
                {
                    throw new Exception(
                        $"Could not find semaphore for value {cookie.Substring(0, Math.Min(10, cookie.Length))}. Another thread might have removed it!");
                }

                semaphore.Release();
                if (semaphore.CurrentCount == 1)
                {
                    locker.Remove(cookie, out _);
                }
            }
#pragma warning restore CS0162
        }
    }
}