
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Sider.Executors;
using Sider.Serialization;

namespace Sider.Tests
{
  public class RedisClientTests : SiderTestBase
  {
    public struct ClientInfo
    {
      public Stream InputStream { get; set; }
      public Stream OutputStream { get; set; }

      public IRedisClient<string> Client { get; set; }

      public RedisClient<string> ClientImpl
      {
        get { return (RedisClient<string>)Client; }
      }
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
    public void Ctor_IncompatibleSerializer_ExceptionThrown()
    {
      // use a string serializer for int clients
      var settings = RedisSettings.Build()
        .OverrideSerializer(new Mock<ISerializer<string>>().Object);

      // prevent Reset() from actually trying to connect to redis
      var clientMock = new Mock<RedisClient<int>>();
      clientMock.Setup(c => c.Reset()).Callback(() => { });

      Assert.Throws<ArgumentException>(() => clientMock.Object.ToString());
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


    [Test]
    public void AllAPIMethods_WhenInvoked_ShouldInvokeCommandWithSameName()
    {
      var exceptions = new HashSet<string> {
        "Pipeline", "Custom", "Reset", "Quit", "Shutdown"
      };

      var pack = createClient();

      // setup command interception
      var lastInvokedCommand = "___NONE___";
      pack.ClientImpl.InterceptCommands(cmd => lastInvokedCommand = cmd);

      // run the commands and verify that method names match the invoked commands
      var methods = typeof(IRedisClient<string>)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Where(m => !(exceptions.Contains(m.Name) ||
          m.IsSpecialName ||
          m.Name.EndsWith("Raw") ||
          m.Name.EndsWith("To") ||
          m.Name.EndsWith("From") ||
          m.Name.StartsWith("Debug") ||
          m.Name.StartsWith("Config")));

      foreach (var method in methods) {
        // reset client state
        ((RedisClientBase)pack.Client).SwitchExecutor<ImmediateExecutor>();

        // setup proper invocation parameters
        var methodParams = method.GetParameters();
        var invokeParams = methodParams
          .Select(p => p.ParameterType)
          .Select(t =>
          {
            if (t.IsArray) return Array.CreateInstance(t.GetElementType(), 0);
            if (t.IsValueType) return Activator.CreateInstance(t);

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
              return Array.CreateInstance(t.GetGenericArguments()[0], 0);

            return null;
          })
          .Cast<object>()
          .ToArray();

        // invoke the method
        Console.Write("Checking: ");
        Console.Write(method.Name);
        Console.Write("(");
        Console.Write(string.Join(", ", invokeParams
          .Select(p => p == null ? "null" : p.ToString())));
        Console.WriteLine(")");

        method.Invoke(pack.Client, invokeParams);

        // check that invoked commands match the method name
        Assert.That(string.IsNullOrEmpty(lastInvokedCommand), Is.False);
        Assert.That(lastInvokedCommand.ToUpper(),
          Is.EqualTo(method.Name.ToUpper()),
          "Invoked commands doesn't match method name for: " + method.Name +
          " (invoked: " + lastInvokedCommand + ")");
      }

    }
  }
}
