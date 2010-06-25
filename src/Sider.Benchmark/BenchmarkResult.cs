
using System;

namespace Sider.Benchmark
{
  public struct BenchmarkResult
  {
    public Job Job { get; set; }

    public int Iterations { get; set; }
    public long MillisecondsTaken { get; set; }
  }
}
