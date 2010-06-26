
using System;
using System.Security.Cryptography;

namespace Sider.Benchmark
{
  public class SetJob : Job
  {
    private RandomNumberGenerator _rng;
    private byte[] _data;


    public override string Description
    {
      get { return "Repeated SET using same data"; }
    }

    public SetJob()
    {
      _rng = RandomNumberGenerator.Create();
      _data = new byte[1024];

      _rng.GetBytes(_data);
    }


    public override void RunOneIteration()
    {
      Client.Set("TEST", _data);
    }

  }
}
