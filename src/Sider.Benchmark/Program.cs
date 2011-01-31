
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sider.Benchmark
{
  internal class Program
  {
    public static void Main(string[] args) { (new Program()).Run(); }


    private RedisSettings _settings;
    private IClientsPool _pool;

    public void Run()
    {
      // configure benchmark parameters here:
      var instances = 3;
      var iterations = 10;

      Func<Job> getJob = () => new MultiplePipelinedResultTypeJob();

      _settings = new RedisSettings(
      reconnectOnIdle: false,
      reissueWriteOnIdle: false);

      _pool = new RoundRobinPool(_settings, instances);


      while (true) {

        // setup
        var options = TaskCreationOptions.LongRunning;

        var tasks = Enumerable
          .Range(0, instances)
          .Select(n => getJob())
          .Select(job => new Task<BenchmarkResult>(
            () => benchMark(job, iterations), options))
          .ToArray();

        Console.WriteLine("{0} instances of `{1}` each doing {2} iterations.",
          tasks.Count(),
          getJob().Description,
          iterations);

        Console.WriteLine("Hit any key to start.");
        Console.ReadKey();

        // starting
        if (instances == 1)
          tasks.First().RunSynchronously();
        else
          Array.ForEach(tasks, t => t.Start());

        Task.WaitAll(tasks);
        Console.WriteLine("All done.");

        // report time taken
        var results = tasks.Select(t => t.Result);

        foreach (var result in results) {
          Console.WriteLine("{0} : {1}ms",
            result.Job.Description,
            result.MillisecondsTaken);
        }

        var avg = results.Average(r => r.MillisecondsTaken);
        var max = results.Max(r => r.MillisecondsTaken);
        var min = results.Min(r => r.MillisecondsTaken);

        Console.WriteLine("Maximum : {0}ms", max);
        Console.WriteLine("Minimum : {0}ms", min);
        Console.WriteLine("Average : {0}ms", avg);

        Console.ReadKey();

      }
    }


    private BenchmarkResult benchMark(Job job, int iterations)
    {
      var client = job.Client =
        _pool == null ? new RedisClient() : _pool.GetClient();
      var sw = new Stopwatch();

      // setop
      sw.Reset();
      job.BeforeBenchmark();

      // run the benchmark
      sw.Start();
      job.Run(iterations);
      sw.Stop();

      // cleanup
      job.AfterBenchmark();
      job.Client = null;

      // dispose client if not managed by a pool
      if (_pool == null) client.Dispose();

      return new BenchmarkResult {
        Job = job,
        Iterations = iterations,
        MillisecondsTaken = sw.ElapsedMilliseconds
      };
    }
  }
}
