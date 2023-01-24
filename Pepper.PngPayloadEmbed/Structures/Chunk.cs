using System.IO.Hashing;

namespace Pepper.PngPayloadEmbed.Structures
{
    internal class Chunk
    {
        private uint Length => (uint) data.Length;
        private readonly uint type;
        private readonly byte[] data;

        public Chunk(uint type, byte[] data)
        {
            this.type = type;
            this.data = data;
        }

        public static uint FourCc(char c1, char c2, char c3, char c4)
        {
            var cc1 = (uint) c1 * 0x100 * 0x100 * 0x100;
            var cc2 = (uint) c2 * 0x100 * 0x100;
            var cc3 = (uint) c3 * 0x100;
            var cc4 = (uint) c4;
            return cc1 + cc2 + cc3 + cc4;
        }

        public byte[] Serialize()
        {
            var lenBytes = BitConverter.GetBytes(Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lenBytes);
            }
            var typeBytes = BitConverter.GetBytes(type);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(typeBytes);
            }
            var payload = typeBytes.Concat(data).ToArray();
            var crc = Crc32.Hash(payload);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(crc);
            }
            var output = lenBytes.Concat(payload).Concat(crc).ToArray();
            return output;
        }
    }
}