
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Scheduler = System.Reactive.Concurrency.Scheduler;

namespace Sider.Samples
{
  public class HighLoadSample : Sample
  {
    public const int MaxClients = 128;
    public const int ClientsStepping = 2;
    public const int SteppingDelay = 500;

    public const double DyingSimProb = 0.00;
    public const int ClientLifetime = int.MaxValue;

    public const int HitDelay = 0;
    public const double IdleClientProb = 0.01;
    public const int IdleClientTime = 7000;

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
      var clientsPool = new ThreadwisePool<string>();
      var clientsCount = 0;

      // display ping times continuously
      var pingTimes = pingThread(clientsPool)
        .ToObservable()
        .ObserveOn(Scheduler.NewThread)
        .SubscribeOn(Scheduler.CurrentThread)
        .Sample(TimeSpan.FromMilliseconds(PingStatInterval / PingStatWindow))
        .Buffer(TimeSpan.FromMilliseconds(PingStatInterval))
        .Select(times => times.DefaultIfEmpty(0.0).Average())
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

      // stall
      WriteLine("Spawned all clients. To stop, close this application.");
      Thread.Sleep(Timeout.Infinite);
    }


    private IEnumerable<double> pingThread(IClientsPool<string> pool)
    {
      while (true) {
        double result;

        try {
          var client = pool.GetClient();

          var startTime = DateTime.Now;
          Trace.Assert(client.Ping());
          var endTime = DateTime.Now;

          result = (endTime - startTime).TotalMilliseconds;
        }
        catch (Exception ex) {
          logException(ex);
          result = 0.0;
        }

        yield return result;
      }
    }


    private void spawnThreads(IClientsPool<string> pool, int count)
    {
      for (var i = 0; i < count; i++)
        newThread(pool).Start();

      logClientsCount();
    }

    private Thread newThread(IClientsPool<string> pool)
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
          if (rand.NextDouble() < DyingSimProb)
            break;

          var key = getRandomKey();
          var value = getRandomValue();

          try {
            client.Set(key, value);
            Trace.Assert(client.Get(key) == value);
            client.Del(key);
          }
          catch (Exception ex) { logException(ex); }

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

    private void logException(Exception ex)
    {
      var msg = "Unhandled exception : " + ex.GetType().Name;
      if (ex.InnerException != null)
        msg += "\r\n    inner exception : " + ex.InnerException.GetType().Name;

      WriteLine(msg);
    }

  }
}
