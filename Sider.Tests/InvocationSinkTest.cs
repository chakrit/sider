using System.IO;
using NUnit.Framework;

namespace Sider.Tests {
  [TestFixture]
  public class InvocationSinkTest : SiderTest {
    [Test]
    public void TestQueue() {
      var spy = new Spy();
      var inv = new Invocation<object>(
                  spy.Action<RedisWriter>(),
                  null
                );

      var stream = new MemoryStream();
      var sink = new InvocationSink(stream, RandomSettings());

      sink.Queue(inv);
      Assert.IsTrue(spy.Called);
      Assert.IsInstanceOf<RedisWriter>(spy.CalledArgs[0]);
    }
  }
}

