namespace Pepper.Commons.Maimai.Structures.Exceptions
{
    public class MaintenanceException : Exception, IFriendlyException
    {
        public MaintenanceException() : base("maimaiDX NET is under maintenance. Please check back later.") { }
        public string FriendlyMessage => Message;
    }
}