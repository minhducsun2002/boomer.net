using Pepper.PngPayloadEmbed.Structures;

namespace Pepper.PngPayloadEmbed
{
    internal static class PresetChunks
    {
        private static readonly Chunk HeaderChunk = new(
            Chunk.FourCc('I', 'H', 'D', 'R'),
            new byte[] { 0, 0, 0, 0x64, 0, 0, 0, 0x14, 0x08, 0x02, 0, 0, 0 }
        );

        // 100x20 fully black pixel
        private static readonly Chunk DataChunk = new(
            Chunk.FourCc('I', 'D', 'A', 'T'),
            new byte[]
            {
                0x58, 0xc3, 0xed, 0xc1,
                0x31, 1, 0, 0,
                0, 0xc2, 0xa0, 0xf5,
                0x4f, 0x6d, 0x0a, 0x3f,
                0xa0, 0, 0, 0,
                0, 0, 0x80, 0x87,
                1, 0x17, 0x84, 0, 1
            }
        );

        private static readonly Chunk EndingChunk = new(
            Chunk.FourCc('I', 'E', 'N', 'D'),
            Array.Empty<byte>()
        );

        private static readonly Lazy<byte[]> HeaderLazy = new(() => HeaderChunk.Serialize());
        private static readonly Lazy<byte[]> DataLazy = new(() => DataChunk.Serialize());
        private static readonly Lazy<byte[]> EndingLazy = new(() => EndingChunk.Serialize());

        public static byte[] Header => HeaderLazy.Value;
        public static byte[] Data => DataLazy.Value;
        public static byte[] Ending => EndingLazy.Value;
        public static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    }
}