
using System;
using System.Linq;

namespace Sider.Samples
{
  public class ClientListSample : Sample
  {
    public override string Name { get { return "CLIENT LIST"; } }

    public override void Run()
    {
      using (var client = new RedisClient()) {
        var lines = client
          .ClientList()
          .Select((dict, i) => string.Format("\r\nClient #{0}\r\n{1}", i, string.Join("", dict
            .Select(kv => string.Format("{0,30} : {1,30}", kv.Key, kv.Value))
            .ToArray())))
          .ToArray();

        Array.ForEach(lines, WriteLine);
      }
    }
  }
}
