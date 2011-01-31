
using System.Diagnostics;
using System.Linq;

namespace Sider.Benchmark
{
  public class MultiplePipelinedResultTypeJob : Job
  {
    private string _numKey;
    private string _strKey;


    public override string Description
    {
      get { return "Pipeline with multiple return types."; }
    }

    public MultiplePipelinedResultTypeJob()
    {
      _numKey = "NUM" + InstanceNumber.ToString();
      _strKey = "HELLO" + InstanceNumber.ToString();
    }


    public override void BeforeBenchmark()
    {
      Debug.WriteLine(InstanceNumber.ToString() + ": BeforeBenchmark");
    }

    public override void RunOneIteration()
    {
      Debug.WriteLine(InstanceNumber.ToString() + ": RunOneIteration");

      var result = Client.Pipeline(c =>
      {
        for (var i = 0; i < 100; i++)
          c.Set(_strKey + i.ToString(), "WORLD" + i.ToString());

        c.Set(_numKey, "0");
        for (var i = 1; i < 100; i++)
          c.Incr(_numKey);

        for (var i = 0; i < 100; i++)
          c.Get(_strKey + i.ToString());
      });

      var arr = result.ToArray();
      for (var i = 0; i < 100; i++) // verify 100 SETs
        Assert(arr[i] is bool && (bool)arr[i]);

      Assert(arr[100] is bool && (bool)arr[100]); // verify num=0 SET
      for (var i = 1; i < 100; i++) // verify INCRs
        Assert(arr[i + 100] is long && (long)arr[i + 100] == i);

      for (var i = 0; i < 100; i++) // verify GETs
        Assert(arr[i + 200] is string &&
          (string)arr[i + 200] == "WORLD" + i.ToString());
    }

    public override void AfterBenchmark()
    {
      Client.Pipeline(c =>
      {
        c.Del(_numKey);
        for (var i = 0; i < 100; i++)
          c.Del(_strKey + i.ToString());
      });
    }
  }
}
