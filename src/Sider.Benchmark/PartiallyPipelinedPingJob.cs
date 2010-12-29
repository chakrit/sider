
namespace Sider.Benchmark
{
  public class PartiallyPipelinedPingJob : Job
  {
    public override string Description
    {
      get { return "Batch of 5 pipelined PINGS"; }
    }


    public override void Run(int iterations)
    {
      for (var i = 0; i < iterations; i += 5) {
        var result = Client.Pipeline(
          c => c.Ping(),
          c => c.Ping(),
          c => c.Ping(),
          c => c.Ping(),
          c => c.Ping());

        Assert(result.Item1 && result.Item2 &&
          result.Item3 && result.Item4 &&
          result.Item5);
      }
    }
  }
}
