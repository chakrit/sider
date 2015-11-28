using NUnit.Framework;
using Sider;
using System;
using System.Runtime.Remoting;

namespace Sider.Tests {
  [TestFixture()]
  public class RedisSettingsTest : SiderTestBase {
    [Test()]
    public void TestDefault() {
      var def = RedisSettings.Default;
      var ctor = new RedisSettings();

      Assert.IsNotNull(def);
      Assert.IsNotNull(ctor);
      Assert.AreEqual(def.Host, ctor.Host);
      Assert.AreEqual(def.Port, ctor.Port);
    }

    [Test()]
    public void TestCloneable() {
      var s1 = RandomInstance();
      var s2 = ((ICloneable)s1).Clone();
      Assert.AreEqual(s1, s2);
    }

    [Test()]
    public void TestBuilder() {
      var b1 = RedisSettings.Default.Build();
      Assert.IsNotNull(b1);

      var host = RandomString();
      b1 = b1.Host(host);
      Assert.IsNotNull(b1);
      Assert.AreEqual(host, b1.Build().Host);
    }

    [Test()]
    public void TestEquals() {
      var host = RandomString();
      var s1 = new RedisSettings.Builder().Host(host).Build();
      var s2 = new RedisSettings.Builder().Host(host).Build();
      Assert.AreEqual(s1, s2);
    }

    RedisSettings RandomInstance() {
      return new RedisSettings.Builder()
        .Host(RandomString())
        .Port(RandomInt())
        .Build();
    }
  }
}

