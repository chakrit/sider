
using System;
using NUnit.Framework;

namespace Sider.Tests
{
  public class RedisSettingsTests : SiderTestBase
  {
    [Test]
    public void Ctor_HostIsNullOrEmpty_ExpceptionThrown()
    {
      Assert.Throws<ArgumentException>(() => new RedisSettings(host: null));
      Assert.Throws<ArgumentException>(() => new RedisSettings(host: ""));
    }

    [Test]
    public void Ctor_PortOutOfRange_ExceptionThrown()
    {
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new RedisSettings(port: int.MinValue));
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new RedisSettings(port: 0)); // min port - 1
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new RedisSettings(port: 65536)); // max port + 1
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new RedisSettings(port: int.MaxValue));
    }

    [Test]
    public void Ctor_TooLowReadBufferSize_ExceptionThrown()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() =>
        new RedisSettings(readBufferSize: 0));
      Assert.Throws<ArgumentOutOfRangeException>(() =>
        new RedisSettings(readBufferSize: int.MinValue));
    }

    [Test]
    public void Ctor_TooLowWriteBufferSize_ExceptionThrown()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() =>
        new RedisSettings(writeBufferSize: 0));
      Assert.Throws<ArgumentOutOfRangeException>(() =>
        new RedisSettings(writeBufferSize: int.MinValue));
    }
  }
}
