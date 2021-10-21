using System.Collections.Generic;
using System.Text;

namespace Pepper.Structures
{
    public static class StringExtensions
    {
        public static string[] SmartSplit(this string content, char quote, char whitespace = ' ',
            char escape = '\\')
        {
            var output = new List<string>();
            var piece = new StringBuilder();

            bool isEscaping = false, isQuoting = false;
            foreach (var character in content)
            {
                if (isEscaping)
                {
                    piece.Append(character);
                    isEscaping = false;
                    continue;
                }

                if (character == escape)
                {
                    isEscaping = true;
                    continue;
                }

                if (character == quote)
                {
                    isQuoting = !isQuoting;
                }

                if (character == whitespace)
                {
                    // chunk
                    output.Add(piece.ToString());
                    piece.Clear();
                    continue;
                }

                piece.Append(character);
            }

            if (piece.Length >= 0)
            {
                output.Add(piece.ToString());
            }

            return output.ToArray();
        }
    }
}