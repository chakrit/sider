
using System;
using NUnit.Framework;

namespace Sider.Tests
{
  public class RedisSettingsTests : SiderTestBase
  {
    protected RedisSettings.Builder Builder { get; private set; }

    [SetUp]
    public void Init() { Builder = RedisSettings.New(); }


    [Test]
    public void Ctor_HostIsNullOrEmpty_ExpceptionThrown()
    {
      Assert.Throws<ArgumentException>(() => Builder.Host(null));
      Assert.Throws<ArgumentException>(() => Builder.Host(""));
    }

    [Test]
    public void Ctor_PortOutOfRange_ExceptionThrown()
    {
      throwsOutOfRange(() => Builder.Port(int.MinValue));
      throwsOutOfRange(() => Builder.Port(0)); // min - 1
      throwsOutOfRange(() => Builder.Port(65536)); // max + 1
      throwsOutOfRange(() => Builder.Port(int.MaxValue));
    }

    [Test]
    public void Ctor_TooLowReadBufferSize_ExceptionThrown()
    {
      throwsOutOfRange(() => Builder.ReadBufferSize(0));
      throwsOutOfRange(() => Builder.ReadBufferSize(int.MinValue));
    }

    [Test]
    public void Ctor_TooLowWriteBufferSize_ExceptionThrown()
    {
      throwsOutOfRange(() => Builder.WriteBufferSize(0));
      throwsOutOfRange(() => Builder.WriteBufferSize(int.MinValue));
    }

    [Test]
    public void Ctor_NullKeyEncoding_ExceptionThrown()
    {
      Assert.Throws<ArgumentNullException>(() => Builder.KeyEncoding(null));
    }

    [Test]
    public void Ctor_NullValueEncoding_ExceptionThrown()
    {
      Assert.Throws<ArgumentNullException>(() => Builder.ValueEncoding(null));
    }


    private void throwsOutOfRange(TestDelegate del)
    {
      Assert.Throws<ArgumentOutOfRangeException>(del);
    }
  }
}
