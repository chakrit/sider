using System;
using NUnit.Framework;
using System.IO;
using System.Configuration;

namespace Sider.Tests {
  [TestFixture]
  public class StreamWrapperTest : SiderTestBase {
    [Test]
    public void TestCtor() {
      var ms = new MemoryStream();
      var settings = RandomSettings();

      Assert.Throws<ArgumentNullException>(() => new DummyWrapper(null, null));
      Assert.Throws<ArgumentNullException>(() => new DummyWrapper(ms, null));
      Assert.Throws<ArgumentNullException>(() => new DummyWrapper(null, settings));

      var wrapper = new DummyWrapper(ms, settings);
      Assert.AreEqual(ms, wrapper.Stream);
      Assert.AreEqual(settings, wrapper.Settings);
    }

    class DummyWrapper : StreamWrapper {
      public new Stream Stream { get { return base.Stream; }}
      public new RedisSettings Settings { get { return base.Settings; } }

      public DummyWrapper(Stream stream, RedisSettings settings)
        : base(stream, settings) {
      }

      public override void Dispose() {
      }
    }
  }
}

