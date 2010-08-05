
using System;
using System.IO;

namespace Sider
{
  public class AbsorbingStreamWrapper : Stream
  {
    private Stream _target;
    private Exception _ex;


    public override bool CanRead { get { return false; } }
    public override bool CanSeek { get { return false; } }
    public override bool CanWrite { get { return true; } }

    public Exception Error { get { return _ex; } }
    public bool HasError { get { return _ex != null; } }


    public AbsorbingStreamWrapper(Stream target)
    {
      _target = target;
      _ex = null;
    }


    public override void Flush() { _target.Flush(); }

    public override void Write(byte[] buffer, int offset, int count)
    {
      if (_ex != null)
        return; // absorb writes

      try { _target.Write(buffer, offset, count); }
      catch (Exception ex) { _ex = ex; }
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

    public override int Read(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
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
