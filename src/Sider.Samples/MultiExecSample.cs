
using System.Linq;
using System.Threading;

namespace Sider.Samples
{
  public class MultiExecSample : Sample
  {
    private string[] _keys = Enumerable
      .Range(0, 10)
      .Select(n => "C" + n.ToString())
      .ToArray();

    public override string Name { get { return "Multi/Exec Usage"; } }

    public override void Run()
    {
      var logThread = new Thread(logger);
      var incrThread = new Thread(incrementor);

      WriteLine("If MULTI EXEC is working values on the same line should" +
        " always equal since INCR is done inside a MULTI/EXEC block");

      logThread.Start();
      incrThread.Start();

      // hangs
      logThread.Join();
      incrThread.Join();
    }

    private void logger()
    {
      var client = new RedisClient();

      // log the value of all keys at any instant every 1sec
      while (true) {
        WriteLine(string.Join(" ", client.MGet(_keys)));
        Thread.Sleep(1000);
      }
    }

    private void incrementor()
    {
      var client = new RedisClient();

      // increment all keys at once
      while (true) {
        client.Multi();
        foreach (var key in _keys)
          client.Incr(key);
        client.Exec();
        Thread.Sleep(250);
      }
    }
  }
}
