
using System.IO;
using NUnit.Framework;
using Sider.Serialization;

namespace Sider.Tests.Serialization
{
  public abstract class SerializationTestBase<TSerializer, TOut> : SiderTestBase
    where TSerializer : ISerializer<TOut>
  {
    protected Stream TempStream { get; private set; }
    protected byte[] TempBuffer { get; private set; }

    protected TSerializer Serializer { get; private set; }
    protected RedisSettings Settings { get; private set; }


    protected abstract TSerializer BuildSerializer();


    [TestFixtureSetUp]
    public void FixtureSetup() { Settings = RedisSettings.Default; }

    [SetUp]
    public void Setup()
    {
      Serializer = BuildSerializer();
      Serializer.Init(Settings);

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
      var bytesNeeded = Serializer.GetBytesNeeded(obj);

      Serializer.Write(obj, TempStream, bytesNeeded);
      RewindStream();
      return (TOut)Serializer.Read(TempStream, bytesNeeded);
    }

  }
}
