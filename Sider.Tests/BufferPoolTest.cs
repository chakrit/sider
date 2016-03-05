using System;
using NUnit.Framework;
using Sider;

namespace Sider.Tests {
  [TestFixture]
  public class BufferPoolTest : SiderTest {
    [Test]
    public void TestCtor() {
      Assert.Throws<ArgumentNullException>(() => new BufferPool(null));
      Assert.DoesNotThrow(() => new BufferPool(RedisSettings.Default));
    }

    [Test]
    public void TestUse() {
      var pool = DefaultPool();
      Assert.Throws<ArgumentNullException>(() => pool.Use(1024, null));
      Assert.Throws<ArgumentOutOfRangeException>(() => pool.Use(-1, b => {
        // no-op
      }));

      Assert.Throws<TestException>(() => pool.Use(1024, buffer => {
        throw new TestException("should be propagated.");
      }));
        
      var returnValue = pool.Use(1024, buffer => {
        Assert.IsNotNull(buffer);
        Assert.AreEqual(1024, buffer.Length);
        return "Hello";
      });

      Assert.AreEqual("Hello", returnValue);
    }

    [Test, Timeout(1000)]
    public void TestUseConstrained() {
      var pool = ConstrainedPool();
      pool.Use(1024, b1 => pool.Use(1024, b2 => pool.Use(1024, b3 =>
        pool.Use(1024, b4 => pool.Use(1024, b5 => pool.Use(1024, b6 => {            
        var buffers = new[] { b1, b2, b3, b4, b5, b6 };
        for (var i = 1; i < buffers.Length; i++) {
          Assert.AreNotSame(buffers[0], buffers[i]);
        }

        foreach (var buffer in buffers) {
          Assert.AreEqual(1024, buffer.Length);
        }
      }))))));
    }

    BufferPool DefaultPool() {
      return new BufferPool(RedisSettings.Default);
    }

    BufferPool ConstrainedPool() {
      return new BufferPool(new RedisSettings.Builder()
        .MaxBufferPoolSize(4096)
        .MaxBufferSize(1024)
        .Build());
    }
  }
}

