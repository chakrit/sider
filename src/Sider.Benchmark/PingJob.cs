
namespace Sider.Benchmark
{
  public class PingJob : Job
  {
    public override string Description
    {
      get { return "Simple PING"; }
    }


    public override void RunOneIteration()
    {
      Assert(Client.Ping());
    }
  }
}
