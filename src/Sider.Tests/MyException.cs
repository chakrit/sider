
using System;

namespace Sider.Tests
{
  // a specific exception for testing so we don't accidentally caught
  // other unrelated types of exceptions while testing exception cases
  public class MyException : Exception
  {
  }
}
