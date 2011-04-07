
using System;

namespace Sider.Samples
{
  public abstract class Sample
  {
    public abstract string Name { get; }

    public abstract void Run();


    protected void Log(string str)
    {
      Console.WriteLine(str);
    }
  }
}
