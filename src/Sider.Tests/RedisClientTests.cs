
using System;
using System.IO;
using NUnit.Framework;

namespace Sider.Tests
{
  public class RedisClientTests : SiderTestBase
  {
    public struct ClientInfo
    {
      public Stream InputStream { get; set; }
      public Stream OutputStream { get; set; }

      public IRedisClient<string> Client { get; set; }
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


    [Test]
    public void IsDisposed_JustCreated_ShouldEqualsFalse()
    {
      var client = createClient();
      Assert.That(client.Client.IsDisposed, Is.False);
    }

    [Test]
    public void IsDisposed_AfterDispose_ShouldEqualsTrue()
    {
      var client = createClient();

      client.Client.Dispose();
      Assert.That(client.Client.IsDisposed, Is.True);
    }

    [Test]
    public void Ctor_HostIsNullOrEmpty_ExceptionThrown()
    {
      Assert.Throws<ArgumentException>(() => new RedisClient(host: null));
      Assert.Throws<ArgumentException>(() => new RedisClient(host: ""));
    }

    [Test]
    public void Ctor_PortOutOfRange_ExceptionThrown()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => new RedisClient(port: 0));
      Assert.Throws<ArgumentOutOfRangeException>(() => new RedisClient(port: 65536));
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new RedisClient(port: int.MinValue));
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new RedisClient(port: int.MaxValue));
    }

    [Test]
    public void Ctor_SettingsIsNull_ExceptionThrown()
    {
      Assert.Throws<ArgumentNullException>(() => new RedisClient(settings: null));
    }

    [Test]
    public void MostAPIMethods_AfterDispose_ShouldThrowException()
    {
      // MOST = try a few commands that behave differently since
      //        most of the commands are similar and should be skipped

      Action<Action<IRedisClient<string>>> test = clientCallback =>
      {
        // no need to fill in data because the exception
        // should be thrown whether there is data or not
        var client = createClient().Client;
        client.Dispose();

        Assert.Throws<ObjectDisposedException>(
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
