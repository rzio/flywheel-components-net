using Io.Rz.FlywheelComponents.Util;
using System.IO;

namespace Io.Rz.FlywheelComponents.Chunking
{
    public class BuzHashRollingHasher : AbstractRollingHasher
    {

        BuzHashFunction hash = new BuzHashFunction();

        public BuzHashRollingHasher(Stream stream, int windowSize)
            : base(stream, windowSize)
        { }

        protected override uint InitHash()
        {
            return hash.Hash(stream, windowSize);
        }

        protected override uint SlideWindow(byte oldByte, byte newByte)
        {
            return hash.UpdateHash(currentHashValue, newByte, oldByte, windowSize);
        }

        public override HasherType Type
        {
            get
            {
                return HasherType.BuzHash;
            }
        }
    }
}
