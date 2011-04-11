
using System;

namespace Sider.Tests.Serialization
{
  [Serializable]
  public struct TestStruct
  {
    [Serializable]
    public struct InnerStruct
    {
      public string Str { get; set; }
      public int Int { get; set; }
    }


    public string Str { get; set; }
    public int Int { get; set; }

    public InnerStruct Nested { get; set; }


    public TestStruct(string str, int i, string str2, int i2) :
      this()
    {
      Str = str;
      Int = i;
      Nested = new InnerStruct { Str = str2, Int = i2 };
    }
  }
}
