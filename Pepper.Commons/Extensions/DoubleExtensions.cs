using System.Globalization;

namespace Pepper.Commons.Extensions
{
    public static class DoubleExtensions
    {
        private static string Padded(long length)
        {
            return length.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
        }
        public static string SerializeAsMiliseconds(this double timestamp)
        {
            var length = (long) timestamp;
            return $@"{Padded(length / 60000)}:{Padded(length % 60000 / 1000)}";
        }
    }
}