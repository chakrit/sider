
using System.Linq;
using System;

namespace Sider.Samples
{
  public class Program
  {
    internal static void Main(string[] args) { new Program().Run(); }


    public void Run()
    {
      var sampleType = typeof(Sample);
      var samples = sampleType
        .Assembly
        .GetTypes()
        .Where(t => sampleType.IsAssignableFrom(t))
        .Where(t => !(t.IsAbstract || t.IsInterface))
        .Select(t => (Sample)Activator.CreateInstance(t))
        .ToArray();

      while (true) {
        // list samples as old-style dos menu
        Console.Clear();
        Console.WriteLine("Select sample to run:");
        for (var i = 0; i < samples.Length; i++)
          Console.WriteLine(" {0,2}. {1}", i, samples[i].Name);

        // get user selection
        var userInput = Console.ReadLine();
        int sampleIdx;
        if (!int.TryParse(userInput, out sampleIdx))
          continue;

        // run the selected sample
        var sample = samples[sampleIdx];
        Console.WriteLine("Running sample: {0}", sample.Name);

        sample.Run();
        Console.ReadKey();
      }
    }
  }
}
