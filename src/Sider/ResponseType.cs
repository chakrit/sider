
using System;

namespace Sider
{
  // Per http://code.google.com/p/redis/wiki/ProtocolSpecification
  // as of June 8th, 2010
  public enum ResponseType
  {
    Error = '-',
    SingleLine = '+',
    Bulk = '$',
    MultiBulk = '*',
    Integer = ':',
  }
}
