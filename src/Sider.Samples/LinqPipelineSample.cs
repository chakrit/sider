
using System;
using System.Linq;

namespace Sider.Samples
{
  public class LinqPipelineSample : Sample
  {
    public override string Name
    {
      get { return "Linq + Pipelining."; }
    }

    public override void Run()
    {
      var client = new RedisClient<int>(RedisSettings.Default);

      // initialize 1000 counters with a number
      var success = client
        .Pipeline(c =>
        {
          var rand = new Random();
          for (var i = 0; i < 1000; i++)
            c.Set("counter" + i.ToString(), rand.Next(100, 999));
        })
        .Cast<bool>()
        .All(b => b);

      WriteLine(success ?
        "All counters set to [100,999]." :
        "Unable to set some counters");

      var verified = client
        .Pipeline(c => Enumerable
          .Range(0, 1000)
          .Select(n => c.Get("counter" + n.ToString()))
          .AsEnumerable())
        .Cast<int>()
        .All(n => n >= 100 && n < 1000);

      WriteLine(verified ?
        "Counter values verified." :
        "Some counters are invalid.");

    }
  }
}
