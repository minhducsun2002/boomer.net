using Pepper.PngPayloadEmbed.Structures;

namespace Pepper.PngPayloadEmbed
{
    public static class PngPayloadWrapper
    {
        static PngPayloadWrapper()
        {
            var sig = PresetChunks.Signature;
            var header = PresetChunks.Header;
            var data = PresetChunks.Data;

            Start = new byte[sig.Length + header.Length + data.Length];
            Array.Copy(sig, 0, Start, 0, sig.Length);
            Array.Copy(header, 0, Start, 0 + sig.Length, header.Length);
            Array.Copy(data, 0, Start, 0 + sig.Length + header.Length, data.Length);
        }

        private static readonly byte[] Start;
        private static readonly byte[] End = PresetChunks.Ending;

        public static byte[] Wrap(byte[] payload)
        {
            if (payload.Length > (1 << 30))
            {
                throw new ArgumentException("too much data", nameof(payload));
            }

            // tag is a lowercase `a`, then separator, then decompression method (0xFF is bogus) 
            var baseChunk = new byte[] { 97, 0x00, 0xFF };
            var chunkData = new byte[baseChunk.Length + payload.Length];
            Array.Copy(baseChunk, chunkData, baseChunk.Length);
            Array.Copy(payload, 0, chunkData, baseChunk.Length, payload.Length);

            var ztxt = new Chunk(Chunk.FourCc('z', 'T', 'X', 't'), chunkData).Serialize();
            var final = new byte[Start.Length + ztxt.Length + End.Length];
            Array.Copy(Start, 0, final, 0, Start.Length);
            Array.Copy(ztxt, 0, final, 0 + Start.Length, ztxt.Length);
            Array.Copy(End, 0, final, 0 + Start.Length + ztxt.Length, End.Length);

            return final;
        }
    }
}