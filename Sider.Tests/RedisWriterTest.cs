using NUnit.Framework;
using System.IO;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Sider.Tests {
  [TestFixture]
  public class RedisWriterTest : SiderTest {
    [Test]
    public void TestCtor() {
      var ms = new MemoryStream();
      var settings = RandomSettings();

      Assert.Throws<ArgumentNullException>(() => new RedisWriter(null, null));
      Assert.Throws<ArgumentNullException>(() => new RedisWriter(ms, null));
      Assert.Throws<ArgumentNullException>(() => new RedisWriter(null, settings));

      Assert.DoesNotThrow(() => new RedisWriter(ms, settings));
    }

    [Test]
    public void TestWriteType() {
      AssertProtocolException(w => w.WriteType((ResponseType)'X'));
      foreach (var pair in RedisReaderTest.ResponseTypes) {
        AssertWrite(pair.Key, w => w.WriteType(pair.Value));
      }
    }

    [Test]
    public void TestWriteStringLine() {
      Assert.Throws<ArgumentNullException>(() => {
        using (var writer = BuildWriter()) {
          writer.WriteStringLine(null);
        }
      });

      Assert.Throws<ArgumentNullException>(() => {
        using (var writer = BuildWriter()) {
          writer.WriteStringLine("");
        }
      });
        
      AssertWrite("line\r\n", w => w.WriteStringLine("line"));
    }
      
    protected RedisWriter BuildWriter() {
      return BuildWriter(new MemoryStream());
    }

    protected RedisWriter BuildWriter(Stream stream) {
      return new RedisWriter(stream, RedisSettings.Default);
    }

    protected void AssertWrite(string output, Action<RedisWriter> writeAction) {
      var ms = new MemoryStream();
      using (var writer = BuildWriter(ms)) {
        writeAction(writer);

        var result = Encoding.UTF8.GetString(ms.ToArray());
        Assert.AreEqual(output, result);
      }
    }

    protected void AssertProtocolException(Action<RedisWriter> writeAction) {
      Assert.Throws<ProtocolException>(() => {
        using (var writer = BuildWriter()) {
          writeAction(writer);
        }
      });
    }
  }
}

