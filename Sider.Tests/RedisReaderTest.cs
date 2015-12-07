using System;
using NUnit.Framework;
using System.IO;

namespace Sider.Tests {
  [TestFixture]
  public class RedisReaderTest : SiderTestBase {
    [Test]
    public void TestCtor() {
      var ms = new MemoryStream();
      var settings = RandomSettings();

      Assert.Throws<ArgumentNullException>(() => new RedisReader(null, null));
      Assert.Throws<ArgumentNullException>(() => new RedisReader(ms, null));
      Assert.Throws<ArgumentNullException>(() => new RedisReader(null, settings));

      Assert.DoesNotThrow(() => new RedisReader(ms, settings));
    }
  }
}

