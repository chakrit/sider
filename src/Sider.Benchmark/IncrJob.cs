
namespace Sider.Benchmark
{
  public class IncrJob : Job
  {
    private string _key;

    public override string Description
    {
      get { return "Test number parsing with INCR"; }
    }


    public IncrJob()
    {
      _key = "NUM_" + InstanceNumber.ToString();
    }


    public override void RunOneIteration()
    {
      Client.Incr(_key);
    }

    public override void AfterBenchmark()
    {
      Client.Del(_key);
    }
  }
}
