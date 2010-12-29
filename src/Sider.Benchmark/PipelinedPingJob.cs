
using System;
using System.Linq;

namespace Sider.Benchmark
{
  public class PipelinedPingJob : Job
  {
    public override string Description
    {
      get { return "Pipelined PINGs"; }
    }


    public override void Run(int iterations)
    {
      var result = Client.Pipeline(c =>
      {
        for (int i = 0; i < iterations; i++)
          c.Ping();
      }).ToArray();

      Assert(Array.TrueForAll(result, obj => (bool)obj));
    }
  }
}
