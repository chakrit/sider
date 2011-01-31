
using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sider.Tests
{
  [TestClass]
  public class RedisClientTests : SiderTestBase
  {
    public struct ClientInfo
    {
      public Stream InputStream { get; set; }
      public Stream OutputStream { get; set; }

      public IRedisClient Client { get; set; }
    }

    private ClientInfo createClient()
    {
      var inStream = new MemoryStream();
      var outStream = new MemoryStream();

      return new ClientInfo {
        InputStream = inStream,
        OutputStream = outStream,
        Client = new RedisClient(inStream, outStream)
      };
    }


    [TestMethod]
    public void IsDisposed_JustCreated_ShouldEqualsFalse()
    {
      var client = createClient();
      Assert.IsFalse(client.Client.IsDisposed);
    }

    [TestMethod]
    public void IsDisposed_AfterDispose_ShouldEqualsTrue()
    {
      var client = createClient();

      client.Client.Dispose();
      Assert.IsTrue(client.Client.IsDisposed);
    }

    [TestMethod]
    public void Ctor_HostIsNullOrEmpty_ExceptionThrown()
    {
      Expect<ArgumentException>(() => new RedisClient(host: null));
      Expect<ArgumentException>(() => new RedisClient(host: ""));
    }

    [TestMethod]
    public void Ctor_PortOutOfRange_ExceptionThrown()
    {
      Expect<ArgumentOutOfRangeException>(() => new RedisClient(port: int.MinValue));
      Expect<ArgumentOutOfRangeException>(() => new RedisClient(port: 0));
      Expect<ArgumentOutOfRangeException>(() => new RedisClient(port: 65536));
      Expect<ArgumentOutOfRangeException>(() => new RedisClient(port: int.MaxValue));
    }

    [TestMethod]
    public void Ctor_SettingsIsNull_ExceptionThrown()
    {
      Expect<ArgumentNullException>(() => new RedisClient(settings: null));
    }

    [TestMethod]
    public void MostAPIMethods_AfterDispose_ShouldThrowException()
    {
      // MOST = try a few commands that behave differently since
      //        most of the commands are similar and should be skipped

      Action<Action<IRedisClient>> test = clientCallback =>
      {
        // no need to fill in data because the exception
        // should be thrown whether there is data or not
        var client = createClient().Client;
        client.Dispose();

        Expect<ObjectDisposedException>(
          () => clientCallback(client));
      };

      test(c => c.Ping());
      test(c => c.Quit());
      test(c => c.Get("TEST"));
      test(c => c.Set("TEST", "TEST"));
      test(c => c.SetNX("TEST", "TEST"));
      test(c => c.GetRaw("YAY"));
      test(c => c.GetTo("TEST", new MemoryStream()));
      test(c => c.Incr("ASADF"));
      test(c => c.MGet("SDF", "ASD"));
    }


  }
}
