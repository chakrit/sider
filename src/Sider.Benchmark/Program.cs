
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
    private IClientsPool<string> _pool;

    public void Run()
    {
      // configure benchmark parameters here:
      const int Instances = 7;
      const int Iterations = 10000;
      const bool QuietMode = false; // turn to false to make it wait for key inputs

      Func<Job> getJob = () => new MultiplePipelinedResultTypeJob();

      _settings = RedisSettings.New()
        .ReconnectOnIdle(false);

      _pool = new RoundRobinPool<string>(_settings, Instances);


      do {

        // setup
        var options = TaskCreationOptions.LongRunning;

        var tasks = Enumerable
          .Range(0, Instances)
          .Select(n => getJob())
          .Select(job => new Task<BenchmarkResult>(
            () => benchMark(job, Iterations), options))
          .ToArray();

        if (!QuietMode) {
          Console.WriteLine("{0} instances of `{1}` each doing {2} iterations.",
            tasks.Count(),
            getJob().Description,
            Iterations);

          Console.WriteLine("Hit any key to start.");
          Console.ReadKey();
        }

        // starting
        if (Instances == 1)
          tasks.First().RunSynchronously();
        else
          Array.ForEach(tasks, t => t.Start());

        Task.WaitAll(tasks);

        // report time taken
        if (!QuietMode) {
          Console.WriteLine("All done.");

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

      } while (!QuietMode); // don't repeat in quietmode
    }


    private BenchmarkResult benchMark(Job job, int iterations)
    {
      var client = job.Client =
        _pool == null ? new RedisClient<string>() : _pool.GetClient();
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
