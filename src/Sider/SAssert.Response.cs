
using System.Diagnostics;

namespace Sider
{
  internal static partial class SAssert
  {
    [Conditional("DEBUG")]
    public static void ResponseType(ResponseType expected, ResponseType actual)
    {
      if (expected != actual)
        throw new ResponseException(
          "Expected a `{0}` reply, got instead `{1}` reply.".F(expected, actual));
    }
  }
}
