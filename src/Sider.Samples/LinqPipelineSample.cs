
using System;
using System.Linq;

namespace Sider.Samples
{
  public class LinqPipelineSample : Sample
  {
    public const int CounterAmount = 1000;


    public override string Name
    {
      get { return "Linq + Pipelining."; }
    }

    public override void Run()
    {
      var client = new RedisClient<int>(RedisSettings.Default);

      // NOTE: This is for demonstartion purpose only
      //       real-world code should just use MGet and MSet which are made
      //       specifically for tasks like this

      // initialize 1000 counters with a number
      var success = client
        .Pipeline(c =>
        {
          var rand = new Random();
          for (var i = 0; i < CounterAmount; i++)
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
