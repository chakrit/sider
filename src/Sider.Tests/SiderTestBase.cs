
using System;
using NUnit.Framework;

namespace Sider.Tests
{
  [TestFixture]
  public abstract class SiderTestBase
  {
    protected TestContext TestContext
    {
      get { return TestContext.CurrentContext; }
    }


    public void Log(string message)
    {
      Console.WriteLine(message);
    }
  }
}
