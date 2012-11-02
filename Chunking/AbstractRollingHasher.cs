using System;
using System.Collections.Generic;
using System.IO;

namespace Io.Rz.FlywheelComponents.Chunking
{

    public abstract class AbstractRollingHasher
    {
        protected Stream stream;
        protected int windowSize;
        protected Queue<byte> window;
        protected uint currentHashValue = 0;

        bool initialized = false;

        public AbstractRollingHasher(Stream stream, int windowSize)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Non-readable stream", "stream");

            this.windowSize = windowSize;
            window = new Queue<byte>(windowSize);
            this.stream = new WindowUpdatingStream(stream, window);
        }
        protected abstract uint InitHash();

        protected abstract uint SlideWindow(byte oldByte, byte newByte);

        public abstract HasherType Type
        {
            get;
        }

        public bool AtEnd
        {
            get
            {
                return stream.Position == stream.Length;
            }
        }

        public uint NextHash()
        {
            if (!initialized)
            {
                initialized = true;
                currentHashValue = InitHash();
                return currentHashValue;
            }
            else
            {
                currentHashValue = UpdateWindow();
                return currentHashValue;
            }
        }

        private uint UpdateWindow()
        {
            byte oldByte = window.Dequeue();
            byte newByte = (byte)stream.ReadByte();
            return SlideWindow(oldByte, newByte);
        }

        public static AbstractRollingHasher createInstance(HasherType type, Stream stream, int windowSize)
        {
            switch (type)
            {
                case HasherType.BuzHash:
                    return new BuzHashRollingHasher(stream, windowSize);
                default:
                    return null;
            }
        }

        protected class WindowUpdatingStream : Stream
        {
            readonly Stream baseStream;
            readonly Queue<byte> window;
            public WindowUpdatingStream(Stream baseStream, Queue<byte> window)
            {
                this.baseStream = baseStream;
                this.window = window;
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
                for (var j = offset; j < offset + count; j++)
                {
                    window.Enqueue(buffer[j]);
                }
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
        }
    }

    public enum HasherType
    {
        None = 0,
        BuzHash,
        Rabin,
    }



}
