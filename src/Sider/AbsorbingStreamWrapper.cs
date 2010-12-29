
using System;
using System.Diagnostics;
using System.IO;

namespace Sider
{
  internal class AbsorbingStreamWrapper : Stream
  {
    private Stream _target;
    private Exception _ex;


    public override bool CanRead { get { return _target.CanRead; } }
    public override bool CanWrite { get { return _target.CanWrite; } }
    public override bool CanSeek { get { return false; } }

    public Exception Error { get { return _ex; } }
    public bool HasError { get { return _ex != null; } }


    public AbsorbingStreamWrapper(Stream target)
    {
      _target = target;
      _ex = null;
    }


    [DebuggerHidden, DebuggerStepThrough]
    public void ThrowIfError()
    {
      if (_ex != null) throw _ex;
    }


    public override void Flush() { _target.Flush(); }

    public override void WriteByte(byte value)
    {
      _target.WriteByte(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      if (_ex != null) return;

      // absorb exceptions
      try { _target.Write(buffer, offset, count); }
      catch (Exception ex) { _ex = ex; }
    }

    public override int ReadByte()
    {
      return _target.ReadByte();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      if (_ex != null) return count;

      // absorb exceptions
      try { return _target.Read(buffer, offset, count); }
      catch (Exception ex) {
        _ex = ex;

        // return a zero-ed out buffer incase of exception
        if (count > buffer.Length) count = buffer.Length;

        for (var i = 0; i < count; i++)
          buffer[i] = 0x00;

        return count;
      }
    }


    #region Unsupported members

    public override long Length
    {
      get { throw new NotSupportedException(); }
    }

    public override long Position
    {
      get { throw new NotSupportedException(); }
      set { throw new NotSupportedException(); }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    #endregion
  }
}
