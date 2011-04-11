
using System.Diagnostics;

namespace Sider.Benchmark
{
  public abstract class Job
  {
    // for jobs which could interfere each other when run in parallel
    private static int _instanceCount = 0;


    public int InstanceNumber { get; private set; }
    public abstract string Description { get; }

    public IRedisClient<string> Client { get; set; }

    public Job()
    {
      InstanceNumber = _instanceCount++;
    }


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
