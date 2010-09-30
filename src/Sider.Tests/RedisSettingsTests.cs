
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sider.Tests
{
  [TestClass]
  public class RedisSettingsTests : SiderTestBase
  {
    [TestMethod]
    public void Ctor_HostIsNullOrEmpty_ExpceptionThrown()
    {
      Expect<ArgumentException>(() => new RedisSettings(host: null));
      Expect<ArgumentException>(() => new RedisSettings(host: ""));
    }

    [TestMethod]
    public void Ctor_PortOutOfRange_ExceptionThrown()
    {
      Expect<ArgumentOutOfRangeException>(() =>
        new RedisSettings(port: int.MinValue));
      Expect<ArgumentOutOfRangeException>(() =>
        new RedisSettings(port: 0)); // min port - 1
      Expect<ArgumentOutOfRangeException>(() =>
        new RedisSettings(port: 65536)); // max port + 1
      Expect<ArgumentOutOfRangeException>(() =>
        new RedisSettings(port: int.MaxValue));
    }

    [TestMethod]
    public void Ctor_TooLowReadBufferSize_ExceptionThrown()
    {
      Expect<ArgumentOutOfRangeException>(() =>
        new RedisSettings(readBufferSize: 0));
      Expect<ArgumentOutOfRangeException>(() =>
        new RedisSettings(readBufferSize: int.MinValue));
    }

    [TestMethod]
    public void Ctor_TooLowWriteBufferSize_ExceptionThrown()
    {
      Expect<ArgumentOutOfRangeException>(() =>
        new RedisSettings(writeBufferSize: 0));
      Expect<ArgumentOutOfRangeException>(() =>
        new RedisSettings(writeBufferSize: int.MinValue));
    }
  }
}
