
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sider.Benchmark
{
  internal class Program
  {
    public static void Main(string[] args) { (new Program()).Run(); }


    public void Run()
    {
      // configuration
      var instances = 1;
      var iterations = 10000;

      Func<Job> getJob = () => new SetJob();


      while (true) {

        // setup
        var tasks = Enumerable
          .Range(0, instances)
          .Select(n => getJob())
          .Select(job => new Task<BenchmarkResult>(() => benchMark(job, iterations)))
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
          foreach (var task in tasks)
            task.Start();

        Task.WaitAll(tasks);
        Console.WriteLine("All done.");

        // report time taken
        foreach (var task in tasks) {
          Console.WriteLine("{0} : {1}ms",
            task.Result.Job.Description,
            task.Result.MillisecondsTaken);
        }
        Console.ReadKey();

      }
    }


    private BenchmarkResult benchMark(Job job, int iterations,
      Action<int> onIteration = null)
    {
      var client = job.Client = new RedisClient();
      onIteration = onIteration ?? (_ => { });

      var sw = new Stopwatch();
      sw.Reset();

      // run the benchmark
      sw.Start();
      for (var i = 0; i < iterations; i++) {
        onIteration(i);
        job.RunOneIteration();
      }
      sw.Stop();

      job.Client = null;
      client.Dispose();

      return new BenchmarkResult {
        Job = job,
        Iterations = iterations,
        MillisecondsTaken = sw.ElapsedMilliseconds
      };
    }
  }
}
