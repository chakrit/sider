using System;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Sider.Tests {
  [TestFixture]
  public class InvocationPumpTest : SiderTestBase {
    [Test]
    public void TestCtor() {
      var ms = new MemoryStream();
      var settings = RandomSettings();

      Assert.Throws<ArgumentNullException>(() => new InvocationPump(null, null));
      Assert.Throws<ArgumentNullException>(() => new InvocationPump(ms, null));
      Assert.Throws<ArgumentNullException>(() => new InvocationPump(null, settings));
      Assert.DoesNotThrow(() => new InvocationPump(ms, settings));
    }

    [Test, Timeout(1000)]
    public void TestQueue() {
      var spy = new Spy();
      var inv = new Invocation<object>(
                  null,
                  spy.Func<RedisReader>()
                );

      object result;
      spy.Returns = new object();

      var ms = new MemoryStream();
      using (var pump = new InvocationPump(ms, RandomSettings())) {
        pump.Queue(inv);
        result = inv.Result;
      }

      Assert.IsTrue(spy.Called);
      Assert.IsInstanceOf<RedisReader>(spy.CalledArgs[0]);
      Assert.AreEqual(spy.Returns, result);
    }

    [Test, Timeout(1000)]
    public void TestQueue_Multiple() {
      var pairs = Enumerable
        .Range(0, 100)
        .Select(num => new {
          Number = num,
          Invocation = new Invocation<int>(null, _ => num)
        })
        .ToArray();
      
      var ms = new MemoryStream();
      var settings = RandomSettings();
      using (var pump = new InvocationPump(ms, settings)) {
        foreach (var pair in pairs) pump.Queue(pair.Invocation);
        foreach (var pair in pairs) Assert.AreEqual(pair.Number, pair.Invocation.Result);
      }
    }

    [Test, Timeout(1000)]
    public void TestQueue_Dispose() {
      var ms = new MemoryStream();
      var settings = RandomSettings();
      var pump = new InvocationPump(ms, settings);
      pump.Dispose(); // should not deadlock.
    }
  }
}

