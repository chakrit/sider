using System;
using System.IO;

namespace Sider {
  public class RedisReader {
    readonly Stream stream;
    readonly RedisSettings settings;

    public RedisReader(Stream stream, RedisSettings settings) {
      if (stream == null) throw new ArgumentNullException("stream");
      if (settings == null) throw new ArgumentNullException("settings");

      this.stream = stream;
      this.settings = settings;
    }
  }
}

