
namespace Sider.Benchmark
{
  public class SetWithStringSerializerJob : Job
  {
    public static readonly string TestString = new string('x', 1024);


    public override string Description
    {
      get
      {
        return string.Format(
          "Repeated SET with {0} characters string using the StringSerializer.",
          TestString.Length);
      }
    }


    public override void RunOneIteration()
    {
      Client.Set("test_key", TestString);
    }

    public override void AfterBenchmark()
    {
      Client.Del("test_key");
    }
  }
}
