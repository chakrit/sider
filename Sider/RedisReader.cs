using System;
using System.IO;

namespace Sider {
  public class RedisReader {
    readonly Stream stream;

    public RedisReader(Stream stream) {
      this.stream = stream;
    }
  }
}

