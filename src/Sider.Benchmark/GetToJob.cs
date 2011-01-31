
using System.IO;
using System.Security.Cryptography;

namespace Sider.Benchmark
{
  public class GetToJob : Job
  {
    private byte[] _data;
    private Stream _stream;
    private string _key;


    public override string Description
    {
      get
      {
        return string.Format("Client.GetTo() with {0} bytes of data payload",
          _data.Length);
      }
    }


    public GetToJob(int dataSize = 1024)
    {
      using (var rng = RandomNumberGenerator.Create()) {
        rng.GetBytes(_data = new byte[dataSize]);

        _key = "TESTGETTO" + _data[0].ToString();
      }
    }


    public override void BeforeBenchmark()
    {
      Client.SetRaw(_key, _data);
      _stream = new MemoryStream(_data.Length);
    }

    public override void RunOneIteration()
    {
      _stream.Seek(0, SeekOrigin.Begin);
      Client.GetTo(_key, _stream);
    }

    public override void AfterBenchmark()
    {
      _stream.Dispose();
    }
  }
}
