using Io.Rz.FlywheelComponents.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Io.Rz.FlywheelComponents.Chunking
{
    public class Chunker : IEnumerable<byte[]>
    {

        private readonly uint significantBitsMask;
        private readonly AbstractRollingHasher hasher;
        private readonly CopyingStream copyingStream;

        public const int WINDOW_SIZE = 32;
        public const uint DEFAULT_SIGNIFICANT_BITS_NUMBER = 12;
        public Chunker(byte[] data, HasherType type)
            : this(new MemoryStream(data), type, DEFAULT_SIGNIFICANT_BITS_NUMBER)
        {
        }
        public Chunker(Stream stream, HasherType type)
            : this(stream, type, DEFAULT_SIGNIFICANT_BITS_NUMBER)
        {
        }

        public Chunker(Stream stream, HasherType type, uint significantHashBits)
        {
            //significantBitsMask = (uint)Math.Pow(2, significantHashBits);
            for (var i = 0; i < significantHashBits; i++)
            {
                significantBitsMask = (uint)((significantBitsMask << 1) + 1);
            }

            copyingStream = new CopyingStream(stream);
            this.hasher = AbstractRollingHasher.createInstance(type, copyingStream, WINDOW_SIZE);
        }

        public Chunker(byte[] data, HasherType type, uint significantHashBits)
            : this(new MemoryStream(data), type, significantHashBits)
        {
        }



        public IEnumerator<byte[]> GetEnumerator()
        {
            uint hash = 0;
            while (!hasher.AtEnd)
            {
                hash = hasher.NextHash();
                if ((hash & significantBitsMask) == 1)
                {
                    yield return copyingStream.GetCopiedBytesAndReset();
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        protected class CopyingStream : Stream
        {
            readonly Stream baseStream;
            MemoryStream copyStream;
            public CopyingStream(Stream baseStream)
            {
                this.baseStream = baseStream;
                copyStream = new MemoryStream();
            }

            public override bool CanRead
            {
                get { return baseStream.CanRead; }
            }

            public override bool CanSeek
            {
                get { return baseStream.CanSeek; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override void Flush()
            {
                baseStream.Flush();
            }

            public override long Length
            {
                get { return baseStream.Length; }
            }

            public override long Position
            {
                get
                {
                    return baseStream.Position;
                }
                set
                {
                    baseStream.Position = value;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var i = baseStream.Read(buffer, offset, count);
                copyStream.Write(buffer, offset, count);
                return i;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return baseStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                baseStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new System.NotImplementedException();
            }

            public byte[] GetCopiedBytesAndReset()
            {
                var buffer = copyStream.GetBuffer();
                copyStream = new MemoryStream();
                return buffer;
            }

            public void ResetCopy()
            {
                copyStream = new MemoryStream();
            }

        }
    }
}
