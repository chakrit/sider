
using System;

namespace Sider.Tests.Serialization
{
  [Serializable]
  public class TestObj
  {
    public string Str { get; set; }
    public int Int { get; set; }

    public TestObj Nested { get; set; }


    private TestObj() { }

    public TestObj(string str, int i, string str2, int i2)
    {
      Str = str;
      Int = i;
      Nested = new TestObj { Str = str2, Int = i2 };
    }
  }
}
