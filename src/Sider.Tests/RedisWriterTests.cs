
using System;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sider.Tests
{
  [TestClass]
  public class RedisWriterTests
  {
    [TestMethod, ExpectedException(typeof(ArgumentNullException)), Conditional("DEBUG")]
    public void Ctor_StreamIsNull_ExceptionThrown()
    {
      new RedisWriter(null);
    }


  }
}
