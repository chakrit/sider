using System;

namespace Sider {
  public enum ResponseType {
    Error = '-',
    SingleLine = '+',
    Bulk = '$',
    MultiBulk = '*',
    Integer = ':',
  }
}

