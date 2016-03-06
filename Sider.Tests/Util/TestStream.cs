using System.IO;
using System;
using System.Text;

namespace Sider.Tests {
  public class TestStream : Stream {
    readonly MemoryStream memoryStream;

    public override bool CanRead { get { return true; } }
    public override bool CanSeek { get { return true; } }
    public override bool CanWrite { get { return false; } }

    public override long Length { get { return memoryStream.Length; } }

    public override long Position {
      get { return memoryStream.Position; }
      set { memoryStream.Position = value; }
    }


    public TestStream(string input) : this(Encoding.UTF8.GetBytes(input)) {
    }

    public TestStream(byte[] buffer) {
      memoryStream = new MemoryStream(buffer, false);
    }


    public override void Flush() {
      throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin) {
      return memoryStream.Seek(offset, origin);
    }

    public override void SetLength(long value) {
      memoryStream.SetLength(value);
    }

    public override int Read(byte[] buffer, int offset, int count) {
      if (memoryStream.Position == memoryStream.Length) {
        throw new EndOfStreamException();
      }

      return memoryStream.Read(buffer, offset, count);
    }

    public override void Write(byte[] buffer, int offset, int count) {
      throw new NotSupportedException();
    }
  }
}

