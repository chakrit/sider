
using System;

namespace Sider.Samples
{
  public class MonitorSample : Sample
  {
    public override string Name { get { return "MONITOR with IObservable."; } }


    public override void Run()
    {
      WriteLine("Hit ENTER to stop monitoring.");

      var session = new RedisClient()
        .Monitor()
        .Subscribe(WriteLine);

      ReadLine();
      session.Dispose();
    }
  }
}
