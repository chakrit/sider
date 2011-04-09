
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Sider.Samples
{
  public class ThreadPoolSample : Sample
  {
    public const int TotalTasks = 100000;
    public const int TotalKeys = 10;


    public override string Name
    {
      get { return "Thread pool usage."; }
    }

    public override void Run()
    {
      var pool = new ThreadwisePool();
      var mainClient = pool.GetClient();

      // setup a bag of keys to randomly increments
      var keys = Enumerable
        .Range(0, TotalKeys)
        .Select(k => "counter" + k.ToString())
        .ToArray();
      var keysBag = new ConcurrentBag<string>();

      var keySets = TotalTasks / TotalKeys;
      for (var i = 0; i < keySets; i++)
        foreach (var key in keys)
          keysBag.Add(key);

      // ensure key is empty
      foreach (var key in keys)
        mainClient.Del(key);

      // increment keys from multiple thread
      WriteLine("Incrementing keys: " + string.Join(", ", keys));
      Task.WaitAll(Enumerable
        .Range(0, TotalTasks)
        .Select(n => Task.Factory.StartNew(() =>
        {
          // obtain a key to increment
          string key;
          if (!keysBag.TryTake(out key))
            WriteLine("ERROR");

          // obtain an IRedisClient from the pool
          // and use it to INCR a key
          pool.GetClient().Incr(key);
        }))
        .ToArray());

      // log result
      WriteLine("Result (should be equal):");
      foreach (var key in keys)
        WriteLine(key + " : " + mainClient.Get(key));
    }
  }
}
