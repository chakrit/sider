
using System.Diagnostics;

namespace Sider.Benchmark
{
  public abstract class Job
  {
    public RedisClient Client { get; set; }
    public abstract string Description { get; }


    public virtual void Setup() { }

    public abstract void RunOneIteration();

    public virtual void Teardown() { }


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
