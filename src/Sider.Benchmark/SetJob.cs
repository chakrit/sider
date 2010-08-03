
using System;
using System.Security.Cryptography;

namespace Sider.Benchmark
{
  public class SetJob : Job
  {
    private byte[] _data;
    private string _key;


    public override string Description
    {
      get { return string.Format("Repeated SET with {0} bytes payload.", _data.Length); }
    }

    public SetJob(int dataSize = 1024)
    {
      using (var rng = RandomNumberGenerator.Create()) {
        rng.GetBytes(_data = new byte[dataSize]);

        // snatch the first byte as a random ID
        _key = "TEST" + _data[0].ToString();
      }
    }


    public override void RunOneIteration()
    {
      Client.SetRaw(_key, _data);
    }

    public override void Teardown()
    {
      Client.Del(_key);
    }

  }
}
