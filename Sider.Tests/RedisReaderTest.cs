using System;
using NUnit.Framework;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Sider.Tests {
  [TestFixture]
  public class RedisReaderTest : SiderTest {
    [Test]
    public void TestCtor() {
      Assert.Throws<ArgumentNullException>(() => new RedisReader(null));
    }

    [Test]
    public void TestReadCRLF() {
      AssertProtocolException("notCRLF", r => r.ReadCRLF());

      using (var reader = BuildReader("\r\n")) {
        reader.ReadCRLF(); // assert not throws.
      }
    }

    [Test]
    public void TestReadType() {
      var typeMaps = new Dictionary<string, ResponseType>
      {
        { "-", ResponseType.Error },
        { "+", ResponseType.SingleLine },
        { "$", ResponseType.Bulk },
        { "*", ResponseType.MultiBulk },
        { ":", ResponseType.Integer },
      };

      AssertProtocolException("x", r => r.ReadType());
      foreach (var pair in typeMaps) {
        AssertReads(pair.Key, pair.Value, r => r.ReadType());
      }
    }

    [Test]
    public void TestReadStatusLine() {
      var badLines = new string[]
      {
        "not-crlf-terminated",
        "partial-crlf\r",
        "partial-crlf\n",
        "bad-crlf\n\r",
      };

      foreach (var line in badLines) {
        AssertProtocolException(line, r => r.ReadStatusLine());
      }

      AssertReads("status line\r\n", "status line", r => r.ReadStatusLine());
    }

    [Test]
    public void TestReadNumberLine() {
      var badLines = new string[]
      {
        "not-number\r\n",
        "64\r",
        "64\n",
        "64\n\r",
      };

      foreach (var line in badLines) {
        AssertProtocolException(line, r => r.ReadNumberLine());
      }

      var goodLines = new Dictionary<string, long>
      {
        { "64\r\n", 64 },
        { "-64\r\n", -64 },
        { "+00001\r\n", 1 },
        { "-0003\r\n", -3 },
        { "\r\n", 0 },
      };

      foreach (var pair in goodLines) {
        AssertReads(pair.Key, pair.Value, r => r.ReadNumberLine());
      }
    }

    [Test, Timeout(2000)]
    public void TestReadBulk() {
      Assert.Throws<ArgumentNullException>(() => {
        using (var reader = BuildReader("asdf")) {
          reader.ReadBulk(null, 0, 0);
        }
      });

      var badArgs = new Action<RedisReader>[]
      {
        r => r.ReadBulk(new byte[] { }, -1, 0),
        r => r.ReadBulk(new byte[] { }, 0, -1),
        r => r.ReadBulk(new byte[16], 17, 0),
        r => r.ReadBulk(new byte[16], 16, 1),
        r => r.ReadBulk(new byte[16], 0, 17),
        r => r.ReadBulk(new byte[16], 15, 15),
      };

      foreach (var call in badArgs) {
        Assert.Throws<ArgumentException>(() => {
          using (var reader = BuildReader("asdf")) {
            call(reader);
          }
        });
      }

      // TODO: Needs timeout handling machinery.
//      Assert.Throws<EndOfStreamException>(() => {
//        using (var reader = BuildReader("short")) {
//          reader.ReadBulk(new byte[16], 0, 16);
//        }
//      });

      var reads = new Tuple<string, int, string>[]
      {
        Tuple.Create("one-item", 3, "one"),
        Tuple.Create("one-item", 8, "one-item")
      };

      var buffer = new byte[16];
      foreach (var read in reads) {
        using (var reader = BuildReader(read.Item1)) {
          var bytesRead = reader.ReadBulk(buffer, 0, read.Item2);
          Assert.AreEqual(read.Item2, bytesRead);

          var result = Encoding.UTF8.GetString(buffer, 0, bytesRead);
          Assert.AreEqual(read.Item3, result);
        }
      }
    }

    protected RedisReader BuildReader(string input) {
      var buffer = Encoding.UTF8.GetBytes(input);
      var stream = new MemoryStream(buffer);
      return new RedisReader(stream);
    }

    protected void AssertProtocolException(string inputLine, Action<RedisReader> readAction) {
      Assert.Throws<ProtocolException>(() => {
        using (var reader = BuildReader(inputLine)) {
          readAction(reader);
        }
      });
    }

    protected T AssertReads<T>(string inputLine, T expectedOutput, Func<RedisReader, T> readAction) {
      using (var reader = BuildReader(inputLine)) {
        var result = readAction(reader);
        Assert.AreEqual(expectedOutput, result);
        return result;
      }
    }
  }
}

