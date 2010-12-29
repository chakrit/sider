
using System.Diagnostics;

namespace Sider.Benchmark
{
  public abstract class Job
  {
    public IRedisClient Client { get; set; }
    public abstract string Description { get; }


    public virtual void BeforeBenchmark() { }

    public virtual void Run(int iterations)
    {
      for (var i = 0; i < iterations; i++)
        RunOneIteration();
    }

    public virtual void RunOneIteration() { }

    public virtual void AfterBenchmark() { }


    [DebuggerStepThrough]
    protected void Assert(bool condition)
    {
      if (!condition)
        Trace.Fail("Assertion failed for the running benchmark job.");
    }


    public override string ToString()
    {
      return Description;
    }
  }
}
