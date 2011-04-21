
using System;
using System.Linq;

namespace Sider.Samples
{
  public class InfoSample : Sample
  {
    public override string Name { get { return "INFO"; } }

    public override void Run()
    {
      using (var client = new RedisClient()) {
        var lines = client
          .Info()
          .Select(kv => string.Format("{0,30} : {1,30}", kv.Key, kv.Value))
          .ToArray();

        Array.ForEach(lines, WriteLine);
      }

      ReadLine();
    }
  }
}
