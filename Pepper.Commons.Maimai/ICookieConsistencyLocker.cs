namespace Pepper.Commons.Maimai
{
    public interface ICookieConsistencyLocker
    {
        public void Lock(string cookie);
        public void Unlock(string cookie);
    }
}