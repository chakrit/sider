
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Concurrency;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Sider.Samples
{
  public class HighLoadSample : Sample
  {
    public const int MaxClients = 300;
    public const int ClientsStepping = 2;
    public const int SteppingDelay = 1500;

    public const double DyingProb = 0.01;
    public const int ClientLifetime = 10000;

    public const int HitDelay = 0;
    public const double IdleClientProb = 0.01;
    public const int IdleClientTime = 10000;

    public const int PingStatInterval = SteppingDelay;
    public const int PingStatWindow = 100;


    private IDictionary<int, Thread> _threads =
      new ConcurrentDictionary<int, Thread>();


    public override string Name
    {
      get { return "Load (" + MaxClients.ToString() + " threads repeatedly GET/SET)"; }
    }

    public override void Run()
    {
      var clientsPool = new ThreadwisePool();
      var clientsCount = 0;

      // display ping times continuously
      var pingTimes = pingThread(clientsPool)
        .ToObservable()
        .ObserveOn(Scheduler.NewThread)
        .SubscribeOn(Scheduler.CurrentThread)
        .BufferWithTime(TimeSpan.FromMilliseconds(PingStatInterval))
        .Select(times => times.Reverse().Take(PingStatWindow).Average())
        .Subscribe(logPingTime);

      // begin spawing clients
      while (clientsCount < MaxClients) {
        var diff = (clientsCount * ClientsStepping) - clientsCount;
        diff = Math.Max(1, Math.Min(diff, MaxClients - clientsCount));

        GC.Collect();
        spawnThreads(clientsPool, diff);
        clientsCount += diff;

        Thread.Sleep(SteppingDelay);
      }

      WriteLine("Please restart the app to stop.");
      ReadLine();
    }


    private IEnumerable<double> pingThread(IClientsPool pool)
    {
      while (true) {
        var client = pool.GetClient();

        var startTime = DateTime.Now;
        Trace.Assert(client.Ping());
        var endTime = DateTime.Now;

        yield return (endTime - startTime).TotalMilliseconds;
      }
    }


    private void spawnThreads(IClientsPool pool, int count)
    {
      for (var i = 0; i < count; i++)
        newThread(pool).Start();

      logClientsCount();
    }

    private Thread newThread(IClientsPool pool)
    {
      Thread t = null;
      t = new Thread(_ =>
      {
        Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

        var hitCounter = 0;
        var client = pool.GetClient();
        var rand = new Random();

        while (hitCounter < ClientLifetime) {
          Thread.Sleep(HitDelay);

          // simulate idle clients
          if (rand.NextDouble() < IdleClientProb) {
            Thread.Sleep(IdleClientTime);
            continue;
          }

          // simulate dying clients
          if (rand.NextDouble() < DyingProb)
            break;

          var key = getRandomKey();
          var value = getRandomValue();

          client.Set(key, value);
          Trace.Assert(client.Get(key) == value);
          client.Del(key);

          hitCounter++;
        }

        // self-restart;
        _threads.Remove(t.ManagedThreadId);
        newThread(pool).Start();
      });

      _threads.Add(t.ManagedThreadId, t);
      return t;
    }


    private string getRandomKey() { return Guid.NewGuid().ToString(); }
    private string getRandomValue() { return Guid.NewGuid().ToString(); }


    private void logPingTime(double milliseconds)
    {
      WriteLine("PING: " + milliseconds.ToString() + "ms");
    }

    private void logClientsCount()
    {
      WriteLine("Running threads/clients: " + _threads.Count.ToString());
    }

  }
}
