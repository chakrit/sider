
using System.Security.Cryptography;

namespace Sider.Benchmark
{
  public class GetJob : Job
  {
    private byte[] _data;
    private string _key;


    public override string Description
    {
      get { return string.Format("Repeated GET with {0} bytes payload.", _data.Length); }
    }

    public GetJob(int dataSize = 1024)
    {
      using (var rng = RandomNumberGenerator.Create()) {
        rng.GetBytes(_data = new byte[dataSize]);

        _key = "TEST" + _data[0].ToString();
      }
    }


    public override void Setup()
    {
      Client.SetRaw(_key, _data);
    }

    public override void RunOneIteration()
    {
      Client.Get(_key);
    }

    public override void Teardown()
    {
      Client.Del(_key);
    }
  }
}
