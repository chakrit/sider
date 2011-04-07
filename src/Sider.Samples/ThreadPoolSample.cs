
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

namespace Sider.Samples
{
  public class ThreadPoolSample : Sample
  {
    public override string Name
    {
      get { return "Thread pool usage."; }
    }

    public override void Run()
    {
      var pool = new ThreadwisePool();

      // setup a bag of keys to randomly increments
      var keys = new[] { "counter1", "counter2", "counter3", "counter4" };
      var keysBag = new ConcurrentBag<string>();

      for (var i = 0; i < 250; i++)
        foreach (var key in keys)
          keysBag.Add(key);

      // increment keys from multiple thread
      Log("Incrementing keys: " + string.Join(", ", keys));
      Task.WaitAll(Enumerable
        .Range(0, 1000)
        .Select(n => Task.Factory.StartNew(() =>
        {
          // obtain a key to increment
          string key;
          if (!keysBag.TryTake(out key))
            Log("ERROR");

          // obtain an IRedisClient from the pool
          // and use it to INCR a key
          pool.GetClient().Incr(key);
        }))
        .ToArray());

      // log result
      var client = pool.GetClient();
      Log("Result (should be equal):");
      foreach (var key in keys)
        Log(key + " : " + client.Get(key));
    }
  }
}
