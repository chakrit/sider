using System;
using NUnit.Framework;

namespace Sider.Tests {
  [TestFixture]
  public class InvocationTest : SiderTest {
    [Test]
    public void TestCtor() {
      Action<RedisWriter> write = w => {
      };
      Func<RedisReader, object> read = r => null;
        
      var inv = new Invocation<object>(write, read);
      Assert.AreEqual(write, inv.WriteAction);
      Assert.AreEqual(read, inv.ReadAction);
    }

    [Test]
    public void TestAction() {
      var writeSpy = new Spy();
      var readSpy = new Spy();
      readSpy.Returns = new object();

      var inv = new Invocation<object>(
                  writeSpy.Action<RedisWriter>(),
                  readSpy.Func<RedisReader>()
                );

      inv.Write(null);
      Assert.IsTrue(writeSpy.Called);

      var result = inv.Read(null);
      Assert.IsTrue(readSpy.Called);
      Assert.AreEqual(readSpy.Returns, result);
    }

    [Test]
    public void TestSetResult() {
      var inv = BuildInvocation();
      var result = new object();
      inv.SetResult(result);
      Assert.AreEqual(result, inv.Result);
    }

    [Test]
    public void TestSetException() {
      var inv = BuildInvocation();
      var exception = new AggregateException();
      inv.SetException(exception);
      Assert.AreEqual(exception, inv.Exception.InnerException);
    }
  }
}

