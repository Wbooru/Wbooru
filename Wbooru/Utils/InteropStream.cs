using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Utils
{
    public class InteropStream : Stream
    {
        private Stream base_stream;

        public InteropStream(Stream stream)
        {
            base_stream = stream;
        }

        public override bool CanRead => base_stream.CanRead;

        public override bool CanSeek => base_stream.CanSeek;

        public override bool CanWrite => base_stream.CanWrite;

        public override long Length => base_stream.Length;

        public override long Position { get => base_stream.Position; set => base_stream.Position = value; }

        public override void Flush()
        {
            base_stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = base_stream.Read(buffer, offset, count);
            OnAfterRead?.Invoke(buffer, offset, count, read);
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return base_stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            base_stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base_stream.Write(buffer, offset, count);
            OnAfterWrite?.Invoke(buffer, offset, count);
        }

        public delegate void HookAfterRead(byte[] buffer, int offset, int count, int read);
        public delegate void HookWrite(byte[] buffer, int offset, int count);

        public event HookAfterRead OnAfterRead;
        public event HookWrite OnAfterWrite;
    }

    public static class InteropStreamExtension
    {
        public static InteropStream Interopable(this Stream stream)
        {
            return new InteropStream(stream);
        }
    }
}
