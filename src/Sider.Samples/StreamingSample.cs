
using System.IO;

namespace Sider.Samples
{
  public class StreamingSample : Sample
  {
    public override string Name
    {
      get { return "Streaming commands for working with binary data."; }
    }

    public override void Run()
    {
      var client = new RedisClient();

      // ask user for 2 file paths
      WriteLine("Enter source file path:");
      var srcPath = Path.GetFullPath(ReadLine());
      WriteLine("Enter destination file path:");
      var destPath = Path.GetFullPath(ReadLine());

      // validate
      if (!File.Exists(srcPath)) {
        WriteLine("Specified source file does not exist.");
        return;
      }

      if (File.Exists(destPath)) {
        WriteLine("Destination file already exist, this will overwrite the file.");
        WriteLine("Press ENTER to continue.");
        ReadLine();
      }

      // perform a file copy via redis
      WriteLine("Writing contents of " + srcPath + " to Redis.");
      using (var fs = File.OpenRead(srcPath)) {
        client.SetFrom("file", fs, (int)fs.Length);
        fs.Close();
      }

      // "file" now store the content of srcPath file
      // reads it out to the destination stream
      WriteLine("Writing data from Redis to " + destPath);
      using (var fs = File.OpenWrite(destPath)) {
        var bytesWrote = client.GetTo("file", fs);
        WriteLine(bytesWrote.ToString() + " bytes written.");

        fs.Flush();
        fs.Close();
      }

      WriteLine("Copy completed.");
      WriteLine("Please check that the source and destination file mathces.");

      client.Dispose();
    }
  }
}
