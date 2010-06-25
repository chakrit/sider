
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Sider.Tests
{
  [TestClass]
  public class BasicClientTests
  {
    private RedisClient getClient() { return new RedisClient(); }


    [TestMethod]
    public void ShouldConnectAndDisconnect()
    {
      var client = getClient();
      client.Dispose();
    }

    [TestMethod]
    public void ShouldPing()
    {
      using (var client = getClient()) {
        Assert.IsTrue(client.Ping());
      }
    }

    //[TestMethod, Ignore]
    //public void GetNonExistentShouldReturnEmptyBuffer()
    //{
    //  using (var client = getClient()) {
    //    var result = client.Get("RandomKeyLoremIpsum");

    //    Assert.AreEqual(result.Length, 0);
    //  }
    //}

    //[TestMethod, Ignore]
    //public void SimpleGetSetShouldWorks()
    //{
    //  var key = "MyKey";
    //  var data = "The quick brown fox jumps over the lazy dog";
    //  var dataBytes = Encoding.ASCII.GetBytes(data);

    //  using (var client = getClient()) {
    //    client.Set(key, dataBytes);
    //    var resultBytes = client.Get(key);

    //    CollectionAssert.AreEquivalent(dataBytes, resultBytes);
    //  }
    //}
  }
}
