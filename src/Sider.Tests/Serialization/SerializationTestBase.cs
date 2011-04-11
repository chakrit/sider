
using System;
using System.IO;
using NUnit.Framework;

namespace Sider.Tests.Serialization
{
  public abstract class SerializationTestBase<TSerializer, TOut> : SiderTestBase
    where TSerializer : ISerializer<TOut>, new()
  {
    protected Stream TempStream { get; private set; }
    protected byte[] TempBuffer { get; private set; }

    protected TSerializer Serializer { get; private set; }
    protected RedisSettings Settings { get; private set; }


    [TestFixtureSetUp]
    public void FixtureSetup() { Settings = new RedisSettings(); }

    [SetUp]
    public void Setup()
    {
      Serializer = new TSerializer();
      TempStream = new MemoryStream();
      TempBuffer = new byte[1024];
    }

    [TearDown]
    public void TearDown()
    {
      try { TempStream.Dispose(); }
      catch { /* absorbed */ }
    }


    protected void RewindStream()
    {
      TempStream.Seek(0, SeekOrigin.Begin);
    }

    protected TOut SerializationRoundtrip(TOut obj)
    {
      Serializer.ResetWrite(obj);

      // perform writes
      var totalBytes = Serializer.GetBytesNeeded(Settings);
      var bytesLeft = totalBytes;
      while (bytesLeft > 0) {
        var chunkSize = Math.Min(bytesLeft, TempBuffer.Length);
        var bytesWrote = Serializer.Write(TempBuffer, 0, chunkSize);

        bytesLeft -= bytesWrote;
        TempStream.Write(TempBuffer, 0, bytesWrote);
      }

      // read out obj
      RewindStream();
      return Serializer.Read(Settings, TempStream, totalBytes);
    }

  }
}
