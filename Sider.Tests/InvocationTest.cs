using System;
using NUnit.Framework;
using System.Runtime.Remoting.Messaging;
using System.CodeDom;

namespace Sider.Tests {
  [TestFixture]
  public class InvocationTest : SiderTestBase {
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
  }
}

