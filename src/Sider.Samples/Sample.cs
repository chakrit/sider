
using System;

namespace Sider.Samples
{
  public abstract class Sample
  {
    public abstract string Name { get; }

    public abstract void Run();


    protected string ReadLine()
    {
      return Console.ReadLine();
    }

    protected void WriteLine(string str)
    {
      Console.WriteLine(str);
    }
  }
}
