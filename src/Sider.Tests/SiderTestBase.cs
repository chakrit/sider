
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sider.Tests
{
  public class SiderTestBase
  {
    public TestContext TestContext { get; set; }


    protected void Expect<T>(Action throwAct)
      where T : Exception
    {
      try {
        throwAct();
        Assert.Fail("Expecting exception of type `{0}` but none thrown."
          .F(typeof(T).Name));
      }
      catch (T) {
        return;
      }
    }
  }
}
