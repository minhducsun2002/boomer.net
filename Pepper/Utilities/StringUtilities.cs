namespace Pepper.Utilities
{
    public static class StringUtilities
    {
        public static string Plural(long count, string plural = "s", string singular = "")
            => count > 1 ? plural : singular;
    }
}